using EXAT.ECM.FED.API.DAL;
using EXAT.ECM.FED.API.Services.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace EXAT.ECM.FED.API.Services
{
    public interface IBatchInsertService
    {
        Task<(int inserted, int failed, List<object> errors)> InsertBatchAsync(
            string tableName,
            string[] headers,
            List<string[]> batch,
            string importBatchId,
            dynamic fileMeta,
            Func<string[], string[], (bool isValid, string? errorMessage)> validator,
            string? headerId = null,
            string templateName = "TEXT_TEMPLATE");
    }

    public class BatchInsertService : IBatchInsertService
    {
        private readonly OracleDbContext _oracleContext;
        private readonly string _connectionString;
        private readonly IConfigService _configService;

        public BatchInsertService( OracleDbContext oracleContext, IConfiguration configuration )
        {
            _oracleContext = oracleContext;
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            //_connectionString = configuration.GetConnectionString("OracleConnection");
        }

        public async Task<(int inserted, int failed, List<object> errors)> InsertBatchAsync(
            string tableName,
            string[] headers,
            List<string[]> batch,
            string importBatchId,
            dynamic fileMeta,
            Func<string[], string[], (bool isValid, string? errorMessage)> validator,
            string? headerId = null,
            string templateName = "TEXT_TEMPLATE")
        {
            //using var conn = await _oracleContext.GetOpenConnectionAsync();
            using var conn = new OracleConnection(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            int inserted = 0;
            int failed = 0;
            var errors = new List<object>();

            // Load table columns once
            var validColumns = await GetTableColumnsAsync(tableName);
            var headerCols = headers.Where(h => validColumns.Contains(h.ToUpperInvariant())).ToList();

            // Metadata columns
            bool hasFileName = validColumns.Contains("FILE_NAME");
            bool hasFileSize = validColumns.Contains("FILE_SIZE");
            bool hasCreated = validColumns.Contains("FILE_CREATED_DATE");
            bool hasModified = validColumns.Contains("FILE_LAST_MODIFIED_DATE");
            bool hasBatch = validColumns.Contains("IMPORT_BATCH_ID");
            bool hasHeaderId = validColumns.Contains("HEADER_ID");
            bool hasTemplateName = validColumns.Contains("TEMPLATE_NAME");

            var metaCols = new List<string>();
            if (hasBatch) metaCols.Add("IMPORT_BATCH_ID");
            if (hasHeaderId) metaCols.Add("HEADER_ID");
            if (hasTemplateName) metaCols.Add("TEMPLATE_NAME");
            if (hasFileName) metaCols.Add("FILE_NAME");
            if (hasFileSize) metaCols.Add("FILE_SIZE");
            if (hasCreated) metaCols.Add("FILE_CREATED_DATE");
            if (hasModified) metaCols.Add("FILE_LAST_MODIFIED_DATE");

            var allCols = metaCols.Concat(headerCols).ToList();

            // Use Oracle Array Binding for batch insert
            using var cmd = conn.CreateCommand();
            cmd.ArrayBindCount = batch.Count;

            var paramNames = new List<string>();
            var batchData = new Dictionary<string, object[]>();

            // Prepare batch data
            foreach (var col in allCols)
            {
                string paramName = col;           // parameter name without :
                string bindName = ":" + col;      // bind variable
                paramNames.Add(bindName);

                object[] values = new object[batch.Count];
                for (int i = 0; i < batch.Count; i++)
                {
                    if (col == "IMPORT_BATCH_ID") values[i] = importBatchId;
                    else if (col == "HEADER_ID") values[i] = !string.IsNullOrEmpty(headerId) ? (object)headerId : DBNull.Value;
                    else if (col == "TEMPLATE_NAME") values[i] = templateName;
                    else if (col == "FILE_NAME") values[i] = (object)fileMeta.FileName ?? DBNull.Value;
                    else if (col == "FILE_SIZE") values[i] = (object)Convert.ToDecimal(fileMeta.FileSize);
                    else if (col == "FILE_CREATED_DATE") values[i] = (object)(DateTime)fileMeta.CreatedDate;
                    else if (col == "FILE_LAST_MODIFIED_DATE") values[i] = (object)(DateTime)fileMeta.ModifiedDate;
                    else
                    {
                        var idx = Array.FindIndex(headers, h => string.Equals(h, col, StringComparison.OrdinalIgnoreCase));
                        values[i] = idx >= 0 && idx < batch[i].Length ? (object)batch[i][idx] : DBNull.Value;
                    }
                }
                batchData[paramName] = values;
            }

            // Bind arrays
            foreach (var kvp in batchData)
            {
                string col = kvp.Key;
                object[] values = kvp.Value;

                OracleDbType dbType = OracleDbType.Varchar2;

                if (col == "FILE_SIZE")
                    dbType = OracleDbType.Decimal;
                else if (col == "FILE_CREATED_DATE" || col == "FILE_LAST_MODIFIED_DATE")
                    dbType = OracleDbType.Date;

                var param = new OracleParameter(col, dbType)
                {
                    Value = values
                };

                cmd.Parameters.Add(param);
            }

            cmd.CommandText = $"INSERT INTO {tableName} ({string.Join(",", allCols)}) VALUES ({string.Join(",", paramNames)})";

            try
            {
                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                inserted = rowsAffected;
            }
            catch (Exception ex)
            {
                // Fallback to single-row insert if batch fails
                foreach (var row in batch)
                {
                    var (isValid, validationError) = validator(headers, row);
                    if (!isValid)
                    {
                        failed++;
                        if (errors.Count < 20)
                            errors.Add(new { row = inserted + failed, error = validationError });
                        continue;
                    }

                    try
                    {
                        await InsertSingleRowAsync(conn, tableName, headers, row, importBatchId, fileMeta, validColumns, headerId);
                        inserted++;
                    }
                    catch (Exception rowEx)
                    {
                        failed++;
                        if (errors.Count < 20)
                            errors.Add(new { row = inserted + failed, error = rowEx.Message });
                    }
                }
            }

            return (inserted, failed, errors);
        }

        //private async Task<HashSet<string>> GetTableColumnsAsync( string tableName)
        //{
        //    var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        //    using var connection = new OracleConnection(_connectionString);
        //    if (connection.State != ConnectionState.Open)
        //    {
        //        await connection.OpenAsync(); 
        //    }

        //    using var cmd = connection.CreateCommand();

        //    cmd.CommandText = @"SELECT COLUMN_NAME FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :t";

        //    var param = cmd.CreateParameter();
        //    param.ParameterName = "t";
        //    param.Value = tableName.ToUpperInvariant();
        //    cmd.Parameters.Add(param);

        //    using var reader = await cmd.ExecuteReaderAsync();



        //    while (await reader.ReadAsync())
        //    {
        //        columns.Add(reader.GetString(0));
        //    }
        //    return columns;
        //}

        private async Task<HashSet<string>> GetTableColumnsAsync(string tableName)
        {
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // แยก schema กับ table
            string? owner = null;
            string pureTableName = tableName;

            var parts = tableName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                owner = parts[0].ToUpperInvariant();        // EFM_FED
                pureTableName = parts[1].ToUpperInvariant(); // FLEET_CARD_TEMP_TRANS_TEXT
            }
            else
            {
                pureTableName = tableName.ToUpperInvariant();
            }

            using var connection = new OracleConnection(_connectionString);
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var cmd = connection.CreateCommand();
            cmd.BindByName = true;

            if (owner != null)
            {
                // ถ้ามี schema → ใช้ ALL_TAB_COLUMNS + OWNER
                cmd.CommandText = @"
            SELECT COLUMN_NAME 
            FROM ALL_TAB_COLUMNS 
            WHERE OWNER = :owner 
              AND TABLE_NAME = :t";

                var ownerParam = cmd.CreateParameter();
                ownerParam.ParameterName = "owner";
                ownerParam.Value = owner;
                cmd.Parameters.Add(ownerParam);
            }
            else
            {
                // ถ้าไม่มี schema → ใช้ USER_TAB_COLUMNS ของ user ปัจจุบัน
                cmd.CommandText = @"
            SELECT COLUMN_NAME 
            FROM USER_TAB_COLUMNS 
            WHERE TABLE_NAME = :t";
            }

            var tableParam = cmd.CreateParameter();
            tableParam.ParameterName = "t";
            tableParam.Value = pureTableName;
            cmd.Parameters.Add(tableParam);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }

            return columns;
        }

        private async Task InsertSingleRowAsync(IDbConnection conn, string tableName, string[] headers, string[] row,
            string importBatchId, dynamic fileMeta, HashSet<string> validColumns, string? headerId = null)
        {
            using var cmd = (OracleCommand)conn.CreateCommand();
            var cols = new List<string>();
            var paramNames = new List<string>();

            if (validColumns.Contains("IMPORT_BATCH_ID"))
            {
                cols.Add("IMPORT_BATCH_ID");
                var p = new OracleParameter(":IMPORT_BATCH_ID", importBatchId);
                cmd.Parameters.Add(p);
                paramNames.Add(p.ParameterName);
            }

            if (validColumns.Contains("HEADER_ID"))
            {
                cols.Add("HEADER_ID");
                var p = new OracleParameter(":HEADER_ID", !string.IsNullOrEmpty(headerId) ? (object)headerId : DBNull.Value);
                cmd.Parameters.Add(p);
                paramNames.Add(p.ParameterName);
            }

            for (int i = 0; i < headers.Length; i++)
            {
                var col = headers[i].ToUpperInvariant();
                if (validColumns.Contains(col))
                {
                    cols.Add(col);
                    var p = new OracleParameter($":{col}", i < row.Length ? (object)row[i] : DBNull.Value);
                    cmd.Parameters.Add(p);
                    paramNames.Add(p.ParameterName);
                }
            }

            cmd.CommandText = $"INSERT INTO {tableName} ({string.Join(",", cols)}) VALUES ({string.Join(",", paramNames)})";
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
