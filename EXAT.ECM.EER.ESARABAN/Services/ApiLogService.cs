using Oracle.ManagedDataAccess.Client;

namespace EXAT.ECM.EER.ESARABAN.Services
{
    /// <summary>
    /// Service สำหรับบันทึก API Log ลง S_API_ESARABAN_LOG
    /// </summary>
    public interface IApiLogService
    {
        Task LogRequestAsync(ApiLogEntry logEntry);
        Task LogErrorAsync(string endpoint, string httpMethod, string username, Exception ex, string? requestBody = null);
        Task LogSuccessAsync(string endpoint, string httpMethod, string username, int statusCode, long executionTime, string? requestBody = null, string? responseData = null);
    }

    public class ApiLogService : IApiLogService
    {
        private readonly string _connectionString;
        private readonly ILogger<ApiLogService> _logger;
        private readonly string _sequenceQualifiedName;

        public ApiLogService(IConfiguration configuration, ILogger<ApiLogService> logger)
        {
            //_connectionString = configuration.GetConnectionString("OracleConnection")
            //    ?? throw new InvalidOperationException("Oracle connection string not found");

            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            _logger = logger;
            // Read sequence configuration (schema + sequence). If not provided, fallback to legacy name.
            var apiLogSection = configuration.GetSection("ApiLog");
            var schema = apiLogSection.GetValue<string>("Schema");
            var sequence = apiLogSection.GetValue<string>("SequenceName");
            if (!string.IsNullOrWhiteSpace(schema) && !string.IsNullOrWhiteSpace(sequence))
            {
                // e.g. EFM_EER.SEQ_S_API_ESERVICE_LOG.NEXTVAL
                _sequenceQualifiedName = $"{schema}.{sequence}.NEXTVAL";
            }
            else
            {
                // fallback to previous hard-coded sequence
                _sequenceQualifiedName = "EFM_EER.S_API_ESARABAN_LOG_SEQ.NEXTVAL";
            }
        }

        /// <summary>
        /// บันทึก Log ลง S_API_ESARABAN_LOG
        /// </summary>
        public async Task LogRequestAsync(ApiLogEntry logEntry)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                var sql = $@"
                    INSERT INTO EFM_EER.S_API_ESARABAN_LOG (
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
                        CREATED_DATE
                    ) VALUES (
                        {_sequenceQualifiedName},
                        :LOG_LEVEL,
                        :ENDPOINT,
                        :HTTP_METHOD,
                        :REQUEST_PATH,
                        :REQUEST_PARAMETERS,
                        :USERNAME,
                        :CUSTOMER_ID,
                        :EMAIL,
                        :STATUS_CODE,
                        :SUCCESS_FLAG,
                        :MESSAGE,
                        :ERROR_MESSAGE,
                        :EXECUTION_TIME,
                        :REQUEST_TIMESTAMP,
                        :RESPONSE_TIMESTAMP,
                        SYSTIMESTAMP
                    )";

                using var command = new OracleCommand(sql, connection);

