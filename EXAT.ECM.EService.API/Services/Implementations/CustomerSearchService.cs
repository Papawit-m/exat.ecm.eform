using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Model.Responses;
using EXAT.ECM.EService.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using EXAT.ECM.EService.API.Model.Requests;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    public class CustomerSearchService : ICustomerSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ThaiEpassApiSettings _settings;
        private readonly IThaiEpassAuthService _authService;
        private readonly ILogger<CustomerSearchService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public CustomerSearchService(
            HttpClient httpClient,
            IOptions<ThaiEpassApiSettings> settings,
            IThaiEpassAuthService authService,
            ILogger<CustomerSearchService> logger)
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

        public async Task<CustomerSearchResponse?> SearchCustomerAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default)
        {
            var url = string.IsNullOrWhiteSpace(_settings.CustomerSearchEndpoint)
                ? "exat_cs/customer/search"
                : _settings.CustomerSearchEndpoint;

            _logger.LogInformation("Calling CustomerSearch API: POST {Url}", url);

            // 1) ขอ access_token
            var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Cannot get access token from ThaiEpass auth API");
                return null;
            }

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            httpRequest.Headers.Add("system", _settings.System);
            httpRequest.Headers.Add("source", _settings.Source);
            httpRequest.Headers.Add("type", _settings.Type);
            httpRequest.Headers.Add("language", _settings.Language);
            httpRequest.Headers.Add("transactionid", GenerateTransactionId());
            httpRequest.Headers.Add("requestdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            var json = JsonSerializer.Serialize(request, _jsonOptions);
            _logger.LogDebug("CustomerSearch Request Body: {Body}", json);

            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("CustomerSearch Response Status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("CustomerSearch Response Body: {Body}", responseContent);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                // ไม่มีข้อมูล
                return new CustomerSearchResponse
                {
                    StatusCode = "204",
                    Status = "success",
                    Result = "No content",
                    Message = "No customer found.",
                    Data = null
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "CustomerSearch API returned non-success status code {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    responseContent);

                throw new HttpRequestException(
                    $"CustomerSearch API error {(int)response.StatusCode} {response.StatusCode}: {responseContent}");
            }

            var result = JsonSerializer.Deserialize<CustomerSearchResponse>(responseContent, _jsonOptions);
            return result;
        }

        private static string GenerateTransactionId()
        {
            var random = RandomNumberGenerator.GetInt32(100000, 999999);
            return $"T{DateTime.Now:yyyyMMddHHmmssfff}-{random}";
        }
    }
}
