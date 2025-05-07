using EXAT.ECM.PRS.API.Configurations;
using EXAT.ECM.PRS.API.Helper;
using EXAT.ECM.PRS.API.Models;
using EXAT.ECM.PRS.API.Models.APIModel;
using EXAT.ECM.PRS.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Diagnostics;
using static EXAT.ECM.PRS.API.Models.APIModel.ResponseModel;

namespace EXAT.ECM.PRS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EXATPRSController : ControllerBase
    {
        private readonly ILogger<EXATPRSController> _logger;
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;
        private readonly IPRSService _prsService;
        private readonly string _connectionString;

        private string PRSSummaryReportTemplate = "DocumentTemplate/PRS/PRSSummaryReportTemplate.docx";
        private string PRSRequestFormTemplate = "DocumentTemplate/PRS/PRSRequestFormTemplate.docx";

        

        public EXATPRSController(ILogger<EXATPRSController> logger
                                  , IOptions<AsposeOption> asposeOption
                                  , IWebHostEnvironment environment
                                  , IPRSService prsService
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
            _prsService = prsService;
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

        // GET: api/DownloadPrintFormPRS
        [HttpGet("DownloadPrintFormPRS")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadPrintFormPRS(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
        {
            _logger.LogInformation("Start processing DownloadPrintFormPRS");

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

                PRSParameterModel request = new PRSParameterModel();
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
                            case "p_REQUEST_DOCNO": request.p_REQUEST_DOCNO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_SUBJECT": request.p_REQUEST_SUBJECT = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DIV_CODE": request.p_DIV_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_SEC_CODE": request.p_SEC_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DEP_CODE": request.p_DEP_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_STATUS_ID": request.p_STATUS_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_DOC_DATE_FROM": request.p_REQUEST_DOC_DATE_FROM = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_DOC_DATE_TO": request.p_REQUEST_DOC_DATE_TO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_USER_AD": request.p_USER_AD = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                        }
                    }
                }
                #endregion

                if (p_Template == "PRSSummaryReportTemplate")
                {
                    _logger.LogInformation("Start processing PRSSummaryReportTemplate");

                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormPRS");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, PRSSummaryReportTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _prsService.GetPRSSummaryAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);

                    var data_footer = new PRS_DETAIL_FOOTER_REPORT();
                    var d_footer = new List<Dictionary<string, ReplaceObject>>();
                    if (data.Result != null && data.Result.Detail != null)
                    {
                        foreach (var row in data.Result.Detail)
                        {
                            data_footer.ON_TIME_REQUEST = row.ON_TIME_REQUEST;
                            data_footer.OVERDUE_REQUEST = row.OVERDUE_REQUEST;
                            data_footer.TOTAL_REQUEST = row.TOTAL_REQUEST;
                        }
                        d_footer = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data_footer);
                    }

                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.ReplaceNodeText(document, d_footer);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                    document.Save(memoryStream, p_FileName);

                    _logger.LogInformation("End processing PRSSummaryReportTemplate");
                }
                
                if (p_Template == "PRSRequestFormTemplate")
                {
                    _logger.LogInformation("Start processing PRSRequestFormTemplate");
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormPRSRequest");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, PRSRequestFormTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _prsService.GetPRSRequestFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    
                    replacWords.ReplaceNodeText(document, d_detail);
                    replacWords.ReplaceNodeText(document, d_header);

                    document.Save(memoryStream, p_FileName);

                    _logger.LogInformation("END processing PRSRequestFormTemplate");
                }

                if (memoryStream != null)
                    bytes = memoryStream.ToArray();

                return File(bytes, "application/octet-stream", p_FileName);

                _logger.LogInformation("End processing DownloadPrintFormPRS");

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
                _logger.LogInformation(ex.Message);

                return StatusCode(500, errorResponse);  // Return 500 if an error occurs
            }

        } 

    }
}
