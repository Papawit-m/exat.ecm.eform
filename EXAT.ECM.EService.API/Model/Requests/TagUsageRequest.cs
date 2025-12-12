using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Requests
{
    public class TagUsageRequest
    {
        [JsonPropertyName("by")]
        public string? By { get; set; }

        [JsonPropertyName("keyword")]
        public string? Keyword { get; set; }

        [JsonPropertyName("start_date")]
        public string? StartDate { get; set; }

        [JsonPropertyName("end_date")]
        public string? EndDate { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; } = "th";
    }
}
