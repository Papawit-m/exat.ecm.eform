using EXAT.ECM.Business.Configurations;
using EXAT.ECM.Business.Helper;
using EXAT.ECM.Business.Models;
using EXAT.ECM.Business.Models.APIModel;
using EXAT.ECM.Business.Models.PPA;
using EXAT.ECM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static EXAT.ECM.Business.Models.APIModel.ResponseModel;

namespace EXAT.ECM.PPA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EXATPPAController : ControllerBase
    {
        private readonly ILogger<EXATPPAController> _logger;
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;
        private readonly IPPAService _ppaService;

        private string PPASummaryReportTemplate = "DocumentTemplate/PPA/PPASummaryReportTemplate.docx";

        public EXATPPAController(ILogger<EXATPPAController> logger
                                   , IOptions<AsposeOption> asposeOption
                                   , IWebHostEnvironment environment
                                   , IPPAService ppaService
                                   )
        {
            _logger = logger;

            if (asposeOption == null)
            {
                throw new ArgumentNullException(nameof(asposeOption));
            }
            _asposeOption = asposeOption.Value;
            _environment = environment;
            _ppaService = ppaService;
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


        // GET: api/DownloadPrintFormPPA
        [HttpGet("DownloadPrintFormPPA")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadPrintFormPPA(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
        {
            _logger.LogInformation("Start processing DownloadPrintFormPPA");

            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            byte[] bytes = null;

            if (string.IsNullOrEmpty(p_FileName))
            {
                p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormPPA");
            }

            try
            {
                var result = new SuccessResponse()
                {
                    Status = "S",
                    StatusCode = "200",
                    Data = string.Format("Success"),
                };

                PPAParameterModel request = new PPAParameterModel();
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
                            case "p_PROJECT_NAME": request.p_PROJECT_NAME = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_PROJECTSECRETARY_BY": request.p_PROJECTSECRETARY_BY = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_CONTRACT_NUMBER": request.p_CONTRACT_NUMBER = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_PROJECT_START_DATE": request.p_PROJECT_START_DATE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_PROJECT_END_DATE": request.p_PROJECT_END_DATE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_STATUS_ID": request.p_STATUS_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_USER_AD": request.p_USER_AD = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                        }
                    }
                }
                #endregion


                string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, PPASummaryReportTemplate);
                var apOption = new AsposeHelperOption(_asposeOption, _environment);
                AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                ReplaceWords replacWords = new ReplaceWords();

                // get data from oracle
                var data = _ppaService.GetPPASummaryAsync(request);

                var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);

                replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                replacWords.ReplaceNodeText(document, d_header);

                document.Save(memoryStream, p_FileName);

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
