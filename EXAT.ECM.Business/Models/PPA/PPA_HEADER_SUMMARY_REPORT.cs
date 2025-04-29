namespace EXAT.ECM.Business.Models.PPA
{

    public class PPA_HEADER_SUMMARY_REPORT
    {
        public string? DOCNO { get; set; }
        public string? PROJECT_NAME { get; set; }
        public string? PROJECTSECRETARY_NAME { get; set; }
        public string? CONTRACT_NUMBER { get; set; }
        public string? STATUS_NAME { get; set; }
        public string? PROJECT_START_DATE { get; set; }
        public string? PROJECT_END_DATE { get; set; }

        public List<PPA_DETAIL_SUMMARY_REPORT> Detail { get; set; } = new List<PPA_DETAIL_SUMMARY_REPORT>();
    }

    public class PPA_DETAIL_SUMMARY_REPORT
    {
        public string? NO { get; set; }
        public string? DOCNO { get; set; }
        public string? PROJECT_NAME { get; set; }
        public string? PROJECTSECRETARY_NAME { get; set; }
        public string? CONTRACT_NUMBER { get; set; }
        public string? PROJECT_START_DATE { get; set; }
        public string? PROJECT_END_DATE { get; set; }
        public string? STATUS_NAME { get; set; }
    }
}
