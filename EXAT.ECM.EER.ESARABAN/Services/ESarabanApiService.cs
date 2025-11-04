using EXAT.ECM.EER.ESARABAN.Models;
using System.Text.Json;
using System.Text;

namespace EXAT.ECM.EER.ESARABAN.Services
{
    /// <summary>
    /// Service for calling eSaraban External API
    /// </summary>
    public class ESarabanApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ESarabanApiService> _logger;
        private readonly ESarabanApiSettings _settings;

        public ESarabanApiService(
            HttpClient httpClient,
            ILogger<ESarabanApiService> logger,
            Microsoft.Extensions.Options.IOptions<ESarabanApiSettings> settings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;

            // HttpClient configuration is now handled in Program.cs
            // No need to configure BaseAddress, Timeout, or headers here
        }

        /// <summary>
        /// Call eSaraban API: POST /api/books/create
        /// </summary>
        public async Task<ESarabanCreateBookResponse?> CreateBookAsync(ESarabanCreateBookRequest request)
        {
            try
            {
                var fullUrl = $"{_httpClient.BaseAddress}{_settings.Endpoints.BooksCreate}";
                _logger.LogInformation($"[DEBUG] Calling eSaraban API");
                _logger.LogInformation($"[DEBUG] BaseAddress: {_httpClient.BaseAddress}");
                _logger.LogInformation($"[DEBUG] Endpoint: {_settings.Endpoints.BooksCreate}");
                _logger.LogInformation($"[DEBUG] Full URL: {fullUrl}");

                // Ensure arrays have at least one object - eSaraban API requires objects not empty arrays
                if (request.bookAttach == null || request.bookAttach.Count == 0)
                {
                    request.bookAttach = new List<BookAttachment> { new BookAttachment() };
                }

                if (request.bookFile == null || request.bookFile.Count == 0)
                {
                    request.bookFile = new List<BookFile> { new BookFile() };
                }

                if (request.bookHistory == null || request.bookHistory.Count == 0)
                {
                    request.bookHistory = new List<BookHistory> { new BookHistory() };
                }

                if (request.bookReferences == null || request.bookReferences.Count == 0)
                {
                    request.bookReferences = new List<BookReference> { new BookReference() };
                }

                if (request.bookReferenceAttach == null || request.bookReferenceAttach.Count == 0)
                {
                    request.bookReferenceAttach = new List<BookReferenceAttachment> { new BookReferenceAttachment() };
                }

                // eSaraban API expects snake_case field names (NOT camelCase)
                // Use custom SnakeCaseNamingPolicy to convert property names
                // DO NOT ignore null fields - eSaraban API requires all fields to be present
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = Utils.SnakeCaseNamingPolicy.Instance,
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
                };

                var jsonContent = JsonSerializer.Serialize(request, jsonOptions);

                _logger.LogInformation($"[DEBUG] Request body length: {jsonContent.Length} bytes");

                // Log full JSON for debugging (to temp file to avoid log truncation)
                try
                {
                    var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"k2rest-request-{DateTime.Now:yyyyMMddHHmmss}.json");
                    System.IO.File.WriteAllText(tempFile, jsonContent, System.Text.Encoding.UTF8);
                    _logger.LogInformation($"[DEBUG] Full request JSON saved to: {tempFile}");
                }
                catch { }

                // Log first 800 characters for quick review
                var jsonPreview = jsonContent.Length > 800 ? jsonContent.Substring(0, 800) + "..." : jsonContent;
                _logger.LogInformation($"[DEBUG] Request JSON (preview): {jsonPreview}");

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // eSaraban API requires user_ad in query string (not just in body)
                var endpoint = $"{_settings.Endpoints.BooksCreate}?user_ad={Uri.EscapeDataString(request.user_ad)}";
                _logger.LogInformation($"[DEBUG] Calling endpoint: {endpoint}");

