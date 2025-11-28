using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Interop;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EXAT.ECM.FED.API.DAL;
using EXAT.ECM.FED.API.Models.IMPORT;
using EXAT.ECM.FED.API.Services.Interfaces;
using Oracle.ManagedDataAccess.Client;

namespace EXAT.ECM.FED.API.Services
{
    public class FleetCardRepository : IFleetCardRepository
    {
        private readonly OracleDbContext _oracleContext;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public FleetCardRepository(OracleDbContext oracleContext, IConfiguration configuration)
        {
            _oracleContext = oracleContext;
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            //_connectionString = configuration.GetConnectionString("OracleConnection");
        }

        // -------- INSERT --------
        public Task InsertTransactionAsync(FleetCardTransaction transaction)
            => InsertTransactionAsync(transaction, CancellationToken.None);

        public async Task InsertTransactionAsync(FleetCardTransaction transaction, CancellationToken ct)
        {
            ArgumentNullException.ThrowIfNull(transaction);

            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new OracleCommand(@"
                    INSERT INTO EFM_FED.FLEET_CARD_TRANSACTIONS (
                        CARD_NUMBER, PLATE_NUMBER, TRANSACTION_DATE, MERCHANT_ID, TAX_ID, STATION_NAME, LOCATION, TAX_ADDRESS, BRANCH_NUMBER, INVOICE_NO,
                        PRODUCT_NAME, QUANTITY, QUANTITY_KG, UNIT_PRICE, AMOUNT_EXCLUDE_VAT, VAT_AMOUNT, TOTAL_AMOUNT, ODOMETER, DISTANCE_KM,
                        CONSUMPTION_KM_LITRE, CONSUMPTION_BAHT_KM, CONSUMPTION_KM_KG_NGV, CONSUMPTION_BAHT_KM_NGV, CONSUMPTION_KM_LITRE_LPG, CONSUMPTION_BAHT_KM_LPG,
                        STATUS, DEPARTMENT, COST_CENTER, HEADER_ID,
                        REPORT_FROM_DATE, REPORT_TO_DATE, REPORT_PROCESS_DATE, REPORT_ACCOUNT_NO, REPORT_CREDIT_LINE
                    ) VALUES (
                        :cardNumber, :plateNumber, :transactionDate, :merchantId, :taxId, :stationName, :location, :taxAddress, :branchNumber, :invoiceNo,
                        :productName, :quantity, :quantityKg, :unitPrice, :amountExcludeVat, :vatAmount, :totalAmount, :odometer, :distanceKm,
                        :consumptionKmLitre, :consumptionBahtKm, :consumptionKmKgNGV, :consumptionBahtKmNGV, :consumptionKmLitreLPG, :consumptionBahtKmLPG,
                        :status, :department, :costCenter, :headerId,
                        :reportFromDate, :reportToDate, :reportProcessDate, :reportAccountNo, :reportCreditLine
                    )", connection)
                {
                    CommandType = CommandType.Text,
                    BindByName = true
                };

                static object V2(string? s, int max) =>
                    (object?)((s ?? string.Empty).Length > max ? s!.Substring(0, max) : s) ?? DBNull.Value;

                command.Parameters.Add(":cardNumber", OracleDbType.Varchar2, 20).Value = V2(transaction.CardNumber, 20);
                command.Parameters.Add(":plateNumber", OracleDbType.Varchar2, 20).Value = V2(transaction.PlateNumber, 20);
                // NOTE: เปลี่ยนเป็น OracleDbType.Date ถ้าคอลัมน์เป็น DATE
                command.Parameters.Add(":transactionDate", OracleDbType.Varchar2, 50).Value = V2(transaction.TransactionDate,50);
                command.Parameters.Add(":merchantId", OracleDbType.Varchar2, 50).Value = V2(transaction.MerchantId, 50);
                command.Parameters.Add(":taxId", OracleDbType.Varchar2, 50).Value = V2(transaction.TaxId, 50);
                command.Parameters.Add(":stationName", OracleDbType.Varchar2, 100).Value = V2(transaction.StationName, 100);
                command.Parameters.Add(":location", OracleDbType.Varchar2, 200).Value = V2(transaction.Location, 200);
                command.Parameters.Add(":taxAddress", OracleDbType.Varchar2, 200).Value = V2(transaction.TaxAddress, 200);
                command.Parameters.Add(":branchNumber", OracleDbType.Varchar2, 20).Value = V2(transaction.BranchNumber, 20);
                command.Parameters.Add(":invoiceNo", OracleDbType.Varchar2, 50).Value = V2(transaction.InvoiceNo, 50);
                command.Parameters.Add(":productName", OracleDbType.Varchar2, 100).Value = V2(transaction.ProductName, 100);

                command.Parameters.Add(":quantity", OracleDbType.Decimal).Value = (object?)transaction.Quantity ?? DBNull.Value;
                command.Parameters.Add(":quantityKg", OracleDbType.Decimal).Value = (object?)transaction.QuantityKg ?? DBNull.Value;
                command.Parameters.Add(":unitPrice", OracleDbType.Decimal).Value = (object?)transaction.UnitPrice ?? DBNull.Value;
                command.Parameters.Add(":amountExcludeVat", OracleDbType.Decimal).Value = (object?)transaction.AmountExcludeVat ?? DBNull.Value;
                command.Parameters.Add(":vatAmount", OracleDbType.Decimal).Value = (object?)transaction.VatAmount ?? DBNull.Value;
                command.Parameters.Add(":totalAmount", OracleDbType.Decimal).Value = (object?)transaction.TotalAmount ?? DBNull.Value;

                command.Parameters.Add(":odometer", OracleDbType.Int64).Value = (object?)transaction.Odometer ?? DBNull.Value;
                command.Parameters.Add(":distanceKm", OracleDbType.Decimal).Value = (object?)transaction.DistanceKm ?? DBNull.Value;
                command.Parameters.Add(":consumptionKmLitre", OracleDbType.Decimal).Value = (object?)transaction.ConsumptionKmLitre ?? DBNull.Value;
                command.Parameters.Add(":consumptionBahtKm", OracleDbType.Decimal).Value = (object?)transaction.ConsumptionBahtKm ?? DBNull.Value;
                command.Parameters.Add(":consumptionKmKgNGV", OracleDbType.Decimal).Value = (object?)transaction.ConsumptionKmKg_NGV ?? DBNull.Value;
                command.Parameters.Add(":consumptionBahtKmNGV", OracleDbType.Decimal).Value = (object?)transaction.ConsumptionBahtKm_NGV ?? DBNull.Value;
                command.Parameters.Add(":consumptionKmLitreLPG", OracleDbType.Decimal).Value = (object?)transaction.ConsumptionKmLitre_LPG ?? DBNull.Value;
                command.Parameters.Add(":consumptionBahtKmLPG", OracleDbType.Decimal).Value = (object?)transaction.ConsumptionBahtKm_LPG ?? DBNull.Value;

                command.Parameters.Add(":status", OracleDbType.Varchar2, 20).Value = V2(transaction.Status, 20);
                command.Parameters.Add(":department", OracleDbType.Varchar2, 100).Value = V2(transaction.Department, 100);
                command.Parameters.Add(":costCenter", OracleDbType.Varchar2, 100).Value = V2(transaction.CostCenter, 100);
                command.Parameters.Add(":headerId", OracleDbType.Varchar2, 50).Value = V2(transaction.HeaderId, 50);

                command.Parameters.Add(":reportFromDate", OracleDbType.Varchar2, 100).Value = V2(transaction.ReportFromDate, 100);
                command.Parameters.Add(":reportToDate", OracleDbType.Varchar2, 100).Value = V2(transaction.ReportToDate, 100);
                command.Parameters.Add(":reportProcessDate", OracleDbType.Varchar2, 100).Value = V2(transaction.ReportProcessDate, 100);
                command.Parameters.Add(":reportAccountNo", OracleDbType.Varchar2, 100).Value = V2(transaction.ReportAccountNo, 100);
                command.Parameters.Add(":reportCreditLine", OracleDbType.Varchar2, 100).Value = V2(transaction.ReportCreditLine, 100);

                await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[ERROR] InsertTransactionAsync: {ex}");
                throw;
            }
        }

