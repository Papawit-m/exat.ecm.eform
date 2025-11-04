using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using EXAT.ECM.EService.API.Services.Interfaces;

namespace EXAT.ECM.EService.API.Services.Implementations
{
    public class ExatApiService : IExatApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ExatApiSettings _settings;
        private readonly ILogger<ExatApiService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ExatApiService(
            HttpClient httpClient,
            IOptions<ExatApiSettings> settings,
            ILogger<ExatApiService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

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
                            AccessToken = tokenResponse.AccessToken,
                            ExpiredIn = tokenResponse.ExpiredIn,
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
                    var exatResponse = JsonSerializer.Deserialize<ExatApiResponse<LoginData>>(responseContent, _jsonOptions);

                    if (exatResponse?.Data != null)
                    {
                        // Convert LoginData to Member
                        var member = new Member
                        {
                            CustomerId = exatResponse.Data.CustomerId,
                            Email = exatResponse.Data.Email,
                            FirstName = exatResponse.Data.FirstName,
                            LastName = exatResponse.Data.LastName,
                            PhoneNumber = exatResponse.Data.UserType, // Placeholder, adjust as needed
                            Status = exatResponse.Data.Active,
                            contact_address_house_no = exatResponse.Data.ContactAddress.HouseNo,
                            contact_address_village_no = exatResponse.Data.ContactAddress.VillageNo,
                            contact_address_village_name = exatResponse.Data.ContactAddress.VillageName,
                            contact_address_lane = exatResponse.Data.ContactAddress.Lane,
                            contact_address_road = exatResponse.Data.ContactAddress.Road,
                            contact_address_sub_district = exatResponse.Data.ContactAddress.SubDistrict,
                            contact_address_district = exatResponse.Data.ContactAddress.District,
                            contact_address_province = exatResponse.Data.ContactAddress.Province,
                            contact_address_postal_code = exatResponse.Data.ContactAddress.PostalCode,
                            tax_address_house_no = exatResponse.Data.TaxAddress.HouseNo,
                            tax_address_village_no = exatResponse.Data.TaxAddress.VillageNo,
                            tax_address_village_name = exatResponse.Data.TaxAddress.VillageName,
                            tax_address_lane = exatResponse.Data.TaxAddress.Lane,
                            tax_address_road = exatResponse.Data.TaxAddress.Road,
                            tax_address_sub_district = exatResponse.Data.TaxAddress.SubDistrict,
                            tax_address_district = exatResponse.Data.TaxAddress.District


                        };

                        return new ApiResponse<Member>
                        {
                            Success = true,
                            Data = member,
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
                    var exatResponse = JsonSerializer.Deserialize<ExatApiResponse<LoginData>>(responseContent, _jsonOptions);

                    if (exatResponse?.Data != null)
                    {
                        // Convert LoginData to Member
                        var member = new Member
                        {
                            CustomerId = exatResponse.Data.CustomerId,
                            Email = exatResponse.Data.Email,
                            FirstName = exatResponse.Data.FirstName,
                            LastName = exatResponse.Data.LastName,
                            PhoneNumber = exatResponse.Data.UserType, // Placeholder, adjust as needed
                            Status = exatResponse.Data.Active
                        };

                        return new ApiResponse<Member>
                        {
                            Success = true,
                            Data = member,
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

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                _logger.LogDebug("Login Request Body: {Body}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Create request message to add Bearer token (use relative path without leading slash)
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "authen/login")
                {
                    Content = content
                };
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                _logger.LogInformation("Calling EXAT Login API: POST {BaseUrl}/authen/login", _httpClient.BaseAddress);

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Login Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("Login Response Content: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    // Deserialize EXAT API response format
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

                        return new ApiResponse<LoginResponse>
                        {
                            Success = true,
                            Data = loginResponse,
                            Message = exatResponse.Message ?? "Login successful"
                        };
                    }
                    else
                    {
                        _logger.LogError("EXAT API returned success but no data: {Content}", responseContent);
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
                    _logger.LogError("Failed to login: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = $"Failed to login: {response.StatusCode}",
                        ErrorCode = response.StatusCode.ToString()
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

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, "authen/login");
                requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var json = JsonSerializer.Serialize(new { login_token = request.LoginToken }, _jsonOptions);
                _logger.LogDebug("LoginWithToken Request: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                requestMessage.Content = content;

                var response = await _httpClient.SendAsync(requestMessage);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("LoginWithToken Response Status: {StatusCode}", response.StatusCode);
                _logger.LogDebug("LoginWithToken Response: {Content}", responseContent);

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

                        return new ApiResponse<LoginResponse>
                        {
                            Success = true,
                            Data = loginResponse,
                            Message = exatResponse.Message ?? "Login with token successful"
                        };
                    }
                    else
                    {
                        _logger.LogError("EXAT API returned success but no data: {Content}", responseContent);
                        return new ApiResponse<LoginResponse>
                        {
                            Success = false,
                            Message = exatResponse?.Message ?? "No data returned from EXAT API",
                            ErrorCode = "NO_DATA"
                        };
                    }
                }
                else
                {
                    _logger.LogError("Failed to login with token: {StatusCode} - {Content}", response.StatusCode, responseContent);

                    var errorResponse = JsonSerializer.Deserialize<ExatApiResponse<object>>(responseContent, _jsonOptions);

                    return new ApiResponse<LoginResponse>
                    {
                        Success = false,
                        Message = errorResponse?.Message ?? $"Failed to login with token: {response.StatusCode}",
                        ErrorCode = errorResponse?.StatusCode.ToString() ?? response.StatusCode.ToString()
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
    }
}
