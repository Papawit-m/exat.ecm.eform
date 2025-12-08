namespace EXAT.ECM.EService.API.Model.Configuration
{
    public class TFMNotiSettings
    {
        public string BaseUrl { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 120;
        public string Token { get; set; } = "";
    }
}
