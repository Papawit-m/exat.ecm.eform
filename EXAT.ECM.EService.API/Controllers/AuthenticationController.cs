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
using EXAT.ECM.EService.API.DAL;
using EXAT.ECM.EService.API.Services.Mappers;
using System.Runtime;

namespace EXAT.ECM.EService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IExatApiService _exatApiService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly ExatApiSettings _exatSettings;
        private readonly TFMNotiSettings _TFNSettings;
        private readonly ISessionService _sessionService;
        private readonly IOracleLoggerService _oracleLogger;
        private readonly INotificationService _notificationService;
        //private readonly HttpContext httpContext;

        public AuthenticationController(
            IExatApiService exatApiService,
            ILogger<AuthenticationController> logger,
            IOptions<ExatApiSettings> exatSettings,
            IOptions<TFMNotiSettings> tfnSettings,
            ISessionService sessionService, 
            OracleDbContext oracleContext,
            IOracleLoggerService oracleLogger,
            INotificationService notificationService
            )
        {
            _exatApiService = exatApiService;
            _sessionService = sessionService;
            _exatSettings = exatSettings.Value;
            _TFNSettings = tfnSettings.Value;
            _logger = logger;
            _oracleLogger = oracleLogger;
            _notificationService = notificationService;
        }

        [HttpPost("access-token")]
        [ProducesResponseType(typeof(K2ApiResponse<K2AccessTokenResponse>), 200)]
        [ProducesResponseType(typeof(K2ErrorResponse), 400)]
        [ProducesResponseType(typeof(K2ErrorResponse), 500)]
        public async Task<IActionResult> GetAccessToken()
        {
            var startTime = DateTime.UtcNow;
            var endpoint = "K2GetAccessToken";
            var httpMethod = "POST";
            var requestPath = $"{Request.Path}";

            _logger.LogInformation("Starting K2 GetAccessToken request");

            var request = new AccessTokenRequest
            {
                Username = _exatSettings.AccessTokenUsername,
                Password = _exatSettings.AccessTokenPassword
            };

            try
            {
                              

                if (request == null)
                {
                    _logger.LogWarning("K2 GetAccessToken request failed: Request cannot be null");
                    await _oracleLogger.LogWarningAsync(endpoint, httpMethod, requestPath, "Request cannot be null", null, null, null, null);

                    var errorResponse = K2ResponseMapper.CreateErrorResponse(
                        "Request cannot be null",
                        "INVALID_REQUEST");
                    return BadRequest(errorResponse);
                }

                _logger.LogDebug("K2 GetAccessToken request parameters - Username: {Username}", request.Username);

                var result = await _exatApiService.GetAccessTokenAsync(request);
                // Convert to K2 format
                var k2Response = K2ResponseMapper.MapToK2AccessToken(result);

                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogDebug("K2 GetAccessToken execution time: {ExecutionTime}ms", executionTime);

                if (result.Success)
                {
                    _logger.LogInformation("K2 GetAccessToken completed successfully in {ExecutionTime}ms", executionTime);
                    await _oracleLogger.LogInformationAsync(
                        endpoint, httpMethod, requestPath,
                        "Access token retrieved successfully (K2 format)",
                        request.Username, null, null, executionTime,
                        request, k2Response);

                    return Ok(new
                                { k2Response.Success
                                 ,k2Response.Message
                                 ,k2Response.Data.AccessToken
                                 ,k2Response.Data.ExpiredIn
                                }
                        );
                }
                else
                {
                    _logger.LogWarning("K2 GetAccessToken failed: {Message}", k2Response.Message);
                    await _oracleLogger.LogWarningAsync(
                        endpoint, httpMethod, requestPath,
                        k2Response.Message ?? "Failed to get access token",
                        request.Username, null, null, request);
                    return StatusCode(500, k2Response);
                }
            }
            catch (Exception ex)
            {
                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "Error in K2 GetAccessToken endpoint after {ExecutionTime}ms", executionTime);
                await _oracleLogger.LogErrorAsync(
                    endpoint, httpMethod, requestPath,
                    "Error in K2 GetAccessToken endpoint",
                    ex, request?.Username, null, null, executionTime, request);

                var errorResponse = K2ResponseMapper.CreateErrorResponse(
                    "Internal server error",
                    "INTERNAL_ERROR",
                    ex.Message);
                return StatusCode(500, errorResponse);
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
            var startTime = DateTime.UtcNow;
            var endpoint = "K2LoginWithToken";
            var httpMethod = "POST";
            var requestPath = $"{Request.Path}";

            _logger.LogInformation("Starting K2 LoginWithToken request");

            try
            {
                if (LoginToken == null )
                {
                    _logger.LogWarning("K2 LoginWithToken request failed: Request cannot be null");
                    await _oracleLogger.LogWarningAsync(endpoint, httpMethod, requestPath, "Request cannot be null", null, null, null, null);

                    var errorResponse = K2ResponseMapper.CreateErrorResponse(
                        "Request cannot be null",
                        "INVALID_REQUEST");

                    return BadRequest("Login token cannot be null or empty");
                }

                _logger.LogDebug("K2 LoginWithToken request parameters - LoginToken: ***");

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

                _logger.LogDebug("Requesting access token for K2 LoginWithToken");
                var accessTokenResult = await _exatApiService.GetAccessTokenAsync(tokenRequest);

                if (!accessTokenResult.Success || string.IsNullOrEmpty(accessTokenResult.Data?.AccessToken))
                {
                    var failureMessage = accessTokenResult.Message ?? "Failed to get access token";
                    _logger.LogWarning("K2 LoginWithToken failed: {Message}", failureMessage);
                    await _oracleLogger.LogWarningAsync(
                        endpoint, httpMethod, requestPath,
                        failureMessage,
                        null, null, null, request);

                    var errorResponse = K2ResponseMapper.CreateErrorResponse(
                        failureMessage,
                        accessTokenResult.ErrorCode ?? "TOKEN_ERROR");

                    return StatusCode(500, errorResponse);
                }

                var accessToken = accessTokenResult.Data.AccessToken;

                var result = await _exatApiService.LoginWithTokenAsync(accessToken, request);

                // Propagate token expiry if available
                if (result.Data != null && accessTokenResult.Data != null)
                {
                    result.Data.ExpiredIn = accessTokenResult.Data.ExpiredIn;
                }

                // Get Member by Email if available
                if (result.Data?.LoginData?.Email != null)
                {
                    _logger.LogInformation("K2 LoginWithToken: Fetching member info by email: {Email}", result.Data.LoginData.Email);

                    var memberByEmailResponse = await _exatApiService.GetMemberByEmailAsync(accessToken, result.Data.LoginData.Email);
                    if (memberByEmailResponse.Success && memberByEmailResponse.Data != null)
                    {
                        result.Data.MemberByEmail = memberByEmailResponse.Data;
                        _logger.LogInformation("K2 LoginWithToken: Member info by email retrieved successfully");
                    }
                    else
                    {
                        _logger.LogWarning("K2 LoginWithToken: Failed to get member info by email: {Message}", memberByEmailResponse.Message);
                    }
                }

                // Get Member by Customer ID if available
                if (result.Data?.LoginData?.CustomerId != null)
                {
                    _logger.LogInformation("K2 LoginWithToken: Fetching member info by customer ID: {CustomerId}", result.Data.LoginData.CustomerId);

                    var memberByIdResponse = await _exatApiService.GetMemberByCustomerIdAsync(accessToken, result.Data.LoginData.CustomerId);
                    if (memberByIdResponse.Success && memberByIdResponse.Data != null)
                    {
                        result.Data.MemberByCustomerId = memberByIdResponse.Data;
                        _logger.LogInformation("K2 LoginWithToken: Member info by customer ID retrieved successfully");
                    }
                    else
                    {
                        _logger.LogWarning("K2 LoginWithToken: Failed to get member info by customer ID: {Message}", memberByIdResponse.Message);
                    }
                }

                // result.Data is LoginResponse, not ApiResponse<LoginResponse>
                var k2Response = result.Data != null
                    ? K2ResponseMapper.MapToK2Login(result.Data)
                    : new K2ApiResponse<K2LoginResponseData>
                    {
                        Success = false,
                        Message = result.Message ?? "Login with token failed",
                        ErrorCode = result.ErrorCode ?? "LOGIN_FAILED"
                    };

                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogDebug("K2 LoginWithToken execution time: {ExecutionTime}ms", executionTime);

                if (k2Response.Success)
                {
                    var customerId = k2Response.Data?.LoginData?.CustomerId;
                    var email = k2Response.Data?.LoginData?.Email;

                    _logger.LogInformation("K2 LoginWithToken completed successfully in {ExecutionTime}ms", executionTime);
                    await _oracleLogger.LogInformationAsync(
                        endpoint, httpMethod, requestPath,
                        "Login with token successful (K2 format)",
                        null, customerId, email, executionTime,
                        request, k2Response);
                    return Ok(new
                            {
                                k2Response.Success,
                                k2Response.Message,
                                k2Response.Data.LoginData.MemberId,
                                k2Response.Data.LoginData.CustomerId,
                                k2Response.Data.LoginData.Email,
                                k2Response.Data.LoginData.UserType,
                                k2Response.Data.LoginData.AccountType,
                                k2Response.Data.LoginData.Title,
                                k2Response.Data.LoginData.FirstName,
                                k2Response.Data.LoginData.LastName,
                                k2Response.Data.LoginData.CreatedAt,
                                k2Response.Data.LoginData.CreatedBy,
                                k2Response.Data.LoginData.Active,
                                k2Response.Data.LoginData.IsConsentLatest,
                                k2Response.Data.MemberByCustomerId.DateOfBirth,
                                k2Response.Data.MemberByCustomerId.PhoneNo,
                                Status = k2Response.Data.MemberByCustomerId.Active,
                                contact_address_house_no = k2Response.Data.MemberByCustomerId.ContactAddress.HouseNo,
                                contact_address_village_no = k2Response.Data.MemberByCustomerId.ContactAddress.VillageNo,
                                contact_address_village_name = k2Response.Data.MemberByCustomerId.ContactAddress.VillageName,
                                contact_address_road = k2Response.Data.MemberByCustomerId.ContactAddress.Road,
                                contact_address_lane = k2Response.Data.MemberByCustomerId.ContactAddress.Lane,
                                contact_address_sub_district = k2Response.Data.MemberByCustomerId.ContactAddress.SubDistrict,
                                contact_address_district = k2Response.Data.MemberByCustomerId.ContactAddress.District,
                                contact_address_province = k2Response.Data.MemberByCustomerId.ContactAddress.Province,
                                contact_address_postal_code = k2Response.Data.MemberByCustomerId.ContactAddress.PostalCode,
                                tax_address_house_no = k2Response.Data.MemberByCustomerId.TaxAddress.HouseNo,
                                tax_address_village_no = k2Response.Data.MemberByCustomerId.TaxAddress.VillageNo,
                                tax_address_village_name = k2Response.Data.MemberByCustomerId.TaxAddress.VillageName,
                                tax_address_road = k2Response.Data.MemberByCustomerId.TaxAddress.Road,
                                tax_address_lane = k2Response.Data.MemberByCustomerId.TaxAddress.Lane,
                                tax_address_sub_district = k2Response.Data.MemberByCustomerId.TaxAddress.SubDistrict,
                                tax_address_district = k2Response.Data.MemberByCustomerId.TaxAddress.District,
                                tax_address_province = k2Response.Data.MemberByCustomerId.TaxAddress.Province,
                                tax_address_postal_code = k2Response.Data.MemberByCustomerId.TaxAddress.PostalCode
                            }
                    );
                }
                else
                {
                    _logger.LogWarning("K2 LoginWithToken failed: {Message}", k2Response.Message);
                    await _oracleLogger.LogWarningAsync(
                        endpoint, httpMethod, requestPath,
                        k2Response.Message ?? "Login with token failed",
                        null, null, null, request);
                    return StatusCode(500, k2Response);
                }
                
            }
            catch (Exception ex)
            {
                var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
                _logger.LogError(ex, "Error in K2 LoginWithToken endpoint after {ExecutionTime}ms", executionTime);
                await _oracleLogger.LogErrorAsync(
                    endpoint, httpMethod, requestPath,
                    "Error in K2 LoginWithToken endpoint",
                    ex, null, null, null, executionTime, LoginToken);

                var errorResponse = K2ResponseMapper.CreateErrorResponse(
                    "Internal server error",
                    "INTERNAL_ERROR",
                    ex.Message);
                return StatusCode(500, errorResponse);
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
                var statusCodesesion = session.IsNewSession ? 0 : 1;
                var messagesesion = session.IsNewSession
                                ? "Session token created successfully. Store this token in K2 SmartObject for future requests."
                                 : "มี session อยู่แล้วและยังไม่หมดอายุ (request ซ้ำ) - Active session already exists. Using existing token.";

                var response = K2Response<SessionTokenResponse>.Success(
                                session,
                                "Session token retrieved successfully. Store this token in K2 SmartObject for future requests."
                            );
                var firstNic = response.Data.ServerDeviceInfo.NetworkInterfaces.FirstOrDefault();

                return Ok(new
                {
                    statusCode = statusCodesesion,
                    message = messagesesion,
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

        [HttpPost("TFNInsert")]
        [ProducesResponseType(typeof(TFNNotiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InsertNotification
            ( [FromQuery] string? p_HIGHWAY_ID
            , [FromQuery] string? p_TITLE
            , [FromQuery] string? p_MESSAGE
            , [FromQuery] string? p_START_DATE
            , [FromQuery] string? p_END_DATE
            , [FromQuery] string? p_START_TIME
            , [FromQuery] string? p_END_TIME
            , [FromQuery] string? p_LINK
            , [FromQuery] string? p_SCHEDULE_DATE
            , [FromQuery] string? p_SCHEDULE_TIME
            , [FromQuery] string? p_REGISTER_DATE
            , [FromQuery] string? p_REGISTER_BY
            , [FromQuery] string? p_TOKEN
            )
        {
            var request = new TFNNotiRequest
            { 
                HIGHWAY_ID = p_HIGHWAY_ID
                ,TITLE = p_TITLE
                ,MESSAGE = p_MESSAGE
                ,START_DATE = p_START_DATE
                ,END_DATE = p_END_DATE
                ,START_TIME = p_START_TIME
                ,END_TIME = p_END_TIME
                ,LINK = p_LINK
                ,SCHEDULE_DATE = p_SCHEDULE_DATE
                ,SCHEDULE_TIME = p_SCHEDULE_TIME
                ,REGISTER_DATE = p_REGISTER_DATE
                ,REGISTER_BY = p_REGISTER_BY
                ,TOKEN = string.IsNullOrEmpty(p_TOKEN) ? _TFNSettings.Token : p_TOKEN
            };
            try
            {
                if (request == null)
                    return BadRequest("Request cannot be null");

                var result = await _notificationService.InsertNotiAsync(request);

                // สมมติว่าถ้า STATUS_CODE = "200" คือสำเร็จ
                if (result.StatusCode == 201)
                {
                    return Ok(new {
                        statusCode = result.StatusCode
                      , message = result.Message
                      , noti_id = result.NotiId                                    
                    });
                }

                // เผื่ออยาก log กรณี status code อื่น
                _logger.LogWarning("insert_noti returned non-200 status_code: {StatusCode}, message={Message}",
                    result.StatusCode, result.Message);

                return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling insert_noti");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("TFNUpdate")]
        [ProducesResponseType(typeof(TFNNotiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateNotification(
              [FromQuery] int notiId
            , [FromQuery] string? p_HIGHWAY_ID
            , [FromQuery] string? p_TITLE
            , [FromQuery] string? p_MESSAGE
            , [FromQuery] string? p_START_DATE
            , [FromQuery] string? p_END_DATE
            , [FromQuery] string? p_START_TIME
            , [FromQuery] string? p_END_TIME
            , [FromQuery] string? p_LINK
            , [FromQuery] string? p_SCHEDULE_DATE
            , [FromQuery] string? p_SCHEDULE_TIME
            , [FromQuery] string? p_REGISTER_DATE
            , [FromQuery] string? p_REGISTER_BY
            , [FromQuery] string? p_TOKEN)
        {
            if (notiId == null)
                return BadRequest("Request cannot be null");
            var request = new TFNNotiRequest
            {
                HIGHWAY_ID = p_HIGHWAY_ID
               ,TITLE = p_TITLE
               ,MESSAGE = p_MESSAGE
               ,START_DATE = p_START_DATE
               ,END_DATE = p_END_DATE
               ,START_TIME = p_START_TIME
               ,END_TIME = p_END_TIME
               ,LINK = p_LINK
               ,SCHEDULE_DATE = p_SCHEDULE_DATE
               ,SCHEDULE_TIME = p_SCHEDULE_TIME
               ,REGISTER_DATE = p_REGISTER_DATE
               ,REGISTER_BY = p_REGISTER_BY
               ,TOKEN = string.IsNullOrEmpty(p_TOKEN) ? _TFNSettings.Token : p_TOKEN
            };

            var result = await _notificationService.UpdateNotiAsync(notiId, request);

            // ตามสเปกเดิม STATUS_CODE / message / NOTI_ID
            if (result.StatusCode == 203)
                return Ok(new
                {
                    statusCode = result.StatusCode
                   ,message = result.Message
                   ,noti_id = notiId
                });

            // เผื่อ STATUS_CODE อื่น
            _logger.LogWarning("update_noti returned status_code: {StatusCode}, message={Message}",
                result.StatusCode, result.Message);

            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        [HttpPost("TFNUpdateStatus")]
        [ProducesResponseType(typeof(TFNNotiResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateNotificationStatus([FromQuery] int p_notiId
            , [FromQuery] string? p_STATUS
            , [FromQuery] string? p_APPROVE_DATE
            , [FromQuery] string? p_APPROVE_BY
            , [FromQuery] string? p_LINK
            , [FromQuery] string? p_TOKEN
            )
        {
            var request = new TFNNotiRequest
            {
                NOTI_ID = p_notiId,
                STATUS  = p_STATUS,
                APPROVE_DATE = p_APPROVE_DATE,
                APPROVE_BY = p_APPROVE_BY,
                LINK = p_LINK,
                TOKEN = string.IsNullOrEmpty(p_TOKEN) ? _TFNSettings.Token : p_TOKEN
            };

            if (request == null)
                return BadRequest("Request cannot be null");

            var result = await _notificationService.UpdateStatusAsync(p_notiId, request);

            if (result.StatusCode == 203)
                return Ok(new
                {
                    statusCode = result.StatusCode
                   ,message = result.Message
                   ,noti_id = p_notiId
                });

            _logger.LogWarning("update_status returned status_code: {StatusCode}, message={Message}",
                result.StatusCode, result.Message);

            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

    }
}
