namespace EXAT.ECM.LCI.API.Models
{
    public class LCI_HEADER_SUMMARY_REPORT
    {
        public string? DIVISION_NAME { get; set; }
        public string? SECTION_NAME { get; set; }
        public string? DEPARTMENT_NAME { get; set; }
        public string? STATUS_NAME { get; set; }
        public string? REQUEST_DOC_DATE_FROM { get; set; }
        public string? REQUEST_DOC_DATE_TO { get; set; }

        public List<LCI_DETAIL_SUMMARY_REPORT> Detail { get; set; } = new List<LCI_DETAIL_SUMMARY_REPORT>();
    }

    public class LCI_DETAIL_SUMMARY_REPORT
    {
        public string? NO { get; set; }
        public string? REQUEST_DOCNO { get; set; }
        public string? LEGALDEP_DOCNO { get; set; }
        public string? REQUEST_SUBJECT { get; set; }
        public string? REQUEST_DOCDATE { get; set; }
        public string? PREPARE_NAME_TH { get; set; }
        public string? EMPLOYEE_NAME_TH { get; set; }
        public string? ACTION_DATE { get; set; }
        public string? LEGALDEP_ANSWER_DATE { get; set; }
        public string? STATUS_NAME { get; set; }
    }

}
