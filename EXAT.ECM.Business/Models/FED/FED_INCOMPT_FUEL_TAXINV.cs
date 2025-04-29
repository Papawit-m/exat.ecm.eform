namespace EXAT.ECM.Business.Models.FED
{
    public class FED_INCOMPT_FUEL_TAXINV
    {
        public string? MONTH { get; set; }
        public string? YEAR { get; set; }

        public List<DETAIL_FED_INCOMPT_FUEL_TAXINV> Detail { get; set; } = new List<DETAIL_FED_INCOMPT_FUEL_TAXINV>();
    }
    public class DETAIL_FED_INCOMPT_FUEL_TAXINV
    {
        public string? NO { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? TRANSACTION_DATE { get; set; }
        public string? TAX_INVOICE_NO { get; set; }
        public string? AMOUNT { get; set; }
        public string? DEP_ABB { get; set; }
        public string? REMARKS { get; set; }
    }
}
