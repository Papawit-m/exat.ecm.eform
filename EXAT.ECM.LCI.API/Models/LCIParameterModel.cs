namespace EXAT.ECM.LCI.API.Models
{
    public class LCIParameterModel
    {
        // for header + detail
        public string? p_DIV_CODE { get; set; }
        public string? p_SEC_CODE { get; set; }
        public string? p_DEP_CODE { get; set; }
        public string? p_STATUS_ID { get; set; }
        public string? p_REQUEST_DOC_DATE_FROM { get; set; }
        public string? p_REQUEST_DOC_DATE_TO { get; set; }
        public string? p_USER_AD { get; set; }
        public string? p_HEADER_ID { get; set; }

        // for detail
        public string? p_REQUEST_DOCNO { get; set; }
        public string? p_LEGALDEP_DOCNO { get; set; }
        public string? p_REQUEST_SUBJECT { get; set; }
        public string? p_SECRET_ID { get; set; }
        public string? p_SPEED_ID { get; set; }
    }
}
