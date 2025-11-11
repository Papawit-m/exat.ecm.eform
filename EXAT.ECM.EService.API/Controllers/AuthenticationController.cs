using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EXAT.ECM.EService.API.Model.Responses;

namespace EXAT.ECM.EService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IExatApiService _exatApiService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly ExatApiSettings _exatSettings;

        public AuthenticationController(IExatApiService exatApiService, ILogger<AuthenticationController> logger,IOptions<ExatApiSettings> exatSettings)
        {
            _exatApiService = exatApiService;
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
    }
}
