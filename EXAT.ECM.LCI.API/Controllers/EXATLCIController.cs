using Aspose.Words.Bibliography;
using EXAT.ECM.Business.Models.LCI;
using EXAT.ECM.Business.Configurations;
using EXAT.ECM.Business.Helper;
using EXAT.ECM.Business.Models.APIModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using static EXAT.ECM.Business.Models.APIModel.ResponseModel;
using Microsoft.EntityFrameworkCore;
using EXAT.ECM.Business.Services.Interfaces;
using EXAT.ECM.Business.Services;
using System.Reflection;
using EXAT.ECM.Business.Models;
using System.Configuration;

namespace EXAT.ECM.LCI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EXATLCIController : ControllerBase
    {
        private readonly ILogger<EXATLCIController> _logger;
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;
        private readonly ILCIService _lciService;
        private readonly string _connectionString;

        private string LCISummaryReportTemplate = "DocumentTemplate/LCI/LCISummaryReportTemplate.docx";
        private string LCIRequestFormTemplate = "DocumentTemplate/LCI/LCIRequestFormTemplate.docx";

        public EXATLCIController(ILogger<EXATLCIController> logger
                                    , IOptions<AsposeOption> asposeOption
                                    , IWebHostEnvironment environment
                                    , ILCIService lciService
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
            _lciService = lciService;
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

        // GET: api/DownloadPrintFormLCI
        [HttpGet("DownloadPrintFormLCI")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadPrintFormLCI(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
        {
            _logger.LogInformation("Start processing DownloadPrintFormLCI");

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

                LCIParameterModel request = new LCIParameterModel();
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
                            // Param for summary report
                            case "p_DIV_CODE": request.p_DIV_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_SEC_CODE": request.p_SEC_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DEP_CODE": request.p_DEP_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_STATUS_ID": request.p_STATUS_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_DOC_DATE_FROM": request.p_REQUEST_DOC_DATE_FROM = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_DOC_DATE_TO": request.p_REQUEST_DOC_DATE_TO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_USER_AD": request.p_USER_AD = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;

                            case "p_REQUEST_DOCNO": request.p_REQUEST_DOCNO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_LEGALDEP_DOCNO": request.p_LEGALDEP_DOCNO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_SUBJECT": request.p_REQUEST_SUBJECT = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_SECRET_ID": request.p_SECRET_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_SPEED_ID": request.p_SPEED_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                                // Param for xxx
                        }
                    }
                }
                #endregion

                if (p_Template == "LCISummaryReportTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormLCI");
                    }

                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, LCISummaryReportTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _lciService.GetLCISummaryAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);

                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    document.Save(memoryStream, p_FileName);
                }
                if (p_Template == "LCIRequestFormTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormLCI");
                    }

                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, LCIRequestFormTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _lciService.GetLCIRequestAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    var d_footer_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);

                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_footer_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    //replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    document.Save(memoryStream, p_FileName);
                }
                /*
                var zzzz = ToDataTable(zz);
                var aadd = ToDataTable(data);

                List<Dictionary<string, ReplaceObject>> lsDic = new List<Dictionary<string, ReplaceObject>>();
                Dictionary<string, ReplaceObject> dicObj = null;

                foreach (DataRow dr in zzzz.Rows)
                {
                    dicObj = new Dictionary<string, ReplaceObject>();
                    foreach (DataColumn dc in zzzz.Columns)
                    {
                        if (dc.ColumnName.Contains("_html"))
                            dicObj.Add(dc.ColumnName.Replace("_html", ""), new ReplaceObject() { Value = dr[dc], Format = "", IsHtml = true });
                        else
                            dicObj.Add(dc.ColumnName, new ReplaceObject() { Value = dr[dc], Format = "", IsHtml = false });
                    }

                    if (dicObj != null && dicObj.Count() > 0)
                    {
                        lsDic.Add(dicObj);
                    }
                }

                //for (int i = 0; i < lsDic.Count; i++)
                //{
                //    ReplaceWords.ReplaceNodeText(document, lsDic[i]);
                //}

                ReplaceWords.ReplaceNodeText(document, lsDic);
                
                */

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

                return StatusCode(500, errorResponse);
                //return StatusCode(500, errorResponse);  // Return 500 if an error occurs
            }
        }

    }
}
