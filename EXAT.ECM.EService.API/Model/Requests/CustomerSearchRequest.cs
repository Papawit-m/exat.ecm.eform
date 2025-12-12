using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Requests
{
    public class CustomerSearchRequest
    {
        [JsonPropertyName("by")]
        public string? By { get; set; }

        [JsonPropertyName("keyword")]
        public string? Keyword { get; set; }
    }
}