                var response = await _httpClient.PostAsync(endpoint, content);

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"[DEBUG] eSaraban API Response: {response.StatusCode}");
                _logger.LogInformation($"[DEBUG] Response body: {responseBody.Substring(0, Math.Min(200, responseBody.Length))}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"eSaraban API Error ({response.StatusCode}): {responseBody}");
                    return null;
                }

                var result = JsonSerializer.Deserialize<ESarabanCreateBookResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[DEBUG] Exception calling eSaraban CreateBook API");
                _logger.LogError($"[DEBUG] Exception Type: {ex.GetType().Name}");
                _logger.LogError($"[DEBUG] Exception Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"[DEBUG] Inner Exception: {ex.InnerException.Message}");
                }
                return null;
            }
        }

        /// <summary>
        /// Call eSaraban API: GET /api/books/generate-code
        /// </summary>
        public async Task<GenerateCodeResponse?> GenerateCodeAsync(string userAd, string bookId)
        {
            try
            {
                var endpoint = $"{_settings.Endpoints.BooksGenerateCode}?user_ad={Uri.EscapeDataString(userAd)}&book_id={Uri.EscapeDataString(bookId)}";
                _logger.LogInformation($"Calling eSaraban API: GET {endpoint}");

                var response = await _httpClient.GetAsync(endpoint);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"eSaraban API Response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"eSaraban API Error: {responseBody}");
                    return null;
                }

                var result = JsonSerializer.Deserialize<GenerateCodeResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling eSaraban GenerateCode API: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Call eSaraban API: POST /api/books/transfer
        /// </summary>
        public async Task<TransferBookResponse?> TransferBookAsync(TransferBookRequest request)
        {
            try
            {
                _logger.LogInformation($"Calling eSaraban API: POST {_settings.Endpoints.BooksTransfer}");

                var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_settings.Endpoints.BooksTransfer, content);

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"eSaraban API Response: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"eSaraban API Error: {responseBody}");
                    return null;
                }

                var result = JsonSerializer.Deserialize<TransferBookResponse>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling eSaraban TransferBook API: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Final Organizations by Action (WITH Alert)
        /// Calls: GET /api/books/final-orgs/by-action
        /// </summary>
        public async Task<FinalOrgsResponse?> GetFinalOrgsByActionAsync(string userAd, string bookId)
        {
            try
            {
                var endpoint = $"/api/books/final-orgs/by-action?user_ad={Uri.EscapeDataString(userAd)}&book_id={Uri.EscapeDataString(bookId)}";
                _logger.LogInformation($"Calling eSaraban API: GET {endpoint}");

                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"eSaraban API returned error: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"eSaraban API response received: {content.Length} bytes");

                // Deserialize raw response from eSaraban API (NO modifications)
                // K2 SmartObject Compatible: Keep exact field names and values from API
                var result = JsonSerializer.Deserialize<FinalOrgsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null  // Keep original field names from API
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling eSaraban GetFinalOrgsByAction API: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get Final Organizations by Action (NO Alert)
        /// Calls: GET /api/books/final-orgs/by-action/no-alert
        /// </summary>
        public async Task<FinalOrgsResponse?> GetFinalOrgsByActionNoAlertAsync(string userAd, string bookId)
        {
            try
            {
                var endpoint = $"/api/books/final-orgs/by-action/no-alert?user_ad={Uri.EscapeDataString(userAd)}&book_id={Uri.EscapeDataString(bookId)}";
                _logger.LogInformation($"Calling eSaraban API: GET {endpoint}");

                var response = await _httpClient.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"eSaraban API returned error: {response.StatusCode}, Content: {errorContent}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"eSaraban API response received: {content.Length} bytes");

                // Deserialize raw response from eSaraban API (NO modifications)
                // K2 SmartObject Compatible: Keep exact field names and values from API
                var result = JsonSerializer.Deserialize<FinalOrgsResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = null  // Keep original field names from API
                });

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling eSaraban GetFinalOrgsByActionNoAlert API: {ex.Message}");
                return null;
            }
        }
    }
}
