using EXAT.ECM.EER.ESARABAN.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace EXAT.ECM.EER.ESARABAN.Controllers
{
    /// <summary>
    /// Hello/Test Controller - ทดสอบการเชื่อมต่อ eSaraban UAT API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HelloController : ControllerBase
    {
        private readonly ILogger<HelloController> _logger;
        private readonly ESarabanApiService _esarabanApi;

        public HelloController(
            ILogger<HelloController> logger,
            ESarabanApiService esarabanApi)
        {
            _logger = logger;
            _esarabanApi = esarabanApi;
        }

        /// <summary>
        /// Test eSaraban API Connection
        /// ทดสอบการเชื่อมต่อ eSaraban UAT API โดยเรียก /api/books/generate-code
        /// </summary>
        /// <remarks>
        /// Test endpoint to verify connection to eSaraban UAT API.
        /// 
        /// Uses real API call from Postman Collection:
        /// - Endpoint: GET /api/books/generate-code
        /// - Parameters: user_ad (from Postman), book_id (generated GUID)
        /// 
        /// Example request:
        /// ```
        /// GET /api/hello
        /// ```
        /// 
        /// Example response (Success):
        /// ```json
        /// {
        ///   "success": true,
        ///   "message": "✅ Connected to eSaraban UAT API successfully!",
        ///   "endpoint_tested": "GET /api/books/generate-code",
        ///   "user_ad": "EXAT\\ECMUSR07",
        ///   "book_id": "26806062-775f-44c7-9776-e9a2b23b9856",
        ///   "esaraban_response": {
        ///     "status": "S",
        ///     "statusCode": "200",
        ///     "book_code": "null",
        ///     "to_date": "null"
        ///   },
        ///   "connection_info": {
        ///     "base_url": "http://api-uat.exat.co.th/esrb-external-api",
        ///     "proxy_enabled": true,
        ///     "ssl_validation": "bypassed (development)",
        ///     "timeout": "30s"
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <response code="200">Success - เชื่อมต่อ eSaraban API สำเร็จ</response>
        /// <response code="503">Service Unavailable - ไม่สามารถเชื่อมต่อ eSaraban API</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Test eSaraban API Connection",
            Description = "ทดสอบการเชื่อมต่อ eSaraban UAT API โดยเรียก /api/books/generate-code ด้วยข้อมูลจริงจาก Postman Collection",
            Tags = new[] { "Hello/Test" }
        )]
        [SwaggerResponse(200, "Success - เชื่อมต่อ eSaraban API สำเร็จ", typeof(HelloResponse))]
        [SwaggerResponse(503, "Service Unavailable - ไม่สามารถเชื่อมต่อ eSaraban API", typeof(HelloResponse))]
        public async Task<IActionResult> Get()
        {
            try
            {
                _logger.LogInformation("Hello endpoint called - Testing eSaraban UAT API connection");

                // Use real user_ad from Postman Collection
                string userAd = "EXAT\\ECMUSR07";

                // Generate test book_id (GUID)
                string bookId = Guid.NewGuid().ToString();

                _logger.LogInformation($"Testing with user_ad: {userAd}, book_id: {bookId}");

                // Call eSaraban API - Generate Code endpoint (from Postman Collection)
                var apiResponse = await _esarabanApi.GenerateCodeAsync(userAd, bookId);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to connect to eSaraban API");

                    return StatusCode(503, new HelloResponse
                    {
                        Success = false,
                        Message = "❌ Failed to connect to eSaraban UAT API",
                        EndpointTested = "GET /api/books/generate-code",
                        UserAd = userAd,
                        BookId = bookId,
                        ESarabanResponse = null,
                        ConnectionInfo = new ConnectionInfo
                        {
                            BaseUrl = "http://api-uat.exat.co.th/esrb-external-api",
                            ProxyEnabled = true,
                            SslValidation = "bypassed (development)",
                            Timeout = "30s"
                        },
                        ErrorDetails = "API returned null response. Check logs for details."
                    });
                }

                // Success response
                _logger.LogInformation($"eSaraban API responded: status={apiResponse.Status}, statusCode={apiResponse.StatusCode}");

                return Ok(new HelloResponse
                {
                    Success = true,
                    Message = "✅ Connected to eSaraban UAT API successfully!",
                    EndpointTested = "GET /api/books/generate-code",
                    UserAd = userAd,
                    BookId = bookId,
                    ESarabanResponse = new ESarabanResponseInfo
                    {
                        Status = apiResponse.Status,
                        StatusCode = apiResponse.StatusCode,
                        BookCode = apiResponse.BookCode,
                        ToDate = apiResponse.ToDate
                    },
                    ConnectionInfo = new ConnectionInfo
                    {
                        BaseUrl = "http://api-uat.exat.co.th/esrb-external-api",
                        ProxyEnabled = true,
                        SslValidation = "bypassed (development)",
                        Timeout = "30s"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in Hello endpoint: {ex.Message}");

                return StatusCode(503, new HelloResponse
                {
                    Success = false,
                    Message = "❌ Exception occurred while connecting to eSaraban API",
                    EndpointTested = "GET /api/books/generate-code",
                    UserAd = "EXAT\\ECMUSR07",
                    BookId = null,
                    ESarabanResponse = null,
                    ConnectionInfo = new ConnectionInfo
                    {
                        BaseUrl = "http://api-uat.exat.co.th/esrb-external-api",
                        ProxyEnabled = true,
                        SslValidation = "bypassed (development)",
                        Timeout = "30s"
                    },
                    ErrorDetails = $"{ex.GetType().Name}: {ex.Message}"
                });
            }
        }
    }

    /// <summary>
    /// Hello/Test Response
    /// </summary>
    public class HelloResponse
    {
        /// <summary>Connection success status</summary>
        public bool Success { get; set; }

        /// <summary>Response message</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>eSaraban endpoint tested</summary>
        public string EndpointTested { get; set; } = string.Empty;

        /// <summary>User AD used in test (from Postman Collection)</summary>
        public string UserAd { get; set; } = string.Empty;

        /// <summary>Book ID used in test (generated GUID)</summary>
        public string? BookId { get; set; }

        /// <summary>eSaraban API response</summary>
        public ESarabanResponseInfo? ESarabanResponse { get; set; }

        /// <summary>Connection configuration info</summary>
        public ConnectionInfo? ConnectionInfo { get; set; }

        /// <summary>Error details (if failed)</summary>
        public string? ErrorDetails { get; set; }
    }

    /// <summary>
    /// eSaraban API Response Info
    /// </summary>
    public class ESarabanResponseInfo
    {
        /// <summary>Status (S=Success, E=Error)</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>HTTP Status Code</summary>
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>Generated book code (from eSaraban)</summary>
        public string? BookCode { get; set; }

        /// <summary>Document date</summary>
        public string? ToDate { get; set; }
    }

    /// <summary>
    /// Connection Configuration Info
    /// </summary>
    public class ConnectionInfo
    {
        /// <summary>eSaraban UAT API base URL</summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>Proxy enabled status</summary>
        public bool ProxyEnabled { get; set; }

        /// <summary>SSL validation setting</summary>
        public string SslValidation { get; set; } = string.Empty;

        /// <summary>HTTP timeout</summary>
        public string Timeout { get; set; } = string.Empty;
    }
}
