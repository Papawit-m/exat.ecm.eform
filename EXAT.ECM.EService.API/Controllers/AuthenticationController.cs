using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EXAT.ECM.EService.API.Model.Responses;
using EXAT.ECM.EService.API.Helpers;
using EXAT.ECM.EService.API.Services.Implementations;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Mail;

namespace EXAT.ECM.EService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IExatApiService _exatApiService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly ExatApiSettings _exatSettings;
        private readonly ISessionService _sessionService;
        //private readonly HttpContext httpContext;

        public AuthenticationController(
            IExatApiService exatApiService,
            ILogger<AuthenticationController> logger,
            IOptions<ExatApiSettings> exatSettings,
            ISessionService sessionService
            )
        {
            _exatApiService = exatApiService;
            _sessionService = sessionService;
            _exatSettings = exatSettings.Value;
            _logger = logger;
        }

        [HttpPost("access-token")]
        [ProducesResponseType(typeof(AccessTokenData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAccessToken()
        {
            try
            {
                
                //string AccessTokenUsername = _exatSettings.AccessTokenUsername;
                //string AccessTokenPassword = _exatSettings.AccessTokenPassword;

                var request = new AccessTokenRequest
                {
                    Username = _exatSettings.AccessTokenUsername,
                    Password = _exatSettings.AccessTokenPassword
                };

                if (request == null)
                {
                    return BadRequest("Request cannot be null");
                }

                var result = await _exatApiService.GetAccessTokenAsync(request);

                                
                if (result.Success)
                {
                    return Ok(new
                                { result.Success
                                 ,result.Message
                                 ,result.AccessToken
                                 ,result.ExpiredIn
                                }
                        );
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessToken endpoint");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        //[HttpPost("login")]
        //public async Task<IActionResult> Login([FromBody] Model.Requests.LoginRequest request)
        //{
        //    try
        //    {
        //        if (request == null)
        //        {
        //            return BadRequest("Request cannot be null");
        //        }

        //        var result = await _exatApiService.LoginAsync(request);

        //        if (result.Success)
        //        {
        //            return Ok(result);
        //        }
        //        else
        //        {
        //            return StatusCode(500, result);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error in Login endpoint");
        //        return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        //    }
        //}

        [HttpPost("login-with-token")]
        public async Task<IActionResult> LoginWithToken([FromQuery] string? LoginToken)
        {
            try
            {
                if (LoginToken == null )
                {
                    return BadRequest("Login token cannot be null or empty");
                }

                var request = new LoginTokenRequest
                {
                    LoginToken = LoginToken                
                };


                if (request == null || string.IsNullOrEmpty(request.LoginToken))
                {
                    return BadRequest("Login token cannot be null or empty");
                }

                var tokenRequest = new AccessTokenRequest
                {
                    Username = _exatSettings.AccessTokenUsername,
                    Password = _exatSettings.AccessTokenPassword
                };

               
                var accessTokenResult = await _exatApiService.GetAccessTokenAsync(tokenRequest);

                if (!accessTokenResult.Success || string.IsNullOrEmpty(accessTokenResult.Data?.AccessToken))
                {
                    return StatusCode(500, new { message = "Failed to get access token", error = accessTokenResult.Message });
                }

                var result = await _exatApiService.LoginWithTokenAsync(accessTokenResult.Data.AccessToken, request);

                if (!result.Success || result.Data?.LoginData == null)
                {
                    return StatusCode(500, result);
                }

                // Get Member by Email if available
                if (!string.IsNullOrEmpty(result.Data.LoginData.Email))
                {
                    _logger.LogInformation("Fetching member info by email: {Email}", result.Data.LoginData.Email);

                    var memberByEmailResponse = await _exatApiService.GetMemberByEmailAsync(accessTokenResult.Data.AccessToken, result.Data.LoginData.Email);

                    if (memberByEmailResponse.Success && memberByEmailResponse.Data != null)
                    {
                        result.Data.MemberByEmail = memberByEmailResponse.Data;
                        _logger.LogInformation("Member info by email retrieved successfully");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to get member info by email: {Message}", memberByEmailResponse.Message);
                    }
                }

                // Get Member by Customer ID if available
                if (!string.IsNullOrEmpty(result.Data.LoginData.CustomerId))
                {
                    _logger.LogInformation("Fetching member info by customer ID: {CustomerId}", result.Data.LoginData.CustomerId);

                    var memberByIdResponse = await _exatApiService.GetMemberByCustomerIdAsync(accessTokenResult.Data.AccessToken, result.Data.LoginData.CustomerId);

                    if (memberByIdResponse.Success && memberByIdResponse.Data != null)
                    {
                        result.Data.MemberByCustomerId = memberByIdResponse.Data;
                        _logger.LogInformation("Member info by customer ID retrieved successfully");
                    }
                    else
                    {
                        _logger.LogWarning("Failed to get member info by customer ID: {Message}", memberByIdResponse.Message);
                    }
                }

                return Ok( new {
                            result.Success
                            ,result.Message
                            ,result.Data.LoginData.MemberId
                            ,result.Data.LoginData.CustomerId
                            ,result.Data.LoginData.Email
                            ,result.Data.LoginData.UserType
                            ,result.Data.LoginData.AccountType
                            ,result.Data.LoginData.Title
                            ,result.Data.LoginData.FirstName
                            ,result.Data.LoginData.LastName
                            ,result.Data.LoginData.PhoneNo
                            ,result.Data.LoginData.CreatedAt
                            ,result.Data.LoginData.CreatedBy
                            ,result.Data.LoginData.Active
                            ,result.Data.LoginData.IsConsentLatest
                            ,result.Data.MemberByCustomerId.DateOfBirth
                            ,result.Data.MemberByCustomerId.Status     
                            ,result.Data.MemberByCustomerId.contact_address_house_no
                            ,result.Data.MemberByCustomerId.contact_address_village_no
                            ,result.Data.MemberByCustomerId.contact_address_village_name
                            ,result.Data.MemberByCustomerId.contact_address_road
                            ,result.Data.MemberByCustomerId.contact_address_lane
                            ,result.Data.MemberByCustomerId.contact_address_sub_district
                            ,result.Data.MemberByCustomerId.contact_address_district
                            ,result.Data.MemberByCustomerId.contact_address_province
                            ,result.Data.MemberByCustomerId.contact_address_postal_code
                            ,result.Data.MemberByCustomerId.tax_address_house_no
                            ,result.Data.MemberByCustomerId.tax_address_village_no
                            ,result.Data.MemberByCustomerId.tax_address_village_name
                            ,result.Data.MemberByCustomerId.tax_address_road
                            ,result.Data.MemberByCustomerId.tax_address_lane
                            ,result.Data.MemberByCustomerId.tax_address_sub_district
                            ,result.Data.MemberByCustomerId.tax_address_district


                }
                        );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoginWithToken endpoint");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("customer-id/{customerId}")]
        public async Task<IActionResult> GetMemberByCustomerId(
            string customerId,
            [FromHeader(Name = "Authorization")] string? authHeader = null,
            [FromQuery] string? accessToken = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customerId))
                {
                    return BadRequest("Customer ID cannot be null or empty");
                }

                // Extract token from Authorization header or query parameter
                string? token = accessToken;
                if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(authHeader))
                {
                    // Remove "Bearer " prefix if present
                    token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authHeader.Substring(7)
                        : authHeader;
                }

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("Access token is required");
                }

                var result = await _exatApiService.GetMemberByCustomerIdAsync(token, customerId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMemberByCustomerId endpoint");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetMemberByEmail(
            string email,
            [FromHeader(Name = "Authorization")] string? authHeader = null,
            [FromQuery] string? accessToken = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    return BadRequest("Email cannot be null or empty");
                }

                // Extract token from Authorization header or query parameter
                string? token = accessToken;
                if (string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(authHeader))
                {
                    // Remove "Bearer " prefix if present
                    token = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? authHeader.Substring(7)
                        : authHeader;
                }

                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized("Access token is required");
                }

                var result = await _exatApiService.GetMemberByEmailAsync(token, email);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return StatusCode(500, result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMemberByEmail endpoint");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("CreateSessionToken")]
        public async Task<IActionResult> CreateSessionToken([FromQuery] string? clientId = null)
        {
            try
            {
                K2Response<SessionTokenResponse> errorResponse = null!;

                // Client must provide clientId (unique identifier from client browser/device)
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    errorResponse = K2Response<SessionTokenResponse>.Error(400,
                                        "clientId is required. Please provide a unique client identifier (e.g., GUID generated on client-side). Example: /api/session/create?clientId=12345678-1234-1234-1234-123456789abc"
                                    );

                    return Ok(new {
                        statusCode = errorResponse.StatusCode,
                        message = errorResponse.Message
                    });
                }

                // Use clientId as deviceId
                var deviceId = clientId.Trim();

                // Create device info from HTTP request
                var deviceInfo = new DeviceInfo
                {
                    MacAddress = deviceId, // Store clientId in MacAddress field (for internal use only, not returned)
                    IpAddress = DeviceInfoExtractor.GetClientIpAddress(HttpContext),
                    RealIpAddress = DeviceInfoExtractor.GetRealIpAddress(HttpContext),
                    UserAgent = DeviceInfoExtractor.GetUserAgent(HttpContext),
                    IsVpnConnection = DeviceInfoExtractor.IsVpnConnection(HttpContext),
                    RegisteredAt = DateTime.Now,
                    LastConnectedAt = DateTime.Now,
                    Status = "Active"
                };

                // Get server device information (hostname and MAC addresses)
                var serverDeviceInfo = new ServerDeviceInfoModel
                {
                    Hostname = ClientDeviceInfo.GetHostname(),
                    PrimaryMacAddress = ClientDeviceInfo.GetPrimaryMacAddress(),
                    NetworkInterfaces = ClientDeviceInfo.GetNetworkInterfaces(),
                    RetrievedAt = DateTime.Now
                };

                // Create or get existing session token
                var session = await _sessionService.CreateSessionAsync(deviceId, null, deviceInfo);

                // Add server device info to response
                session.ServerDeviceInfo = serverDeviceInfo;

                var response = K2Response<SessionTokenResponse>.Success(
                                session,
                                "Session token retrieved successfully. Store this token in K2 SmartObject for future requests."
                            );
                var firstNic = response.Data.ServerDeviceInfo.NetworkInterfaces.FirstOrDefault();

                return Ok(new
                {
                    statusCode = response.StatusCode,
                    message = response.Message,
                    totalRecords = response.TotalRecords,
                    sessionToken = response.Data.SessionToken,
                    expiresAt = response.Data.ExpiresAt,
                    isNewSession = response.Data.IsNewSession,
                    ipAddress = response.Data.DeviceInfo.IpAddress,
                    realIpAddress = response.Data.DeviceInfo.RealIpAddress,
                    userAgent = response.Data.DeviceInfo.UserAgent,
                    deviceInfosessionToken = response.Data.DeviceInfo.SessionToken,
                    isVpnConnection = response.Data.DeviceInfo.IsVpnConnection,
                    registeredAt = response.Data.DeviceInfo.RegisteredAt,
                    lastConnectedAt = response.Data.DeviceInfo.LastConnectedAt,
                    deviceInfostatus = response.Data.DeviceInfo.Status,
                    serverDeviceInfohostname = response.Data.ServerDeviceInfo.Hostname,
                    primaryMacAddress = response.Data.ServerDeviceInfo.PrimaryMacAddress,
                    networkInterfacesname = firstNic?.Name,
                    networkInterfacesdescription = firstNic?.Description,
                    networkInterfacestype = firstNic?.Type,
                    networkInterfacesstatus = firstNic?.Status,
                    networkInterfacesmacAddress = firstNic?.MacAddress,
                    networkInterfacesisActive = firstNic?.IsActive,
                    networkInterfacesdhcpEnabled = firstNic?.DhcpEnabled,
                    networkInterfacesipv4Addresses = firstNic?.IPv4Addresses,
                    networkInterfacesipv6Addresses = firstNic?.IPv6Addresses,
                    networkInterfacessubnetMasks = firstNic?.SubnetMasks,
                    networkInterfacesdefaultGateways = firstNic?.DefaultGateways,
                    networkInterfacesdnsServers = firstNic?.DnsServers,
                    networkInterfacesdnsSuffix = firstNic?.DnsSuffix,
                    metadata = response.Metadata
                }
                );
            }
            catch (Exception ex)
            {
                var errorResponse = K2Response<SessionTokenResponse>.Error(
                                        2,
                                        $"Error: {ex.Message}"
                                    );
                return Ok(new
                {
                    statusCode = errorResponse.StatusCode,
                    message = errorResponse.Message
                });
            }
        }

        [HttpGet("ValidateSessionToken")]
        public async Task<IActionResult> ValidateSessionToken([FromQuery] string? token = null) 
        {
            try
            {
                K2Response<object> errorResponse = null!;
                if (string.IsNullOrWhiteSpace(token))
                {
                    errorResponse = K2Response<object>.Error(400,
                                        "Session token is required (?token=xxx)"
                                    );

                    return Ok(new
                    {
                        statusCode = errorResponse.StatusCode,
                        message = errorResponse.Message
                    });
                }

                var deviceInfo = new DeviceInfo
                {
                    SessionToken = token,
                    IpAddress = DeviceInfoExtractor.GetClientIpAddress(HttpContext),
                    RealIpAddress = DeviceInfoExtractor.GetRealIpAddress(HttpContext),
                    UserAgent = DeviceInfoExtractor.GetUserAgent(HttpContext),
                    DeviceName = DeviceInfoExtractor.GetDeviceName(HttpContext),
                    IsVpnConnection = DeviceInfoExtractor.IsVpnConnection(HttpContext),
                    LastConnectedAt = DateTime.Now
                };
                var response = K2Response<DeviceInfo>.Success(
                                deviceInfo,
                                "Session validated successfully"
                            );

                return Ok(new
                {
                    statusCode = response.StatusCode,
                    message = response.Message,
                    sessionToken = response.Data.SessionToken,
                    clientIp = response.Data.IpAddress,
                    realIp = response.Data.RealIpAddress,
                    userAgent = response.Data.UserAgent,
                    deviceName = response.Data.DeviceName,
                    isVpnConnection = response.Data.IsVpnConnection,
                    validatedAt = response.Data.LastConnectedAt

                }
                );
            }
            catch (Exception ex)
            {

                var errorResponse = K2Response<object>.Error(
                                        1,
                                        $"Error: {ex.Message}"
                                    );
                return Ok(new
                {
                    statusCode = errorResponse.StatusCode,
                    message = errorResponse.Message
                });
            }
        }

        [HttpPost("ClearSessionToken")]
        public async Task<IActionResult> ClearSessionToken([FromQuery] string? token = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    var errorResponse = K2Response<object>.Error(
                                        400,
                                        $"Session token is required (?token=xxx)"
                                    );
                    return Ok(new
                    {
                        statusCode = errorResponse.StatusCode,
                        message = errorResponse.Message
                    });
                }

                var result = await _sessionService.ClearSessionAsync(token);

                if (result)
                {
                    var successResponse = K2Response<object>.Success(
                                        200,
                                        $"Session token cleared successfully"
                                    );
                    return Ok(new
                    {
                        statusCode = successResponse.StatusCode,
                        message = successResponse.Message,
                        token,
                        ClearedAt = DateTime.Now
                    });
                }
                else
                {
                    var errorResponse = K2Response<object>.Error(
                                        404,
                                        $"Session token not found or already expired"
                                    );
                    return Ok(new
                    {
                        statusCode = errorResponse.StatusCode,
                        message = errorResponse.Message
                    });
                }
            }
            catch (Exception ex)
            {
                var errorResponse = K2Response<object>.Error(
                                        1,
                                        $"Error: {ex.Message}"
                                    );
                return Ok(new
                {
                    statusCode = errorResponse.StatusCode,
                    message = errorResponse.Message
                });
                
            }
        }
       
        [HttpPost("ClearAllSessions")]
        public async Task<IActionResult> ClearAllSessions()
        {
            try
            {
                var count = await _sessionService.ClearAllSessionsAsync();

                var successResponse = K2Response<object>.Success(
                                        200,
                                        $"Cleared {count} session token(s) successfully"
                                    );
                return Ok(new
                {
                    statusCode = successResponse.StatusCode,
                    message = successResponse.Message,
                    ClearedCount = count,
                    ClearedAt = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                var errorResponse = K2Response<object>.Error(
                                        1,
                                        $"Error: {ex.Message}"
                                    );
                return Ok(new
                {
                    statusCode = errorResponse.StatusCode,
                    message = errorResponse.Message
                });
            }
        }

    }
}
