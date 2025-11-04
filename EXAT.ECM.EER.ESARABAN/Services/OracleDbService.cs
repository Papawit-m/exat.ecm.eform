using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace EXAT.ECM.EER.ESARABAN.Services
{
    /// <summary>
    /// Interface for Oracle Database Service
    /// </summary>
    public interface IOracleDbService
    {
        /// <summary>
        /// Get Oracle connection for normal user
        /// </summary>
        OracleConnection GetConnection();

        /// <summary>
        /// Get Oracle connection for SYSDBA user
        /// </summary>
        OracleConnection GetSysDbaConnection();

        /// <summary>
        /// Test database connection
        /// </summary>
        Task<bool> TestConnectionAsync();

        /// <summary>
        /// Execute a query and return data table
        /// </summary>
        Task<DataTable> ExecuteQueryAsync(string query, OracleParameter[]? parameters = null, bool useSysDba = false);

        /// <summary>
        /// Execute non-query command (INSERT, UPDATE, DELETE)
        /// </summary>
        Task<int> ExecuteNonQueryAsync(string query, OracleParameter[]? parameters = null, bool useSysDba = false);

        /// <summary>
        /// Execute scalar query (returns single value)
        /// </summary>
        Task<object?> ExecuteScalarAsync(string query, OracleParameter[]? parameters = null, bool useSysDba = false);
    }

    /// <summary>
    /// Oracle Database Service Implementation
    /// </summary>
    public class OracleDbService : IOracleDbService
    {
        private readonly string _normalConnectionString;
        private readonly string _sysDbaConnectionString;
        private readonly ILogger<OracleDbService> _logger;

        public OracleDbService(IConfiguration configuration, ILogger<OracleDbService> logger)
        {
            //_normalConnectionString = configuration.GetConnectionString("OracleConnection")
            //    ?? throw new ArgumentNullException("OracleConnection string is not configured");

            //_sysDbaConnectionString = configuration.GetConnectionString("OracleConnectionSYSDBA")
            //    ?? throw new ArgumentNullException("OracleConnectionSYSDBA string is not configured");

            _normalConnectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            _sysDbaConnectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            _logger = logger;
        }

        /// <summary>
        /// Get Oracle connection for normal user
        /// </summary>
        public OracleConnection GetConnection()
        {
            return new OracleConnection(_normalConnectionString);
        }

        /// <summary>
        /// Get Oracle connection for SYSDBA user
        /// </summary>
        public OracleConnection GetSysDbaConnection()
        {
            return new OracleConnection(_sysDbaConnectionString);
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = GetConnection();
                await connection.OpenAsync();
                _logger.LogInformation("Oracle database connection test successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oracle database connection test failed");
                return false;
            }
        }

        /// <summary>
        /// Execute a query and return data table
        /// </summary>
        public async Task<DataTable> ExecuteQueryAsync(string query, OracleParameter[]? parameters = null, bool useSysDba = false)
        {
            try
            {
                using var connection = useSysDba ? GetSysDbaConnection() : GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                using var adapter = new OracleDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);

                _logger.LogInformation("Query executed successfully. Rows returned: {RowCount}", dataTable.Rows.Count);
                return dataTable;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Execute non-query command (INSERT, UPDATE, DELETE)
        /// </summary>
        public async Task<int> ExecuteNonQueryAsync(string query, OracleParameter[]? parameters = null, bool useSysDba = false)
        {
            try
            {
                using var connection = useSysDba ? GetSysDbaConnection() : GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var rowsAffected = await command.ExecuteNonQueryAsync();
                _logger.LogInformation("Non-query executed successfully. Rows affected: {RowsAffected}", rowsAffected);
                return rowsAffected;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing non-query: {Query}", query);
                throw;
            }
        }

        /// <summary>
        /// Execute scalar query (returns single value)
        /// </summary>
        public async Task<object?> ExecuteScalarAsync(string query, OracleParameter[]? parameters = null, bool useSysDba = false)
        {
            try
            {
                using var connection = useSysDba ? GetSysDbaConnection() : GetConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandType = CommandType.Text;

                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var result = await command.ExecuteScalarAsync();
                _logger.LogInformation("Scalar query executed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing scalar query: {Query}", query);
                throw;
            }
        }
    }
}
