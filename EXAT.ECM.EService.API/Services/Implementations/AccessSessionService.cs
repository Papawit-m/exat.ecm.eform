using EXAT.ECM.EService.API.DAL;
using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Services.Interfaces;
using Oracle.ManagedDataAccess.Client;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    public class AccessSessionService : IAccessSessionService
    {
        private readonly OracleDbContext _db;
        private readonly string _connectionString;

        public AccessSessionService(OracleDbContext db)
        {
            _db = db;
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
        }

        public async Task<AccessSessionModel?> GetSessionAsync(string token)
        {

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("EFM_EER.SP_7003_GETSESSIONAUTHEN", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_TOKEN", OracleDbType.Varchar2).Value = token ?? (object)DBNull.Value;

            var output = new OracleParameter("p_output", OracleDbType.RefCursor)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(output);

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            if (!reader.HasRows)
                return null;

            // อ่านแถวแรกพอ
            if (await reader.ReadAsync())
            {
                // 👇 ปรับชื่อ column ให้ตรงกับที่ SP return จริงนะ
                var entity = new AccessSessionModel
                {
                    TOKEN = reader["TOKEN"]?.ToString(),
                    DEVICE_ID = reader["DEVICE_ID"] == DBNull.Value ? null : reader["DEVICE_ID"].ToString(),
                    IS_ACTIVE = Convert.ToInt32(reader["IS_ACTIVE"])
                };

                return entity;
            }

            return null;
        }

        public async Task UpdateDeviceId(string token, string deviceId)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("EFM_EER.SP_7003_UPDATE_DEVICEID", conn); // เปลี่ยนเป็นชื่อ SP จริง
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("p_TOKEN", OracleDbType.Varchar2).Value = token;
            cmd.Parameters.Add("p_DEVICE_ID", OracleDbType.Varchar2).Value = deviceId;

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        
    }
}
