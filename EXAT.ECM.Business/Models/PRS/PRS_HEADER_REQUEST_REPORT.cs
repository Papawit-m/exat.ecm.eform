namespace EXAT.ECM.Business.Models.PRS
{
    public class PRS_HEADER_REQUEST_REPORT
    {
        public string? DOC_DATE { get; set; }
        public string? USER_NAME { get; set; }
        public string? POSITION_NAME { get; set; }
        public string? DEPARTMENT_NAME { get; set; }
        public string? PHONE_NUMBER { get; set; }
        public string? REQUEST_SUBJECT { get; set; }
        public string? REQUEST_LOCATION { get; set; }
        public string? DUE_DATE { get; set; }
        public string? REQUEST_ADDITIONAL_NOTE { get; set; }

        public List<PRS_DETAIL_REQUEST_REPORT> Detail { get; set; } = new List<PRS_DETAIL_REQUEST_REPORT>();
    }

    public class PRS_DETAIL_REQUEST_REPORT
    {
        public string? DETAIL_ITEM_HTML { get; set; }        
    }
}