        // -------- SEARCH --------
        public Task<IEnumerable<FleetCardTransaction>> SearchTransactionsAsync(TransactionSearchCriteria criteria)
            => SearchTransactionsAsync(criteria, CancellationToken.None);

        public async Task<IEnumerable<FleetCardTransaction>> SearchTransactionsAsync(TransactionSearchCriteria criteria, CancellationToken ct)
        {
            var items = new List<FleetCardTransaction>();
            var sql = new StringBuilder(@"
                SELECT
                    TRANSACTION_ID, CARD_NUMBER, PLATE_NUMBER, TRANSACTION_DATE, MERCHANT_ID, TAX_ID, STATION_NAME, LOCATION, TAX_ADDRESS,
                    BRANCH_NUMBER, INVOICE_NO, PRODUCT_NAME, QUANTITY, QUANTITY_KG, UNIT_PRICE, AMOUNT_EXCLUDE_VAT, VAT_AMOUNT, TOTAL_AMOUNT,
                    ODOMETER, DISTANCE_KM, CONSUMPTION_KM_LITRE, CONSUMPTION_BAHT_KM, CONSUMPTION_KM_KG_NGV, CONSUMPTION_BAHT_KM_NGV,
                    CONSUMPTION_KM_LITRE_LPG, CONSUMPTION_BAHT_KM_LPG, STATUS, DEPARTMENT, COST_CENTER,
                    REPORT_FROM_DATE, REPORT_TO_DATE, REPORT_PROCESS_DATE, REPORT_ACCOUNT_NO, REPORT_CREDIT_LINE
                FROM EFM_FED.FLEET_CARD_TRANSACTIONS
                WHERE 1=1");

            var ps = new List<OracleParameter>();

            if (!string.IsNullOrWhiteSpace(criteria.CardNumber))
            {
                sql.Append(" AND CARD_NUMBER = :p_CardNumber");
                ps.Add(new OracleParameter("p_CardNumber", OracleDbType.Varchar2, 20, criteria.CardNumber, ParameterDirection.Input));
            }
            if (criteria.FromDate.HasValue)
            {
                sql.Append(" AND TRANSACTION_DATE >= :p_FromDate");
                ps.Add(new OracleParameter("p_FromDate", OracleDbType.TimeStamp, criteria.FromDate.Value, ParameterDirection.Input));
            }
            if (criteria.ToDate.HasValue)
            {
                sql.Append(" AND TRANSACTION_DATE < :p_ToDate");
                ps.Add(new OracleParameter("p_ToDate", OracleDbType.TimeStamp, criteria.ToDate.Value, ParameterDirection.Input));
            }
            if (!string.IsNullOrWhiteSpace(criteria.PlateNumber))
            {
                sql.Append(" AND PLATE_NUMBER = :p_PlateNumber");
                ps.Add(new OracleParameter("p_PlateNumber", OracleDbType.Varchar2, 20, criteria.PlateNumber, ParameterDirection.Input));
            }
            if (!string.IsNullOrWhiteSpace(criteria.Status))
            {
                sql.Append(" AND STATUS = :p_Status");
                ps.Add(new OracleParameter("p_Status", OracleDbType.Varchar2, 20, criteria.Status, ParameterDirection.Input));
            }

            sql.Append(" ORDER BY TRANSACTION_DATE DESC");

            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand(sql.ToString(), conn)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };
            if (ps.Count > 0) cmd.Parameters.AddRange(ps.ToArray());

            using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                items.Add(MapReaderToTransaction(reader));
            }
            return items;
        }

