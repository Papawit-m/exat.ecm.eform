using EXAT.ECM.EER.ESARABAN.Models;
using EXAT.ECM.EER.ESARABAN.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EXAT.ECM.EER.ESARABAN.Controllers
{
    /// <summary>
    /// Database schema management and DDL operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SchemaController : ControllerBase
    {
        private readonly IOracleDbService _oracleDbService;
        private readonly ILogger<SchemaController> _logger;

        public SchemaController(IOracleDbService oracleDbService, ILogger<SchemaController> logger)
        {
            _oracleDbService = oracleDbService;
            _logger = logger;
        }

        /// <summary>
        /// Get table DDL (CREATE TABLE statement)
        /// </summary>
        /// <param name="tableName">Source table name</param>
        /// <returns>CREATE TABLE DDL statement</returns>
        [HttpGet("tables/{tableName}/ddl")]
        [SwaggerOperation(
            Summary = "Get table DDL",
            Description = "Retrieves the CREATE TABLE DDL statement for a specific table",
            OperationId = "GetTableDDL"
        )]
        [SwaggerResponse(200, "DDL retrieved successfully", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Failed to retrieve DDL")]
        public async Task<ActionResult<ApiResponse<object>>> GetTableDDL(string tableName)
        {
            try
            {
                // Get table structure from USER_TAB_COLUMNS
                var query = @"
                    SELECT 
                        COLUMN_NAME,
                        DATA_TYPE,
                        DATA_LENGTH,
                        DATA_PRECISION,
                        DATA_SCALE,
                        NULLABLE,
                        DATA_DEFAULT
                    FROM USER_TAB_COLUMNS 
                    WHERE TABLE_NAME = :tableName 
                    ORDER BY COLUMN_ID";

                var parameters = new Oracle.ManagedDataAccess.Client.OracleParameter[]
                {
                    new Oracle.ManagedDataAccess.Client.OracleParameter("tableName", tableName.ToUpper())
                };

                var dataTable = await _oracleDbService.ExecuteQueryAsync(query, parameters);

                if (dataTable.Rows.Count == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Table {tableName} not found"
                    });
                }

                // Build CREATE TABLE statement
                var ddl = new System.Text.StringBuilder();
                ddl.AppendLine($"CREATE TABLE {tableName.ToUpper()} (");

                var columns = new List<string>();
                foreach (System.Data.DataRow row in dataTable.Rows)
                {
                    var colName = row["COLUMN_NAME"].ToString();
                    var dataType = row["DATA_TYPE"].ToString();
                    var dataLength = row["DATA_LENGTH"].ToString();
                    var dataPrecision = row["DATA_PRECISION"] == DBNull.Value ? "" : row["DATA_PRECISION"].ToString();
                    var dataScale = row["DATA_SCALE"] == DBNull.Value ? "" : row["DATA_SCALE"].ToString();
                    var nullable = row["NULLABLE"].ToString() == "N" ? "NOT NULL" : "";
                    var defaultValue = row["DATA_DEFAULT"] == DBNull.Value ? "" : $"DEFAULT {row["DATA_DEFAULT"].ToString()?.Trim()}";

                    var colDef = $"    {colName} ";

                    // Add data type with appropriate size/precision
                    switch (dataType)
                    {
                        case "VARCHAR2":
                        case "CHAR":
                            colDef += $"{dataType}({dataLength})";
                            break;
                        case "NUMBER":
                            if (!string.IsNullOrEmpty(dataPrecision))
                            {
                                if (!string.IsNullOrEmpty(dataScale) && dataScale != "0")
                                    colDef += $"{dataType}({dataPrecision},{dataScale})";
                                else
                                    colDef += $"{dataType}({dataPrecision})";
                            }
                            else
                            {
                                colDef += dataType;
                            }
                            break;
                        default:
                            colDef += dataType;
                            break;
                    }

                    if (!string.IsNullOrEmpty(defaultValue))
                        colDef += $" {defaultValue}";

                    if (!string.IsNullOrEmpty(nullable))
                        colDef += $" {nullable}";

                    columns.Add(colDef);
                }

                ddl.AppendLine(string.Join(",\n", columns));
                ddl.AppendLine(")");

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"DDL for table {tableName} retrieved successfully",
                    Data = new
                    {
                        TableName = tableName.ToUpper(),
                        DDL = ddl.ToString(),
                        ColumnCount = dataTable.Rows.Count
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table DDL for {TableName}", tableName);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving DDL for table {tableName}",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Clone table structure from source table to new table
        /// </summary>
        /// <param name="request">Clone table request</param>
        /// <returns>Result of table creation</returns>
        [HttpPost("tables/clone")]
        [SwaggerOperation(
            Summary = "Clone table structure",
            Description = "Creates a new table by cloning the structure from an existing table",
            OperationId = "CloneTable"
        )]
        [SwaggerResponse(200, "Table cloned successfully", typeof(ApiResponse<object>))]
        [SwaggerResponse(400, "Invalid request")]
        [SwaggerResponse(500, "Failed to clone table")]
        public async Task<ActionResult<ApiResponse<object>>> CloneTable([FromBody] CloneTableRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SourceTable) || string.IsNullOrWhiteSpace(request.NewTable))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Source table and new table names are required",
                        Error = "Invalid input"
                    });
                }

                var sourceTable = request.SourceTable.ToUpper();
                var newTable = request.NewTable.ToUpper();

                // Check if source table exists
                var checkQuery = "SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = :tableName";
                var checkParams = new Oracle.ManagedDataAccess.Client.OracleParameter[]
                {
                    new Oracle.ManagedDataAccess.Client.OracleParameter("tableName", sourceTable)
                };

                var exists = await _oracleDbService.ExecuteScalarAsync(checkQuery, checkParams);
                if (Convert.ToInt32(exists) == 0)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Source table {sourceTable} not found"
                    });
                }

                // Check if new table already exists
                checkParams = new Oracle.ManagedDataAccess.Client.OracleParameter[]
                {
                    new Oracle.ManagedDataAccess.Client.OracleParameter("tableName", newTable)
                };

                var newTableExists = await _oracleDbService.ExecuteScalarAsync(checkQuery, checkParams);
                if (Convert.ToInt32(newTableExists) > 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Table {newTable} already exists",
                        Error = "Table already exists"
                    });
                }

                // Create table using CREATE TABLE AS SELECT with no data
                string createTableQuery;
                if (request.IncludeData)
                {
                    createTableQuery = $"CREATE TABLE {newTable} AS SELECT * FROM {sourceTable}";
                }
                else
                {
                    createTableQuery = $"CREATE TABLE {newTable} AS SELECT * FROM {sourceTable} WHERE 1=0";
                }

                var rowsAffected = await _oracleDbService.ExecuteNonQueryAsync(createTableQuery, null, request.UseSysDba);

                _logger.LogInformation("Table {NewTable} created successfully from {SourceTable}", newTable, sourceTable);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Table {newTable} created successfully from {sourceTable}",
                    Data = new
                    {
                        SourceTable = sourceTable,
                        NewTable = newTable,
                        IncludeData = request.IncludeData,
                        RowsCopied = request.IncludeData ? rowsAffected : 0,
                        CreatedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cloning table from {SourceTable} to {NewTable}",
                    request.SourceTable, request.NewTable);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error cloning table",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Drop/delete a table
        /// </summary>
        /// <param name="tableName">Table name to drop</param>
        /// <param name="useSysDba">Use SYSDBA connection</param>
        /// <returns>Result of drop operation</returns>
        [HttpDelete("tables/{tableName}")]
        [SwaggerOperation(
            Summary = "Drop table",
            Description = "Drops/deletes a table from the database",
            OperationId = "DropTable"
        )]
        [SwaggerResponse(200, "Table dropped successfully", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Failed to drop table")]
        public async Task<ActionResult<ApiResponse<object>>> DropTable(string tableName, [FromQuery] bool useSysDba = false)
        {
            try
            {
                var upperTableName = tableName.ToUpper();
                var dropQuery = $"DROP TABLE {upperTableName}";

                await _oracleDbService.ExecuteNonQueryAsync(dropQuery, null, useSysDba);

                _logger.LogInformation("Table {TableName} dropped successfully", upperTableName);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = $"Table {upperTableName} dropped successfully",
                    Data = new
                    {
                        TableName = upperTableName,
                        DroppedAt = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dropping table {TableName}", tableName);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error dropping table {tableName}",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Check if table exists
        /// </summary>
        /// <param name="tableName">Table name to check</param>
        /// <returns>Table existence status</returns>
        [HttpGet("tables/{tableName}/exists")]
        [SwaggerOperation(
            Summary = "Check if table exists",
            Description = "Checks if a table exists in the current schema",
            OperationId = "TableExists"
        )]
        [SwaggerResponse(200, "Check completed", typeof(ApiResponse<object>))]
        public async Task<ActionResult<ApiResponse<object>>> TableExists(string tableName)
        {
            try
            {
                var query = "SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = :tableName";
                var parameters = new Oracle.ManagedDataAccess.Client.OracleParameter[]
                {
                    new Oracle.ManagedDataAccess.Client.OracleParameter("tableName", tableName.ToUpper())
                };

                var count = await _oracleDbService.ExecuteScalarAsync(query, parameters);
                var exists = Convert.ToInt32(count) > 0;

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = exists ? $"Table {tableName} exists" : $"Table {tableName} does not exist",
                    Data = new
                    {
                        TableName = tableName.ToUpper(),
                        Exists = exists
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking table existence for {TableName}", tableName);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error checking table {tableName}",
                    Error = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// Clone Table Request Model
    /// </summary>
    public class CloneTableRequest
    {
        /// <summary>
        /// Source table name to clone from
        /// </summary>
        public string SourceTable { get; set; } = string.Empty;

        /// <summary>
        /// New table name to create
        /// </summary>
        public string NewTable { get; set; } = string.Empty;

        /// <summary>
        /// Include data from source table (default: false, structure only)
        /// </summary>
        public bool IncludeData { get; set; } = false;

        /// <summary>
        /// Use SYSDBA connection (default: false)
        /// </summary>
        public bool UseSysDba { get; set; } = false;
    }
}
