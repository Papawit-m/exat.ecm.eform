using EXAT.ECM.EService.API.Services.Interfaces;
using System.Text.Json;
using System.Text;
using EXAT.ECM.EService.API.Model.Responses;
using EXAT.ECM.EService.API.Model.Requests;
using System.Net;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public NotificationService(HttpClient httpClient, ILogger<NotificationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,              // เพราะเราใช้ชื่อ field ตรง ๆ (HIGHWAY_ID ฯลฯ)
                PropertyNameCaseInsensitive = true,
                WriteIndented = false
            };
        }

        public async Task<TFNNotiResponse> InsertNotiAsync(TFNNotiRequest request, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(new { data = request }, _jsonOptions);
            _logger.LogInformation("Calling insert_noti with body: {Body}", json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("insert_noti", content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("insert_noti response status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("insert_noti response body: {Body}", responseBody);

            if (!response.IsSuccessStatusCode)
            {
                // fallback ถ้า status code != 200 แต่อยากได้ message กลับ
                return new TFNNotiResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Message = $"HTTP error from insert_noti: {response.StatusCode}, body={responseBody}",
                    NotiId = 0
                };
            }

            var result = JsonSerializer.Deserialize<TFNNotiResponse>(responseBody, _jsonOptions)
                         ?? new TFNNotiResponse
                         {
                             StatusCode = 500,
                             Message = "Cannot deserialize response from insert_noti",
                             NotiId = 0
                         };

            return result;
        }

        public async Task<TFNNotiResponse> UpdateNotiAsync(int notiId, TFNNotiRequest request, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            _logger.LogInformation("Calling update_noti/{NotiId} with body: {Body}", notiId, json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"update_noti/{notiId}", content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("update_noti response status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("update_noti response body: {Body}", responseBody);

            if (!response.IsSuccessStatusCode)
            {
                return new TFNNotiResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Message = $"HTTP error from update_noti: {response.StatusCode}, body={responseBody}",
                    NotiId = notiId
                };
            }

            var result = JsonSerializer.Deserialize<TFNNotiResponse>(responseBody, _jsonOptions)
                         ?? new TFNNotiResponse
                         {
                             StatusCode = 500,
                             Message = "Cannot deserialize response from update_noti",
                             NotiId = notiId
                         };

            return result;
        }

        public async Task<TFNNotiResponse> UpdateStatusAsync(int notiId, TFNNotiRequest request, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            _logger.LogInformation("Calling update_status/{NotiId} with body: {Body}", notiId, json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"update_status/{notiId}", content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("update_status response status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("update_status response body: {Body}", responseBody);

            if (!response.IsSuccessStatusCode)
            {
                return new TFNNotiResponse
                {
                    StatusCode = (int)response.StatusCode,
                    Message = $"HTTP error from update_status: {response.StatusCode}, body={responseBody}",
                    NotiId = notiId
                };
            }

            var result = JsonSerializer.Deserialize<TFNNotiResponse>(responseBody, _jsonOptions)
                         ?? new TFNNotiResponse
                         {
                             StatusCode = 500,
                             Message = "Cannot deserialize response from update_status",
                             NotiId = notiId
                         };

            return result;
        }
    }
}