        private static FleetCardTransaction MapReaderToTransaction(IDataReader reader)
        {
            static int Ord(IDataReader r, string col) => r.GetOrdinal(col);
            static bool IsNull(IDataReader r, string col) { var i = Ord(r, col); return r.IsDBNull(i); }
            static string? GetStr(IDataReader r, string col) => IsNull(r, col) ? null : r[col]?.ToString();
            static T? Get<T>(IDataReader r, string col, Func<object, T> conv) where T : struct
                => IsNull(r, col) ? (T?)null : conv(r[col]);

            return new FleetCardTransaction
            {
                TransactionId = IsNull(reader, "TRANSACTION_ID") ? 0L : Convert.ToInt64(reader["TRANSACTION_ID"]),
                CardNumber = GetStr(reader, "CARD_NUMBER"),
                PlateNumber = GetStr(reader, "PLATE_NUMBER"),
                TransactionDate = GetStr(reader, "TRANSACTION_DATE"),// IsNull(reader, "TRANSACTION_DATE") ? DateTime.MinValue : Convert.ToDateTime(reader["TRANSACTION_DATE"]),
                MerchantId = GetStr(reader, "MERCHANT_ID"),
                TaxId = GetStr(reader, "TAX_ID"),
                StationName = GetStr(reader, "STATION_NAME"),
                Location = GetStr(reader, "LOCATION"),
                TaxAddress = GetStr(reader, "TAX_ADDRESS"),
                BranchNumber = GetStr(reader, "BRANCH_NUMBER"),
                InvoiceNo = GetStr(reader, "INVOICE_NO"),
                ProductName = GetStr(reader, "PRODUCT_NAME"),

                Quantity = Get<decimal>(reader, "QUANTITY", Convert.ToDecimal),
                QuantityKg = Get<decimal>(reader, "QUANTITY_KG", Convert.ToDecimal),
                UnitPrice = Get<decimal>(reader, "UNIT_PRICE", Convert.ToDecimal),
                AmountExcludeVat = Get<decimal>(reader, "AMOUNT_EXCLUDE_VAT", Convert.ToDecimal),
                VatAmount = Get<decimal>(reader, "VAT_AMOUNT", Convert.ToDecimal),
                TotalAmount = Get<decimal>(reader, "TOTAL_AMOUNT", Convert.ToDecimal),

                Odometer = Get<long>(reader, "ODOMETER", Convert.ToInt64),
                DistanceKm = Get<decimal>(reader, "DISTANCE_KM", Convert.ToDecimal),
                ConsumptionKmLitre = Get<decimal>(reader, "CONSUMPTION_KM_LITRE", Convert.ToDecimal),
                ConsumptionBahtKm = Get<decimal>(reader, "CONSUMPTION_BAHT_KM", Convert.ToDecimal),
                ConsumptionKmKg_NGV = Get<decimal>(reader, "CONSUMPTION_KM_KG_NGV", Convert.ToDecimal),
                ConsumptionBahtKm_NGV = Get<decimal>(reader, "CONSUMPTION_BAHT_KM_NGV", Convert.ToDecimal),
                ConsumptionKmLitre_LPG = Get<decimal>(reader, "CONSUMPTION_KM_LITRE_LPG", Convert.ToDecimal),
                ConsumptionBahtKm_LPG = Get<decimal>(reader, "CONSUMPTION_BAHT_KM_LPG", Convert.ToDecimal),

                Status = GetStr(reader, "STATUS"),
                Department = GetStr(reader, "DEPARTMENT"),
                CostCenter = GetStr(reader, "COST_CENTER"),

                ReportFromDate = GetStr(reader, "REPORT_FROM_DATE"),
                ReportToDate = GetStr(reader, "REPORT_TO_DATE"),
                ReportProcessDate = GetStr(reader, "REPORT_PROCESS_DATE"),
                ReportAccountNo = GetStr(reader, "REPORT_ACCOUNT_NO"),
                ReportCreditLine = GetStr(reader, "REPORT_CREDIT_LINE"),
            };
        }
    }
}
