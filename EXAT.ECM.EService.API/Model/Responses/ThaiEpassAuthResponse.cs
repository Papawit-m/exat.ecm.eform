using EXAT.ECM.EService.API.Converters;
using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Responses
{
    public class ThaiEpassAuthData
    {
        [JsonPropertyName("rate_limit_per_minute")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? RateLimitPerMinute { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("access_token_generate_time")]
        public string? AccessTokenGenerateTime { get; set; }

        [JsonPropertyName("access_token_expire_time")]
        public string? AccessTokenExpireTime { get; set; }
    }

    public class ThaiEpassAuthResponse
    {
        [JsonPropertyName("status_code")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? StatusCode { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("result_code")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? ResultCode { get; set; }

        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public ThaiEpassAuthData? Data { get; set; }
    }
}
