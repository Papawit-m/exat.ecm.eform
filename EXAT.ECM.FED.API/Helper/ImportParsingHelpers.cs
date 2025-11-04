using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using EXAT.ECM.FED.API.Models.IMPORT;
using OfficeOpenXml;
using TConfig = EXAT.ECM.FED.API.Models.IMPORT.TemplateFieldConfig;

namespace EXAT.ECM.FED.API.Helper
{
    public record ReportHeaderInfo(
        string? ReportFromDate,
        string? ReportToDate,
        string? ProcessDate,
        string? AccountNo,
        string? CreditLine
    );

    public record ContinuationState(
        string CardNumber,
        string PlateNumber,
        string Department,
        string CostCenter
    )
    {
        public static ContinuationState Empty => new("", "", "", "");
        public ContinuationState With(
            string? cardNumber = null,
            string? plateNumber = null,
            string? department = null,
            string? costCenter = null
        ) => new(
            cardNumber ?? CardNumber,
            plateNumber ?? PlateNumber,
            department ?? Department,
            costCenter ?? CostCenter
        );
    }
    public static class ImportParsingHelpers
    {
        // ---------- Culture & Number parsing ----------
        private static readonly CultureInfo ThCulture = new("th-TH");
        private static readonly NumberStyles NumStyles = NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        public static bool TryParseDecimal(string? text, out decimal value)
        {
            text = CleanNumericString(text);
            return decimal.TryParse(text, NumStyles, ThCulture, out value)
                   || decimal.TryParse(text, NumStyles, CultureInfo.InvariantCulture, out value);
        }

        public static bool TryParseLong(string? text, out long value)
        {
            text = CleanNumericString(text);
            return long.TryParse(text, NumberStyles.Integer, ThCulture, out value)
                   || long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static string CleanNumericString(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            // ตัดคำหน่วย เช่น "Baht", เว้นวรรคพิเศษ, เครื่องหมายสกุลเงิน
            var s = Regex.Replace(input, @"[^\d\-\.,]", "", RegexOptions.Multiline);
            // กรณี 1.234,56 → ควร normalize เองถ้ารู้ format ตายตัว
            return s.Trim();
        }

        // ---------- Date parsing ----------
        private static readonly string[] DateTimeFormats =
        {
            "dd/MM/yyyy HH:mm:ss",
            "dd-MM-yyyy  hh:mm:ss tt",
            "dd-MM-yyyy hh:mm:ss tt",
            "dd-MM-yyyy H:mm:ss",
            "dd/MM/yyyy H:mm:ss",
            "dd/MM/yyyy",
            "dd-MM-yyyy"
        };

        public static (bool ok, string value, string? error) TryParseTransactionDate(string? text)
        {
            //var enGb = new CultureInfo("en-GB");
            //string[] formats = {
            //                        "dd/MM/yyyy",
            //                        "yyyy-MM-dd",
            //                        "MM/dd/yyyy HH:mm:ss",
            //                        "dd-MM-yyyy",
            //                        "d/M/yyyy"
            //                    };

            //// ✅ Culture ทั้งไทยและสากล
            //var ThCulture = new CultureInfo("th-TH");

            //DateTime dt = DateTime.ParseExact( text,"dd/MM/yyyy HH:mm:ss", new CultureInfo("en-GB"), DateTimeStyles.None);

            if (string.IsNullOrWhiteSpace(text))
            {
                return (false, default, "Empty date text");
            }
            else
            {
                return (true, text, null);
            }

            //// ✅ Exact match (InvariantCulture ก่อน)
            //if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtExactIv))
            //    return (true, dtExactIv, null);

            //// ✅ Exact match (ภาษาไทย)
            //if (DateTime.TryParseExact(text, formats, ThCulture, DateTimeStyles.None, out var dtExactTh))
            //    return (true, dtExactTh, null);

            //// ✅ Fallback แบบเดาทั่วไป (ไม่มี format ชัดเจน)
            //if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtIv))
            //    return (true, dtIv, null);

            //if (DateTime.TryParse(text, ThCulture, DateTimeStyles.None, out var dtTh))
            //    return (true, dtTh, null);

            //// ❌ กรณี parse ไม่ได้เลย
            //return (false, default, $"Cannot parse date: '{text}'");
        }

        public static string? ExtractDdMmYyyyToYyyyMmDd(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var m = Regex.Match(text, @"(\d{2}/\d{2}/\d{4})");
            if (!m.Success) return null;
            if (DateTime.TryParseExact(m.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt.ToString("yyyy-MM-dd");
            return null;
        }

        // ---------- Worksheet helpers ----------
        public static string NormalizeMultiline(string? text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }

        public static ReportHeaderInfo ReadReportHeader(ExcelWorksheet ws,IReadOnlyDictionary<string, TConfig?> config,int headerRow = 7,int processBlockRow = 6)
        {
            string? reportFromDate = null, reportToDate = null, processDate = null, accountNo = null, creditLine = null;

            if (config.TryGetValue("ReportFormDate", out var c1) && c1?.SourceColumnIndex is int col1)
                reportFromDate = ws.Cells[headerRow, col1]?.Text?.Trim();

            if (config.TryGetValue("ReportToDate", out var c2) && c2?.SourceColumnIndex is int col2)
                reportToDate = ws.Cells[headerRow, col2]?.Text?.Trim();

            reportFromDate = ExtractDdMmYyyyToYyyyMmDd(reportFromDate);
            reportToDate = ExtractDdMmYyyyToYyyyMmDd(reportToDate);

            string? processBlock = null;
            if (config.TryGetValue("ReportProcessDate", out var c3) && c3?.SourceColumnIndex is int col3)
                processBlock = ws.Cells[processBlockRow, col3]?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(processBlock))
            {
                foreach (var line in NormalizeMultiline(processBlock).Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0))
                {
                    if (line.StartsWith("Process Date", StringComparison.OrdinalIgnoreCase))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(line, @"(\d{2}/\d{2}/\d{4})");
                        if (m.Success && DateTime.TryParseExact(m.Value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                            processDate = dt.ToString("yyyy-MM-dd");
                    }
                    else if (line.StartsWith("Account No", StringComparison.OrdinalIgnoreCase))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(line, @"\d+");
                        if (m.Success) accountNo = m.Value;
                    }
                    else if (line.StartsWith("Credit Line", StringComparison.OrdinalIgnoreCase))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(line, @"[\d,\.]+");
                        if (m.Success) creditLine = CleanNumericString(m.Value).Replace(",", "");
                    }
                }
            }

            return new ReportHeaderInfo(reportFromDate, reportToDate, processDate, accountNo, creditLine);
        }

