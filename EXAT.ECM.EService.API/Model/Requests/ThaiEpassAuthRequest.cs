using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Requests
{
    public class ThaiEpassAuthRequest
    {
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [JsonPropertyName("get_access_list")]
        public string GetAccessList { get; set; } = "0";
    }
}
