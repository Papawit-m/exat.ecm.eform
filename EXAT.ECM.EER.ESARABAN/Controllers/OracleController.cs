using EXAT.ECM.EER.ESARABAN.Models;
using EXAT.ECM.EER.ESARABAN.Services;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;

namespace EXAT.ECM.EER.ESARABAN.Controllers
{
    /// <summary>
    /// Oracle Database connectivity and management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OracleController : ControllerBase
    {
        private readonly IOracleDbService _oracleDbService;
        private readonly ILogger<OracleController> _logger;

        public OracleController(IOracleDbService oracleDbService, ILogger<OracleController> logger)
        {
            _oracleDbService = oracleDbService;
            _logger = logger;
        }

        /// <summary>
        /// Test Oracle database connection
        /// </summary>
        /// <returns>Connection test result</returns>
        [HttpGet("test-connection")]
        [SwaggerOperation(
            Summary = "Test Oracle connection",
            Description = "Tests the connection to Oracle database using normal user credentials",
            OperationId = "TestOracleConnection"
        )]
        [SwaggerResponse(200, "Connection successful", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Connection failed")]
        public async Task<ActionResult<ApiResponse<object>>> TestConnection()
        {
            try
            {
                var isConnected = await _oracleDbService.TestConnectionAsync();

                if (isConnected)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = "Oracle database connection successful",
                        Data = new
                        {
                            Status = "Connected",
                            DatabaseType = "Oracle 11g",
                            User = "EFM_EER",
                            ServiceName = "ecmdev"
                        }
                    });
                }
                else
                {
                    return StatusCode(500, new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to connect to Oracle database",
                        Error = "Connection test failed"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing Oracle connection");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error testing database connection",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get Oracle database version
        /// </summary>
        /// <returns>Database version information</returns>
        [HttpGet("version")]
        [SwaggerOperation(
            Summary = "Get database version",
            Description = "Retrieves Oracle database version information",
            OperationId = "GetDatabaseVersion"
        )]
        [SwaggerResponse(200, "Version retrieved successfully", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Failed to retrieve version")]
        public async Task<ActionResult<ApiResponse<object>>> GetVersion()
        {
            try
            {
                var query = "SELECT BANNER FROM V$VERSION WHERE ROWNUM = 1";
                var result = await _oracleDbService.ExecuteScalarAsync(query);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Database version retrieved successfully",
                    Data = new
                    {
                        Version = result?.ToString() ?? "Unknown",
                        Timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database version");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving database version",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get current database time
        /// </summary>
        /// <returns>Current database timestamp</returns>
        [HttpGet("current-time")]
        [SwaggerOperation(
            Summary = "Get database time",
            Description = "Retrieves current timestamp from Oracle database",
            OperationId = "GetDatabaseTime"
        )]
        [SwaggerResponse(200, "Time retrieved successfully", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Failed to retrieve time")]
        public async Task<ActionResult<ApiResponse<object>>> GetCurrentTime()
        {
            try
            {
                var query = "SELECT SYSDATE FROM DUAL";
                var result = await _oracleDbService.ExecuteScalarAsync(query);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Database time retrieved successfully",
                    Data = new
                    {
                        DatabaseTime = result,
                        ServerTime = DateTime.Now,
                        UtcTime = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database time");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error retrieving database time",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Execute custom SQL query
        /// </summary>
        /// <param name="request">SQL query request</param>
        /// <returns>Query results</returns>
        [HttpPost("execute-query")]
        [SwaggerOperation(
            Summary = "Execute SQL query",
            Description = "Executes a custom SQL query and returns results",
            OperationId = "ExecuteQuery"
        )]
        [SwaggerResponse(200, "Query executed successfully", typeof(ApiResponse<List<Dictionary<string, object>>>))]
        [SwaggerResponse(400, "Invalid query")]
        [SwaggerResponse(500, "Query execution failed")]
        public async Task<ActionResult<ApiResponse<List<Dictionary<string, object>>>>> ExecuteQuery([FromBody] SqlQueryRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest(new ApiResponse<List<Dictionary<string, object>>>
                    {
                        Success = false,
                        Message = "Query cannot be empty",
                        Error = "Invalid query"
                    });
                }

                var dataTable = await _oracleDbService.ExecuteQueryAsync(request.Query, null, request.UseSysDba);

                // Convert DataTable to List of Dictionary
                var results = new List<Dictionary<string, object>>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in dataTable.Columns)
                    {
                        dict[col.ColumnName] = row[col] == DBNull.Value ? null! : row[col];
                    }
                    results.Add(dict);
                }

                return Ok(new ApiResponse<List<Dictionary<string, object>>>
                {
                    Success = true,
                    Message = $"Query executed successfully. {results.Count} rows returned.",
                    Data = results
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query");
                return StatusCode(500, new ApiResponse<List<Dictionary<string, object>>>
                {
                    Success = false,
                    Message = "Error executing query",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get list of tables in the schema
        /// </summary>
        /// <returns>List of table names</returns>
        [HttpGet("tables")]
        [SwaggerOperation(
            Summary = "Get list of tables",
            Description = "Retrieves list of all tables in the current schema",
            OperationId = "GetTables"
        )]
        [SwaggerResponse(200, "Tables retrieved successfully", typeof(ApiResponse<List<string>>))]
        [SwaggerResponse(500, "Failed to retrieve tables")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetTables()
        {
            try
            {
                var query = @"SELECT TABLE_NAME 
                             FROM USER_TABLES 
                             ORDER BY TABLE_NAME";

                var dataTable = await _oracleDbService.ExecuteQueryAsync(query);

                var tables = new List<string>();
                foreach (DataRow row in dataTable.Rows)
                {
                    tables.Add(row["TABLE_NAME"].ToString()!);
                }

                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Message = $"Found {tables.Count} tables in the schema",
                    Data = tables
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tables list");
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "Error retrieving tables list",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get table structure/columns
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <returns>Table column information</returns>
        [HttpGet("tables/{tableName}/structure")]
        [SwaggerOperation(
            Summary = "Get table structure",
            Description = "Retrieves column information for a specific table",
            OperationId = "GetTableStructure"
        )]
        [SwaggerResponse(200, "Table structure retrieved successfully", typeof(ApiResponse<List<Dictionary<string, object>>>))]
        [SwaggerResponse(500, "Failed to retrieve table structure")]
        public async Task<ActionResult<ApiResponse<List<Dictionary<string, object>>>>> GetTableStructure(string tableName)
        {
            try
            {
                var query = @"SELECT COLUMN_NAME, DATA_TYPE, DATA_LENGTH, NULLABLE 
                             FROM USER_TAB_COLUMNS 
                             WHERE TABLE_NAME = :tableName 
                             ORDER BY COLUMN_ID";

                var parameters = new[]
                {
                    new OracleParameter("tableName", tableName.ToUpper())
                };

                var dataTable = await _oracleDbService.ExecuteQueryAsync(query, parameters);

                var columns = new List<Dictionary<string, object>>();
                foreach (DataRow row in dataTable.Rows)
                {
                    columns.Add(new Dictionary<string, object>
                    {
                        ["ColumnName"] = row["COLUMN_NAME"],
                        ["DataType"] = row["DATA_TYPE"],
                        ["Length"] = row["DATA_LENGTH"],
                        ["Nullable"] = row["NULLABLE"]
                    });
                }

                return Ok(new ApiResponse<List<Dictionary<string, object>>>
                {
                    Success = true,
                    Message = $"Table structure for {tableName} retrieved successfully",
                    Data = columns
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table structure for {TableName}", tableName);
                return StatusCode(500, new ApiResponse<List<Dictionary<string, object>>>
                {
                    Success = false,
                    Message = $"Error retrieving table structure for {tableName}",
                    Error = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// SQL Query Request Model
    /// </summary>
    public class SqlQueryRequest
    {
        /// <summary>
        /// SQL query to execute
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// Use SYSDBA connection (default: false)
        /// </summary>
        public bool UseSysDba { get; set; } = false;
    }
}
