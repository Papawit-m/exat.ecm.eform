using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Services.Implementations;
using EXAT.ECM.EService.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EXAT.ECM.EService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThaiEpassController : ControllerBase
    {
        private readonly IThaiEpassAuthService _authService;
        private readonly ITagUsageService _tagUsageService;
        private readonly ICustomerSearchService _customerSearchService;
        private readonly ILogger<ThaiEpassController> _logger;

        public ThaiEpassController(
            IThaiEpassAuthService authService,
            ITagUsageService tagUsageService,
            ICustomerSearchService customerSearchService,
            ILogger<ThaiEpassController> logger)
        {
            _authService = authService;
            _tagUsageService = tagUsageService;
            _customerSearchService = customerSearchService;
            _logger = logger;
        }

        /// <summary>
        /// Unit Test: ใช้ยิงขอ access_token โดยใช้ user_name/password จาก appsettings
        /// </summary>
        [HttpPost("access-token")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetAccessToken()
        {
            try
            {
                var response = await _authService.GetAuthResponseAsync();

                if (response == null)
                    return StatusCode(StatusCodes.Status502BadGateway,
                        new { message = "Failed to call auth/access_token" });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling ThaiEpass access-token");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Internal server error", error = ex.Message });
            }
        }

        /// <summary>
        /// Proxy เรียก ThaiEpass Tag Usage Report
        /// </summary>
        [HttpPost("tag-usage")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> SearchTagUsage([FromQuery] string? p_by, [FromQuery] string? p_keyword, [FromQuery] string? p_start_date, [FromQuery] string? p_end_date, [FromQuery] string? p_language)
        {

            var request = new TagUsageRequest
            {
                By = p_by,
                Keyword = p_keyword,
                StartDate = p_start_date,
                EndDate = p_end_date,
                Language = p_language
            };

            if (request == null)
                return BadRequest("Request cannot be null.");

            try
            {
                var result = await _tagUsageService.SearchTagUsageAsync(request);
                if (result == null)
                {
                    // กรณีไม่มี data จาก ThaiEpass
                    return NoContent(); // หรือจะเปลี่ยนเป็น 200 + message ด้านล่างก็ได้
                }
                return Ok(new
                {
                    statusCode = result.StatusCode
                   ,
                    status = result.Status
                   ,
                    resultCode = result.ResultCode
                   ,
                    result = result.Result
                   ,
                    message = result.Message
                   ,
                    messageext = result.MessageExt
                   ,
                    reportname = result.Data.ReportName
                   ,
                    customerid = result.Data.Profile.CustomerId
                   ,
                    customername = result.Data.Profile.CustomerName
                   ,
                    cusid = result.Data.Profile.CustId
                   ,
                    custaccid = result.Data.Profile.CustAcctId
                   ,
                    pannum = result.Data.Profile.PanNum
                   ,
                    smartcardid = result.Data.Profile.SmartcardId
                   ,
                    balance = result.Data.Profile.Balance
                   ,
                    custaccstatus = result.Data.Profile.CustAcctStatus
                   ,
                    registerdate = result.Data.Profile.RegisterDate
                   ,
                    licenseplate = result.Data.Profile.LicensePlate
                   ,
                    carddetail = result.Data.Profile.CarDetail
                   ,
                    address1 = result.Data.Profile.Address1
                   ,
                    address2 = result.Data.Profile.Address2
                   ,
                    phone = result.Data.Profile.Phone
                   ,
                    txnsort = result.Data.Profile.TxnSort
                   ,
                    txnduring = result.Data.Profile.TxnDuring
                   ,
                    cardname = result.Data.Profile.CardName
                   ,
                    tagusage = result.Data.TagUsage
                });
            }
            catch (HttpRequestException ex)
            {
                // error จากการ call ThaiEpass
                _logger.LogError(ex, "Error calling tag_usage API");
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "Error calling tag_usage API",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                // error อื่น ๆ ในระบบเราเอง
                _logger.LogError(ex, "Error in SearchTagUsage");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        [HttpPost("customer-search")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> CustomerSearch([FromQuery] string? p_by, [FromQuery] string? p_keyword)
        {

            var request = new CustomerSearchRequest
            {
                By = p_by,
                Keyword = p_keyword
            };

            if (request == null)
                return BadRequest("Request cannot be null.");

            try
            {
                var result = await _customerSearchService.SearchCustomerAsync(request);

                return Ok(new
                {
                    statusCode = result.StatusCode
                   ,
                    status = result.Status
                   ,
                    resultCode = result.ResultCode
                   ,
                    result = result.Result
                   ,
                    message = result.Message
                   ,
                    txnstatus = result.Data?.FirstOrDefault()?.TxnStatus
                    ,accountNumber = result.Data?.FirstOrDefault()?.AccountNumber
                    ,accountType = result.Data?.FirstOrDefault()?.AccountType
                    ,customerType = result.Data?.FirstOrDefault()?.CustomerType
                    ,highwayNo = result.Data?.FirstOrDefault()?.HighwayNo
                    ,title = result.Data?.FirstOrDefault()?.Title
                    ,titleEng = result.Data?.FirstOrDefault()?.TitleEng
                    ,family_Name = result.Data?.FirstOrDefault()?.FamilyName
                    ,family_NameEng = result.Data?.FirstOrDefault()?.FamilyNameEng
                    ,given_Name = result.Data?.FirstOrDefault()?.GivenName
                    ,given_NameEng = result.Data?.FirstOrDefault()?.GivenNameEng
                    ,gender = result.Data?.FirstOrDefault()?.Gender
                    ,birthDate = result.Data?.FirstOrDefault()?.BirthDate
                    ,iCPassportType = result.Data?.FirstOrDefault()?.ICPassportType
                    ,iCPassport = result.Data?.FirstOrDefault()?.ICPassport
                    ,address1 = result.Data?.FirstOrDefault()?.Address1
                    ,address2 = result.Data?.FirstOrDefault()?.Address2
                    ,address3 = result.Data?.FirstOrDefault()?.Address3
                    ,province = result.Data?.FirstOrDefault()?.Province
                    ,city     = result.Data?.FirstOrDefault()?.City
                    ,plateNo    = result.Data?.FirstOrDefault()?.PlateNo
                    ,plateProvince  = result.Data?.FirstOrDefault()?.PlateProvince
                    ,postalCode    = result.Data?.FirstOrDefault()?.PostalCode
                    ,telMobile = result.Data?.FirstOrDefault()?.TelMobile
                    ,telHome = result.Data?.FirstOrDefault()?.TelHome
                    ,telOffice = result.Data?.FirstOrDefault()?.TelOffice
                    ,email = result.Data?.FirstOrDefault()?.Email
                    ,remark = result.Data?.FirstOrDefault()?.Remark
                    ,customerAccountStatus = result.Data?.FirstOrDefault()?.CustomerAccountStatus
                    ,pan_num = result.Data?.FirstOrDefault()?.PanNum
                    ,smartcardID = result.Data?.FirstOrDefault()?.SmartcardID
                    ,ac_balance = result.Data?.FirstOrDefault()?.ACBalance
                    ,tagAction = result.Data?.FirstOrDefault()?.TagAction
                    ,tagStatus = result.Data?.FirstOrDefault()?.TagStatus
                    ,taxID = result.Data?.FirstOrDefault()?.TaxID
                    ,branchID = result.Data?.FirstOrDefault()?.BranchID
                    ,customerID = result.Data?.FirstOrDefault()?.CustomerID
                    ,carModel = result.Data?.FirstOrDefault()?.CarModel
                    ,carColor = result.Data?.FirstOrDefault()?.CarColor
                    ,ac_balance2 = result.Data?.FirstOrDefault()?.ACBalance2
                    ,cardName = result.Data?.FirstOrDefault()?.CardName
                    ,tagActionText = result.Data?.FirstOrDefault()?.TagActionText  
                    ,tagStatusText = result.Data?.FirstOrDefault()?.TagStatusText
                    ,ac_balanceShow= result.Data?.FirstOrDefault()?.ACBalanceShow
                });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling customer/search API");
                return StatusCode(StatusCodes.Status502BadGateway, new
                {
                    message = "Error calling customer/search API",
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CustomerSearch");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = "Internal server error",
                    error = ex.Message
                });
            }

        }
    }
}
