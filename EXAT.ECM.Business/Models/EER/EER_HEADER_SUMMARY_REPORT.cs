namespace EXAT.ECM.Business.Models.EER
{
    public class EER_HEADER_SUMMARY_REPORT
    {
        public string? EXPRESSWAY_NAME {get;set;}
        public string? DIRECTION_NAME {get;set;}
        public string? REQUEST_DOCDATE_FROM {get;set;}
        public string? REQUEST_DOCDATE_TO {get;set;}
        public string? DOC_COMPLETE {get;set;}
        public string? DOC_ONPROCESS {get;set;}
        public string? DOC_CANCEL {get;set;}
        public string? DOC_TOTAL { get; set; }

        public List<EER_DETAIL_SUMMARY_REPORT> Detail { get; set; } = new List<EER_DETAIL_SUMMARY_REPORT>();
    }

    public class EER_DETAIL_SUMMARY_REPORT
    {
        public string? NO { get; set; }
        public string? DOCNO { get; set; }
        public string? PERSONTYPE_NAME { get; set; }
        public string? TEXT_NAME { get; set; }
        public string? DOC_DATE { get; set; }
        public string? EXPRESSWAY_NAME { get; set; }
        public string? DIRECTION_NAME { get; set; }
    }

    public class EER_DETAIL_FOOTER_REPORT
    {
        public string? ON_TIME_REQUEST { get; set; }
        public string? OVERDUE_REQUEST { get; set; }
        public string? TOTAL_REQUEST { get; set; }
    }
}
