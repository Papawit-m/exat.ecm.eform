namespace EXAT.ECM.PRS.API.Models
{
    public class PRS_HEADER_SUMMARY_REPORT
    {
        public string? REQUEST_DOCNO { get; set; }
        public string? REQUEST_SUBJECT { get; set; }
        public string? DIVISION_NAME { get; set; }
        public string? SECTION_NAME { get; set; }
        public string? DEPARTMENT_NAME { get; set; }
        public string? STATUS_NAME { get; set; }
        public string? REQUEST_DOC_DATE_FROM { get; set; }
        public string? REQUEST_DOC_DATE_TO { get; set; }

        public List<PRS_DETAIL_SUMMARY_REPORT> Detail { get; set; } = new List<PRS_DETAIL_SUMMARY_REPORT>();
    }

    public class PRS_DETAIL_SUMMARY_REPORT
    {
        public string? NO { get; set; }
        public string? REQUEST_DOCNO { get; set; }
        public string? REQUEST_SUBJECT { get; set; }
        public string? ORG_NAME { get; set; }
        public string? SERVICETASK_DESC { get; set; }
        public string? REQUEST_PRPUBLICATIONDATE { get; set; }
        public string? REQUEST_DUEDATE { get; set; }
        public string? PERFORMANCE_RESULTS { get; set; }
        public string? REMARK { get; set; }
        public string? STATUS_NAME { get; set; }
        public string? DUEDATE { get; set; }
        public string? ON_TIME_REQUEST { get; set; }
        public string? OVERDUE_REQUEST { get; set; }
        public string? TOTAL_REQUEST { get; set; }
    }

    public class PRS_DETAIL_FOOTER_REPORT
    {
        public string? ON_TIME_REQUEST { get; set; }
        public string? OVERDUE_REQUEST { get; set; }
        public string? TOTAL_REQUEST { get; set; }
    }
}
