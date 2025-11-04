namespace EXAT.ECM.FED.API.Models.IMPORT
{
    /// <summary>
    /// คลาสสำหรับรับเงื่อนไขในการค้นหารายการ Transactions
    /// </summary>
    public class TransactionSearchCriteria
    {
        public string? CardNumber { get; set; }
        public string? PlateNumber { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Department { get; set; }
        public string? CostCenter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
    }
}
