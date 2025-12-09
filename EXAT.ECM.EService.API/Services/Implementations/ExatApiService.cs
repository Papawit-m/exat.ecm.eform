using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using EXAT.ECM.EService.API.Services.Interfaces;
using System.Security.Cryptography;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    public class ExatApiService : IExatApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ExatApiSettings _settings;
        private readonly ILogger<ExatApiService> _logger;
        private readonly IEncryptionService _encryptionService;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExatApiService(
            HttpClient httpClient,
            IOptions<ExatApiSettings> settings,
            ILogger<ExatApiService> logger,
            IEncryptionService encryptionService)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _encryptionService = encryptionService;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Use camelCase for JSON
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

            // Note: BaseAddress and Timeout are already configured in Program.cs via AddHttpClient
            // Do not set them here as it may cause issues with HttpClient factory

            // Set Basic Authentication header
            if (!string.IsNullOrEmpty(_settings.BasicAuthUsername) && !string.IsNullOrEmpty(_settings.BasicAuthPassword))
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.BasicAuthUsername}:{_settings.BasicAuthPassword}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            }

            // Note: Custom header "Bas: kong" is disabled in Postman collection, so we don't add it
        }

        public async Task<ApiResponse<AccessTokenResponse>> GetAccessTokenAsync(AccessTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Requesting access token from EXAT API");
                _logger.LogDebug("Request URL: {BaseUrl}/authen/access-token", _httpClient.BaseAddress);
                _logger.LogDebug("Request Body: {Username}", request.Username);

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                _logger.LogDebug("Serialized Request: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Use relative path (without leading slash) to append to BaseAddress
                var response = await _httpClient.PostAsync("authen/access-token", content);
                _logger.LogInformation("Calling EXAT API: POST {BaseUrl}/authen/access-token", _httpClient.BaseAddress);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Response Content: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize EXAT API response format
                    var exatResponse = JsonSerializer.Deserialize<ExatApiResponse<AccessTokenData>>(responseContent, _jsonOptions);

                    if (exatResponse?.Data != null)
                    {
                        var tokenResponse = new AccessTokenResponse
                        {
                            AccessToken = exatResponse.Data.AccessToken,
                            ExpiredIn = exatResponse.Data.ExpiredIn
                        };

                        return new ApiResponse<AccessTokenResponse>
                        {
                            Success = true,
                            Data = tokenResponse,
                            Message = exatResponse.Message ?? "Access token retrieved successfully"
                        };
                    }
                    else
                    {
                        _logger.LogError("EXAT API returned success but no data: {Content}", responseContent);
                        return new ApiResponse<AccessTokenResponse>
                        {
                            Success = false,
                            Message = "No data returned from EXAT API",
                            ErrorCode = "NO_DATA"
                        };
                    }
                }
                else
                {
                    _logger.LogError("Failed to get access token: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new ApiResponse<AccessTokenResponse>
                    {
                        Success = false,
                        Message = $"Failed to get access token: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting access token");
                return new ApiResponse<AccessTokenResponse>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "EXCEPTION"
                };
            }
        }

        public async Task<ApiResponse<Member>> GetMemberByCustomerIdAsync(string accessToken, string customerId)
        {
            try
            {
                _logger.LogInformation("Getting member by customer ID: {CustomerId}", customerId);

                // Create request with Bearer token (use relative path without leading slash)
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"member/customer-id/{customerId}");
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("GetMemberByCustomerId Response: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize EXAT API response format
                    var exatResponse = JsonSerializer.Deserialize<ExatApiResponse<Member>>(responseContent, _jsonOptions);

                    if (exatResponse?.Data != null)
                    {
                        // Convert LoginData to Member
                        return new ApiResponse<Member>
                        {
                            Success = true,
                            Data = exatResponse.Data,
                            DataMemberId = exatResponse.Data.MemberId,
                            DataCustomerId = exatResponse.Data.CustomerId,
                            DataEmail = exatResponse.Data.Email,
                            DataUserType = exatResponse.Data.UserType,
                            DataAccountType = exatResponse.Data.AccountType,
                            DataTitle = exatResponse.Data.Title,
                            DataFirstName = exatResponse.Data.FirstName,
                            DataLastName = exatResponse.Data.LastName,
                            DataPhoneNo = exatResponse.Data.PhoneNo,
                            DataLineId = exatResponse.Data.LineId,
                            DataDateOfBirth = exatResponse.Data.DateOfBirth,
                            DataIsConsentLatest = exatResponse.Data.IsConsentLatest,
                            DataContactAddressHouseNo = exatResponse.Data.ContactAddress.HouseNo,
                            DataContactAddressVillageNo = exatResponse.Data.ContactAddress.VillageNo,
                            DataContactAddressVillageName = exatResponse.Data.ContactAddress.VillageName,
                            DataContactAddressRoad = exatResponse.Data.ContactAddress.Road,
                            DataContactAddressLane = exatResponse.Data.ContactAddress.Lane,
                            DataContactAddressSubDistrict = exatResponse.Data.ContactAddress.SubDistrict,
                            DataContactAddressDistrict = exatResponse.Data.ContactAddress.District,
                            DataContactAddressProvince = exatResponse.Data.ContactAddress.Province,
                            DataContactAddressPostalCode = exatResponse.Data.ContactAddress.PostalCode,
                            DataTaxAddressHouseNo = exatResponse.Data.TaxAddress.HouseNo,
                            DataTaxAddressVillageNo = exatResponse.Data.TaxAddress.VillageNo,
                            DataTaxAddressVillageName = exatResponse.Data.TaxAddress.VillageName,
                            DataTaxAddressRoad = exatResponse.Data.TaxAddress.Road,
                            DataTaxAddressLane = exatResponse.Data.TaxAddress.Lane,
                            DataTaxAddressSubDistrict = exatResponse.Data.TaxAddress.SubDistrict,
                            DataTaxAddressDistrict = exatResponse.Data.TaxAddress.District,
                            DataTaxAddressProvince = exatResponse.Data.TaxAddress.Province,
                            DataTaxAddressPostalCode = exatResponse.Data.TaxAddress.PostalCode,
                            DataActive = exatResponse.Data.Active,
                            DataIsCsMember = exatResponse.Data.IsCsMember,
                            DataCsMemberId = exatResponse.Data.CsMemberId,
                            DataLastLogin = exatResponse.Data.LastLogin,
                            Message = exatResponse.Message ?? "Member retrieved successfully"
                        };

                    }
                    else
                    {
                        _logger.LogError("EXAT API returned success but no data: {Content}", responseContent);
                        return new ApiResponse<Member>
                        {
                            Success = false,
                            Message = exatResponse?.Message ?? "No data returned from EXAT API",
                            ErrorCode = "NO_DATA"
                        };
                    }
                }
                else
                {
                    _logger.LogError("Failed to get member by customer ID: {StatusCode} - {Content}", response.StatusCode, responseContent);

                    // Try to parse error response
                    var errorResponse = JsonSerializer.Deserialize<ExatApiResponse<object>>(responseContent, _jsonOptions);

                    return new ApiResponse<Member>
                    {
                        Success = false,
                        Message = errorResponse?.Message ?? $"Failed to get member by customer ID: {response.StatusCode}",
                        ErrorCode = errorResponse?.StatusCode.ToString() ?? response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting member by customer ID: {CustomerId}", customerId);
                return new ApiResponse<Member>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "EXCEPTION"
                };
            }
        }

        public async Task<ApiResponse<Member>> GetMemberByEmailAsync(string accessToken, string email)
        {
            try
            {
                _logger.LogInformation("Getting member by email: {Email}", email);

                // Create request with Bearer token (use relative path without leading slash)
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"member/email/{email}");
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogDebug("GetMemberByEmail Response: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize EXAT API response format
                    var exatResponse = JsonSerializer.Deserialize<ExatApiResponse<Member>>(responseContent, _jsonOptions);

                    if (exatResponse?.Data != null)
                    {
                        
                        return new ApiResponse<Member>
                        {
                            Success = true,
                            Data = exatResponse.Data,
                            DataMemberId = exatResponse.Data.MemberId,
                            DataCustomerId = exatResponse.Data.CustomerId,
                            DataEmail = exatResponse.Data.Email,
                            DataUserType = exatResponse.Data.UserType,
                            DataAccountType = exatResponse.Data.AccountType,
                            DataTitle = exatResponse.Data.Title,
                            DataFirstName = exatResponse.Data.FirstName,
                            DataLastName = exatResponse.Data.LastName,
                            DataPhoneNo = exatResponse.Data.PhoneNo,
                            DataLineId = exatResponse.Data.LineId,
                            DataDateOfBirth = exatResponse.Data.DateOfBirth,
                            DataIsConsentLatest = exatResponse.Data.IsConsentLatest,
                            DataContactAddressHouseNo = exatResponse.Data.ContactAddress.HouseNo,
                            DataContactAddressVillageNo = exatResponse.Data.ContactAddress.VillageNo,
                            DataContactAddressVillageName = exatResponse.Data.ContactAddress.VillageName,
                            DataContactAddressRoad = exatResponse.Data.ContactAddress.Road,
                            DataContactAddressLane = exatResponse.Data.ContactAddress.Lane,
                            DataContactAddressSubDistrict = exatResponse.Data.ContactAddress.SubDistrict,
                            DataContactAddressDistrict = exatResponse.Data.ContactAddress.District,
                            DataContactAddressProvince = exatResponse.Data.ContactAddress.Province,
                            DataContactAddressPostalCode = exatResponse.Data.ContactAddress.PostalCode,
                            DataTaxAddressHouseNo = exatResponse.Data.TaxAddress.HouseNo,
                            DataTaxAddressVillageNo = exatResponse.Data.TaxAddress.VillageNo,
                            DataTaxAddressVillageName = exatResponse.Data.TaxAddress.VillageName,
                            DataTaxAddressRoad = exatResponse.Data.TaxAddress.Road,
                            DataTaxAddressLane = exatResponse.Data.TaxAddress.Lane,
                            DataTaxAddressSubDistrict = exatResponse.Data.TaxAddress.SubDistrict,
                            DataTaxAddressDistrict = exatResponse.Data.TaxAddress.District,
                            DataTaxAddressProvince = exatResponse.Data.TaxAddress.Province,
                            DataTaxAddressPostalCode = exatResponse.Data.TaxAddress.PostalCode,
                            DataActive = exatResponse.Data.Active,
                            DataIsCsMember = exatResponse.Data.IsCsMember,
                            DataCsMemberId = exatResponse.Data.CsMemberId,
                            DataLastLogin = exatResponse.Data.LastLogin,
                            Message = exatResponse.Message ?? "Member retrieved successfully"
                        };
                    }
                    else
                    {
                        _logger.LogError("EXAT API returned success but no data: {Content}", responseContent);
                        return new ApiResponse<Member>
                        {
                            Success = false,
                            Message = exatResponse?.Message ?? "No data returned from EXAT API",
                            ErrorCode = "NO_DATA"
                        };
                    }
                }
                else
                {
                    _logger.LogError("Failed to get member by email: {StatusCode} - {Content}", response.StatusCode, responseContent);

                    // Try to parse error response
                    var errorResponse = JsonSerializer.Deserialize<ExatApiResponse<object>>(responseContent, _jsonOptions);

                    return new ApiResponse<Member>
                    {
                        Success = false,
                        Message = errorResponse?.Message ?? $"Failed to get member by email: {response.StatusCode}",
                        ErrorCode = errorResponse?.StatusCode.ToString() ?? response.StatusCode.ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting member by email: {Email}", email);
                return new ApiResponse<Member>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "EXCEPTION"
                };
            }
        }

        public async Task<ApiResponse<LoginResponse>> LoginWithAccessTokenAsync(string accessToken, LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Logging in user: {Username}", request.Username);
                _logger.LogDebug("Using Access Token (first 20 chars): {Token}...", accessToken?.Substring(0, Math.Min(20, accessToken?.Length ?? 0)));

                // Prepare request body with optional encryption
                var json = JsonSerializer.Serialize(request, _jsonOptions);
                _logger.LogDebug("Login Request Body: {Body}", json);

                string contentToSend = json;
                if (_settings.UseEncryption)
                {
                    _logger.LogInformation("🔐 Encrypting login request for Production environment");
                    contentToSend = _encryptionService.Encrypt(json);
                }

                var content = new StringContent(contentToSend, Encoding.UTF8, "application/json");

                // Create request message to add Bearer token (use relative path without leading slash)
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "authen/login")
                {
                    Content = content
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                _logger.LogInformation("Calling EXAT Login API: POST {BaseUrl}/authen/login", _httpClient.BaseAddress);

                var response = await _httpClient.SendAsync(requestMessage);

                _logger.LogInformation("Login Response Status: {StatusCode}", response.StatusCode);

                // ===== DEBUG: Log raw response before any processing =====
                var rawResponseContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("🔍 RAW LOGIN RESPONSE:");
                _logger.LogWarning("Status Code: {StatusCode}", response.StatusCode);
                _logger.LogWarning("Content Length: {Length} bytes", rawResponseContent.Length);
                _logger.LogWarning("Content Type: {ContentType}", response.Content.Headers.ContentType);
                _logger.LogWarning("Full Raw Response Body:");
                _logger.LogWarning("{RawContent}", rawResponseContent);
                _logger.LogWarning("=============================================");

                if (response.IsSuccessStatusCode)
                {
                    // Need to create new response with content since we already read it
                    var newResponse = new HttpResponseMessage(response.StatusCode)
                    {
                        Content = new StringContent(rawResponseContent, Encoding.UTF8, "application/json")
                    };

                    // Use helper method for decryption if needed
                    var exatResponse = await ReadResponseWithDecryptionAsync<ExatApiResponse<LoginData>>(newResponse);

                    if (exatResponse?.Data != null)
                    {
                        var loginResponse = new LoginResponse
                        {
                            Success = true,
                            Message = exatResponse.Message ?? "Login successful",
                            AccessToken = accessToken,
                            LoginData = exatResponse.Data,
                            MemberByEmail = null,
                            MemberByCustomerId = null
                        };

                        return new ApiResponse<LoginResponse>
                        {
                            Success = true,
                            Data = loginResponse,
                            Message = exatResponse.Message ?? "Login successful"
                        };
                    }
                    else
                    {
                        _logger.LogError("EXAT API returned success but no data");
                        return new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = "No data returned from EXAT API",
                            ErrorCode = "NO_DATA"
                        };
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to login: {StatusCode} - {Content}", response.StatusCode, errorContent);

                    // Try to parse error response to get EXAT API error message and status_code
                    var errorResponse = await ReadResponseWithDecryptionAsync<ExatApiResponse<object>>(
                        new HttpResponseMessage(response.StatusCode)
                        {
                            Content = new StringContent(errorContent, Encoding.UTF8, "application/json")
                        });

                    var errorMessage = errorResponse?.Message ?? $"Login failed with status code {response.StatusCode}";
                    var errorCode = errorResponse?.StatusCode.ToString() ?? response.StatusCode.ToString();

                    _logger.LogWarning("EXAT API error - Status: {StatusCode}, Message: {ErrorMessage}", errorCode, errorMessage);

                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = errorMessage,
                        ErrorCode = errorCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "EXCEPTION"
                };
            }
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Starting combined login process for user: {Username}", request.Username);

                // Step 1: Get Access Token
                var tokenRequest = new AccessTokenRequest
                {
                    Username = _settings.AccessTokenUsername, // Use Access Token credentials
                    Password = _settings.AccessTokenPassword
                };

                _logger.LogInformation("Step 1: Getting access token...");
                var tokenResponse = await GetAccessTokenAsync(tokenRequest);

                if (!tokenResponse.Success || string.IsNullOrEmpty(tokenResponse.Data?.AccessToken))
                {
                    _logger.LogError("Failed to get access token: {Message}", tokenResponse.Message);
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = $"Failed to get access token: {tokenResponse.Message}",
                        ErrorCode = tokenResponse.ErrorCode ?? "TOKEN_ERROR"
                    };
                }

                _logger.LogInformation("Step 2: Logging in with access token...");

                // Step 2: Login with Access Token
                var loginResponse = await LoginWithAccessTokenAsync(tokenResponse.Data.AccessToken, request);

                if (!loginResponse.Success || loginResponse.Data?.LoginData == null)
                {
                    return loginResponse;
                }

                _logger.LogInformation("Login successful for user: {Username}", request.Username);

                // Step 3: Get Customer Info by Email
                if (!string.IsNullOrEmpty(loginResponse.Data.LoginData.Email))
                {
                    _logger.LogInformation("Step 3: Getting customer info by email: {Email}", loginResponse.Data.LoginData.Email);

                    var memberByEmailResponse = await GetMemberByEmailAsync(tokenResponse.Data.AccessToken, loginResponse.Data.LoginData.Email);

                    if (memberByEmailResponse.Success && memberByEmailResponse.Data != null)
                    {
                        _logger.LogInformation("Member info by email retrieved successfully");
                        loginResponse.Data.MemberByEmail = memberByEmailResponse.Data;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to get member info by email: {Message}", memberByEmailResponse.Message);
                    }
                }

                // Step 4: Get Customer Info by Customer ID
                if (!string.IsNullOrEmpty(loginResponse.Data.LoginData.CustomerId))
                {
                    _logger.LogInformation("Step 4: Getting customer info by customer ID: {CustomerId}", loginResponse.Data.LoginData.CustomerId);

                    var memberByIdResponse = await GetMemberByCustomerIdAsync(tokenResponse.Data.AccessToken, loginResponse.Data.LoginData.CustomerId);

                    if (memberByIdResponse.Success && memberByIdResponse.Data != null)
                    {
                        _logger.LogInformation("Member info by customer ID retrieved successfully");
                        loginResponse.Data.MemberByCustomerId = memberByIdResponse.Data;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to get member info by customer ID: {Message}", memberByIdResponse.Message);
                    }
                }

                _logger.LogInformation("Combined login completed for user: {Username}", request.Username);

                return loginResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during combined login");
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "EXCEPTION"
                };
            }
        }

        public async Task<ApiResponse<LoginResponse>> LoginWithTokenAsync(string accessToken, LoginTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Login with token to EXAT API");
                _logger.LogInformation("📝 Request object - LoginToken length: {Length}, First 50 chars: {Token}...",
                    request?.LoginToken?.Length ?? 0,
                    request?.LoginToken?.Substring(0, Math.Min(50, request?.LoginToken?.Length ?? 0)));

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "authen/login");
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var json = JsonSerializer.Serialize(new { login_token = request.LoginToken }, _jsonOptions);
                _logger.LogInformation("📤 Serialized JSON (full): {Json}", json);
                _logger.LogInformation("📤 JSON Length: {Length} bytes", json.Length);

                // Encrypt payload when configured (Production), else send plain JSON
                string contentToSend;
                if (_settings.UseEncryption)
                {
                    _logger.LogInformation("🔐 Encrypting login-with-token request for Production environment");
                    var encryptedData = _encryptionService.Encrypt(json);
                    var encryptedBody = new { data_encrypt = encryptedData };
                    contentToSend = JsonSerializer.Serialize(encryptedBody, _jsonOptions);
                    _logger.LogDebug("LoginWithToken Encrypted Body: {Body}", contentToSend);
                }
                else
                {
                    contentToSend = json;
                }

                _logger.LogInformation("📤 Content to send: {Content}", contentToSend);
                requestMessage.Content = new StringContent(contentToSend, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("LoginWithToken Response Status: {StatusCode} (IsSuccess: {IsSuccess})",
                    response.StatusCode, response.IsSuccessStatusCode);
                _logger.LogDebug("LoginWithToken Response: {Content}", responseContent);

                // Recreate response for decrypt-aware reader (original response stream already read)
                var responseForParsing = new HttpResponseMessage(response.StatusCode)
                {
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                };

                if (response.IsSuccessStatusCode)
                {
                    var exatResponse = JsonSerializer.Deserialize<ExatApiResponse<LoginData>>(responseContent, _jsonOptions);

                    if (exatResponse?.Data != null)
                    {
                        var loginResponse = new LoginResponse
                        {
                            Success = true,
                            Message = exatResponse.Message ?? "Login successful",
                            AccessToken = accessToken,
                            LoginData = exatResponse.Data,
                            MemberByEmail = null,
                            MemberByCustomerId = null
                        };

                        // Fetch member info by email (if available)
                        if (!string.IsNullOrEmpty(loginResponse.LoginData.Email))
                        {
                            var memberByEmailResponse = await GetMemberByEmailAsync(accessToken, loginResponse.LoginData.Email);
                            if (memberByEmailResponse.Success && memberByEmailResponse.Data != null)
                            {
                                loginResponse.MemberByEmail = memberByEmailResponse.Data;
                            }
                            else
                            {
                                _logger.LogWarning("Failed to get member info by email: {Message}", memberByEmailResponse.Message);
                            }
                        }

                        // Fetch member info by customer ID (if available)
                        if (!string.IsNullOrEmpty(loginResponse.LoginData.CustomerId))
                        {
                            var memberByIdResponse = await GetMemberByCustomerIdAsync(accessToken, loginResponse.LoginData.CustomerId);
                            if (memberByIdResponse.Success && memberByIdResponse.Data != null)
                            {
                                loginResponse.MemberByCustomerId = memberByIdResponse.Data;
                            }
                            else
                            {
                                _logger.LogWarning("Failed to get member info by customer ID: {Message}", memberByIdResponse.Message);
                            }
                        }

                        return new ApiResponse<LoginResponse>
                        {
                            Success = true,
                            Data = loginResponse,
                            Message = exatResponse.Message ?? "Login with token successful"
                        };
                    }
                    else
                    {
                        _logger.LogError("EXAT API returned HTTP {StatusCode} with success=true but no data in response body", response.StatusCode);
                        _logger.LogError("Response content: {Content}", responseContent);

                        return new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = exatResponse?.Message ?? "No data returned from EXAT API",
                            ErrorCode = exatResponse?.StatusCode > 0 ? exatResponse.StatusCode.ToString() : "NO_DATA"
                        };
                    }
                }
                else
                {
                    _logger.LogError("Failed to login with token: {StatusCode} - {Content}", response.StatusCode, responseContent);

                    var errorResponse = await ReadResponseWithDecryptionAsync<ExatApiResponse<object>>(responseForParsing);

                    var errorMessage = errorResponse?.Message ?? $"Request failed with status code {response.StatusCode}";
                    var errorCode = errorResponse?.StatusCode.ToString() ?? ((int)response.StatusCode).ToString();

                    _logger.LogWarning("EXAT API error - Status: {StatusCode}, Message: {ErrorMessage}", errorCode, errorMessage);

                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = errorMessage,
                        ErrorCode = errorCode
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging in with token");
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = ex.Message,
                    ErrorCode = "EXCEPTION"
                };
            }
        }


        /// <summary>
        /// Read response with optional decryption
        /// </summary>
        private async Task<T?> ReadResponseWithDecryptionAsync<T>(HttpResponseMessage response) where T : class
        {
            try
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("📥 Response received, Status: {Status}, Content Length: {Length}",
                    response.StatusCode, responseContent.Length);
                _logger.LogDebug("Raw response content: {Content}", responseContent);

                // If encryption is enabled, check for encrypted response
                if (_settings.UseEncryption && responseContent.Contains("data_encrypt"))
                {
                    _logger.LogInformation("� Production mode detected - encrypted response found");
                    _logger.LogInformation("🔓 Starting decryption process...");

                    var encryptedResponse = JsonSerializer.Deserialize<EncryptedResponse>(responseContent, _jsonOptions);

                    if (encryptedResponse?.DataEncrypt != null)
                    {
                        _logger.LogInformation("✅ Encrypted data extracted from 'data_encrypt' field");
                        _logger.LogDebug("Encrypted data length: {Length} chars", encryptedResponse.DataEncrypt.Length);
                        _logger.LogDebug("Encrypted data (first 100 chars): {Data}",
                            encryptedResponse.DataEncrypt.Length > 100
                                ? encryptedResponse.DataEncrypt.Substring(0, 100) + "..."
                                : encryptedResponse.DataEncrypt);

                        _logger.LogInformation("🔓 Calling AesEncryptionService.Decrypt()...");
                        var decryptedContent = _encryptionService.Decrypt(encryptedResponse.DataEncrypt);

                        _logger.LogInformation("✅ Decryption successful! Decrypted content length: {Length}", decryptedContent.Length);
                        _logger.LogDebug("Decrypted JSON (first 500 chars): {Json}",
                            decryptedContent.Length > 500
                                ? decryptedContent.Substring(0, 500) + "..."
                                : decryptedContent);

                        _logger.LogInformation("📦 Deserializing decrypted JSON to type {Type}...", typeof(T).Name);
                        var result = JsonSerializer.Deserialize<T>(decryptedContent, _jsonOptions);
                        _logger.LogInformation("✅ Successfully deserialized to {Type}", typeof(T).Name);

                        return result;
                    }
                    else
                    {
                        _logger.LogError("❌ Response contains 'data_encrypt' field but value is null or empty");
                        _logger.LogError("encryptedResponse is null? {IsNull}", encryptedResponse == null);
                        return null;
                    }
                }
                else
                {
                    // Plain JSON response (UAT mode)
                    _logger.LogInformation("📄 UAT mode or plain JSON response detected");
                    _logger.LogDebug("Plain JSON response: {Content}", responseContent);
                    return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                }
            }
            catch (CryptographicException cryptoEx)
            {
                _logger.LogError(cryptoEx, "❌ CRYPTOGRAPHIC ERROR in ReadResponseWithDecryptionAsync");
                _logger.LogError("Error message: {Message}", cryptoEx.Message);
                _logger.LogError("Inner exception: {Inner}", cryptoEx.InnerException?.Message);
                _logger.LogError("Stack trace: {StackTrace}", cryptoEx.StackTrace);
                throw;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "❌ JSON PARSING ERROR in ReadResponseWithDecryptionAsync");
                _logger.LogError("Error message: {Message}", jsonEx.Message);
                _logger.LogError("Position: Line {Line}, Position {Position}", jsonEx.LineNumber, jsonEx.BytePositionInLine);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ UNEXPECTED ERROR in ReadResponseWithDecryptionAsync");
                _logger.LogError("Error type: {Type}", ex.GetType().FullName);
                _logger.LogError("Error message: {Message}", ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                throw;
            }
        }

    }
}
