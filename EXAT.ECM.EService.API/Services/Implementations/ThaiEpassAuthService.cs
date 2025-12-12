using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;
using EXAT.ECM.EService.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    public class ThaiEpassAuthService : IThaiEpassAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ThaiEpassApiSettings _settings;
        private readonly ILogger<ThaiEpassAuthService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ThaiEpassAuthService(
            HttpClient httpClient,
            IOptions<ThaiEpassApiSettings> settings,
            ILogger<ThaiEpassAuthService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
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

        public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var authResponse = await GetAuthResponseAsync(cancellationToken);
            return authResponse?.Data?.AccessToken;
        }

        public async Task<ThaiEpassAuthResponse?> GetAuthResponseAsync(CancellationToken cancellationToken = default)
        {
            var url = string.IsNullOrWhiteSpace(_settings.AuthEndpoint)
                ? "auth/access_token"
                : _settings.AuthEndpoint;

            _logger.LogInformation("Calling ThaiEpass auth API: POST {Url}", url);

            var requestModel = new ThaiEpassAuthRequest
            {
                UserName = _settings.AuthUserName,
                Password = _settings.AuthPassword,
                GetAccessList = _settings.AuthGetAccessList
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Header: transactionid, requestdate, system, source, type, language
            httpRequest.Headers.Add("transactionid", GenerateTransactionId());
            httpRequest.Headers.Add("requestdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            httpRequest.Headers.Add("system", _settings.System);
            httpRequest.Headers.Add("source", _settings.Source);
            httpRequest.Headers.Add("type", _settings.Type);
            httpRequest.Headers.Add("language", _settings.Language);

            var json = JsonSerializer.Serialize(requestModel, _jsonOptions);
            _logger.LogDebug("ThaiEpass auth request body: {Body}", json);

            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("ThaiEpass auth response status: {StatusCode}", response.StatusCode);
            _logger.LogDebug("ThaiEpass auth response body: {Body}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ThaiEpass auth API returned non-success status code {StatusCode}", response.StatusCode);
                return null;
            }

            var result = JsonSerializer.Deserialize<ThaiEpassAuthResponse>(responseContent, _jsonOptions);
            return result;
        }

        private static string GenerateTransactionId()
        {
            var random = RandomNumberGenerator.GetInt32(100000, 999999);
            return $"T{DateTime.Now:yyyyMMddHHmmssfff}-{random}";
        }
    }
}
