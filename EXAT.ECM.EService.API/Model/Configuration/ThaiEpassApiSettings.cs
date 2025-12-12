namespace EXAT.ECM.EService.API.Model.Configuration
{
    public class ThaiEpassApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string System { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Language { get; set; } = "th";
        public int TimeoutSeconds { get; set; } = 120;

        // Auth
        public string AuthUserName { get; set; } = string.Empty;
        public string AuthPassword { get; set; } = string.Empty;
        public string AuthGetAccessList { get; set; } = "0";
        public string AuthEndpoint { get; set; } = "auth/access_token";

        // Tag Usage
        public string TagUsageEndpoint { get; set; } = "exat_cs/report/tag_usage/search";

        public string CustomerSearchEndpoint { get; set; } = "exat_cs/customer/search";
    }
}
