using EXAT.ECM.EER.ESARABAN.Models;
using EXAT.ECM.EER.ESARABAN.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json.Serialization;

namespace EXAT.ECM.EER.ESARABAN.Controllers
{
    /// <summary>
    /// Books API Controller สำหรับจัดการเอกสาร (Books) ในระบบ eSaraban
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BooksController : ControllerBase
    {
        private readonly ILogger<BooksController> _logger;
        private readonly BookDefaultSettings _bookDefaults;
        private readonly ESarabanApiService _esarabanApi;

        public BooksController(
            ILogger<BooksController> logger,
            IOptions<BookDefaultSettings> bookDefaults,
            ESarabanApiService esarabanApi)
        {
            _logger = logger;
            _bookDefaults = bookDefaults.Value;
            _esarabanApi = esarabanApi;
        }

        /// <summary>
        /// สร้างเอกสารแบบง่าย - กรณีอนุมัติ/เข้าสู่หลักเกณ์ (K2 SmartObject Compatible)
        /// </summary>
        /// <remarks>
        /// ตัวอย่าง Response (Flat, K2-compatible):
        ///
        /// ```json
        /// {
        ///   "Status": "S",
        ///   "StatusCode": "200",
        ///   "Message": "Success: generate book.",
        ///   "BookId": "0CEB9F7B3BF144E097CD6871FE177D1F",
        ///   "BookSubject": "ทดสอบ",
        ///   "BookTo": "สผว.",
        ///   "RegistrationBookId": "012...000",
        ///   "ParentBookId": null,
        ///   "ParentOrgId": null,
        ///   "ParentPositionName": null,
        ///   "BookTypeId": 0,
        ///   "BookFile": null,
        ///   "FileCount": 0,
        ///   "BookAttach": null,
        ///   "AttachCount": 0,
        ///   "CreatedBy": "EXAT\\\\ECMUSR07",
        ///   "CreatedDate": "2025-11-07T10:30:00Z"
        /// }
        /// ```
        /// </remarks>
        /// <param name="simpleRequest">ข้อมูลการสร้างเอกสารแบบง่าย (รับผ่าน Request Body)</param>
        /// <returns>ข้อมูลเอกสารที่สร้างขึ้น</returns>
        [HttpPost("create/approved/simple")]
        [SwaggerOperation(
            Summary = "สร้างเอกสารแบบง่าย - กรณีอนุมัติ/เข้าสู่หลักเกณ์ (K2 SmartObject)",
            Description = "สร้างเอกสารใหม่แบบง่าย ๆ โดยส่งเฉพาะพารามิเตอร์ที่จำเป็นผ่าน Request Body (รองรับ K2 REST Service integration และ K2 SmartObject)",
            Tags = new[] { "Books - Create (K2 Compatible)" }
        )]
        // K2 Compatible: Flat response model (no ApiResponse wrapper)
        [SwaggerResponse(200, "Success - เอกสารถูกสร้างสำเร็จ", typeof(CreateBookSimpleResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(CreateBookSimpleResponse))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(CreateBookSimpleResponse))]
        public async Task<IActionResult> CreateBookApprovedSimple(
            [FromBody, SwaggerRequestBody("ข้อมูลการสร้างเอกสารแบบง่าย (K2 SmartObject Compatible)", Required = true)] CreateBookApprovedSimpleRequest simpleRequest)
        {
            try
            {
                _logger.LogInformation($"CreateBookApprovedSimple called by user: {simpleRequest?.user_ad}");

                // Validate input
                if (simpleRequest == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Request body is required",
                        "REQUEST_BODY_REQUIRED"
                    ));
                }

                // Apply user_ad default if not provided
                if (string.IsNullOrEmpty(simpleRequest.user_ad))
                {
                    simpleRequest.user_ad = _bookDefaults.UserAd ?? string.Empty;
                }

                if (string.IsNullOrEmpty(simpleRequest.book_subject))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_subject is required",
                        "BOOK_SUBJECT_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(simpleRequest.book_to))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_to is required",
                        "BOOK_TO_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(simpleRequest.registrationbook_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "registrationbook_id is required",
                        "REGISTRATIONBOOK_ID_REQUIRED"
                    ));
                }

                // Apply default values from configuration
                var fullRequest = new ESarabanCreateBookRequest
                {
                    user_ad = simpleRequest.user_ad,
                    book = new BookData
                    {
                        book_subject = simpleRequest.book_subject,
                        book_to = simpleRequest.book_to,
                        registrationbook_id = simpleRequest.registrationbook_id,

                        // Registration Book Details (7 fields)
                        registrationbook_nameth = simpleRequest.registrationbook_nameth,
                        registrationbook_nameen = simpleRequest.registrationbook_nameen,
                        registrationbook_ogr_id = simpleRequest.registrationbook_ogr_id,
                        registrationbook_org_code = simpleRequest.registrationbook_org_code,
                        registrationbook_org_nameth = simpleRequest.registrationbook_org_nameth,
                        registrationbook_org_nameen = simpleRequest.registrationbook_org_nameen,
                        registrationbook_org_shtname = simpleRequest.registrationbook_org_shtname,

                        // Parent Organization Details
                        parent_bookid = simpleRequest.parent_bookid ?? "",
                        parent_orgid = simpleRequest.parent_orgid ?? "",
                        parent_orgcode = simpleRequest.parent_orgcode,
                        parent_positioncode = simpleRequest.parent_positioncode,
                        parent_positionname = simpleRequest.parent_positionname ?? ""
                    },
                    bookFile = simpleRequest.bookFile,      // เพิ่มไฟล์จาก request
                    bookAttach = simpleRequest.bookAttach   // เพิ่มไฟล์แนบจาก request
                };

                ApplyDefaults(fullRequest, "approved");

                // Call eSaraban External API to create book
                _logger.LogInformation("Calling eSaraban API to create book...");
                var apiResponse = await _esarabanApi.CreateBookAsync(fullRequest);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new CreateBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                // Convert full response to simple response format (K2 Compatible)
                var response = new CreateBookSimpleResponse
                {
                    Status = apiResponse.Status,
                    StatusCode = apiResponse.StatusCode,
                    Message = apiResponse.Message,
                    BookId = apiResponse.BookId,
                    BookSubject = apiResponse.BookSubject,
                    BookTo = apiResponse.BookTo,
                    RegistrationBookId = apiResponse.RegistrationBookId,
                    ParentBookId = apiResponse.ParentBookId,
                    ParentOrgId = apiResponse.ParentOrgId,
                    ParentPositionName = apiResponse.ParentPositionName,
                    BookTypeId = apiResponse.BookTypeId,
                    BookFile = apiResponse.BookFile,
                    FileCount = apiResponse.FileCount,
                    BookAttach = apiResponse.BookAttach,
                    AttachCount = apiResponse.AttachCount,
                    CreatedBy = apiResponse.CreatedBy,
                    CreatedDate = apiResponse.CreatedDate
                };

                _logger.LogInformation($"Book created successfully with ID: {response.BookId} (from eSaraban API)");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(new { response.Status
                              , response.StatusCode
                              , response.Message
                              , response.BookId
                              , response.BookSubject
                              , response.BookTo
                              , response.RegistrationBookId
                              , response.ParentBookId
                              , response.ParentOrgId
                              , response.ParentPositionName
                              , response.BookTypeId
                              , response.BookFile
                              , response.FileCount
                              , response.BookAttach
                              , response.AttachCount
                              , response.CreatedBy
                              , response.CreatedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateBookApprovedSimple: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new CreateBookSimpleResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// กรณี อนุมัติ/เข้าสู่หลักเกณ์ - สร้างเอกสารสำหรับกรณีที่ได้รับการอนุมัติและเข้าสู่หลักเกณ์แล้ว
        /// </summary>
        /// <remarks>
        /// ตัวอย่าง Response (Flat, K2-compatible):
        ///
        /// ```json
        /// {
        ///   "Status": "S",
        ///   "StatusCode": "200",
        ///   "Message": "Success: generate book.",
        ///   "BookId": "0CEB9F7B3BF144E097CD6871FE177D1F",
        ///   "BookSubject": "ทดสอบ",
        ///   "BookTo": "สผว.",
        ///   "RegistrationBookId": "012...000",
        ///   "CreatedBy": "EXAT\\\\ECMUSR07",
        ///   "CreatedDate": "2025-11-07T10:30:00Z"
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">ข้อมูลการสร้างเอกสาร (รวม user_ad, book, bookAttach, bookFile, bookHistory, bookReferences, bookReferenceAttach)</param>
        /// <returns>ข้อมูลเอกสารที่สร้างขึ้น</returns>
        [HttpPost("create/approved")]
        [SwaggerOperation(
            Summary = "สร้างเอกสาร - กรณีอนุมัติ/เข้าสู่หลักเกณ์",
            Description = "สร้างเอกสารใหม่สำหรับกรณีที่ได้รับการอนุมัติและเข้าสู่หลักเกณ์แล้ว (มีหนังสือรับรองจากหน่วยงานที่เกี่ยวข้อง)",
            Tags = new[] { "Books - Create" }
        )]
        // K2 Compatible: Flat response model
        [SwaggerResponse(200, "Success - เอกสารถูกสร้างสำเร็จ", typeof(CreateBookSimpleResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(CreateBookSimpleResponse))]
        [SwaggerResponse(404, "Not Found - ไม่พบข้อมูล", typeof(CreateBookSimpleResponse))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(CreateBookSimpleResponse))]
        public async Task<IActionResult> CreateBookApproved(
            [FromBody, SwaggerRequestBody("ข้อมูลการสร้างเอกสารกรณีอนุมัติ (ตาม eSaraban API Spec)", Required = true)] ESarabanCreateBookRequest request)
        {
            try
            {
                _logger.LogInformation($"CreateBookApproved called by user: {request?.user_ad}");

                // Validate input
                if (request == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Request body is required",
                        "REQUEST_BODY_REQUIRED"
                    ));
                }

                // Apply user_ad default if not provided
                if (string.IsNullOrEmpty(request.user_ad))
                {
                    request.user_ad = _bookDefaults.UserAd ?? string.Empty;
                }

                if (request.book == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book data is required",
                        "BOOK_DATA_REQUIRED"
                    ));
                }

                // Apply default values from configuration
                ApplyDefaults(request, "approved");

                // Call eSaraban External API to create book
                _logger.LogInformation("Calling eSaraban API to create book (Full Format)...");
                var response = await _esarabanApi.CreateBookAsync(request);

                if (response == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new ESarabanCreateBookResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Book created successfully with ID: {response.BookId} (from eSaraban API)");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateBookApproved: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new ESarabanCreateBookResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// สร้างเอกสารแบบง่าย - กรณีไม่เข้าหลักเกณ์ (K2 SmartObject Compatible)
        /// </summary>
        /// <param name="simpleRequest">ข้อมูลการสร้างเอกสารแบบง่าย (รับผ่าน Request Body)</param>
        /// <returns>ข้อมูลเอกสารที่สร้างขึ้น</returns>
        [HttpPost("create/non-compliant/simple")]
        [SwaggerOperation(
            Summary = "สร้างเอกสารแบบง่าย - กรณีไม่เข้าหลักเกณ์ (K2 SmartObject)",
            Description = "สร้างเอกสารใหม่แบบง่าย ๆ สำหรับกรณีที่ไม่เข้าหลักเกณ์ โดยส่งเฉพาะพารามิเตอร์ที่จำเป็นผ่าน Request Body (รองรับ K2 REST Service integration และ K2 SmartObject)",
            Tags = new[] { "Books - Create (K2 Compatible)" }
        )]
        [SwaggerResponse(200, "Success - เอกสารถูกสร้างสำเร็จ", typeof(ApiResponse<object>))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateBookNonCompliantSimple(
            [FromBody, SwaggerRequestBody("ข้อมูลการสร้างเอกสารแบบง่าย - กรณีไม่เข้าหลักเกณ์ (K2 SmartObject Compatible)", Required = true)] CreateBookNonCompliantSimpleRequest simpleRequest)
        {
            try
            {
                _logger.LogInformation($"CreateBookNonCompliantSimple called by user: {simpleRequest?.user_ad}");

                // Validate input
                if (simpleRequest == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Request body is required",
                        "REQUEST_BODY_REQUIRED"
                    ));
                }

                // Apply user_ad default if not provided
                if (string.IsNullOrEmpty(simpleRequest.user_ad))
                {
                    simpleRequest.user_ad = _bookDefaults.UserAd ?? string.Empty;
                }

                if (string.IsNullOrEmpty(simpleRequest.book_subject))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_subject is required",
                        "BOOK_SUBJECT_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(simpleRequest.book_to))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_to is required",
                        "BOOK_TO_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(simpleRequest.registrationbook_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "registrationbook_id is required",
                        "REGISTRATIONBOOK_ID_REQUIRED"
                    ));
                }

                // Apply default values from configuration
                var fullRequest = new ESarabanCreateBookRequest
                {
                    user_ad = simpleRequest.user_ad,
                    book = new BookData
                    {
                        book_subject = simpleRequest.book_subject,
                        book_to = simpleRequest.book_to,
                        registrationbook_id = simpleRequest.registrationbook_id,

                        // Registration Book Details (7 fields)
                        registrationbook_nameth = simpleRequest.registrationbook_nameth,
                        registrationbook_nameen = simpleRequest.registrationbook_nameen,
                        registrationbook_ogr_id = simpleRequest.registrationbook_ogr_id,
                        registrationbook_org_code = simpleRequest.registrationbook_org_code,
                        registrationbook_org_nameth = simpleRequest.registrationbook_org_nameth,
                        registrationbook_org_nameen = simpleRequest.registrationbook_org_nameen,
                        registrationbook_org_shtname = simpleRequest.registrationbook_org_shtname,

                        // Parent Organization Details
                        parent_bookid = simpleRequest.parent_bookid ?? "",
                        parent_orgid = simpleRequest.parent_orgid ?? "",
                        parent_orgcode = simpleRequest.parent_orgcode,
                        parent_positioncode = simpleRequest.parent_positioncode,
                        parent_positionname = simpleRequest.parent_positionname ?? ""
                    },
                    bookFile = simpleRequest.bookFile,      // เพิ่มไฟล์จาก request
                    bookAttach = simpleRequest.bookAttach   // เพิ่มไฟล์แนบจาก request
                };

                ApplyDefaults(fullRequest, "non-compliant");

                // Call eSaraban External API to create book
                _logger.LogInformation("Calling eSaraban API to create book (Non-Compliant)...");
                var apiResponse = await _esarabanApi.CreateBookAsync(fullRequest);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new CreateBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                // Convert full response to simple response format (K2 Compatible)
                var response = new CreateBookSimpleResponse
                {
                    Status = apiResponse.Status,
                    StatusCode = apiResponse.StatusCode,
                    Message = apiResponse.Message,
                    BookId = apiResponse.BookId,
                    BookSubject = apiResponse.BookSubject,
                    BookTo = apiResponse.BookTo,
                    RegistrationBookId = apiResponse.RegistrationBookId,
                    ParentBookId = apiResponse.ParentBookId,
                    ParentOrgId = apiResponse.ParentOrgId,
                    ParentPositionName = apiResponse.ParentPositionName,
                    BookTypeId = apiResponse.BookTypeId,
                    BookFile = apiResponse.BookFile,
                    FileCount = apiResponse.FileCount,
                    BookAttach = apiResponse.BookAttach,
                    AttachCount = apiResponse.AttachCount,
                    CreatedBy = apiResponse.CreatedBy,
                    CreatedDate = apiResponse.CreatedDate
                };

                _logger.LogInformation($"Book created successfully with ID: {response.BookId} (from eSaraban API)");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateBookNonCompliantSimple: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new CreateBookSimpleResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// กรณี แบบไม่เข้าหลักเกณ์ - สร้างเอกสารสำหรับกรณีที่ไม่เข้าหลักเกณ์
        /// </summary>
        /// <param name="request">ข้อมูลการสร้างเอกสาร (รวม user_ad, book, bookAttach, bookFile, bookHistory, bookReferences, bookReferenceAttach)</param>
        /// <returns>ข้อมูลเอกสารที่สร้างขึ้น</returns>
        [HttpPost("create/non-compliant")]
        [SwaggerOperation(
            Summary = "สร้างเอกสาร - กรณีไม่เข้าหลักเกณ์",
            Description = "สร้างเอกสารใหม่สำหรับกรณีที่ไม่เข้าหลักเกณ์ (ไม่มีเอกสารรับรอง/ไม่ผ่านเกณฑ์)",
            Tags = new[] { "Books - Create" }
        )]
        [SwaggerResponse(200, "Success - เอกสารถูกสร้างสำเร็จ", typeof(ApiResponse<object>))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(ApiResponse<object>))]
        [SwaggerResponse(404, "Not Found - ไม่พบข้อมูล", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateBookNonCompliant(
            [FromBody, SwaggerRequestBody("ข้อมูลการสร้างเอกสารกรณีไม่เข้าหลักเกณ์ (ตาม eSaraban API Spec)", Required = true)] ESarabanCreateBookRequest request)
        {
            try
            {
                _logger.LogInformation($"CreateBookNonCompliant called by user: {request?.user_ad}");

                // Validate input
                if (request == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Request body is required",
                        "REQUEST_BODY_REQUIRED"
                    ));
                }

                // Apply user_ad default if not provided
                if (string.IsNullOrEmpty(request.user_ad))
                {
                    request.user_ad = _bookDefaults.UserAd ?? string.Empty;
                }

                if (request.book == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book data is required",
                        "BOOK_DATA_REQUIRED"
                    ));
                }

                // Apply default values from configuration
                ApplyDefaults(request, "non-compliant");

                // Call eSaraban External API to create book
                _logger.LogInformation("Calling eSaraban API to create book (Non-Compliant - Full Format)...");
                var response = await _esarabanApi.CreateBookAsync(request);

                if (response == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new ESarabanCreateBookResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Book created successfully with ID: {response.BookId} (from eSaraban API)");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateBookNonCompliant: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new ESarabanCreateBookResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// สร้างเอกสารแบบง่าย - กรณีอยู่ระหว่างก่อสร้าง (K2 SmartObject Compatible)
        /// </summary>
        /// <param name="simpleRequest">ข้อมูลการสร้างเอกสารแบบง่าย (รับผ่าน Request Body)</param>
        /// <returns>ข้อมูลเอกสารที่สร้างขึ้น</returns>
        [HttpPost("create/under-construction/simple")]
        [SwaggerOperation(
            Summary = "สร้างเอกสารแบบง่าย - กรณีอยู่ระหว่างก่อสร้าง (K2 SmartObject)",
            Description = "สร้างเอกสารใหม่แบบง่าย ๆ สำหรับโครงการที่อยู่ระหว่างก่อสร้างและต้องการขอหนังสือจากที่ปรึกษา โดยส่งเฉพาะพารามิเตอร์ที่จำเป็นผ่าน Request Body (รองรับ K2 REST Service integration และ K2 SmartObject)",
            Tags = new[] { "Books - Create (K2 Compatible)" }
        )]
        [SwaggerResponse(200, "Success - เอกสารถูกสร้างสำเร็จ", typeof(ApiResponse<object>))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateBookUnderConstructionSimple(
            [FromBody, SwaggerRequestBody("ข้อมูลการสร้างเอกสารแบบง่าย - กรณีอยู่ระหว่างก่อสร้าง (K2 SmartObject Compatible)", Required = true)] CreateBookUnderConstructionSimpleRequest simpleRequest)
        {
            try
            {
                _logger.LogInformation($"CreateBookUnderConstructionSimple called by user: {simpleRequest?.user_ad}");

                // Validate input
                if (simpleRequest == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Request body is required",
                        "REQUEST_BODY_REQUIRED"
                    ));
                }

                // Apply user_ad default if not provided
                if (string.IsNullOrEmpty(simpleRequest.user_ad))
                {
                    simpleRequest.user_ad = _bookDefaults.UserAd ?? string.Empty;
                }

                if (string.IsNullOrEmpty(simpleRequest.book_subject))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_subject is required",
                        "BOOK_SUBJECT_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(simpleRequest.book_to))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_to is required",
                        "BOOK_TO_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(simpleRequest.registrationbook_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "registrationbook_id is required",
                        "REGISTRATIONBOOK_ID_REQUIRED"
                    ));
                }

                // Apply default values from configuration
                var fullRequest = new ESarabanCreateBookRequest
                {
                    user_ad = simpleRequest.user_ad,
                    book = new BookData
                    {
                        book_subject = simpleRequest.book_subject,
                        book_to = simpleRequest.book_to,
                        registrationbook_id = simpleRequest.registrationbook_id,

                        // Registration Book Details (7 fields)
                        registrationbook_nameth = simpleRequest.registrationbook_nameth,
                        registrationbook_nameen = simpleRequest.registrationbook_nameen,
                        registrationbook_ogr_id = simpleRequest.registrationbook_ogr_id,
                        registrationbook_org_code = simpleRequest.registrationbook_org_code,
                        registrationbook_org_nameth = simpleRequest.registrationbook_org_nameth,
                        registrationbook_org_nameen = simpleRequest.registrationbook_org_nameen,
                        registrationbook_org_shtname = simpleRequest.registrationbook_org_shtname,

                        // Parent Organization Details
                        parent_bookid = simpleRequest.parent_bookid ?? "",
                        parent_orgid = simpleRequest.parent_orgid ?? "",
                        parent_orgcode = simpleRequest.parent_orgcode,
                        parent_positioncode = simpleRequest.parent_positioncode,
                        parent_positionname = simpleRequest.parent_positionname ?? ""
                    },
                    bookFile = simpleRequest.bookFile,      // เพิ่มไฟล์จาก request
                    bookAttach = simpleRequest.bookAttach   // เพิ่มไฟล์แนบจาก request
                };

                ApplyDefaults(fullRequest, "under-construction");

                // Call eSaraban External API to create book
                _logger.LogInformation("Calling eSaraban API to create book (Under-Construction)...");
                var apiResponse = await _esarabanApi.CreateBookAsync(fullRequest);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new CreateBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                // Convert full response to simple response format (K2 Compatible)
                var response = new CreateBookSimpleResponse
                {
                    Status = apiResponse.Status,
                    StatusCode = apiResponse.StatusCode,
                    Message = apiResponse.Message,
                    BookId = apiResponse.BookId,
                    BookSubject = apiResponse.BookSubject,
                    BookTo = apiResponse.BookTo,
                    RegistrationBookId = apiResponse.RegistrationBookId,
                    ParentBookId = apiResponse.ParentBookId,
                    ParentOrgId = apiResponse.ParentOrgId,
                    ParentPositionName = apiResponse.ParentPositionName,
                    BookTypeId = apiResponse.BookTypeId,
                    BookFile = apiResponse.BookFile,
                    FileCount = apiResponse.FileCount,
                    BookAttach = apiResponse.BookAttach,
                    AttachCount = apiResponse.AttachCount,
                    CreatedBy = apiResponse.CreatedBy,
                    CreatedDate = apiResponse.CreatedDate
                };

                _logger.LogInformation($"Book created successfully with ID: {response.BookId} (from eSaraban API)");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateBookUnderConstructionSimple: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new CreateBookSimpleResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// กรณี อยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา - สร้างเอกสารสำหรับโครงการที่อยู่ระหว่างดำเนินการ
        /// </summary>
        /// <param name="request">ข้อมูลการสร้างเอกสาร (รวม user_ad, book, bookAttach, bookFile, bookHistory, bookReferences, bookReferenceAttach)</param>
        /// <returns>ข้อมูลเอกสารที่สร้างขึ้น</returns>
        [HttpPost("create/under-construction")]
        [SwaggerOperation(
            Summary = "สร้างเอกสาร - กรณีอยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา",
            Description = "สร้างเอกสารใหม่สำหรับโครงการที่อยู่ระหว่างก่อสร้างและต้องการขอหนังสือจากที่ปรึกษา",
            Tags = new[] { "Books - Create" }
        )]
        [SwaggerResponse(200, "Success - เอกสารถูกสร้างสำเร็จ", typeof(ApiResponse<object>))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(ApiResponse<object>))]
        [SwaggerResponse(404, "Not Found - ไม่พบข้อมูล", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateBookUnderConstruction(
            [FromBody, SwaggerRequestBody("ข้อมูลการสร้างเอกสารกรณีอยู่ระหว่างก่อสร้าง (ตาม eSaraban API Spec)", Required = true)] ESarabanCreateBookRequest request)
        {
            try
            {
                _logger.LogInformation($"CreateBookUnderConstruction called by user: {request?.user_ad}");

                // Validate input
                if (request == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Request body is required",
                        "REQUEST_BODY_REQUIRED"
                    ));
                }

                // Apply user_ad default if not provided
                if (string.IsNullOrEmpty(request.user_ad))
                {
                    request.user_ad = _bookDefaults.UserAd ?? string.Empty;
                }

                if (request.book == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book data is required",
                        "BOOK_DATA_REQUIRED"
                    ));
                }

                // Apply default values from configuration
                ApplyDefaults(request, "under-construction");

                // Call eSaraban External API to create book
                _logger.LogInformation("Calling eSaraban API to create book (Under-Construction - Full Format)...");
                var response = await _esarabanApi.CreateBookAsync(request);

                if (response == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new ESarabanCreateBookResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Book created successfully with ID: {response.BookId} (from eSaraban API)");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateBookUnderConstruction: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new ESarabanCreateBookResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// สร้างเอกสาร - Original (ตาม Postman Collection /api/books/create)
        /// </summary>
        /// <param name="request">ข้อมูลการสร้างเอกสาร (รวม user_ad, book, bookAttach, bookFile, bookHistory, bookReferences, bookReferenceAttach)</param>
        /// <returns>ข้อมูลเอกสารที่สร้างขึ้น</returns>
        [HttpPost("create/original")]
        [SwaggerOperation(
            Summary = "สร้างเอกสาร - Original (ตาม Postman Collection)",
            Description = "สร้างเอกสารใหม่ตามรูปแบบมาตรฐานของ eSaraban API - เส้น /api/books/create ต้นฉบับ",
            Tags = new[] { "Books - Create" }
        )]
        [SwaggerResponse(200, "Success - เอกสารถูกสร้างสำเร็จ", typeof(ApiResponse<object>))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(ApiResponse<object>))]
        [SwaggerResponse(404, "Not Found - ไม่พบข้อมูล", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(ApiResponse<object>))]
        public async Task<IActionResult> CreateBookOriginal(
            [FromBody, SwaggerRequestBody("ข้อมูลการสร้างเอกสาร (ตาม eSaraban API Spec - Original)", Required = true)] ESarabanCreateBookRequest request)
        {
            try
            {
                _logger.LogInformation($"CreateBookOriginal (/api/books/create) called by user: {request?.user_ad}");

                // Validate input
                if (request == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Request body is required",
                        "REQUEST_BODY_REQUIRED"
                    ));
                }

                // Apply user_ad default if not provided
                if (string.IsNullOrEmpty(request.user_ad))
                {
                    request.user_ad = _bookDefaults.UserAd ?? string.Empty;
                }

                if (request.book == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book data is required",
                        "BOOK_DATA_REQUIRED"
                    ));
                }

                // Validate required book fields
                if (string.IsNullOrEmpty(request.book.book_subject))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_subject is required",
                        "BOOK_SUBJECT_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(request.book.registrationbook_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "registrationbook_id is required",
                        "REGISTRATIONBOOK_ID_REQUIRED"
                    ));
                }

                // Apply default values from configuration
                ApplyDefaults(request, "original");

                // TODO: เชื่อมต่อกับ Oracle Database เพื่อสร้างเอกสาร
                // TODO: ตรวจสอบสิทธิ์ของผู้ใช้
                // TODO: Validate ข้อมูล registrationbook, booktype, format, speed, secret
                // TODO: บันทึกข้อมูลลง S_API_ESARABAN_LOG
                // TODO: บันทึก bookAttach, bookFile, bookHistory, bookReferences, bookReferenceAttach
                // TODO: เชื่อมต่อ Alfresco สำหรับจัดเก็บไฟล์

                // Call eSaraban External API to create book
                _logger.LogInformation("Calling eSaraban API to create book (Original - Full Format)...");
                var response = await _esarabanApi.CreateBookAsync(request);

                if (response == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new ESarabanCreateBookResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Book created successfully with ID: {response.BookId} by {request.user_ad} (from eSaraban API)");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in CreateBookOriginal: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new ESarabanCreateBookResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// สร้างรหัสเอกสาร (Generate Code) สำหรับ Book
        /// </summary>
        /// <param name="user_ad">Active Directory username (e.g., EXAT\ECMUSR07)</param>
        /// <param name="book_id">Book ID (GUID format)</param>
        /// <returns>รหัสเอกสารที่สร้างขึ้น</returns>
        [HttpGet("generate-code")]
        [SwaggerOperation(
            Summary = "สร้างรหัสเอกสาร (Generate Code)",
            Description = "สร้างรหัสเอกสารสำหรับ Book ที่มีอยู่แล้ว ตามกฎการตั้งชื่อของระบบ",
            Tags = new[] { "Books - Operations" }
        )]
        [SwaggerResponse(200, "Success - รหัสเอกสารถูกสร้างสำเร็จ", typeof(ApiResponse<GenerateCodeResponse>))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(ApiResponse<object>))]
        [SwaggerResponse(404, "Not Found - ไม่พบ Book", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(ApiResponse<object>))]
        public async Task<IActionResult> GenerateCode(
            [FromQuery, SwaggerParameter("Active Directory username (e.g., EXAT\\ECMUSR07)", Required = true)] string user_ad,
            [FromQuery, SwaggerParameter("Book ID (GUID format)", Required = true)] string book_id)
        {
            try
            {
                _logger.LogInformation($"GenerateCode called by user: {user_ad} for book_id: {book_id}");

                // Validate input
                if (string.IsNullOrEmpty(user_ad))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "user_ad is required",
                        "USER_AD_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(book_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_id is required",
                        "BOOK_ID_REQUIRED"
                    ));
                }

                // Validate GUID format
                if (!Guid.TryParse(book_id, out _))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_id must be a valid GUID",
                        "INVALID_BOOK_ID"
                    ));
                }

                // Call eSaraban External API to generate book_code and to_date
                _logger.LogInformation("Calling eSaraban API to generate book_code...");
                var apiResponse = await _esarabanApi.GenerateCodeAsync(user_ad, book_id);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban GenerateCode API");

                    var errorResponse = new GenerateCodeResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Code generated successfully from eSaraban API: {apiResponse.BookCode}");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GenerateCode: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new GenerateCodeResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// โอนย้าย Book ระหว่างองค์กร
        /// </summary>
        /// <param name="user_ad">Active Directory username (e.g., EXAT\ECMUSR07)</param>
        /// <param name="book_id">Book ID ที่ต้องการโอนย้าย</param>
        /// <param name="tranfer_id">Transfer ID (optional)</param>
        /// <param name="original_org_code">รหัสองค์กรต้นทาง</param>
        /// <param name="destination_org_code">รหัสองค์กรปลายทาง</param>
        /// <param name="request">ข้อมูลการโอนย้าย</param>
        /// <returns>ผลการโอนย้าย Book</returns>
        [HttpPost("transfer")]
        [SwaggerOperation(
            Summary = "โอนย้าย Book ระหว่างองค์กร",
            Description = "โอนย้าย Book จากองค์กรหนึ่งไปยังอีกองค์กรหนึ่ง พร้อมบันทึกเหตุผลและรายละเอียด\n\n**🌐 Real eSaraban API Integration** (v1.4)\n\n**Response Format**: Raw Response (ApiResponse wrapper)\n\nData from eSaraban External API:\n- ✅ `status` - from eSaraban API\n- ✅ `statusCode` - from eSaraban API\n- ✅ `message` - from eSaraban API\n\nResponse structure:\n```json\n{\n  \"success\": true,\n  \"message\": \"Book transferred successfully\",\n  \"data\": {\n    \"status\": \"S\",\n    \"statusCode\": \"200\",\n    \"message\": \"Success: sent book transfer.\",\n    \"book_id\": \"...\",\n    \"transfer_id\": \"...\"\n  }\n}\n```",
            Tags = new[] { "Books - Operations" }
        )]
        [SwaggerResponse(200, "Success - โอนย้ายสำเร็จ (with ApiResponse wrapper)", typeof(ApiResponse<TransferBookResponse>))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง", typeof(ApiResponse<object>))]
        [SwaggerResponse(404, "Not Found - ไม่พบ Book หรือองค์กร", typeof(ApiResponse<object>))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ", typeof(ApiResponse<object>))]
        public async Task<IActionResult> TransferBook(
            [FromQuery, SwaggerParameter("Active Directory username (e.g., EXAT\\ECMUSR07)", Required = true)] string user_ad,
            [FromQuery, SwaggerParameter("Book ID ที่ต้องการโอนย้าย", Required = true)] string book_id,
            [FromQuery, SwaggerParameter("รหัสองค์กรต้นทาง", Required = true)] string original_org_code,
            [FromQuery, SwaggerParameter("รหัสองค์กรปลายทาง", Required = true)] string destination_org_code,
            [FromQuery, SwaggerParameter("Transfer ID (optional, send 'null' string if not needed)", Required = false)] string? tranfer_id = "null",
            [FromBody, SwaggerRequestBody("ข้อมูลการโอนย้าย (optional - can be empty)", Required = false)] TransferBookRequest? request = null)
        {
            try
            {
                _logger.LogInformation($"TransferBook called by user: {user_ad}, book_id: {book_id}, from: {original_org_code} to: {destination_org_code}");

                // Validate input
                if (string.IsNullOrEmpty(user_ad))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "user_ad is required",
                        "USER_AD_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(book_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "book_id is required",
                        "BOOK_ID_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(original_org_code))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "original_org_code is required",
                        "ORIGINAL_ORG_CODE_REQUIRED"
                    ));
                }

                if (string.IsNullOrEmpty(destination_org_code))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "destination_org_code is required",
                        "DESTINATION_ORG_CODE_REQUIRED"
                    ));
                }

                // If tranfer_id is null or empty, use "null" string (eSaraban API accepts "null" as valid value)
                if (string.IsNullOrEmpty(tranfer_id))
                {
                    tranfer_id = "null";
                }

                // Request body is optional - create empty request if not provided
                if (request == null)
                {
                    request = new TransferBookRequest();
                }

                // Call real eSaraban External API
                _logger.LogInformation($"Calling eSaraban API to transfer book: {book_id} from {original_org_code} to {destination_org_code}, tranfer_id: {tranfer_id}");
                var apiResponse = await _esarabanApi.TransferBookAsync(
                    user_ad,
                    book_id,
                    tranfer_id ?? "null",
                    original_org_code,
                    destination_org_code,
                    request);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban TransferBook API");
                    return StatusCode(503, ApiResponse<TransferBookResponse>.ErrorResponse(
                        "Failed to connect to eSaraban API. Please try again later.",
                        "ESARABAN_API_UNAVAILABLE"
                    ));
                }

                // Update response with query parameters (since API might not return all fields)
                // Treat empty strings as null for fallback logic (K2 compatibility)
                apiResponse.BookId = string.IsNullOrEmpty(apiResponse.BookId) ? book_id : apiResponse.BookId;
                apiResponse.TransferId = string.IsNullOrEmpty(apiResponse.TransferId) ? (tranfer_id ?? "null") : apiResponse.TransferId;
                apiResponse.OriginalOrgCode = string.IsNullOrEmpty(apiResponse.OriginalOrgCode) ? original_org_code : apiResponse.OriginalOrgCode;
                apiResponse.DestinationOrgCode = string.IsNullOrEmpty(apiResponse.DestinationOrgCode) ? destination_org_code : apiResponse.DestinationOrgCode;
                apiResponse.TransferredBy = string.IsNullOrEmpty(apiResponse.TransferredBy) ? user_ad : apiResponse.TransferredBy;
                apiResponse.TransferReason = string.IsNullOrEmpty(apiResponse.TransferReason) ? (request?.TransferReason ?? "") : apiResponse.TransferReason;
                apiResponse.TransferNote = string.IsNullOrEmpty(apiResponse.TransferNote) ? request?.TransferNote : apiResponse.TransferNote;

                _logger.LogInformation($"Book transferred successfully (from eSaraban API): Status={apiResponse.Status}, Message={apiResponse.Message}");

                // Raw Response: Return with ApiResponse wrapper
                return Ok(ApiResponse<TransferBookResponse>.SuccessResponse(
                    apiResponse,
                    "Book transferred successfully"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in TransferBook: {ex.Message}");

                // Raw Response: Return error with ApiResponse wrapper
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    $"Internal server error: {ex.Message}",
                    "TRANSFER_ERROR"
                ));
            }
        }

        /// <summary>
        /// โอนย้าย Book ระหว่างองค์กร (K2 Compatible - Flat JSON)
        /// </summary>
        /// <param name="user_ad">Active Directory username (e.g., EXAT\ECMUSR07)</param>
        /// <param name="book_id">Book ID ที่ต้องการโอนย้าย</param>
        /// <param name="tranfer_id">Transfer ID (optional)</param>
        /// <param name="original_org_code">รหัสองค์กรต้นทาง</param>
        /// <param name="destination_org_code">รหัสองค์กรปลายทาง</param>
        /// <param name="request">ข้อมูลการโอนย้าย</param>
        /// <returns>ผลการโอนย้าย Book (Flat JSON)</returns>
        [HttpPost("transfer/simple")]
        [SwaggerOperation(
            Summary = "โอนย้าย Book (K2 Compatible - Flat JSON)",
            Description = "โอนย้าย Book จากองค์กรหนึ่งไปยังอีกองค์กรหนึ่ง พร้อมบันทึกเหตุผลและรายละเอียด\n\n**🌐 Real eSaraban API Integration**\n\n**✅ K2 Compatible**: Flat JSON Response (NO wrapper)\n\n**Valid Organization Codes** (J-prefix required):\n- `J10100` - กองจัดหาที่ดิน (Land Acquisition Division)\n- `J10000` - กองที่ดิน (Land Division) \n- `J10200` - กองบริหารที่ดิน (Land Management Division)\n\n**Invalid Codes** (will cause ORA-01403 error):\n- `0000000001`, `0000000002` (numeric format)\n\n**ตัวอย่างการใช้งาน**:\n```json\n// Request Body (Optional - can be empty {})\n{\n  \"transferReason\": \"โอนย้ายเอกสารไปยังหน่วยงานที่เกี่ยวข้อง\",\n  \"transferNote\": \"ทดสอบ K2 API\",\n  \"createBy\": \"EXAT\\\\ECMUSR07\"\n}\n\n// Response (Flat JSON)\n{\n  \"status\": \"S\",\n  \"statusCode\": \"200\",\n  \"message\": \"Success: sent book transfer.\",\n  \"book_id\": \"ABC123\",\n  \"transfer_id\": \"TRF-2025-001\",\n  \"original_org_code\": \"J10100\",\n  \"destination_org_code\": \"J10200\",\n  \"transferred_by\": \"EXAT\\\\ECMUSR07\",\n  \"transferred_date\": \"2025-11-07T18:45:00\"\n}\n```",
            Tags = new[] { "Books - Operations" }
        )]
        [SwaggerResponse(200, "Success - โอนย้ายสำเร็จ (Flat JSON - K2 Compatible)", typeof(TransferBookSimpleResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง")]
        [SwaggerResponse(404, "Not Found - ไม่พบ Book หรือองค์กร")]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ")]
        public async Task<IActionResult> TransferBookSimple(
            [FromQuery, SwaggerParameter("Active Directory username (e.g., EXAT\\ECMUSR07)", Required = true)] string user_ad,
            [FromQuery, SwaggerParameter("Book ID ที่ต้องการโอนย้าย", Required = true)] string book_id,
            [FromQuery, SwaggerParameter("รหัสองค์กรต้นทาง (J-prefix, e.g., J10100)", Required = true)] string original_org_code,
            [FromQuery, SwaggerParameter("รหัสองค์กรปลายทาง (J-prefix, e.g., J10200)", Required = true)] string destination_org_code,
            [FromQuery, SwaggerParameter("Transfer ID (optional, send 'null' string if not needed)", Required = false)] string? tranfer_id = "null",
            [FromBody, SwaggerRequestBody("ข้อมูลการโอนย้าย (optional - can be empty)", Required = false)] TransferBookRequest? request = null)
        {
            try
            {
                _logger.LogInformation($"TransferBookSimple called by user: {user_ad}, book_id: {book_id}, from: {original_org_code} to: {destination_org_code}");

                // Validate input
                if (string.IsNullOrEmpty(user_ad))
                {
                    var errorResponse = new TransferBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = "user_ad is required"
                    };
                    return BadRequest(errorResponse);
                }

                if (string.IsNullOrEmpty(book_id))
                {
                    var errorResponse = new TransferBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = "book_id is required"
                    };
                    return BadRequest(errorResponse);
                }

                if (string.IsNullOrEmpty(original_org_code))
                {
                    var errorResponse = new TransferBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = "original_org_code is required"
                    };
                    return BadRequest(errorResponse);
                }

                if (string.IsNullOrEmpty(destination_org_code))
                {
                    var errorResponse = new TransferBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = "destination_org_code is required"
                    };
                    return BadRequest(errorResponse);
                }

                // Validate organization code format (J-prefix)
                if (!original_org_code.StartsWith("J") || original_org_code.Length != 6)
                {
                    var errorResponse = new TransferBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = $"Invalid original_org_code format: '{original_org_code}'. Must be J-prefix with 5 digits (e.g., J10100)"
                    };
                    return BadRequest(errorResponse);
                }

                if (!destination_org_code.StartsWith("J") || destination_org_code.Length != 6)
                {
                    var errorResponse = new TransferBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = $"Invalid destination_org_code format: '{destination_org_code}'. Must be J-prefix with 5 digits (e.g., J10200)"
                    };
                    return BadRequest(errorResponse);
                }

                // If tranfer_id is null or empty, use "null" string (eSaraban API accepts "null" as valid value)
                if (string.IsNullOrEmpty(tranfer_id))
                {
                    tranfer_id = "null";
                }

                // Request body is optional - create empty request if not provided
                if (request == null)
                {
                    request = new TransferBookRequest
                    {
                        TransferReason = "โอนย้ายเอกสารไปยังหน่วยงานที่เกี่ยวข้อง",
                        TransferNote = "โอนย้ายผ่าน K2 REST API",
                        CreateBy = user_ad
                    };
                }
                else
                {
                    // Apply fallback values if request properties are null or empty
                    if (string.IsNullOrEmpty(request.TransferReason))
                    {
                        request.TransferReason = "โอนย้ายเอกสารไปยังหน่วยงานที่เกี่ยวข้อง";
                    }
                    if (string.IsNullOrEmpty(request.TransferNote))
                    {
                        request.TransferNote = "โอนย้ายผ่าน K2 REST API";
                    }
                    if (string.IsNullOrEmpty(request.CreateBy))
                    {
                        request.CreateBy = user_ad;
                    }
                }

                _logger.LogInformation($"Transfer request prepared: book_id={book_id}, tranfer_id={tranfer_id}, from={original_org_code} to={destination_org_code}");

                // Call eSaraban External API to transfer book
                _logger.LogInformation("Calling eSaraban API to transfer book...");
                var apiResponse = await _esarabanApi.TransferBookAsync(
                    user_ad,
                    book_id,
                    tranfer_id,
                    original_org_code,
                    destination_org_code,
                    request
                );

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban TransferBook API");

                    var errorResponse = new TransferBookSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Book transferred successfully (from eSaraban API): Status={apiResponse.Status}, Message={apiResponse.Message}");

                // K2 Compatible: Convert to flat response
                var simpleResponse = new TransferBookSimpleResponse
                {
                    Status = apiResponse.Status ?? "E",
                    StatusCode = apiResponse.StatusCode ?? "500",
                    Message = apiResponse.Message ?? "Unknown error",
                    BookId = apiResponse.BookId ?? book_id,
                    TransferId = apiResponse.TransferId ?? tranfer_id,
                    OriginalOrgCode = apiResponse.OriginalOrgCode ?? original_org_code,
                    DestinationOrgCode = apiResponse.DestinationOrgCode ?? destination_org_code,
                    TransferReason = apiResponse.TransferReason ?? request.TransferReason,
                    TransferNote = apiResponse.TransferNote ?? request.TransferNote,
                    TransferStatus = apiResponse.TransferStatus ?? "",
                    TransferredBy = apiResponse.TransferredBy ?? user_ad,
                    TransferredDate = apiResponse.TransferredDate
                };

                // Return flat JSON (K2 Compatible)
                return Ok(simpleResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in TransferBookSimple: {ex.Message}");

                // K2 Compatible: Return flat error response
                var errorResponse = new TransferBookSimpleResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// ดึงข้อมูลองค์กรปลายทาง (Final Organizations) พร้อม Alert
        /// 🌐 Real eSaraban API Integration (v1.5) - 100% Integration Achieved
        /// </summary>
        /// <param name="user_ad">Active Directory username (e.g., EXAT\ECMUSR07)</param>
        /// <param name="book_id">Book ID</param>
        /// <returns>รายการองค์กรปลายทาง</returns>
        [HttpGet("final-orgs/by-action")]
        [SwaggerOperation(
            Summary = "ดึงข้อมูลองค์กรปลายทาง (พร้อม Alert) 🌐 Real API",
            Description = "ดึงรายการองค์กรปลายทางสำหรับ Book โดยจะมีการแจ้งเตือน (Alert) ไปยังองค์กรที่เกี่ยวข้อง\n\n**🎉 v1.5 - 100% Real API Integration**\n\n**Response Format**: ✅ Raw Response (NO wrapper)\n\nResponse structure:\n```json\n{\n  \"status\": \"S\",\n  \"statusCode\": \"200\",\n  \"books\": [\n    {\n      \"running_no\": 1,\n      \"send_org_nameth\": \"กองกรรมสิทธิ์ที่ดิน\",\n      \"send_date\": \"01-NOV-25\",\n      \"receive_code\": null,\n      \"receive_date\": null,\n      \"receive_org_nameth\": \"J10000 ฝ่ายกรรมสิทธิ์ที่ดิน\",\n      \"status_nameth\": \"รอดำเนินการรับหนังสือ\",\n      \"receive_comment\": null,\n      \"book_id\": \"ABC123\"\n    }\n  ]\n}\n```",
            Tags = new[] { "Books - Query" }
        )]
        [SwaggerResponse(200, "Success - ดึงข้อมูลสำเร็จ (Raw Response)", typeof(FinalOrgsResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง")]
        [SwaggerResponse(404, "Not Found - ไม่พบข้อมูล")]
        [SwaggerResponse(503, "Service Unavailable - eSaraban API ไม่สามารถเชื่อมต่อได้")]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ")]
        public async Task<IActionResult> GetFinalOrgsByAction(
            [FromQuery, SwaggerParameter("Active Directory username (e.g., EXAT\\ECMUSR07)", Required = true)] string user_ad,
            [FromQuery, SwaggerParameter("Book ID", Required = true)] string book_id)
        {
            try
            {
                _logger.LogInformation($"GetFinalOrgsByAction called by user: {user_ad} for book_id: {book_id}");

                // Validate input
                if (string.IsNullOrEmpty(user_ad))
                {
                    return BadRequest(new
                    {
                        status = "E",
                        statusCode = "400",
                        message = "user_ad is required"
                    });
                }

                if (string.IsNullOrEmpty(book_id))
                {
                    return BadRequest(new
                    {
                        status = "E",
                        statusCode = "400",
                        message = "book_id is required"
                    });
                }

                // Call real eSaraban External API
                _logger.LogInformation("Calling eSaraban API to get final organizations (with alert)...");
                var apiResponse = await _esarabanApi.GetFinalOrgsByActionAsync(user_ad, book_id);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban GetFinalOrgsByAction API");
                    return StatusCode(503, new
                    {
                        status = "E",
                        statusCode = "503",
                        message = "Failed to connect to eSaraban API. Please try again later."
                    });
                }

                _logger.LogInformation($"Final organizations retrieved successfully from API: {apiResponse.Books?.Count ?? 0} organizations found");

                // Raw Response: Return response directly (NO ApiResponse wrapper)
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetFinalOrgsByAction: {ex.Message}");

                // Raw Response: Return error directly (NO ApiResponse wrapper)
                return StatusCode(500, new
                {
                    status = "E",
                    statusCode = "500",
                    message = $"Error retrieving final organizations: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// ดึงข้อมูลองค์กรปลายทาง (Final Organizations) โดยไม่มี Alert
        /// 🌐 Real eSaraban API Integration (v1.5) - 100% Integration Achieved
        /// </summary>
        /// <param name="user_ad">Active Directory username (e.g., EXAT\ECMUSR07)</param>
        /// <param name="book_id">Book ID</param>
        /// <returns>รายการองค์กรปลายทาง</returns>
        [HttpGet("final-orgs/by-action/no-alert")]
        [SwaggerOperation(
            Summary = "ดึงข้อมูลองค์กรปลายทาง (ไม่มี Alert) 🌐 Real API",
            Description = "ดึงรายการองค์กรปลายทางสำหรับ Book โดยไม่มีการแจ้งเตือน (Alert)\n\n**🎉 v1.5 - 100% Real API Integration**\n\n**Response Format**: ✅ Raw Response (NO wrapper)\n\nResponse structure:\n```json\n{\n  \"status\": \"S\",\n  \"statusCode\": \"200\",\n  \"books\": [\n    {\n      \"running_no\": 1,\n      \"send_org_nameth\": \"กองกรรมสิทธิ์ที่ดิน\",\n      \"send_date\": \"01-NOV-25\",\n      \"receive_code\": null,\n      \"receive_date\": null,\n      \"receive_org_nameth\": \"J10000 ฝ่ายกรรมสิทธิ์ที่ดิน\",\n      \"status_nameth\": \"รอดำเนินการรับหนังสือ\",\n      \"receive_comment\": null,\n      \"book_id\": \"ABC123\"\n    }\n  ]\n}\n```",
            Tags = new[] { "Books - Query" }
        )]
        [SwaggerResponse(200, "Success - ดึงข้อมูลสำเร็จ (Raw Response)", typeof(FinalOrgsResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง")]
        [SwaggerResponse(404, "Not Found - ไม่พบข้อมูล")]
        [SwaggerResponse(503, "Service Unavailable - eSaraban API ไม่สามารถเชื่อมต่อได้")]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ")]
        public async Task<IActionResult> GetFinalOrgsByActionNoAlert(
            [FromQuery, SwaggerParameter("Active Directory username (e.g., EXAT\\ECMUSR07)", Required = true)] string user_ad,
            [FromQuery, SwaggerParameter("Book ID", Required = true)] string book_id)
        {
            try
            {
                _logger.LogInformation($"GetFinalOrgsByActionNoAlert called by user: {user_ad} for book_id: {book_id}");

                // Validate input
                if (string.IsNullOrEmpty(user_ad))
                {
                    return BadRequest(new
                    {
                        status = "E",
                        statusCode = "400",
                        message = "user_ad is required"
                    });
                }

                if (string.IsNullOrEmpty(book_id))
                {
                    return BadRequest(new
                    {
                        status = "E",
                        statusCode = "400",
                        message = "book_id is required"
                    });
                }

                // Call real eSaraban External API (NO Alert)
                _logger.LogInformation("Calling eSaraban API to get final organizations (no alert)...");
                var apiResponse = await _esarabanApi.GetFinalOrgsByActionNoAlertAsync(user_ad, book_id);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban GetFinalOrgsByActionNoAlert API");
                    return StatusCode(503, new
                    {
                        status = "E",
                        statusCode = "503",
                        message = "Failed to connect to eSaraban API. Please try again later."
                    });
                }

                _logger.LogInformation($"Final organizations retrieved successfully from API (no alert): {apiResponse.Books?.Count ?? 0} organizations found");

                // Raw Response: Return response directly (NO ApiResponse wrapper)
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetFinalOrgsByActionNoAlert: {ex.Message}");

                // Raw Response: Return error directly (NO ApiResponse wrapper)
                return StatusCode(500, new
                {
                    status = "E",
                    statusCode = "500",
                    message = $"Error retrieving final organizations: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// [K2 Compatible] ดึงข้อมูลองค์กรปลายทาง (First Record Only - Flat JSON)
        /// 🎯 K2 SmartObject Ready - No Arrays, Flat Structure
        /// </summary>
        /// <param name="user_ad">Active Directory username (e.g., EXAT\ECMUSR07)</param>
        /// <param name="book_id">Book ID</param>
        /// <returns>ข้อมูลองค์กรแรก (Flat JSON)</returns>
        [HttpGet("final-orgs/by-action/simple")]
        [SwaggerOperation(
            Summary = "[K2 Compatible] ดึงข้อมูลองค์กรปลายทาง (Flat JSON - First Record)",
            Description = "ดึงข้อมูลองค์กรปลายทางแบบ Flat JSON (ส่งแค่ Organization แรก)\n\n**🎯 K2 SmartObject Compatible**\n\n- ✅ Flat JSON (NO arrays)\n- ✅ First record only\n- ✅ K2 can read all fields directly\n- ✅ With Alert notification\n\nResponse structure:\n```json\n{\n  \"status\": \"S\",\n  \"statusCode\": \"200\",\n  \"message\": \"\",\n  \"book_id\": \"ABC123\",\n  \"total_organizations\": 2,\n  \"running_no\": 1,\n  \"send_org_nameth\": \"กองจัดหาที่ดิน\",\n  \"send_date\": \"07-NOV-25\",\n  \"receive_code\": null,\n  \"receive_date\": null,\n  \"receive_org_nameth\": \"J10000 กองที่ดิน\",\n  \"status_nameth\": \"รอดำเนินการรับหนังสือ\",\n  \"receive_comment\": null,\n  \"query_type\": \"with_alert\",\n  \"queried_by\": \"EXAT\\\\ECMUSR07\",\n  \"queried_date\": \"2025-11-07T19:30:00Z\"\n}\n```\n\n⚠️ Note: ถ้ามี Organization หลายตัว จะส่งแค่ตัวแรก (First Record)",
            Tags = new[] { "Books - Query - K2 Compatible" }
        )]
        [SwaggerResponse(200, "Success - K2 Compatible Flat JSON", typeof(GetFinalOrgsSimpleResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง")]
        [SwaggerResponse(404, "Not Found - ไม่พบข้อมูล")]
        [SwaggerResponse(503, "Service Unavailable - eSaraban API ไม่สามารถเชื่อมต่อได้")]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ")]
        public async Task<IActionResult> GetFinalOrgsByActionSimple(
            [FromQuery, SwaggerParameter("Active Directory username (e.g., EXAT\\ECMUSR07)", Required = true)] string user_ad,
            [FromQuery, SwaggerParameter("Book ID", Required = true)] string book_id)
        {
            try
            {
                _logger.LogInformation($"GetFinalOrgsByActionSimple (K2) called by user: {user_ad} for book_id: {book_id}");

                // Validate input
                if (string.IsNullOrEmpty(user_ad))
                {
                    return BadRequest(new GetFinalOrgsSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = "user_ad is required",
                        BookId = book_id ?? string.Empty
                    });
                }

                if (string.IsNullOrEmpty(book_id))
                {
                    return BadRequest(new GetFinalOrgsSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = "book_id is required",
                        QueriedBy = user_ad
                    });
                }

                // Call real eSaraban External API (with alert)
                _logger.LogInformation("Calling eSaraban API to get final organizations (K2 Simple - with alert)...");
                var apiResponse = await _esarabanApi.GetFinalOrgsByActionAsync(user_ad, book_id);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban GetFinalOrgsByAction API");
                    return StatusCode(503, new GetFinalOrgsSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later.",
                        BookId = book_id,
                        QueriedBy = user_ad,
                        QueriedDate = DateTime.Now
                    });
                }

                // Convert to K2 Flat response (First record only)
                var simpleResponse = new GetFinalOrgsSimpleResponse
                {
                    Status = apiResponse.Status,
                    StatusCode = apiResponse.StatusCode,
                    Message = apiResponse.Books?.Count > 0 ? $"Found {apiResponse.Books.Count} organization(s), returning first record only" : "No organizations found",
                    BookId = book_id,
                    TotalOrganizations = apiResponse.Books?.Count ?? 0,
                    QueryType = "with_alert",
                    QueriedBy = user_ad,
                    QueriedDate = DateTime.Now
                };

                // Fill first organization data (if exists)
                if (apiResponse.Books != null && apiResponse.Books.Count > 0)
                {
                    var firstOrg = apiResponse.Books[0];
                    simpleResponse.RunningNo = firstOrg.RunningNo;
                    simpleResponse.SendOrgNameTh = firstOrg.SendOrgNameTh ?? string.Empty;
                    simpleResponse.SendDate = firstOrg.SendDate ?? string.Empty;
                    simpleResponse.ReceiveCode = firstOrg.ReceiveCode;
                    simpleResponse.ReceiveDate = firstOrg.ReceiveDate;
                    simpleResponse.ReceiveOrgNameTh = firstOrg.ReceiveOrgNameTh ?? string.Empty;
                    simpleResponse.StatusNameTh = firstOrg.StatusNameTh ?? string.Empty;
                    simpleResponse.ReceiveComment = firstOrg.ReceiveComment;
                }

                _logger.LogInformation($"K2 Simple response created: {simpleResponse.TotalOrganizations} organization(s) found");

                // Return K2 Compatible Flat JSON
                return Ok(simpleResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetFinalOrgsByActionSimple: {ex.Message}");

                return StatusCode(500, new GetFinalOrgsSimpleResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Error retrieving final organizations: {ex.Message}",
                    BookId = book_id ?? string.Empty,
                    QueriedBy = user_ad ?? "Unknown",
                    QueriedDate = DateTime.Now
                });
            }
        }

        /// <summary>
        /// [K2 Compatible] ดึงข้อมูลองค์กรปลายทาง (First Record Only - Flat JSON, No Alert)
        /// 🎯 K2 SmartObject Ready - No Arrays, Flat Structure
        /// </summary>
        /// <param name="user_ad">Active Directory username (e.g., EXAT\ECMUSR07)</param>
        /// <param name="book_id">Book ID</param>
        /// <returns>ข้อมูลองค์กรแรก (Flat JSON, No Alert)</returns>
        [HttpGet("final-orgs/by-action/no-alert/simple")]
        [SwaggerOperation(
            Summary = "[K2 Compatible] ดึงข้อมูลองค์กรปลายทาง (Flat JSON - First Record, No Alert)",
            Description = "ดึงข้อมูลองค์กรปลายทางแบบ Flat JSON (ส่งแค่ Organization แรก, ไม่มี Alert)\n\n**🎯 K2 SmartObject Compatible**\n\n- ✅ Flat JSON (NO arrays)\n- ✅ First record only\n- ✅ K2 can read all fields directly\n- ❌ No Alert notification\n\nResponse structure: (Same as simple with alert)\n\n⚠️ Note: ถ้ามี Organization หลายตัว จะส่งแค่ตัวแรก (First Record)",
            Tags = new[] { "Books - Query - K2 Compatible" }
        )]
        [SwaggerResponse(200, "Success - K2 Compatible Flat JSON", typeof(GetFinalOrgsSimpleResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ถูกต้อง")]
        [SwaggerResponse(404, "Not Found - ไม่พบข้อมูล")]
        [SwaggerResponse(503, "Service Unavailable - eSaraban API ไม่สามารถเชื่อมต่อได้")]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาดภายในระบบ")]
        public async Task<IActionResult> GetFinalOrgsByActionNoAlertSimple(
            [FromQuery, SwaggerParameter("Active Directory username (e.g., EXAT\\ECMUSR07)", Required = true)] string user_ad,
            [FromQuery, SwaggerParameter("Book ID", Required = true)] string book_id)
        {
            try
            {
                _logger.LogInformation($"GetFinalOrgsByActionNoAlertSimple (K2) called by user: {user_ad} for book_id: {book_id}");

                // Validate input
                if (string.IsNullOrEmpty(user_ad))
                {
                    return BadRequest(new GetFinalOrgsSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = "user_ad is required",
                        BookId = book_id ?? string.Empty
                    });
                }

                if (string.IsNullOrEmpty(book_id))
                {
                    return BadRequest(new GetFinalOrgsSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "400",
                        Message = "book_id is required",
                        QueriedBy = user_ad
                    });
                }

                // Call real eSaraban External API (no alert)
                _logger.LogInformation("Calling eSaraban API to get final organizations (K2 Simple - no alert)...");
                var apiResponse = await _esarabanApi.GetFinalOrgsByActionNoAlertAsync(user_ad, book_id);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban GetFinalOrgsByActionNoAlert API");
                    return StatusCode(503, new GetFinalOrgsSimpleResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API. Please try again later.",
                        BookId = book_id,
                        QueriedBy = user_ad,
                        QueriedDate = DateTime.Now
                    });
                }

                // Convert to K2 Flat response (First record only)
                var simpleResponse = new GetFinalOrgsSimpleResponse
                {
                    Status = apiResponse.Status,
                    StatusCode = apiResponse.StatusCode,
                    Message = apiResponse.Books?.Count > 0 ? $"Found {apiResponse.Books.Count} organization(s), returning first record only" : "No organizations found",
                    BookId = book_id,
                    TotalOrganizations = apiResponse.Books?.Count ?? 0,
                    QueryType = "no_alert",
                    QueriedBy = user_ad,
                    QueriedDate = DateTime.Now
                };

                // Fill first organization data (if exists)
                if (apiResponse.Books != null && apiResponse.Books.Count > 0)
                {
                    var firstOrg = apiResponse.Books[0];
                    simpleResponse.RunningNo = firstOrg.RunningNo;
                    simpleResponse.SendOrgNameTh = firstOrg.SendOrgNameTh ?? string.Empty;
                    simpleResponse.SendDate = firstOrg.SendDate ?? string.Empty;
                    simpleResponse.ReceiveCode = firstOrg.ReceiveCode;
                    simpleResponse.ReceiveDate = firstOrg.ReceiveDate;
                    simpleResponse.ReceiveOrgNameTh = firstOrg.ReceiveOrgNameTh ?? string.Empty;
                    simpleResponse.StatusNameTh = firstOrg.StatusNameTh ?? string.Empty;
                    simpleResponse.ReceiveComment = firstOrg.ReceiveComment;
                }

                _logger.LogInformation($"K2 Simple response created (no alert): {simpleResponse.TotalOrganizations} organization(s) found");

                // Return K2 Compatible Flat JSON
                return Ok(simpleResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetFinalOrgsByActionNoAlertSimple: {ex.Message}");

                return StatusCode(500, new GetFinalOrgsSimpleResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Error retrieving final organizations: {ex.Message}",
                    BookId = book_id ?? string.Empty,
                    QueriedBy = user_ad ?? "Unknown",
                    QueriedDate = DateTime.Now
                });
            }
        }

        // ============================================================================
        // Combined Workflow APIs - Create + Generate-Code + Transfer
        // ============================================================================

        /// <summary>
        /// Workflow แบบครบวงจร: สร้างเอกสาร (Approved) → สร้างรหัส → โอนย้าย
        /// </summary>
        /// <remarks>
        /// ตัวอย่าง Response (Flat, K2-compatible):
        ///
        /// ```json
        /// {
        ///   "status": "S",
        ///   "statusCode": "200",
        ///   "message": "Success: workflow completed (create → generate → transfer).",
        ///   "book_id": "0CEB9F7B3BF144E097CD6871FE177D1F",
        ///   "file_count": 0,
        ///   "attach_count": 0,
        ///   "create_message": "เอกสารถูกสร้างสำเร็จ (กรณีอนุมัติ/เข้าสู่หลักเกณ์)",
        ///   "generated_code": "กกท 14/12",
        ///   "to_date": "04/11/2568",
        ///   "code_type": "DOCUMENT",
        ///   "generated_date": "2025-11-07T10:30:00Z",
        ///   "generate_message": "รหัสเอกสารถูกสร้างสำเร็จ",
        ///   "transfer_id": "b23468c1-dfc7-47d9-bd26-6faf96e0a82e",
        ///   "original_org_code": "J10100",
        ///   "destination_org_code": "J10000",
        ///   "transfer_status": "COMPLETED",
        ///   "transferred_date": "2025-11-07T10:31:00Z",
        ///   "transfer_message": "โอนย้าย Book สำเร็จ",
        ///   "workflow_type": "APPROVED",
        ///   "executed_by": "EXAT\\\\ECMUSR07",
        ///   "workflow_completed": "2025-11-07T10:31:05Z",
        ///   "overall_message": "Workflow สำเร็จ: สร้างเอกสาร → สร้างรหัส → โอนย้าย (Book: กกท 14/12, Transfer: b23468c1-dfc7-47d9-bd26-6faf96e0a82e)"
        /// }
        /// ```
        /// </remarks>
        [HttpPost("workflow/approved")]
        [SwaggerOperation(
            Summary = "Workflow: สร้างเอกสาร (Approved) + Generate Code + Transfer",
            Description = "API แบบครบวงจร ทำงาน 3 ขั้นตอน: 1) สร้างเอกสาร (กรณีอนุมัติ) 2) สร้างรหัสเอกสาร 3) โอนย้ายเอกสาร - ทั้งหมดในคำขอเดียว",
            Tags = new[] { "Books - Workflow (Combined)" }
        )]
        // K2 Compatible: Flat response model (no ApiResponse wrapper)
        [SwaggerResponse(200, "Success - Workflow สำเร็จทั้ง 3 ขั้นตอน", typeof(CreateGenerateTransferResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ครบถ้วน", typeof(CreateGenerateTransferResponse))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาด", typeof(CreateGenerateTransferResponse))]
        public async Task<IActionResult> WorkflowApproved(
            [FromBody, SwaggerRequestBody("Request Body แบบเดียวกับ /api/books/create/approved/simple (ส่งเฉพาะฟิลด์ของ Simple Create)", Required = true)] CreateGenerateTransferApprovedRequest request,
            [FromQuery, SwaggerParameter("รหัสองค์กรต้นทาง (ถ้าไม่ส่งใน Body สามารถส่งผ่าน Query ได้)")] string? original_org_code,
            [FromQuery, SwaggerParameter("รหัสองค์กรปลายทาง (ถ้าไม่ส่งใน Body สามารถส่งผ่าน Query ได้)")] string? destination_org_code,
            [FromQuery, SwaggerParameter("เหตุผลในการโอนย้าย (optional)")] string? transfer_reason = null,
            [FromQuery, SwaggerParameter("หมายเหตุเพิ่มเติม (optional)")] string? transfer_note = null,
            [FromQuery, SwaggerParameter("Tranfer ID (optional) - ระบุเพื่อ override ค่าที่ระบบจะสร้างให้อัตโนมัติ (สะกดตามสเปค tranfer_id)")] string? tranfer_id = null)
        {
            try
            {
                _logger.LogInformation($"WorkflowApproved called by user: {request.user_ad}");

                // ========== Validation ==========
                if (string.IsNullOrWhiteSpace(request.user_ad) ||
                    string.IsNullOrWhiteSpace(request.book_subject) ||
                    string.IsNullOrWhiteSpace(request.book_to) ||
                    string.IsNullOrWhiteSpace(request.registrationbook_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Missing required fields: user_ad, book_subject, book_to, registrationbook_id are required",
                        "MISSING_REQUIRED_FIELDS"
                    ));
                }
                // Allow transfer org codes via Body or Query to keep body identical to simple create endpoints
                request.original_org_code = string.IsNullOrWhiteSpace(request.original_org_code) ? original_org_code ?? string.Empty : request.original_org_code;
                request.destination_org_code = string.IsNullOrWhiteSpace(request.destination_org_code) ? destination_org_code ?? string.Empty : request.destination_org_code;
                request.transfer_reason ??= transfer_reason;
                request.transfer_note ??= transfer_note;
                // Apply defaults if still missing
                if (string.IsNullOrWhiteSpace(request.original_org_code))
                    request.original_org_code = _bookDefaults.Transfer.DefaultOriginalOrgCode ?? "J10100";
                if (string.IsNullOrWhiteSpace(request.destination_org_code))
                    request.destination_org_code = _bookDefaults.Transfer.DefaultDestinationOrgCode ?? "J10000";

                // ========== Step 1: Create Book ==========
                _logger.LogInformation("Step 1: Creating book (Approved)...");

                // Build full request
                var fullRequest = new ESarabanCreateBookRequest
                {
                    user_ad = request.user_ad,
                    book = new BookData
                    {
                        book_subject = request.book_subject,
                        book_to = request.book_to,
                        registrationbook_id = request.registrationbook_id,

                        // Registration Book Details (7 fields)
                        registrationbook_nameth = request.registrationbook_nameth,
                        registrationbook_nameen = request.registrationbook_nameen,
                        registrationbook_ogr_id = request.registrationbook_ogr_id,
                        registrationbook_org_code = request.registrationbook_org_code,
                        registrationbook_org_nameth = request.registrationbook_org_nameth,
                        registrationbook_org_nameen = request.registrationbook_org_nameen,
                        registrationbook_org_shtname = request.registrationbook_org_shtname,

                        // Parent Organization Details
                        parent_bookid = request.parent_bookid,
                        parent_orgid = request.parent_orgid,
                        parent_orgcode = request.parent_orgcode,
                        parent_positioncode = request.parent_positioncode,
                        parent_positionname = request.parent_positionname
                    },
                    bookFile = request.bookFile,
                    bookAttach = request.bookAttach
                };

                // Apply defaults
                ApplyDefaults(fullRequest, "approved");

                // ========== Step 1: Create Book (Call eSaraban API) ==========
                _logger.LogInformation("Step 1: Creating book via eSaraban API...");
                var createApiResponse = await _esarabanApi.CreateBookAsync(fullRequest);

                if (createApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Create Book). Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                string bookId = createApiResponse.BookId;
                int fileCount = createApiResponse.FileCount;
                int attachCount = createApiResponse.AttachCount;

                _logger.LogInformation($"Step 1 completed: book_id={bookId} (from eSaraban API)");

                // ========== Step 2: Generate Code (Call eSaraban API) ==========
                _logger.LogInformation("Step 2: Generating document code via eSaraban API...");

                var generateApiResponse = await _esarabanApi.GenerateCodeAsync(request.user_ad, bookId);

                if (generateApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban GenerateCode API");

                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Generate Code). Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                string generatedCode = generateApiResponse.BookCode;
                string toDate = generateApiResponse.ToDate;

                _logger.LogInformation($"Step 2 completed: generated_code={generatedCode}, to_date={toDate} (from eSaraban API)");

                // ========== Step 3: Transfer Book (Call eSaraban API) ==========
                _logger.LogInformation("Step 3: Transferring book via eSaraban API...");

                string transferId = string.IsNullOrWhiteSpace(tranfer_id) ? Guid.NewGuid().ToString() : tranfer_id;

                // Create transfer request
                var transferRequest = new TransferBookRequest
                {
                    TransferReason = request.transfer_reason ?? "Workflow Transfer",
                    TransferNote = request.transfer_note
                };

                var transferApiResponse = await _esarabanApi.TransferBookAsync(
                    request.user_ad,
                    bookId,
                    transferId,
                    request.original_org_code,
                    request.destination_org_code,
                    transferRequest
                );

                if (transferApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban TransferBook API");
                    
                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Transfer Book). Please try again later.",
                        BookId = bookId,
                        GeneratedCode = generatedCode,
                        ToDate = toDate
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Step 3 completed: transfer_id={transferId}");

                // ========== Build Response (K2 Compatible) ==========
                var response = new CreateGenerateTransferResponse
                {
                    Status = "S",
                    StatusCode = "200",
                    Message = "Success: workflow completed (create → generate → transfer).",

                    // Step 1: Create
                    BookId = bookId,
                    FileCount = fileCount,
                    AttachCount = attachCount,
                    CreateMessage = "เอกสารถูกสร้างสำเร็จ (กรณีอนุมัติ/เข้าสู่หลักเกณ์)",

                    // Step 2: Generate Code
                    GeneratedCode = generatedCode,
                    ToDate = toDate,
                    CodeType = "DOCUMENT",
                    GeneratedDate = DateTime.Now,
                    GenerateMessage = "รหัสเอกสารถูกสร้างสำเร็จ",

                    // Step 3: Transfer
                    TransferId = transferId,
                    OriginalOrgCode = request.original_org_code,
                    DestinationOrgCode = request.destination_org_code,
                    TransferStatus = "COMPLETED",
                    TransferredDate = DateTime.Now,
                    TransferMessage = "โอนย้าย Book สำเร็จ",

                    // Overall
                    WorkflowType = "APPROVED",
                    ExecutedBy = request.user_ad,
                    WorkflowCompleted = DateTime.Now,
                    OverallMessage = $"Workflow สำเร็จ: สร้างเอกสาร → สร้างรหัส → โอนย้าย (Book: {generatedCode}, Transfer: {transferId})"
                };

                _logger.LogInformation($"WorkflowApproved completed successfully: {response.GeneratedCode}");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(new { response.Status
                              , response.StatusCode
                              , response.Message
                              , response.BookId
                              , response.FileCount
                              , response.AttachCount
                              , response.CreateMessage
                              , response.GeneratedCode
                              , response.ToDate
                              , response.CodeType
                              , response.GeneratedDate
                              , response.GenerateMessage
                              , response.TransferId
                              , response.OriginalOrgCode
                              , response.DestinationOrgCode
                              , response.TransferStatus
                              , response.TransferredDate
                              , response.TransferMessage
                              , response.WorkflowType
                              ,response.ExecutedBy
                              , response.WorkflowCompleted
                              , response.OverallMessage                
                             }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in WorkflowApproved: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new CreateGenerateTransferResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Workflow แบบครบวงจร: สร้างเอกสาร (Non-Compliant) → สร้างรหัส → โอนย้าย
        /// </summary>
        /// <remarks>
        /// ตัวอย่าง Response (Flat, K2-compatible):
        ///
        /// ```json
        /// {
        ///   "status": "S",
        ///   "statusCode": "200",
        ///   "message": "Success: workflow completed (create → generate → transfer).",
        ///   "workflow_type": "NON-COMPLIANT",
        ///   "book_id": "...",
        ///   "generated_code": "...",
        ///   "to_date": "...",
        ///   "transfer_id": "...",
        ///   "original_org_code": "J10100",
        ///   "destination_org_code": "J10000",
        ///   "transfer_status": "COMPLETED"
        /// }
        /// ```
        /// </remarks>
        [HttpPost("workflow/non-compliant")]
        [SwaggerOperation(
            Summary = "Workflow: สร้างเอกสาร (Non-Compliant) + Generate Code + Transfer",
            Description = "API แบบครบวงจร ทำงาน 3 ขั้นตอน: 1) สร้างเอกสาร (กรณีไม่เข้าหลักเกณ์) 2) สร้างรหัสเอกสาร 3) โอนย้ายเอกสาร - ทั้งหมดในคำขอเดียว",
            Tags = new[] { "Books - Workflow (Combined)" }
        )]
        // K2 Compatible: Flat response model (no ApiResponse wrapper)
        [SwaggerResponse(200, "Success - Workflow สำเร็จทั้ง 3 ขั้นตอน", typeof(CreateGenerateTransferResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ครบถ้วน", typeof(CreateGenerateTransferResponse))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาด", typeof(CreateGenerateTransferResponse))]
        public async Task<IActionResult> WorkflowNonCompliant(
            [FromBody, SwaggerRequestBody("Request Body แบบเดียวกับ /api/books/create/non-compliant/simple (ส่งเฉพาะฟิลด์ของ Simple Create)", Required = true)] CreateGenerateTransferNonCompliantRequest request,
            [FromQuery, SwaggerParameter("รหัสองค์กรต้นทาง (ถ้าไม่ส่งใน Body สามารถส่งผ่าน Query ได้)")] string? original_org_code,
            [FromQuery, SwaggerParameter("รหัสองค์กรปลายทาง (ถ้าไม่ส่งใน Body สามารถส่งผ่าน Query ได้)")] string? destination_org_code,
            [FromQuery, SwaggerParameter("เหตุผลในการโอนย้าย (optional)")] string? transfer_reason = null,
            [FromQuery, SwaggerParameter("หมายเหตุเพิ่มเติม (optional)")] string? transfer_note = null,
            [FromQuery, SwaggerParameter("Tranfer ID (optional) - ระบุเพื่อ override ค่าที่ระบบจะสร้างให้อัตโนมัติ (สะกดตามสเปค tranfer_id)")] string? tranfer_id = null)
        {
            try
            {
                _logger.LogInformation($"WorkflowNonCompliant called by user: {request.user_ad}");

                // ========== Validation ==========
                if (string.IsNullOrWhiteSpace(request.user_ad) ||
                    string.IsNullOrWhiteSpace(request.book_subject) ||
                    string.IsNullOrWhiteSpace(request.book_to) ||
                    string.IsNullOrWhiteSpace(request.registrationbook_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Missing required fields: user_ad, book_subject, book_to, registrationbook_id are required",
                        "MISSING_REQUIRED_FIELDS"
                    ));
                }

                // Allow transfer org codes via Body or Query to keep body identical to simple create endpoints
                request.original_org_code = string.IsNullOrWhiteSpace(request.original_org_code) ? original_org_code ?? string.Empty : request.original_org_code;
                request.destination_org_code = string.IsNullOrWhiteSpace(request.destination_org_code) ? destination_org_code ?? string.Empty : request.destination_org_code;
                request.transfer_reason ??= transfer_reason;
                request.transfer_note ??= transfer_note;
                // Apply defaults if still missing
                if (string.IsNullOrWhiteSpace(request.original_org_code))
                    request.original_org_code = _bookDefaults.Transfer.DefaultOriginalOrgCode ?? "J10100";
                if (string.IsNullOrWhiteSpace(request.destination_org_code))
                    request.destination_org_code = _bookDefaults.Transfer.DefaultDestinationOrgCode ?? "J10000";

                // ========== Step 1: Create Book ==========
                _logger.LogInformation("Step 1: Creating book (Non-Compliant)...");

                // Build full request
                var fullRequest = new ESarabanCreateBookRequest
                {
                    user_ad = request.user_ad,
                    book = new BookData
                    {
                        book_subject = request.book_subject,
                        book_to = request.book_to,
                        registrationbook_id = request.registrationbook_id,

                        // Registration Book Details (7 fields)
                        registrationbook_nameth = request.registrationbook_nameth,
                        registrationbook_nameen = request.registrationbook_nameen,
                        registrationbook_ogr_id = request.registrationbook_ogr_id,
                        registrationbook_org_code = request.registrationbook_org_code,
                        registrationbook_org_nameth = request.registrationbook_org_nameth,
                        registrationbook_org_nameen = request.registrationbook_org_nameen,
                        registrationbook_org_shtname = request.registrationbook_org_shtname,

                        // Parent Organization Details
                        parent_bookid = request.parent_bookid,
                        parent_orgid = request.parent_orgid,
                        parent_orgcode = request.parent_orgcode,
                        parent_positioncode = request.parent_positioncode,
                        parent_positionname = request.parent_positionname
                    },
                    bookFile = request.bookFile,
                    bookAttach = request.bookAttach
                };

                // Apply defaults
                ApplyDefaults(fullRequest, "non-compliant");

                // ========== Step 1: Create Book (Call eSaraban API) ==========
                _logger.LogInformation("Step 1: Creating book via eSaraban API...");
                var createApiResponse = await _esarabanApi.CreateBookAsync(fullRequest);

                if (createApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Create Book). Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                string bookId = createApiResponse.BookId;
                int fileCount = createApiResponse.FileCount;
                int attachCount = createApiResponse.AttachCount;

                _logger.LogInformation($"Step 1 completed: book_id={bookId} (from eSaraban API)");

                // ========== Step 2: Generate Code (Call eSaraban API) ==========
                _logger.LogInformation("Step 2: Generating document code via eSaraban API...");

                var generateApiResponse = await _esarabanApi.GenerateCodeAsync(request.user_ad, bookId);

                if (generateApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban GenerateCode API");

                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Generate Code). Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                string generatedCode = generateApiResponse.BookCode;
                string toDate = generateApiResponse.ToDate;

                _logger.LogInformation($"Step 2 completed: generated_code={generatedCode}, to_date={toDate} (from eSaraban API)");

                // ========== Step 3: Transfer Book (Call eSaraban API) ==========
                _logger.LogInformation("Step 3: Transferring book via eSaraban API...");

                string transferId = string.IsNullOrWhiteSpace(tranfer_id) ? Guid.NewGuid().ToString() : tranfer_id;

                // Create transfer request
                var transferRequest = new TransferBookRequest
                {
                    TransferReason = request.transfer_reason ?? "Workflow Transfer",
                    TransferNote = request.transfer_note
                };

                var transferApiResponse = await _esarabanApi.TransferBookAsync(
                    request.user_ad,
                    bookId,
                    transferId,
                    request.original_org_code,
                    request.destination_org_code,
                    transferRequest
                );

                if (transferApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban TransferBook API");
                    
                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Transfer Book). Please try again later.",
                        BookId = bookId,
                        GeneratedCode = generatedCode,
                        ToDate = toDate
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Step 3 completed: transfer_id={transferId}, status={transferApiResponse.Status} (from eSaraban API)");

                // ========== Build Response (K2 Compatible) ==========
                var response = new CreateGenerateTransferResponse
                {
                    Status = "S",
                    StatusCode = "200",
                    Message = "Success: workflow completed (create → generate → transfer).",

                    // Step 1: Create
                    BookId = bookId,
                    FileCount = fileCount,
                    AttachCount = attachCount,
                    CreateMessage = "เอกสารถูกสร้างสำเร็จ (กรณีไม่เข้าหลักเกณ์)",

                    // Step 2: Generate Code
                    GeneratedCode = generatedCode,
                    ToDate = toDate,
                    CodeType = "DOCUMENT",
                    GeneratedDate = DateTime.Now,
                    GenerateMessage = "รหัสเอกสารถูกสร้างสำเร็จ",

                    // Step 3: Transfer
                    TransferId = transferId,
                    OriginalOrgCode = request.original_org_code,
                    DestinationOrgCode = request.destination_org_code,
                    TransferStatus = "COMPLETED",
                    TransferredDate = DateTime.Now,
                    TransferMessage = "โอนย้าย Book สำเร็จ",

                    // Overall
                    WorkflowType = "NON-COMPLIANT",
                    ExecutedBy = request.user_ad,
                    WorkflowCompleted = DateTime.Now,
                    OverallMessage = $"Workflow สำเร็จ: สร้างเอกสาร → สร้างรหัส → โอนย้าย (Book: {generatedCode}, Transfer: {transferId})"
                };

                _logger.LogInformation($"WorkflowNonCompliant completed successfully: {response.GeneratedCode}");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(new { response.Status
                              , response.StatusCode
                              , response.Message
                              , response.BookId
                              , response.FileCount
                              , response.AttachCount
                              , response.CreateMessage
                              , response.GeneratedCode
                              , response.ToDate
                              , response.CodeType
                              , response.GeneratedDate
                              , response.GenerateMessage
                              , response.TransferId
                              , response.OriginalOrgCode
                              , response.DestinationOrgCode
                              , response.TransferStatus
                              , response.TransferredDate
                              , response.TransferMessage
                              , response.WorkflowType
                              , response.ExecutedBy
                              , response.WorkflowCompleted
                              , response.OverallMessage
                }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in WorkflowNonCompliant: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new CreateGenerateTransferResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        /// <summary>
        /// Workflow แบบครบวงจร: สร้างเอกสาร (Under-Construction) → สร้างรหัส → โอนย้าย
        /// </summary>
        /// <remarks>
        /// ตัวอย่าง Response (Flat, K2-compatible):
        ///
        /// ```json
        /// {
        ///   "status": "S",
        ///   "statusCode": "200",
        ///   "message": "Success: workflow completed (create → generate → transfer).",
        ///   "workflow_type": "UNDER-CONSTRUCTION",
        ///   "book_id": "...",
        ///   "generated_code": "...",
        ///   "to_date": "...",
        ///   "transfer_id": "...",
        ///   "original_org_code": "J10100",
        ///   "destination_org_code": "J10000",
        ///   "transfer_status": "COMPLETED"
        /// }
        /// ```
        /// </remarks>
        [HttpPost("workflow/under-construction")]
        [SwaggerOperation(
            Summary = "Workflow: สร้างเอกสาร (Under-Construction) + Generate Code + Transfer",
            Description = "API แบบครบวงจร ทำงาน 3 ขั้นตอน: 1) สร้างเอกสาร (กรณีอยู่ระหว่างก่อสร้าง) 2) สร้างรหัสเอกสาร 3) โอนย้ายเอกสาร - ทั้งหมดในคำขอเดียว",
            Tags = new[] { "Books - Workflow (Combined)" }
        )]
        // K2 Compatible: Flat response model (no ApiResponse wrapper)
        [SwaggerResponse(200, "Success - Workflow สำเร็จทั้ง 3 ขั้นตอน", typeof(CreateGenerateTransferResponse))]
        [SwaggerResponse(400, "Bad Request - ข้อมูลไม่ครบถ้วน", typeof(CreateGenerateTransferResponse))]
        [SwaggerResponse(500, "Server Error - เกิดข้อผิดพลาด", typeof(CreateGenerateTransferResponse))]
        public async Task<IActionResult> WorkflowUnderConstruction(
            [FromBody, SwaggerRequestBody("Request Body แบบเดียวกับ /api/books/create/under-construction/simple (ส่งเฉพาะฟิลด์ของ Simple Create)", Required = true)] CreateGenerateTransferUnderConstructionRequest request,
            [FromQuery, SwaggerParameter("รหัสองค์กรต้นทาง (ถ้าไม่ส่งใน Body สามารถส่งผ่าน Query ได้)")] string? original_org_code,
            [FromQuery, SwaggerParameter("รหัสองค์กรปลายทาง (ถ้าไม่ส่งใน Body สามารถส่งผ่าน Query ได้)")] string? destination_org_code,
            [FromQuery, SwaggerParameter("เหตุผลในการโอนย้าย (optional)")] string? transfer_reason = null,
            [FromQuery, SwaggerParameter("หมายเหตุเพิ่มเติม (optional)")] string? transfer_note = null,
            [FromQuery, SwaggerParameter("Tranfer ID (optional) - ระบุเพื่อ override ค่าที่ระบบจะสร้างให้อัตโนมัติ (สะกดตามสเปค tranfer_id)")] string? tranfer_id = null)
        {
            try
            {
                _logger.LogInformation($"WorkflowUnderConstruction called by user: {request.user_ad}");

                // ========== Validation ==========
                if (string.IsNullOrWhiteSpace(request.user_ad) ||
                    string.IsNullOrWhiteSpace(request.book_subject) ||
                    string.IsNullOrWhiteSpace(request.book_to) ||
                    string.IsNullOrWhiteSpace(request.registrationbook_id))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Missing required fields: user_ad, book_subject, book_to, registrationbook_id are required",
                        "MISSING_REQUIRED_FIELDS"
                    ));
                }

                // Allow transfer org codes via Body or Query to keep body identical to simple create endpoints
                request.original_org_code = string.IsNullOrWhiteSpace(request.original_org_code) ? original_org_code ?? string.Empty : request.original_org_code;
                request.destination_org_code = string.IsNullOrWhiteSpace(request.destination_org_code) ? destination_org_code ?? string.Empty : request.destination_org_code;
                request.transfer_reason ??= transfer_reason;
                request.transfer_note ??= transfer_note;
                // Apply defaults if still missing
                if (string.IsNullOrWhiteSpace(request.original_org_code))
                    request.original_org_code = _bookDefaults.Transfer.DefaultOriginalOrgCode ?? "J10100";
                if (string.IsNullOrWhiteSpace(request.destination_org_code))
                    request.destination_org_code = _bookDefaults.Transfer.DefaultDestinationOrgCode ?? "J10000";

                // ========== Step 1: Create Book ==========
                _logger.LogInformation("Step 1: Creating book (Under-Construction)...");

                // Build full request
                var fullRequest = new ESarabanCreateBookRequest
                {
                    user_ad = request.user_ad,
                    book = new BookData
                    {
                        book_subject = request.book_subject,
                        book_to = request.book_to,
                        registrationbook_id = request.registrationbook_id,

                        // Registration Book Details (7 fields)
                        registrationbook_nameth = request.registrationbook_nameth,
                        registrationbook_nameen = request.registrationbook_nameen,
                        registrationbook_ogr_id = request.registrationbook_ogr_id,
                        registrationbook_org_code = request.registrationbook_org_code,
                        registrationbook_org_nameth = request.registrationbook_org_nameth,
                        registrationbook_org_nameen = request.registrationbook_org_nameen,
                        registrationbook_org_shtname = request.registrationbook_org_shtname,

                        // Parent Organization Details
                        parent_bookid = request.parent_bookid,
                        parent_orgid = request.parent_orgid,
                        parent_orgcode = request.parent_orgcode,
                        parent_positioncode = request.parent_positioncode,
                        parent_positionname = request.parent_positionname
                    },
                    bookFile = request.bookFile,
                    bookAttach = request.bookAttach
                };

                // Apply defaults
                ApplyDefaults(fullRequest, "under-construction");

                // ========== Step 1: Create Book (Call eSaraban API) ==========
                _logger.LogInformation("Step 1: Creating book via eSaraban API...");
                var createApiResponse = await _esarabanApi.CreateBookAsync(fullRequest);

                if (createApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban CreateBook API");

                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Create Book). Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                string bookId = createApiResponse.BookId;
                int fileCount = createApiResponse.FileCount;
                int attachCount = createApiResponse.AttachCount;

                _logger.LogInformation($"Step 1 completed: book_id={bookId} (from eSaraban API)");

                // ========== Step 2: Generate Code (Call eSaraban API) ==========
                _logger.LogInformation("Step 2: Generating document code via eSaraban API...");

                var generateApiResponse = await _esarabanApi.GenerateCodeAsync(request.user_ad, bookId);

                if (generateApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban GenerateCode API");

                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Generate Code). Please try again later."
                    };
                    return StatusCode(503, errorResponse);
                }

                string generatedCode = generateApiResponse.BookCode;
                string toDate = generateApiResponse.ToDate;

                _logger.LogInformation($"Step 2 completed: generated_code={generatedCode}, to_date={toDate} (from eSaraban API)");

                // ========== Step 3: Transfer Book (Call eSaraban API) ==========
                _logger.LogInformation("Step 3: Transferring book via eSaraban API...");

                string transferId = string.IsNullOrWhiteSpace(tranfer_id) ? Guid.NewGuid().ToString() : tranfer_id;

                // Create transfer request
                var transferRequest = new TransferBookRequest
                {
                    TransferReason = request.transfer_reason ?? "Workflow Transfer",
                    TransferNote = request.transfer_note
                };

                var transferApiResponse = await _esarabanApi.TransferBookAsync(
                    request.user_ad,
                    bookId,
                    transferId,
                    request.original_org_code,
                    request.destination_org_code,
                    transferRequest
                );

                if (transferApiResponse == null)
                {
                    _logger.LogError("Failed to call eSaraban TransferBook API");
                    
                    var errorResponse = new CreateGenerateTransferResponse
                    {
                        Status = "E",
                        StatusCode = "503",
                        Message = "Failed to connect to eSaraban API (Transfer Book). Please try again later.",
                        BookId = bookId,
                        GeneratedCode = generatedCode,
                        ToDate = toDate
                    };
                    return StatusCode(503, errorResponse);
                }

                _logger.LogInformation($"Step 3 completed: transfer_id={transferId}, status={transferApiResponse.Status} (from eSaraban API)");

                // ========== Build Response (K2 Compatible) ==========
                var response = new CreateGenerateTransferResponse
                {
                    Status = "S",
                    StatusCode = "200",
                    Message = "Success: workflow completed (create → generate → transfer).",

                    // Step 1: Create
                    BookId = bookId,
                    FileCount = fileCount,
                    AttachCount = attachCount,
                    CreateMessage = "เอกสารถูกสร้างสำเร็จ (กรณีอยู่ระหว่างก่อสร้าง)",

                    // Step 2: Generate Code
                    GeneratedCode = generatedCode,
                    ToDate = toDate,
                    CodeType = "DOCUMENT",
                    GeneratedDate = DateTime.Now,
                    GenerateMessage = "รหัสเอกสารถูกสร้างสำเร็จ",

                    // Step 3: Transfer
                    TransferId = transferId,
                    OriginalOrgCode = request.original_org_code,
                    DestinationOrgCode = request.destination_org_code,
                    TransferStatus = "COMPLETED",
                    TransferredDate = DateTime.Now,
                    TransferMessage = "โอนย้าย Book สำเร็จ",

                    // Overall
                    WorkflowType = "UNDER-CONSTRUCTION",
                    ExecutedBy = request.user_ad,
                    WorkflowCompleted = DateTime.Now,
                    OverallMessage = $"Workflow สำเร็จ: สร้างเอกสาร → สร้างรหัส → โอนย้าย (Book: {generatedCode}, Transfer: {transferId})"
                };

                _logger.LogInformation($"WorkflowUnderConstruction completed successfully: {response.GeneratedCode}");

                // K2 Compatible: Return direct response without ApiResponse wrapper
                return Ok(new { response.Status
                              , response.StatusCode
                              , response.Message
                              , response.BookId
                              , response.FileCount
                              , response.AttachCount
                              , response.CreateMessage
                              , response.GeneratedCode
                              , response.ToDate
                              , response.CodeType
                              , response.GeneratedDate
                              , response.GenerateMessage
                              , response.TransferId
                              , response.TransferStatus
                              , response.TransferredDate
                              , response.OriginalOrgCode
                              , response.DestinationOrgCode
                              , response.TransferMessage
                              , response.WorkflowType
                              , response.ExecutedBy
                              , response.WorkflowCompleted
                              , response.OverallMessage
                }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in WorkflowUnderConstruction: {ex.Message}");

                // K2 Compatible: Return direct error format
                var errorResponse = new CreateGenerateTransferResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = $"Internal server error: {ex.Message}"
                };
                return StatusCode(500, errorResponse);
            }
        }

        #region Helper Methods

        /// <summary>
        /// Apply default values จาก configuration ไปยัง request
        /// </summary>
        private void ApplyDefaults(ESarabanCreateBookRequest request, string endpointType)
        {
            if (request == null) return;

            // Apply user_ad default if not provided
            if (string.IsNullOrEmpty(request.user_ad))
            {
                request.user_ad = _bookDefaults.UserAd ?? string.Empty;
            }

            // Get endpoint-specific config
            EndpointConfig? endpointConfig = endpointType switch
            {
                "original" => _bookDefaults.Endpoints.Original,
                "approved" => _bookDefaults.Endpoints.Approved,
                "non-compliant" => _bookDefaults.Endpoints.NonCompliant,
                "under-construction" => _bookDefaults.Endpoints.UnderConstruction,
                _ => null
            };

            // Apply BookData defaults
            if (request.book != null)
            {
                ApplyBookDataDefaults(request.book, endpointConfig);
            }

            // Apply BookFile defaults
            if (request.bookFile != null)
            {
                foreach (var file in request.bookFile)
                {
                    ApplyBookFileDefaults(file);
                }
            }

            // Apply BookHistory defaults
            if (request.bookHistory != null)
            {
                foreach (var history in request.bookHistory)
                {
                    ApplyBookHistoryDefaults(history, endpointConfig);
                }
            }

            // Apply BookAttachment defaults
            if (request.bookAttach != null)
            {
                foreach (var attach in request.bookAttach)
                {
                    ApplyBookAttachmentDefaults(attach);
                }
            }

            // Apply BookReferenceAttachment defaults
            if (request.bookReferenceAttach != null)
            {
                foreach (var refAttach in request.bookReferenceAttach)
                {
                    ApplyBookAttachmentDefaults(refAttach);
                }
            }
        }

        /// <summary>
        /// Apply default values สำหรับ BookData
        /// </summary>
        private void ApplyBookDataDefaults(BookData book, EndpointConfig? endpointConfig)
        {
            var defaults = _bookDefaults.BookData;

            // Apply Book Information defaults (string fields)
            if (string.IsNullOrEmpty(book.book_owner))
                book.book_owner = defaults.BookOwner ?? string.Empty;

            if (string.IsNullOrEmpty(book.book_subject))
                book.book_subject = defaults.BookSubject ?? string.Empty;

            if (string.IsNullOrEmpty(book.book_to))
                book.book_to = defaults.BookTo ?? string.Empty;

            book.book_originaldocumentdetail ??= defaults.BookOriginalDocumentDetail;
            book.book_searchterm ??= defaults.BookSearchTerm;
            book.book_remark ??= defaults.BookRemark;

            // Apply Registration Book defaults
            if (string.IsNullOrEmpty(book.registrationbook_id))
                book.registrationbook_id = defaults.RegistrationBookId ?? string.Empty;

            book.registrationbook_nameth ??= defaults.RegistrationBookNameTh;
            book.registrationbook_nameen ??= defaults.RegistrationBookNameEn;
            book.registrationbook_ogr_id ??= defaults.RegistrationBookOgrId;
            book.registrationbook_org_code ??= defaults.RegistrationBookOrgCode;
            book.registrationbook_org_nameth ??= defaults.RegistrationBookOrgNameTh;
            book.registrationbook_org_nameen ??= defaults.RegistrationBookOrgNameEn;
            book.registrationbook_org_shtname ??= defaults.RegistrationBookOrgShtName;

            // Apply Type and Format IDs defaults (only if value is 0 or null)
            if (book.booktype_id == 0 && defaults.BookTypeId.HasValue)
                book.booktype_id = defaults.BookTypeId.Value;

            if (book.sendtype_id == 0 && defaults.SendTypeId.HasValue)
                book.sendtype_id = defaults.SendTypeId.Value;

            if (book.format_id == 0 && defaults.FormatId.HasValue)
                book.format_id = defaults.FormatId.Value;

            if (book.subformat_id == 0 && defaults.SubFormatId.HasValue)
                book.subformat_id = defaults.SubFormatId.Value;

            if (book.speed_id == 0 && defaults.SpeedId.HasValue)
                book.speed_id = defaults.SpeedId.Value;

            if (book.secret_id == 0 && defaults.SecretId.HasValue)
                book.secret_id = defaults.SecretId.Value;

            if (book.optiondate_id == 0 && defaults.OptionDateId.HasValue)
                book.optiondate_id = defaults.OptionDateId.Value;

            if (book.optionlanguage_id == 0 && defaults.OptionLanguageId.HasValue)
                book.optionlanguage_id = defaults.OptionLanguageId.Value;

            if (book.optionno_id == 0 && defaults.OptionNoId.HasValue)
                book.optionno_id = defaults.OptionNoId.Value;

            if (book.create_page == 0 && defaults.CreatePage.HasValue)
                book.create_page = defaults.CreatePage.Value;

            if (book.is_circular == 0 && defaults.IsCircular.HasValue)
                book.is_circular = defaults.IsCircular.Value;

            // Apply Additional Information defaults
            book.request_org_code ??= defaults.RequestOrgCode;

            // Apply Reserved fields (for future use - ตาม eSaraban API spec)
            book.law_id ??= defaults.LawId;
            book.law_code ??= defaults.LawCode;
            book.parent_positioncode ??= defaults.ParentPositionCode;

            // Apply endpoint-specific defaults
            if (endpointConfig != null)
            {
                if (book.status_id == 0 && endpointConfig.StatusId.HasValue)
                    book.status_id = endpointConfig.StatusId.Value;

                // Apply custom defaults from configuration
                if (endpointConfig.CustomDefaults != null)
                {
                    foreach (var kvp in endpointConfig.CustomDefaults)
                    {
                        ApplyCustomDefault(book, kvp.Key, kvp.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Apply default values สำหรับ BookFile
        /// </summary>
        private void ApplyBookFileDefaults(BookFile file)
        {
            var defaults = _bookDefaults.BookFile;

            file.file_extension ??= defaults.FileExtension;
            file.file_path ??= defaults.FilePath;
            file.file_url ??= defaults.FileUrl;
            file.alfresco_parentid ??= defaults.AlfrescoParentId;
            file.alfresco_foldername ??= defaults.AlfrescoFolderName;
            file.alfresco_nodetype ??= defaults.AlfrescoNodeType;
        }

        /// <summary>
        /// Apply default values สำหรับ BookHistory
        /// </summary>
        private void ApplyBookHistoryDefaults(BookHistory history, EndpointConfig? endpointConfig)
        {
            var defaults = _bookDefaults.BookHistory;

            history.action ??= endpointConfig?.HistoryAction ?? defaults.Action;
            history.action_by ??= defaults.ActionBy;
            history.remark ??= defaults.Remark;
        }

        /// <summary>
        /// Apply default values สำหรับ BookAttachment (ใช้ร่วมกับ BookReferenceAttachment)
        /// </summary>
        private void ApplyBookAttachmentDefaults(BookAttachment attach)
        {
            var defaults = _bookDefaults.BookFile;

            attach.file_extension ??= defaults.FileExtension;
            attach.file_path ??= defaults.FilePath;
            attach.file_url ??= defaults.FileUrl;
            attach.alfresco_parentid ??= defaults.AlfrescoParentId;
            attach.alfresco_foldername ??= defaults.AlfrescoFolderName;
            attach.alfresco_nodetype ??= defaults.AlfrescoNodeType;
        }

        /// <summary>
        /// Apply default values สำหรับ BookReferenceAttachment
        /// </summary>
        private void ApplyBookAttachmentDefaults(BookReferenceAttachment refAttach)
        {
            var defaults = _bookDefaults.BookFile;

            refAttach.file_extension ??= defaults.FileExtension;
            refAttach.file_path ??= defaults.FilePath;
            refAttach.file_url ??= defaults.FileUrl;
            refAttach.alfresco_parentid ??= defaults.AlfrescoParentId;
            refAttach.alfresco_foldername ??= defaults.AlfrescoFolderName;
            refAttach.alfresco_nodetype ??= defaults.AlfrescoNodeType;
        }

        /// <summary>
        /// Apply custom default value จาก configuration
        /// </summary>
        private void ApplyCustomDefault(BookData book, string key, object value)
        {
            try
            {
                var property = typeof(BookData).GetProperty(key);
                if (property != null && property.CanWrite)
                {
                    var currentValue = property.GetValue(book);
                    if (currentValue == null)
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(book, convertedValue);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to apply custom default for {key}: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate Book Code based on endpoint configuration
        /// </summary>
        private string GenerateBookCode(string endpointType)
        {
            var prefix = endpointType switch
            {
                "original" => _bookDefaults.Endpoints.Original.BookCodePrefix,
                "approved" => _bookDefaults.Endpoints.Approved.BookCodePrefix,
                "non-compliant" => _bookDefaults.Endpoints.NonCompliant.BookCodePrefix,
                "under-construction" => _bookDefaults.Endpoints.UnderConstruction.BookCodePrefix,
                _ => "BK-"
            };

            var dateString = DateTime.Now.ToString("yyyyMMdd");
            var randomNumber = new Random().Next(1000, 9999);
            return $"{prefix}{dateString}-{randomNumber}";
        }

        #endregion
    }
}
