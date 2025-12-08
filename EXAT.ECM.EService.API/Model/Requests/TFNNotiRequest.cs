namespace EXAT.ECM.EService.API.Model.Requests
{
    public class TFNNotiRequest
    {
        public string HIGHWAY_ID { get; set; } = "";
        public string TITLE { get; set; } = "";
        public string MESSAGE { get; set; } = "";
        public string START_DATE { get; set; } = "";
        public string END_DATE { get; set; } = "";
        public string START_TIME { get; set; } = "";
        public string END_TIME { get; set; } = "";
        public string LINK { get; set; } = "";
        public string SCHEDULE_DATE { get; set; } = "";
        public string SCHEDULE_TIME { get; set; } = "";
        public string REGISTER_DATE { get; set; } = "";
        public string REGISTER_BY { get; set; } = "";
        public int    NOTI_ID { get; set; } 
        public string STATUS { get; set; } = "";
        public string APPROVE_DATE { get; set; } = "";
        public string APPROVE_BY { get; set; } = "";
        public string TOKEN { get; set; } = "";
    }
}
