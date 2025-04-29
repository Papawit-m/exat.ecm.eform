using Aspose.Words.Tables;
using EXAT.ECM.Business.Configurations;
using EXAT.ECM.Business.Helper;
using EXAT.ECM.Business.Models;
using EXAT.ECM.Business.Models.APIModel;
using EXAT.ECM.Business.Models.EON;
using EXAT.ECM.Business.Services;
using EXAT.ECM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using static EXAT.ECM.Business.Models.APIModel.ResponseModel;

namespace EXAT.ECM.EON.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EXATEONController : ControllerBase
    {
        private readonly ILogger<EXATEONController> _logger;
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;
        private readonly IEONService _eonService;
        private readonly string _connectionString;

        private string EONSummaryReportTemplate = "DocumentTemplate/EON/EONSummaryReportTemplate.docx";
        private string EONRequestReportTemplate = "DocumentTemplate/EON/EONRequestFormTemplate.docx";

        public EXATEONController(ILogger<EXATEONController> logger
                                  , IOptions<AsposeOption> asposeOption
                                  , IWebHostEnvironment environment
                                  , IEONService eonService
                                  , IConfiguration configuration
                                  )
        {
            _logger = logger;

            if (asposeOption == null)
            {
                throw new ArgumentNullException(nameof(asposeOption));
            }
            _asposeOption = asposeOption.Value;
            _environment = environment;
            _eonService = eonService;
            _connectionString = configuration.GetConnectionString("OracleConnection");
        }
        // GET: api/TestConnection
        [HttpGet("TestConnection")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public ActionResult TestConnection()
        {
            _logger.LogInformation("Start processing TestConnection");

            try
            {
                var result = new SuccessResponse()
                {
                    Status = "S",
                    StatusCode = "200",
                    Data = string.Format("Success"),
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = ex.Message
                };

                _logger.LogError(ex, ex.Message);
                return StatusCode(500, errorResponse);  // Return 500 if an error occurs
            }
        }

        //Test Connection DB
        [HttpGet("check-connection")]
        public IActionResult CheckDatabaseConnection()
        {
            try
            {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();

                    string dbName = "";
                    using (var command = new OracleCommand("SELECT sys_context('USERENV', 'DB_NAME') FROM dual", connection))
                    {
                        dbName = command.ExecuteScalar()?.ToString();
                    }
                    return Ok(new { status = "✅ Connection Successful!", database = dbName });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "❌ Connection Failed", error = ex.Message });
            }
        }

        // GET: api/DownloadPrintFormEON
        [HttpGet("DownloadPrintFormEON")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadPrintFormEON(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
        {
            _logger.LogInformation("Start processing DownloadPrintFormEON");

            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            byte[] bytes = null;

            
            try
            {
                var result = new SuccessResponse()
                {
                    Status = "S",
                    StatusCode = "200",
                    Data = string.Format("Success"),
                };

                EONParameterModel request = new EONParameterModel();
                #region set parameter
                string[] splitParam = new string[0];
                if (!string.IsNullOrEmpty(p_Parameter))
                    splitParam = p_Parameter.Split(new Char[] { '|' });

                foreach (string paramItem in splitParam)
                {
                    string[] paramVal = paramItem.Split('=');
                    if (paramVal != null && paramVal.Length == 2)
                    {
                        switch (paramVal[0])
                        {
                            case "p_DOCNO": request.p_DOCNO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DEPARTMENT_CODE": request.p_DEPARTMENT_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DIVISION_CODE": request.p_DIVISION_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_SECTION_CODE": request.p_SECTION_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_EXPRESSWAY_ID": request.p_EXPRESSWAY_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DIRECTION_ID": request.p_DIRECTION_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_SPEED_ID": request.p_SPEED_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_STATUS_ID": request.p_STATUS_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_WORKSTART_DATE_FROM": request.p_WORKSTART_DATE_FROM = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_WORKSTART_DATE_TO": request.p_WORKSTART_DATE_TO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_DOCDATE_FROM": request.p_REQUEST_DOCDATE_FROM = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_DOCDATE_TO": request.p_REQUEST_DOCDATE_TO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_USER_AD": request.p_USER_AD = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                        }
                    }
                }
                #endregion

                if (p_Template == "EONSummaryReportTemplate")
                {

                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormEON");
                    }

                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, EONSummaryReportTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();


                    // get data from oracle
                    var data = _eonService.GetEONSummaryAsync(request);
                    var d_detail = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    var data_footer = new EON_DETAIL_FOOTER_REPORT();
                    var d_footer = new List<Dictionary<string, ReplaceObject>>();
                    if (data.Result != null && data.Result != null)
                    {
                        foreach (var row in data.Result)
                        {
                            data_footer.ON_TIME_REQUEST = row.ON_TIME_REQUEST;
                            data_footer.PENDING_REQUEST = row.PENDING_REQUEST;
                            data_footer.TOTAL_REQUEST = row.TOTAL_REQUEST;
                        }
                        d_footer = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data_footer);
                    }
                    
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_footer);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                    document.Save(memoryStream, p_FileName);
                }

                if (p_Template == "EONRequestReportTemplate")
                {

                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormEON");
                    }

                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, EONRequestReportTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();


                    // get data from oracle
                    var data = _eonService.GetEONRequestAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);

                    document.Save(memoryStream, p_FileName);
                }

                if (memoryStream != null)
                    bytes = memoryStream.ToArray();

                return File(bytes, "application/octet-stream", p_FileName);
            }
            catch (Exception ex)
            {
                var errorResponse = new ErrorResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = ex.Message
                };
                _logger.LogError(ex, ex.Message);

                return StatusCode(500, errorResponse);  // Return 500 if an error occurs
            }
        }
    }
}
