namespace EXAT.ECM.Business.Models.EON
{
    public class EON_SUMMARY_REPORT
    {
        public string? NO { get; set; }
        public string? DOCNO { get; set; }
        public string? ORG_NAME { get; set; }
        public string? EXPRESSWAY_NAME { get; set; }
        public string? DIRECTION_NAME { get; set; }
        public string? SPEED_NAME { get; set; }
        public string? WORKSTART_DATE { get; set; }
        public string? DOCDATE { get; set; }
        public string? STATUS_NAME { get; set; }
        public string? ON_TIME_REQUEST { get; set; }
        public string? PENDING_REQUEST { get; set; }
        public string? TOTAL_REQUEST { get; set; }
    }

    public class EON_DETAIL_FOOTER_REPORT
    {
        public string? ON_TIME_REQUEST { get; set; }
        public string? PENDING_REQUEST { get; set; }
        public string? TOTAL_REQUEST { get; set; }
    }
}
