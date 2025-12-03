using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using EXAT.ECM.FED.API.DAL;
using EXAT.ECM.FED.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace EXAT.ECM.FED.API.Services
{
    public class DbLoggingService : ILoggingService
    {
        private readonly OracleDbContext _db;
        private readonly string _connectionString;

        // ปรับชื่อให้สื่อ และไม่ "await" DbContext
        public DbLoggingService(OracleDbContext dbContext, IConfiguration configuration)
        {
            _db = dbContext ;
            //_connectionString = configuration.GetConnectionString("OracleConnection");
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
        }

        // ============== Public APIs ==============

        public Task LogErrorAsync(string logLevel, Exception ex, string message, string detail)
            => LogAsync(logLevel ?? "ERROR", ex, message, detail, CancellationToken.None);

        // เผื่อไว้ถ้าต้องLogแบบไม่ต้องโยน Exception
        public Task LogInfoAsync(string message, string? detail = null, CancellationToken ct = default)
            => LogAsync("INFO", null, message, detail ?? string.Empty, ct);

        public Task LogDebugAsync(string message, string? detail = null, CancellationToken ct = default)
            => LogAsync("DEBUG", null, message, detail ?? string.Empty, ct);

        public Task LogWarnAsync(string message, string? detail = null, CancellationToken ct = default)
            => LogAsync("WARN", null, message, detail ?? string.Empty, ct);

        public async Task LogInfoAsync(string message, string? contextInfo = null)
        {
            try
            {
                await using var connection = await _db.GetOpenConnectionAsync();
                await using var command = new Oracle.ManagedDataAccess.Client.OracleCommand(@"INSERT INTO FLEET_CARD_APP_LOGS (LOG_TIMESTAMP, LOG_LEVEL, MESSAGE, STACK_TRACE, CONTEXT_INFO)
                    VALUES (SYSTIMESTAMP, :logLevel, :message, NULL, :contextInfo)", connection)
                {
                    CommandType = System.Data.CommandType.Text
                };

                command.Parameters.Add(":logLevel", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 20).Value = "INFO";
                command.Parameters.Add(":message", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 4000).Value = message;
                command.Parameters.Add(":contextInfo", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 1000).Value = contextInfo ?? string.Empty;

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WARN: Could not write INFO log. {ex.Message}");
            }
        }

        // ============== Core ==============

        private async Task LogAsync(
            string logLevel,
            Exception? ex,
            string message,
            string detail,
            CancellationToken ct)
        {
            try
            {
                // 1) ดึง ADO.NET connection จาก EF Core อย่างถูกต้อง
                var dbConn = _db.Database.GetDbConnection();
                if (dbConn is not OracleConnection oraConn)
                {
                    // กันกรณี provider ไม่ใช่ Oracle
                    throw new InvalidOperationException("Database connection is not OracleConnection.");
                }

                // 2) เปิด connection ถ้ายังไม่เปิด
                if (oraConn.State != ConnectionState.Open)
                    await oraConn.OpenAsync(ct).ConfigureAwait(false);

                // 3) สร้างคำสั่ง (BindByName = true ให้แมพตามชื่อพารามิเตอร์ชัดเจน)
                using var cmd = oraConn.CreateCommand();
                cmd.BindByName = true;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText =
                    @"INSERT INTO FLEET_CARD_APP_LOGS
                        (LOG_TIMESTAMP, LOG_LEVEL, MESSAGE, STACK_TRACE, CONTEXT_INFO)
                      VALUES
                        (SYSTIMESTAMP, :logLevel, :message, :stackTrace, :contextInfo)";

                // 4) เตรียมค่าข้อความ และจัดการความยาวให้สอดคล้องคอลัมน์ปลายทาง
                //    - สมมติคอลัมน์ MESSAGE VARCHAR2(4000), CONTEXT_INFO VARCHAR2(1000)
                string messageToSave = (ex?.Message ?? message) ?? string.Empty;
                string contextToSave = BuildContextInfo(message, detail);

                messageToSave = TruncateForVarchar2(messageToSave, 4000);
                contextToSave = TruncateForVarchar2(contextToSave, 1000);

                // 5) Add parameters (กำหนด OracleDbType ให้เหมาะ)
                cmd.Parameters.Add(new OracleParameter("logLevel", OracleDbType.Varchar2, 20, logLevel ?? "ERROR", ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("message", OracleDbType.Varchar2, 4000, messageToSave, ParameterDirection.Input));
                // ใช้ CLOB สำหรับ StackTrace (รองรับยาว ๆ)
                var stack = ex?.ToString() ?? string.Empty;
                cmd.Parameters.Add(new OracleParameter("stackTrace", OracleDbType.Clob, stack, ParameterDirection.Input));
                cmd.Parameters.Add(new OracleParameter("contextInfo", OracleDbType.Varchar2, 1000, contextToSave, ParameterDirection.Input));

                // 6) Execute
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
            catch (Exception dbEx)
            {
                // Fallback: อย่าให้การเขียน Log ไป DB ทำให้แอปล้ม — พิมพ์ลง Console แทน
                try
                {
                    Console.Error.WriteLine($"FATAL: Could not write to database log. DB-Error: {dbEx.Message}");
                    if (ex is not null)
                        Console.Error.WriteLine($"Original-Error: {ex.Message}\n{ex}");
                    else
                        Console.Error.WriteLine($"Original-Message: {message}\nDetail: {detail}");
                }
                catch
                {
                    // เงียบไว้ ป้องกัน cascading failures
                }
            }
        }

        // ============== Helpers ==============

        private static string BuildContextInfo(string message, string detail)
        {
            // รูปแบบสั้น ๆ อ่านง่าย
            // ข้อดี: แยก message ตั้งต้น (business context) ออกจาก ex.Message ที่บันทึกในคอลัมน์ MESSAGE
            var msg = message?.Trim() ?? string.Empty;
            var det = detail?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(det))
                return msg;

            return $"{msg} | Detail: {det}";
        }

        private static string TruncateForVarchar2(string input, int maxLen)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Length > maxLen ? input.Substring(0, maxLen) : input;
        }
    }
}