        public static bool TryReadDepartmentRow(ExcelWorksheet ws, int row, out string department, out string costCenter)
        {
            department = "";
            costCenter = "";
            var cellA = ws.Cells[row, 1]?.Text?.Trim() ?? "";
            if (!cellA.StartsWith("Department:", StringComparison.OrdinalIgnoreCase)) return false;

            department = cellA.Replace("Department:", "", StringComparison.OrdinalIgnoreCase).Trim();
            costCenter = ws.Cells[row, 8]?.Text?.Trim() ?? "";
            return true;
        }

        public static bool TryReadCardRow(ExcelWorksheet ws, int row, out string cardNumber, out string plateNumber)
        {
            cardNumber = "";
            plateNumber = "";
            var cellA = ws.Cells[row, 1]?.Text?.Trim() ?? "";
            if (!cellA.StartsWith("Card no.", StringComparison.OrdinalIgnoreCase)) return false;

            var cardNoValue = ws.Cells[row, 4]?.Text?.Trim();
            var plateNoValue = ws.Cells[row, 13]?.Text?.Trim();

            if (!string.IsNullOrWhiteSpace(cardNoValue) && cardNoValue.Length == 16 && cardNoValue.All(char.IsDigit))
            {
                cardNumber = cardNoValue;
                if (!string.IsNullOrWhiteSpace(plateNoValue) &&
                    plateNoValue.StartsWith("Plate No.", StringComparison.OrdinalIgnoreCase))
                    plateNumber = plateNoValue.Substring("Plate No.".Length).Trim();
                else
                    plateNumber = plateNoValue ?? "";
            }
            return true; // แถวนี้คือแถว card; จะถือว่าอ่านสำเร็จ (แม้ card ไม่ครบ 16 หลัก ให้คืนค่าที่ได้)
        }

        public static bool IsTransactionRow(ExcelWorksheet ws, int row, out string rawDateText)
        {
            rawDateText = ws.Cells[row, 1]?.Text?.Trim() ?? "";
            // เป็น "แถวข้อมูล" หาก cellA parse เป็นวันที่ได้ (แบบกว้าง)
            return !string.IsNullOrWhiteSpace(rawDateText)&& DateTime.TryParse(rawDateText, out _);//DateTime.TryParse(rawDateText, out _);
        }

