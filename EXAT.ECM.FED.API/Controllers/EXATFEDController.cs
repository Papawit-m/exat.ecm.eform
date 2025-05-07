
using EXAT.ECM.FED.API.Configurations;
using EXAT.ECM.FED.API.Helper;
using EXAT.ECM.FED.API.Models;
using EXAT.ECM.FED.API.Models.APIModel;
using EXAT.ECM.FED.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;
using static EXAT.ECM.FED.API.Models.APIModel.ResponseModel;
using System.Text.Json; // ต้องเพิ่มที่ด้านบนของไฟล์
namespace EXAT.ECM.FED.API.Controllers
{
    public class EXATFEDController : Controller
    {
        private readonly ILogger<EXATFEDController> _logger;
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;
        private readonly IFEDService _fedService;
        public EXATFEDController(ILogger<EXATFEDController> logger
                                  , IOptions<AsposeOption> asposeOption
                                  , IWebHostEnvironment environment
                                  , IFEDService fedService
                                  )
        {
            _logger = logger;

            if (asposeOption == null)
            {
                throw new ArgumentNullException(nameof(asposeOption));
            }
            _asposeOption = asposeOption.Value;
            _environment = environment;
            _fedService = fedService;
        }
        private string VehicleRequestTemplate = "DocumentTemplate/FED/VehicleRequestTemplate.docx"; 
        private string DailyVehiuseTemplate = "DocumentTemplate/FED/FEDDailyVehiUsageTemplate.docx";
        private string MonthlyVehiuseTemplate = "DocumentTemplate/FED/FEDMonthlyVehicleUsageTemplate.docx";
        private string DriverUsageVehicle = "DocumentTemplate/FED/FEDDriverUsageVehicleTemplate.docx";
        private string MachineUsage = "DocumentTemplate/FED/FEDMaChineUseTemplate.docx";
        private string FEDFuelExpenseReqTemplate = "DocumentTemplate/FED/FEDFuelExpenseReqTemplate.docx";
        private string FEDPoliceFuelExceedTemplate = "DocumentTemplate/FED/FEDPoliceFuelExceedTemplate.docx";
        private string FEDIncomptFuelTaxInvTemplate = "DocumentTemplate/FED/FEDIncomptFuelTaxInvTemplate.docx";
        private string FEDFuelFleetCardTemplate = "DocumentTemplate/FED/FEDFuelFleetCardTemplate.docx";
        // GET: api/DownloadPrintFormFED
        [HttpGet("DownloadPrintFormFED")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadPrintFormFED(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
        {
            _logger.LogInformation("Start processing DownloadPrintFormFED");

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

                FEDParameterModel request = new FEDParameterModel();
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
                            case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DATA": request.p_DATA = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                 
                        }
                    }
                }
                #endregion
                if (p_Template == "VehicleRequestTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormPRSRequest");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, VehicleRequestTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _fedService.GetVEHICLEAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                    document.Save(memoryStream, p_FileName);
                }

                if (p_Template == "FEDFuelExpenseReqTemplate") // ใบเบิกจ่ายค่าน้ำมัน
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormFEDFuelExpenseReq");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, FEDFuelExpenseReqTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _fedService.GetFuelexpenseRequestFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                   
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                    document.Save(memoryStream, p_FileName);
                }

                if (p_Template == "FEDPoliceFuelExceedTemplate") //รายงานรถตำรวจที่ใช้น้ำมันเกินกว่าที่ได้รับสนับสนุนจาก กทพ.
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormFEDPoliceFuelExceed");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, FEDPoliceFuelExceedTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _fedService.GetPolicefuelExceedRequestFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);

                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    replacWords.ReplaceNodeText(document, d_detail);

                    document.Save(memoryStream, p_FileName);
                }

                if (p_Template == "IncomptFuelTaxInvTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintIncomptFuelTaxInvRequest");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, FEDIncomptFuelTaxInvTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _fedService.GetIncomptFuelTaxinvFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);

                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                    document.Save(memoryStream, p_FileName);
                }

                if (p_Template == "FleetCardFuelUsageTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormFuelFleetCard");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, FEDFuelFleetCardTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _fedService.GetFuelFleetCardFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    var d_detail2 = data.Result?.Detail2 == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail2);
                    
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow2", d_detail2);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.ReplaceNodeText(document, d_detail);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow2");
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
        // GET: api/DownloadVehiUsageFED
        [HttpGet("DownloadVehiUsageFED")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        
        public async Task<IActionResult> DownloadVehiUsageFED(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
        {
            _logger.LogInformation("Start processing DownloadPrintFormFED");

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
                FEDParameterModel request = new FEDParameterModel();
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
                            case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_USAGE_ID": request.p_USAGE_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DAILY_ID": request.p_DAILY_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_ID": request.p_REQUEST_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_VEHICLE_ID": request.p_VEHICLE_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_MONTH_NO": request.p_MONTH_NO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_YEAR": request.p_YEAR = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                        }
                    }
                }
                #endregion
                if (p_Template == "DailyVehiuseTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadDailyVehiuseFEDRequest");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, DailyVehiuseTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();
                    // get data from oracle
                    var data = _fedService.GetDailyVehiUsageAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                    document.Save(memoryStream, p_FileName);
                }
                if (p_Template == "MonthlyVehiuseTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadMonthlyVehiuseFEDRequest");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, MonthlyVehiuseTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();
                    // get data from oracle
                    var data = _fedService.GetMonthlyVehiUsageAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
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
        // GET: api/DownloadDriverUsageVehicleFED
        [HttpGet("DownloadDriverUsageVehicleFED")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadDriverUsageVehicleFED(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
        {
            _logger.LogInformation("Start processing DownloadPrintFormFED");

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

                FEDParameterModel request = new FEDParameterModel();
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
                            case "p_DEPT_CODE": request.p_DEPT_CODE= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_VEHICLE_TYPE": request.p_VEHICLE_TYPE= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_VEHICLE_ID": request.p_VEHICLE_ID= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DRIVER_BY": request.p_DRIVER_BY= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_MONTH_NO": request.p_MONTH_NO= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_YEAR": request.p_YEAR= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_START_DATE": request.p_START_DATE= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_END_DATE": request.p_END_DATE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                        }
                    }
                }
                #endregion
                if (p_Template == "DriverUsageVehicle")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadDriverUsageVehicleRequest");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, DriverUsageVehicle);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //get data from oracle
                    var data = _fedService.GetDriverUsageVehicleAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
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
        // GET: api/DownloadMachinerUseFED
        [HttpGet("DownloadMachinerUseFED")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadMachinerUseFED(
                                  [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
                                , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
                                , [FromQuery] string? p_Template    //  Template Name 
                                )
        {
            _logger.LogInformation("Start processing DownloadPrintFormFED");

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

                FEDParameterModel request = new FEDParameterModel();
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
                            case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_REQUEST_ID": request.p_REQUEST_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DATA": request.p_DATA = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                        }
                    }
                }
                #endregion
                if (p_Template == "MachinerUseTemplate")
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormMachinerUse");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, MachineUsage);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from oracle
                    var data = _fedService.GetMachineUseAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
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
        // Post: api/ImportFleetCardFED
        [HttpPost("ImportFleetCardFED")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> ImportFleetCardFED(
                                        [FromQuery] string? p_Parameter
                                        )
        {
            _logger.LogInformation("Start processing ImportFleetCardFED");

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

                FEDParameterModel request = new FEDParameterModel();
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
                            case "p_TEMP_ID": request.p_TEMP_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                        }
                    }
                }
                #endregion

                return Ok(new { message = "Imported successfully" });
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