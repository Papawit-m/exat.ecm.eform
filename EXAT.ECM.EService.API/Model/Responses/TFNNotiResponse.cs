using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Responses
{
    public class TFNNotiResponse
    {
        [JsonPropertyName("STATUS_CODE")]
        public int StatusCode { get; set; } 

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("NOTI_ID")]
        public int NotiId { get; set; }
    }
}
