
using EXAT.ECM.Business.Configurations;
using EXAT.ECM.Business.Helper;
using EXAT.ECM.Business.Models;
using EXAT.ECM.Business.Models.APIModel;
using EXAT.ECM.Business.Models.EER;
using EXAT.ECM.Business.Services;
using EXAT.ECM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using static EXAT.ECM.Business.Models.APIModel.ResponseModel;

namespace EXAT.ECM.EER.API.Controllers
{

    public class EXATEERController : Controller
    {
        private readonly ILogger<EXATEERController> _logger;
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;
        private readonly IEERService _eerService;

        private string EERSummaryReportTemplate = "DocumentTemplate/EER/EERSummaryReportTemplate.docx";
        private string EERRequestFormTemplate = "DocumentTemplate/EER/EERFormReqPermissionEntryExit.docx";

        public EXATEERController(ILogger<EXATEERController> logger
                                  , IOptions<AsposeOption> asposeOption
                                  , IWebHostEnvironment environment
                                  , IEERService eerService
                                  )
        {
            _logger = logger;

            if (asposeOption == null)
            {
                throw new ArgumentNullException(nameof(asposeOption));
            }
            _asposeOption = asposeOption.Value;
            _environment = environment;
            _eerService = eerService;
        }
        // GET: api/DownloadPrintFormPRS
        [HttpGet("DownloadPrintFormEER")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadPrintFormEER(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
            {
            _logger.LogInformation("Start processing DownloadPrintFormEER");

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

                EERParameterModel request = new EERParameterModel();
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
                            case "p_EXPRESSWAY_ID": request.p_EXPRESSWAY_ID= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DIRECTION_ID": request.p_DIRECTION_ID= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "P_REQUEST_DOCDATE_FROM": request.P_REQUEST_DOCDATE_FROM= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "P_REQUEST_DOCDATE_TO": request.P_REQUEST_DOCDATE_TO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_ID": request.p_REQUEST_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                        }
                    }
                }
                #endregion

                if (p_Template == "EERSummaryReportTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormEER");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, EERSummaryReportTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _eerService.GetEERSummaryAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    document.Save(memoryStream, p_FileName);

                    document.Save(memoryStream, p_FileName);
                }

                if (p_Template == "EERRequestFormTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormPRSRequest");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, EERRequestFormTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _eerService.GetEERRequestFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
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