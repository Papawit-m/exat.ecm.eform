namespace EXAT.ECM.Business.Models.LCI
{
    public class LCI_HEADER_REQUEST_REPORT
    {
        public string? DOC_NO { get; set; }
        public string? DOC_DATE { get; set; }
        public string? SUBJECT { get; set; }
        public string? DEAR { get; set; }
        public string? SIGNER_APPROVAL_1{ get; set; }
        public string? NAME_APPROVAL_1 { get; set; }
        public string? POS_APPROVAL_1 { get; set; }

        public List<LCI_DETAIL_REQUEST_REPORT> Detail { get; set; } = new List<LCI_DETAIL_REQUEST_REPORT>();
    }

    public class LCI_DETAIL_REQUEST_REPORT
    {
        public string? REQUEST_DISCUSSIONDETAILS_HTML { get; set; }
    }
}

