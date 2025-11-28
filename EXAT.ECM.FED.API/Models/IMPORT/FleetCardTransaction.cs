namespace EXAT.ECM.FED.API.Models.IMPORT
{
    public class FleetCardTransaction
    {
        public long TransactionId { get; set; }
        public string? CardNumber { get; set; }
        public string? PlateNumber { get; set; }
        public string TransactionDate { get; set; }
        public string? MerchantId { get; set; }
        public string? TaxId { get; set; }
        public string? StationName { get; set; }
        public string? Location { get; set; }
        public string? TaxAddress { get; set; }
        public string? BranchNumber { get; set; }
        public string? InvoiceNo { get; set; }
        public string? ProductName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? QuantityKg { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? AmountExcludeVat { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public long? Odometer { get; set; }
        public decimal? DistanceKm { get; set; }
        public decimal? ConsumptionKmLitre { get; set; }
        public decimal? ConsumptionBahtKm { get; set; }
        public decimal? ConsumptionKmKg_NGV { get; set; }
        public decimal? ConsumptionBahtKm_NGV { get; set; }
        public decimal? ConsumptionKmLitre_LPG { get; set; }
        public decimal? ConsumptionBahtKm_LPG { get; set; }
        public string? Status { get; set; }
        public string? Department { get; set; }      // <-- เพิ่มบรรทัดนี้
        public string? CostCenter { get; set; }      // <-- เพิ่มบรรทัดนี้
        public string? HeaderId { get; set; }      // <-- เพิ่มบรรทัดนี้
        public string? ReportFromDate { get; set; } // <-- เพิ่มบรรทัดนี้
        public string? ReportToDate { get; set; } // <-- เพิ่มบรรทัดนี้
        public string? ReportProcessDate { get; set; } // <-- เพิ่มบรรทัดนี้
        public string? ReportAccountNo { get; set; } // <-- เพิ่มบรรทัดนี้
        public string? ReportCreditLine { get; set; } // <-- เพิ่มบรรทัดนี้
    }
}
