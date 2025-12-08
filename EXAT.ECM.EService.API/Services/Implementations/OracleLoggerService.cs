using EXAT.ECM.EService.API.DAL;
using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Services.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Text.Json;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    public class OracleLoggerService : IOracleLoggerService
    {
        private readonly string _connectionString;
        private readonly ILogger<OracleLoggerService> _logger;
        private readonly OracleDbContext _oracleContext;

        public OracleLoggerService(OracleDbContext oracleContext,IConfiguration configuration, ILogger<OracleLoggerService> logger)
        {
            _oracleContext = oracleContext;
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            //_connectionString = configuration.GetConnectionString("OracleConnection");
            _logger = logger;
        }

        public async Task LogAsync(LogEntry logEntry)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = @"
                    INSERT INTO S_API_ESERVICE_LOG (
                        LOG_ID,
                        LOG_LEVEL,
                        ENDPOINT,
                        HTTP_METHOD,
                        REQUEST_PATH,
                        REQUEST_PARAMETERS,
                        USERNAME,
                        CUSTOMER_ID,
                        EMAIL,
                        STATUS_CODE,
                        SUCCESS_FLAG,
                        MESSAGE,
                        ERROR_MESSAGE,
                        EXECUTION_TIME,
                        REQUEST_TIMESTAMP,
                        RESPONSE_TIMESTAMP,
                        REQUEST_JSON,
                        RESPONSE_JSON
                    ) VALUES (
                        SEQ_S_API_ESERVICE_LOG.NEXTVAL,
                        :LogLevel,
                        :Endpoint,
                        :HttpMethod,
                        :RequestPath,
                        :RequestParameters,
                        :Username,
                        :CustomerId,
                        :Email,
                        :StatusCode,
                        :SuccessFlag,
                        :Message,
                        :ErrorMessage,
                        :ExecutionTime,
                        SYSTIMESTAMP,
                        CASE WHEN :HasResponseTime = 'Y' THEN SYSTIMESTAMP ELSE NULL END,
                        :RequestJson,
                        :ResponseJson
                    )";

                using var command = new OracleCommand(sql, connection);

                command.Parameters.Add(":LogLevel", OracleDbType.Varchar2).Value = logEntry.LogLevel ?? (object)DBNull.Value;
                command.Parameters.Add(":Endpoint", OracleDbType.Varchar2).Value = logEntry.Endpoint ?? (object)DBNull.Value;
                command.Parameters.Add(":HttpMethod", OracleDbType.Varchar2).Value = logEntry.HttpMethod ?? (object)DBNull.Value;
                command.Parameters.Add(":RequestPath", OracleDbType.Varchar2).Value = logEntry.RequestPath ?? (object)DBNull.Value;
                command.Parameters.Add(":RequestParameters", OracleDbType.Clob).Value = logEntry.RequestParameters ?? (object)DBNull.Value;
                command.Parameters.Add(":Username", OracleDbType.Varchar2).Value = logEntry.Username ?? (object)DBNull.Value;
                command.Parameters.Add(":CustomerId", OracleDbType.Varchar2).Value = logEntry.CustomerId ?? (object)DBNull.Value;
                command.Parameters.Add(":Email", OracleDbType.Varchar2).Value = logEntry.Email ?? (object)DBNull.Value;
                command.Parameters.Add(":StatusCode", OracleDbType.Int32).Value = logEntry.StatusCode.HasValue ? logEntry.StatusCode.Value : (object)DBNull.Value;
                command.Parameters.Add(":SuccessFlag", OracleDbType.Char).Value = logEntry.SuccessFlag ?? (object)DBNull.Value;
                command.Parameters.Add(":Message", OracleDbType.Varchar2).Value = logEntry.Message ?? (object)DBNull.Value;
                command.Parameters.Add(":ErrorMessage", OracleDbType.Clob).Value = logEntry.ErrorMessage ?? (object)DBNull.Value;
                // Convert execution time from milliseconds to seconds
                command.Parameters.Add(":ExecutionTime", OracleDbType.Decimal).Value = logEntry.ExecutionTime.HasValue ? Math.Round(logEntry.ExecutionTime.Value / 1000.0, 3) : (object)DBNull.Value;
                // Use database SYSTIMESTAMP for timestamps (always in Gregorian calendar year - ค.ศ.)
                command.Parameters.Add(":HasResponseTime", OracleDbType.Char).Value = logEntry.ResponseTimestamp.HasValue ? "Y" : "N";
                command.Parameters.Add(":RequestJson", OracleDbType.Clob).Value = logEntry.RequestJson ?? (object)DBNull.Value;
                command.Parameters.Add(":ResponseJson", OracleDbType.Clob).Value = logEntry.ResponseJson ?? (object)DBNull.Value;

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                // Log to console if Oracle logging fails
                _logger.LogError(ex, "Failed to log to Oracle database");
            }
        }

        public async Task LogInformationAsync(string endpoint, string httpMethod, string requestPath, string? message = null, string? username = null, string? customerId = null, string? email = null, double? executionTime = null, object? requestData = null, object? responseData = null)
        {
            var logEntry = new LogEntry
            {
                LogLevel = "Information",
                Endpoint = endpoint,
                HttpMethod = httpMethod,
                RequestPath = requestPath,
                Message = message,
                Username = username,
                CustomerId = customerId,
                Email = email,
                ExecutionTime = executionTime,
                SuccessFlag = "Y",
                RequestTimestamp = DateTime.Now,
                RequestJson = requestData != null ? JsonSerializer.Serialize(requestData, new JsonSerializerOptions { WriteIndented = false }) : null,
                ResponseJson = responseData != null ? JsonSerializer.Serialize(responseData, new JsonSerializerOptions { WriteIndented = false }) : null
            };

            await LogAsync(logEntry);
        }

        public async Task LogWarningAsync(string endpoint, string httpMethod, string requestPath, string message, string? username = null, string? customerId = null, string? email = null, object? requestData = null)
        {
            var logEntry = new LogEntry
            {
                LogLevel = "Warning",
                Endpoint = endpoint,
                HttpMethod = httpMethod,
                RequestPath = requestPath,
                Message = message,
                Username = username,
                CustomerId = customerId,
                Email = email,
                SuccessFlag = "N",
                RequestTimestamp = DateTime.Now,
                RequestJson = requestData != null ? JsonSerializer.Serialize(requestData, new JsonSerializerOptions { WriteIndented = false }) : null
            };

            await LogAsync(logEntry);
        }

        public async Task LogErrorAsync(string endpoint, string httpMethod, string requestPath, string message, Exception? exception = null, string? username = null, string? customerId = null, string? email = null, double? executionTime = null, object? requestData = null)
        {
            var logEntry = new LogEntry
            {
                LogLevel = "Error",
                Endpoint = endpoint,
                HttpMethod = httpMethod,
                RequestPath = requestPath,
                Message = message,
                ErrorMessage = exception != null ? $"{exception.Message}\n\nStack Trace:\n{exception.StackTrace}" : null,
                Username = username,
                CustomerId = customerId,
                Email = email,
                ExecutionTime = executionTime,
                SuccessFlag = "N",
                RequestTimestamp = DateTime.Now,
                ResponseTimestamp = DateTime.Now,
                RequestJson = requestData != null ? JsonSerializer.Serialize(requestData, new JsonSerializerOptions { WriteIndented = false }) : null
            };

            await LogAsync(logEntry);
        }

    }
}
