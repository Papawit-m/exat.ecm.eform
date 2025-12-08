using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Responses
{
    public class EncryptedResponse
    {
        [JsonPropertyName("data_encrypt")]
        public string DataEncrypt { get; set; } = string.Empty;
    }
}
