using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;
using EXAT.ECM.EService.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    public class TagUsageService : ITagUsageService
    {
        private readonly HttpClient _httpClient;
        private readonly ThaiEpassApiSettings _settings;
        private readonly IThaiEpassAuthService _authService;
        private readonly ILogger<TagUsageService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public TagUsageService(
            HttpClient httpClient,
            IOptions<ThaiEpassApiSettings> settings,
            IThaiEpassAuthService authService,
            ILogger<TagUsageService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _authService = authService;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            if (!string.IsNullOrEmpty(_settings.BaseUrl))
            {
                _httpClient.BaseAddress = new Uri(_settings.BaseUrl);
            }

            if (_settings.TimeoutSeconds > 0)
            {
                _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
            }
        }

        public async Task<TagUsageResponse?> SearchTagUsageAsync(TagUsageRequest request, CancellationToken cancellationToken = default)
        {
            var url = string.IsNullOrWhiteSpace(_settings.TagUsageEndpoint)
                ? "exat_cs/report/tag_usage/search"
                : _settings.TagUsageEndpoint;

            _logger.LogInformation("Calling TagUsage API: POST {Url}", url);

            // 1) ขอ access_token ก่อน
            var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Cannot get access token from ThaiEpass auth API");
                return null;
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Authorization: Bearer {token ที่ได้จาก auth}
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Header อื่น ๆ จาก settings
            httpRequest.Headers.Add("system", _settings.System);
            httpRequest.Headers.Add("source", _settings.Source);
            httpRequest.Headers.Add("type", _settings.Type);
            httpRequest.Headers.Add("language", _settings.Language);
            httpRequest.Headers.Add("transactionid", GenerateTransactionId());
            httpRequest.Headers.Add("requestdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // Body
            var json = JsonSerializer.Serialize(request, _jsonOptions);
            _logger.LogDebug("TagUsage Request Body: {Body}", json);

            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("TagUsage Response Status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("TagUsage Response Body: {Body}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TagUsage API returned non-success status code {StatusCode}", response.StatusCode);
                return null;

                // โยน exception ออกไปให้ controller จับ แล้วเอา message ไปตอบได้
                throw new HttpRequestException(
                    $"TagUsage API error {(int)response.StatusCode} {response.StatusCode}: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<TagUsageResponse>(responseContent, _jsonOptions);
            return result;
        }

        private static string GenerateTransactionId()
        {
            var random = RandomNumberGenerator.GetInt32(100000, 999999);
            return $"T{DateTime.Now:yyyyMMddHHmmssfff}-{random}";
        }
    }
}