        public static Dictionary<string, string?> BuildRowDataFromConfig(ExcelWorksheet ws,int row,IReadOnlyDictionary<string, TConfig?> config,ContinuationState state)
        {
            var rowData = new Dictionary<string, string?>
            {
                ["CardNumber"] = state.CardNumber,
                ["PlateNumber"] = state.PlateNumber,
                ["Department"] = state.Department,
                ["CostCenter"] = state.CostCenter
            };

            foreach (var (key, def) in config)
            {
                if (key.EndsWith("Header", StringComparison.OrdinalIgnoreCase)) continue;
                if (key is "CardNumber" or "PlateNumber" or "Department" or "CostCenter") continue;

                if (def?.SourceColumnIndex is int col && col > 0)
                {
                    rowData[key] = ws.Cells[row, col]?.Text;
                }
            }
            return rowData;
        }


        public static FleetCardTransaction MapToEntity(
            Dictionary<string, string?> rowData,
            ReportHeaderInfo headerInfo,
            string headerId,
            string txDate
        )
        {
            var tx = new FleetCardTransaction
            {
                CardNumber = rowData.GetValueOrDefault("CardNumber"),
                PlateNumber = rowData.GetValueOrDefault("PlateNumber"),
                Department = rowData.GetValueOrDefault("Department"),
                CostCenter = rowData.GetValueOrDefault("CostCenter"),
                TransactionDate = txDate,
                MerchantId = rowData.GetValueOrDefault("MerchantId"),
                TaxId = rowData.GetValueOrDefault("TaxId"),
                StationName = rowData.GetValueOrDefault("StationName"),
                Location = rowData.GetValueOrDefault("Location"),
                TaxAddress = rowData.GetValueOrDefault("TaxAddress"),
                BranchNumber = rowData.GetValueOrDefault("BranchNumber"),
                InvoiceNo = rowData.GetValueOrDefault("InvoiceNo"),
                ProductName = rowData.GetValueOrDefault("ProductName"),
                Status = "COMPLETED",
                HeaderId = headerId,
                ReportFromDate = headerInfo.ReportFromDate,
                ReportToDate = headerInfo.ReportToDate,
                ReportProcessDate = headerInfo.ProcessDate,
                ReportAccountNo = headerInfo.AccountNo,
                ReportCreditLine = headerInfo.CreditLine
            };

            if (TryParseDecimal(rowData.GetValueOrDefault("Quantity"), out var q)) tx.Quantity = q;
            if (TryParseDecimal(rowData.GetValueOrDefault("QuantityKg"), out var qkg)) tx.QuantityKg = qkg;
            if (TryParseDecimal(rowData.GetValueOrDefault("UnitPrice"), out var up)) tx.UnitPrice = up;
            if (TryParseDecimal(rowData.GetValueOrDefault("AmountExcludeVat"), out var aev)) tx.AmountExcludeVat = aev;
            if (TryParseDecimal(rowData.GetValueOrDefault("VatAmount"), out var vat)) tx.VatAmount = vat;
            if (TryParseDecimal(rowData.GetValueOrDefault("TotalAmount"), out var ta)) tx.TotalAmount = ta;
            if (TryParseLong(rowData.GetValueOrDefault("Odometer"), out var odo)) tx.Odometer = odo;
            if (TryParseDecimal(rowData.GetValueOrDefault("DistanceKm"), out var dk)) tx.DistanceKm = dk;
            if (TryParseDecimal(rowData.GetValueOrDefault("ConsumptionKmLitre"), out var ckl)) tx.ConsumptionKmLitre = ckl;
            if (TryParseDecimal(rowData.GetValueOrDefault("ConsumptionBahtKm"), out var cbk)) tx.ConsumptionBahtKm = cbk;
            if (TryParseDecimal(rowData.GetValueOrDefault("ConsumptionKmKg_NGV"), out var ckkngv)) tx.ConsumptionKmKg_NGV = ckkngv;
            if (TryParseDecimal(rowData.GetValueOrDefault("ConsumptionBahtKm_NGV"), out var cbkngv)) tx.ConsumptionBahtKm_NGV = cbkngv;
            if (TryParseDecimal(rowData.GetValueOrDefault("ConsumptionKmLitre_LPG"), out var cklpg)) tx.ConsumptionKmLitre_LPG = cklpg;
            if (TryParseDecimal(rowData.GetValueOrDefault("ConsumptionBahtKm_LPG"), out var cbklpg)) tx.ConsumptionBahtKm_LPG = cbklpg;

            return tx;
        }
                
    }
}