                command.Parameters.Add("LOG_LEVEL", OracleDbType.Varchar2).Value = logEntry.LogLevel ?? (object)DBNull.Value;
                command.Parameters.Add("ENDPOINT", OracleDbType.Varchar2).Value = logEntry.Endpoint ?? (object)DBNull.Value;
                command.Parameters.Add("HTTP_METHOD", OracleDbType.Varchar2).Value = logEntry.HttpMethod ?? (object)DBNull.Value;
                command.Parameters.Add("REQUEST_PATH", OracleDbType.Varchar2).Value = logEntry.RequestPath ?? (object)DBNull.Value;
                command.Parameters.Add("REQUEST_PARAMETERS", OracleDbType.Clob).Value = logEntry.RequestParameters ?? (object)DBNull.Value;
                command.Parameters.Add("USERNAME", OracleDbType.Varchar2).Value = logEntry.Username ?? (object)DBNull.Value;
                command.Parameters.Add("CUSTOMER_ID", OracleDbType.Varchar2).Value = logEntry.CustomerId ?? (object)DBNull.Value;
                command.Parameters.Add("EMAIL", OracleDbType.Varchar2).Value = logEntry.Email ?? (object)DBNull.Value;
                command.Parameters.Add("STATUS_CODE", OracleDbType.Int32).Value = logEntry.StatusCode.HasValue ? logEntry.StatusCode.Value : DBNull.Value;
                command.Parameters.Add("SUCCESS_FLAG", OracleDbType.Char).Value = logEntry.SuccessFlag ?? (object)DBNull.Value;
                command.Parameters.Add("MESSAGE", OracleDbType.Varchar2).Value = logEntry.Message ?? (object)DBNull.Value;
                command.Parameters.Add("ERROR_MESSAGE", OracleDbType.Clob).Value = logEntry.ErrorMessage ?? (object)DBNull.Value;
                command.Parameters.Add("EXECUTION_TIME", OracleDbType.Int64).Value = logEntry.ExecutionTime.HasValue ? logEntry.ExecutionTime.Value : DBNull.Value;
                command.Parameters.Add("REQUEST_TIMESTAMP", OracleDbType.TimeStamp).Value = logEntry.RequestTimestamp ?? (object)DBNull.Value;
                command.Parameters.Add("RESPONSE_TIMESTAMP", OracleDbType.TimeStamp).Value = logEntry.ResponseTimestamp ?? (object)DBNull.Value;

                await command.ExecuteNonQueryAsync();

                _logger.LogInformation($"API Log saved successfully: {logEntry.Endpoint}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save API log: {ex.Message}");
                // Don't throw - logging should not break the main flow
            }
        }

        /// <summary>
        /// บันทึก Error Log
        /// </summary>
        public async Task LogErrorAsync(string endpoint, string httpMethod, string username, Exception ex, string? requestBody = null)
        {
            var logEntry = new ApiLogEntry
            {
                LogLevel = "ERROR",
                Endpoint = endpoint,
                HttpMethod = httpMethod,
                RequestPath = endpoint,
                RequestParameters = requestBody,
                Username = username,
                StatusCode = 500,
                SuccessFlag = "N",
                Message = "Internal server error",
                ErrorMessage = $"{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                ExecutionTime = null,
                RequestTimestamp = DateTime.Now,
                ResponseTimestamp = DateTime.Now
            };

            await LogRequestAsync(logEntry);
        }

        /// <summary>
        /// บันทึก Success Log
        /// </summary>
        public async Task LogSuccessAsync(string endpoint, string httpMethod, string username, int statusCode, long executionTime, string? requestBody = null, string? responseData = null)
        {
            var logEntry = new ApiLogEntry
            {
                LogLevel = "INFO",
                Endpoint = endpoint,
                HttpMethod = httpMethod,
                RequestPath = endpoint,
                RequestParameters = requestBody,
                Username = username,
                StatusCode = statusCode,
                SuccessFlag = "Y",
                Message = responseData ?? "Success",
                ErrorMessage = null,
                ExecutionTime = executionTime,
                RequestTimestamp = DateTime.Now.AddMilliseconds(-executionTime),
                ResponseTimestamp = DateTime.Now
            };

            await LogRequestAsync(logEntry);
        }
    }

    /// <summary>
    /// Model สำหรับ API Log Entry
    /// </summary>
    public class ApiLogEntry
    {
        public string? LogLevel { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestParameters { get; set; }
        public string? Username { get; set; }
        public string? CustomerId { get; set; }
        public string? Email { get; set; }
        public int? StatusCode { get; set; }
        public string? SuccessFlag { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public long? ExecutionTime { get; set; }
        public DateTime? RequestTimestamp { get; set; }
        public DateTime? ResponseTimestamp { get; set; }
    }
}
