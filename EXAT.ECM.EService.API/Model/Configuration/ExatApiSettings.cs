namespace EXAT.ECM.EService.API.Model.Configuration
{
    public class ExatApiSettings
    {
        public const string SectionName = "ExatApi";

        public string BaseUrl { get; set; } = "https://apigw.exat.co.th/mflow-uat/api/v2/";
        public string BasicAuthUsername { get; set; } = string.Empty;
        public string BasicAuthPassword { get; set; } = string.Empty;
        public string AccessTokenUsername { get; set; } = string.Empty;
        public string AccessTokenPassword { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;

        // Encryption settings for Production environment
        public bool UseEncryption { get; set; } = false;
        public string AesSecretKey { get; set; } = string.Empty; // 32 bytes (256 bits)
        public string AesSecretIvKey { get; set; } = string.Empty; // 16 bytes (128 bits)

        public string AccessTokenEndpoint => $"{BaseUrl}/authen/access-token";
        public string LoginEndpoint => $"{BaseUrl}/authen/login";
        public string MemberByCustomerIdEndpoint => $"{BaseUrl}/member/customer-id";
        public string MemberByEmailEndpoint => $"{BaseUrl}/member/email";
    }
}
