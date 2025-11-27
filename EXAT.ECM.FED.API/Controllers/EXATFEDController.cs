
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
using System.Text.Json;
using System.IO;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using EXAT.ECM.FED.API.Models.IMPORT;
using EXAT.ECM.FED.API.DAL;
using System.Data; 
namespace EXAT.ECM.FED.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EXATFEDController : Controller
    {
        private readonly ILogger<EXATFEDController> _logger;
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;
        private readonly IFEDService _fedService;
        private readonly string _connectionString;
        private readonly ILoggingService _loggingService;
        private readonly OracleDbContext _oracleContext;
        private readonly IFleetCardRepository _repository;
        
        public EXATFEDController(ILogger<EXATFEDController> logger
                                  , IOptions<AsposeOption> asposeOption
                                  , IWebHostEnvironment environment
                                  , IFEDService fedService
                                  , IConfiguration configuration
                                  ,ILoggingService loggingService
                                  ,IFleetCardRepository repository
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
            _loggingService = loggingService;
            _repository = repository;
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
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
        private string FEDFuelFleetCard_Header_Template = "DocumentTemplate/FED/FEDFuelFleetCardTemplate_List.docx"; 
        private string FEDFuelFleetCard_Detail_Template = "DocumentTemplate/FED/FEDFuelFleetCardTemplate_Detail.docx";
<<<<<<< HEAD
        private string test = "DocumentTemplate/FED/test.docx";
        private string VehicleHandoverTemplate = "DocumentTemplate/FED/VehicleHandoverTemplate.docx";
        private string DailyVehicleInspectionTemplate = "DocumentTemplate/FED/DailyVehicleInspectionTemplate.docx";
        private string VehicleRepairRequestTemplate = "DocumentTemplate/FED/VehicleRepairRequestTemplate.docx";
        private string FEDFuelFleetCardBank = "DocumentTemplate/FED/FEDFuelFleetCardBankTemplate.docx";

=======
>>>>>>> 9144d2c8939ff0a00fd4c47d76390e815204a648

        private object stackTrace;

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
                //p_Parameter = "p_USER_AD%3DEXAT%5CAdministrator";
                //p_FileName = "FleetCardFuelUsageTemplate.pdf";
                //p_Template = "FleetCardFuelUsageTemplate";
                FEDParameterModel request = new FEDParameterModel();
                #region set parameter
                string[] splitParam = new string[0];
                p_Parameter = Uri.UnescapeDataString(p_Parameter);
                if (!string.IsNullOrEmpty(p_Parameter))
                splitParam = p_Parameter.Split(new Char[] { '|' });

                foreach (string paramItem in splitParam)
                {
                    _logger.LogInformation($"paramItem {paramItem} ");
                    string[] paramVal = paramItem.Split('=');
                    if (paramVal != null && paramVal.Length == 2)
                    {
                        switch (paramVal[0])
                        {
                            case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_ORG_CODE": request.p_ORG_CODE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_VEHICLE_ID": request.p_VEHICLE_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_MONTH": request.p_MONTH_NO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_YEAR": request.p_YEAR = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DOC_DATE_FROM": request.p_DATE_FROM = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_DOC_DATE_TO": request.p_DATE_TO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_USER_AD": request.p_USER_AD = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
                            case "p_TAX_INVOICE": request.p_TAX_INVOICE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
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

<<<<<<< HEAD
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    //AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    // get data from
                    var mainData = await _fedService.GetLIST_HEADER_REPORTFormAsync(request);
                    if (mainData == null || mainData.Count == 0)
                        throw new Exception("No report header data found.");

                    List<byte[]> pdfFiles = new List<byte[]>();
                    foreach (var header in mainData)
                    {
                        var subRequest = new ParameterHEADER_REPORT
                        {
                            p_ORG_CODE = header.ORG_CODE,
                            p_VEHICLE_ID = header.VEHICLE_ID,
                            p_MONTH_NO = header.MONTH,
                            p_YEAR = header.YEAR,
                            p_REQUEST_DOCDATE_FROM = header.DATE_FROM,
                            p_REQUEST_DOCDATE_TO = header.DATE_TO,
                            p_HEADER_ID = header.HEADER_ID,
                            p_CATEGORY_ID = header.CATEGORY_ID
                        };
                        var data = _fedService.GetFuelFleetCardFormAsync(subRequest);
                        var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                        var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                        var d_detail2 = data.Result?.Detail2 == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail2);

                        using (MemoryStream memoryStreamTemp = new MemoryStream())
                        {
                            AsposeHelper document_temp = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                            replacWords.ReplaceNodeDataRow(document_temp, "bmDataRow", d_detail);
                            replacWords.ReplaceNodeDataRow(document_temp, "bmDataRow2", d_detail2);
                            replacWords.ReplaceNodeText(document_temp, d_header);
                            replacWords.ReplaceNodeText(document_temp, d_detail);
                            replacWords.RemoveRowWithSpecificBookmark(document_temp, "bmDataRow");
                            replacWords.RemoveRowWithSpecificBookmark(document_temp, "bmDataRow2");
                            document_temp.Save(memoryStreamTemp, p_FileName);
                            memoryStreamTemp.Position = 0;
                            var pdfBytes = memoryStreamTemp.ToArray();
                            pdfFiles.Add(pdfBytes);
                            _logger.LogInformation("PDF[{Index}] size: {Size} bytes", pdfFiles.Count, pdfBytes.Length);
                            memoryStreamTemp.Close();
                        }

                        //
                    }
                    //byte[] mergedPdf = document.MergePdfFiles(pdfFiles);
                    AsposeHelper merger = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    _logger.LogInformation($"Merging {pdfFiles.Count} PDF files...");
                    bytes = merger.MergePdfFiles(pdfFiles);
                    _logger.LogInformation($"Merged PDF size: {bytes.Length} bytes");
                    //var data = _fedService.GetFuelFleetCardFormAsync(request);
                    //var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    //var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    //var d_detail2 = data.Result?.Detail2 == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail2);

                    //replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    //replacWords.ReplaceNodeDataRow(document, "bmDataRow2", d_detail2);
                    //replacWords.ReplaceNodeText(document, d_header);
                    //replacWords.ReplaceNodeText(document, d_detail);
                    //replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    //replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow2");
                    //document.Save(memoryStream, p_FileName);
                }
                if (p_Template == "VehicleRepairRequestTemplate") // บันทึกการส่งมอบรถและอุปกรณ์
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "VehicleRepairRequestTemplate");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, VehicleRepairRequestTemplate);
=======
>>>>>>> 9144d2c8939ff0a00fd4c47d76390e815204a648
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    //AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

<<<<<<< HEAD
                    //// get data from oracle
                    var data = _fedService.GetVEHICLEREPAIRREQUESTFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    // part Checkbox
                    replacWords.SetCheckboxes(document, d_header);

                    document.Save(memoryStream, p_FileName);
=======
                    // get data from
                    var mainData = await _fedService.GetLIST_HEADER_REPORTFormAsync(request);
                    if (mainData == null || mainData.Count == 0)
                        throw new Exception("No report header data found.");

                    List<byte[]> pdfFiles = new List<byte[]>();
                    foreach (var header in mainData)
                    {
                        var subRequest = new ParameterHEADER_REPORT
                        {
                            p_ORG_CODE = header.ORG_CODE,
                            p_VEHICLE_ID = header.VEHICLE_ID,
                            p_MONTH_NO = header.MONTH,
                            p_YEAR = header.YEAR,
                            p_REQUEST_DOCDATE_FROM = header.DATE_FROM,
                            p_REQUEST_DOCDATE_TO = header.DATE_TO,
                            p_HEADER_ID = header.HEADER_ID,
                            p_CATEGORY_ID = header.CATEGORY_ID
                        };
                        var data = _fedService.GetFuelFleetCardFormAsync(subRequest);
                        var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                        var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                        var d_detail2 = data.Result?.Detail2 == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail2);

                        using (MemoryStream memoryStreamTemp = new MemoryStream())
                        {
                            AsposeHelper document_temp = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                            replacWords.ReplaceNodeDataRow(document_temp, "bmDataRow", d_detail);
                            replacWords.ReplaceNodeDataRow(document_temp, "bmDataRow2", d_detail2);
                            replacWords.ReplaceNodeText(document_temp, d_header);
                            replacWords.ReplaceNodeText(document_temp, d_detail);
                            replacWords.RemoveRowWithSpecificBookmark(document_temp, "bmDataRow");
                            replacWords.RemoveRowWithSpecificBookmark(document_temp, "bmDataRow2");
                            document_temp.Save(memoryStreamTemp, p_FileName);
                            memoryStreamTemp.Position = 0;
                            var pdfBytes = memoryStreamTemp.ToArray();
                            pdfFiles.Add(pdfBytes);
                            _logger.LogInformation("PDF[{Index}] size: {Size} bytes", pdfFiles.Count, pdfBytes.Length);
                            memoryStreamTemp.Close();
                        }

                        //
                    }
                    //byte[] mergedPdf = document.MergePdfFiles(pdfFiles);
                    AsposeHelper merger = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    _logger.LogInformation($"Merging {pdfFiles.Count} PDF files...");
                    bytes = merger.MergePdfFiles(pdfFiles);
                    _logger.LogInformation($"Merged PDF size: {bytes.Length} bytes");
                    //var data = _fedService.GetFuelFleetCardFormAsync(request);
                    //var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    //var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
                    //var d_detail2 = data.Result?.Detail2 == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail2);

                    //replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    //replacWords.ReplaceNodeDataRow(document, "bmDataRow2", d_detail2);
                    //replacWords.ReplaceNodeText(document, d_header);
                    //replacWords.ReplaceNodeText(document, d_detail);
                    //replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    //replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow2");
                    //document.Save(memoryStream, p_FileName);
>>>>>>> 9144d2c8939ff0a00fd4c47d76390e815204a648
                }
                if (p_Template == "DailyVehicleInspectionTemplate") //บันทึกการตรวจรถประจำวัน
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "DailyVehicleInspectionTemplate");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, DailyVehicleInspectionTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //// get data from oracle
                    var data = _fedService.GetDailyVehicleInspectionFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);
                  
                    replacWords.SetCheckboxes(document, d_header);

                    document.Save(memoryStream, p_FileName);
                }
                if (p_Template == "VehicleHandoverTemplate") //ใบแจ้งซ่อม / บำรุงรักษารถ
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "VehicleHandoverTemplate");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, VehicleHandoverTemplate);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //// get data from oracle
                    var data = _fedService.GetVEHICLEHANDOVERFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.SetCheckboxes(document, d_header);

                    document.Save(memoryStream, p_FileName);
                }
                if (p_Template == "FEDFuelFleetCardBank") // FuelFleetCardBank
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "FEDFuelFleetCardBank");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, FEDFuelFleetCardBank);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //// get data from oracle
                    var data = _fedService.GetFuelFleetCardBank(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
                    var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);

                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                    document.Save(memoryStream, p_FileName);
                }
                if (memoryStream != null)
                {
                    if(p_Template != "FleetCardFuelUsageTemplate")
                        bytes = memoryStream.ToArray();
                }
                    
                   

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
        
        
        // Get: api/ImportFleetCardFED
        [HttpGet("ImportFleetCardFED")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> ImportExcelFleetCardFED(
                                        [FromQuery] string? p_Parameter
                                        )
        {
            _logger.LogInformation("Start processing ImportFleetCardFED");

            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            byte[] bytes = null;
            try
            {
                var result = new SuccessResponseImport()
                {
                    Status = "S",
                    StatusCode = "200",
                    Message = string.Format("Success"),
                    Url     = "",
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

                var data = _fedService.ImportFileExcelFED(request);
                var status = data.Result;

                if (status.Status == "S")
                {

                    result.Status = "S"; 
                    result.StatusCode = "200";
                    result.Message = string.Format(status.Message);
                    
                }
                if (status.Status != "S")
                {

                    result.Status = status.Status;
                    result.StatusCode = "500";
                    result.Message = string.Format(status.Message);
                    //result.Url = string.Format(status.ErrorFileUrl);

                }

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

        // Get: api/DownloadErrorExcel
        [HttpGet("DownloadErrorExcel")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadErrorExcel(
                                        [FromQuery] string? p_Parameter
                                        )
        {
            _logger.LogInformation("Start processing DownloadErrorExcel");

            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            byte[] bytes = null;
            try
            {
                
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

                var data = await _fedService.DownloadErrorExcel(request);
                var dt = Utilities.ToDataTable(data.Detail);

                using var package = new ExcelPackage();
                var ws = package.Workbook.Worksheets.Add("Errors");
                ws.Cells["A1"].LoadFromDataTable(dt, true);

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"ImportError_{request.p_TEMP_ID}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);


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

        [HttpGet("ImportBankFleetCard")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> ImportBankFleetCard([FromQuery] string? p_Parameter)
        {
            _logger.LogInformation("Start processing ImportFleetCardFED");

            try
            {
                // 1) Validate p_Parameter (ว่าง/null/มีแต่ช่องว่าง)
                if (string.IsNullOrWhiteSpace(p_Parameter))
                {
                    var invalid = new SuccessResponseImport
                    {
                        Status = "E",
                        StatusCode = "200",  // คงคอนเวนชันเดิม: business error -> 200 OK
                        Message = "Parameter 'p_Parameter' is required and cannot be empty.",
                        Url = ""
                    };
                    _logger.LogWarning("ImportBankFleetCard: p_Parameter is missing or empty.");
                    return Ok(invalid);
                }

                // 2) Parse p_Parameter: "key=value|key2=value2"
                var map = p_Parameter
                    .Split('|', StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => part.Split('=', 2))
                    .Where(kv => kv.Length == 2)
                    .ToDictionary(kv => kv[0].Trim(), kv => kv[1].Trim(), StringComparer.OrdinalIgnoreCase);

                // 3) Compose request model
                var request = new FEDParameterModel();
                if (map.TryGetValue("p_TEMP_ID", out var tempId))
                {
                    request.p_TEMP_ID = string.IsNullOrWhiteSpace(tempId) ? null : Utilities.CleansingData(tempId);
                }

                if (string.IsNullOrEmpty(request.p_TEMP_ID))
                {
                    var missingTempId = new SuccessResponseImport
                    {
                        Status = "E",
                        StatusCode = "200",
                        Message = "Parameter 'p_TEMP_ID' is required.",
                        Url = ""
                    };
                    _logger.LogWarning("ImportBankFleetCard: missing p_TEMP_ID.");
                    return Ok(missingTempId);
                }

                _logger.LogInformation("ImportBankFleetCard: parsed params. Has p_TEMP_ID: {HasTempId}",
                    !string.IsNullOrEmpty(request.p_TEMP_ID));

                // 4) Call service
                var serviceResult = await _fedService.ImportTransactionsAsync(request);

                // 5) Map service result -> API result (200 เสมอ ยกเว้น throw)

                var result = new SuccessResponseImport
                {
                    Status = serviceResult.Status,
                    StatusCode = serviceResult.Status == "S" ? "200" : "500", // business error ก็ยังคง 200
                    Message = serviceResult.Message,
                    Url = "" // ใส่เพิ่มได้ถ้า service ส่งมา
                };

                // Log summary
                if (serviceResult.Status == "S")
                    _logger.LogInformation("ImportBankFleetCard: completed successfully.");
                else
                    _logger.LogWarning("ImportBankFleetCard: completed with business error: {Message}", serviceResult.Message);

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
                _logger.LogError(ex, "ImportBankFleetCard: unhandled exception.");
                return StatusCode(500, errorResponse);
            }
        }
                

        [HttpGet("test-db-log")]
        public async Task<IActionResult> TestDbLog()
        {
            try
            {
                await _loggingService.LogErrorAsync("ERROR", new Exception("TestException"), "Test log from /api/logtest/test-db-log endpoint.", "Manual test context");
                return Ok(new { message = "Log written to DB (if no error)." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to write log: {ex.Message}" });
            }
        }

        [HttpGet("test-get-import-configs")]
        public async Task<IActionResult> TestGetImportConfigs()
        {
            List<Dictionary<string, object>> configs = new();
            string logMessage;
            string logError = null;
            try
            {
                await using var conn = new OracleConnection(_connectionString);
                await conn.OpenAsync();

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT CONFIG_ID, TEMPLATE_NAME, FIELD_NAME, SOURCE_COLUMN_NAME, SOURCE_COLUMN_INDEX, IS_REQUIRED
                                    FROM EFM_FED.FLEET_CARD_IMPORT_CONFIGS";

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                    }
                    configs.Add(row);
                }

                logMessage = $"SUCCESS: Get {configs.Count} rows from FLEET_CARD_IMPORT_CONFIGS";
            }
            catch (Exception ex)
            {
                logMessage = $"ERROR: {ex.Message}";
            }

            // Insert log to FLEET_CARD_APP_LOGS
            try
            {
                await using var logConn = new OracleConnection(_connectionString);
                await logConn.OpenAsync();

                
                using var logCmd = logConn.CreateCommand();

                logCmd.BindByName = true;
                logCmd.CommandText = @"
                    INSERT INTO EFM_FED.FLEET_CARD_APP_LOGS(LOG_TIMESTAMP, LOG_LEVEL, MESSAGE, STACK_TRACE, CONTEXT_INFO)
                    VALUES (SYSTIMESTAMP, :p_level, :p_msg, :p_stack, :p_ctx)";
                
                logCmd.Parameters.Add(new OracleParameter("p_level", OracleDbType.Varchar2, "INFO", ParameterDirection.Input));

                // ถ้า MESSAGE สั้นแน่ ๆ คงเป็น Varchar2 ได้
                logCmd.Parameters.Add(new OracleParameter("p_msg", OracleDbType.Varchar2, logMessage ?? (object)DBNull.Value, ParameterDirection.Input));

                // LOB: ห้ามผูกเป็น Varchar2
                var stackParam = new OracleParameter("p_stack", OracleDbType.Clob, ParameterDirection.Input)
                {
                    Value = (object)stackTrace ?? DBNull.Value
                };
                logCmd.Parameters.Add(stackParam);

                var ctxJson = System.Text.Json.JsonSerializer.Serialize(configs);
                var ctxParam = new OracleParameter("p_ctx", OracleDbType.Clob, ParameterDirection.Input)
                {
                    Value = (object)ctxJson ?? DBNull.Value
                };
                logCmd.Parameters.Add(ctxParam);

                await logCmd.ExecuteNonQueryAsync();
            }
            catch (Exception logEx)
            {
                logError = $"ERROR insert log: {logEx.Message}";
            }

            return Ok(new { message = logMessage, logError, configs });
        }

        [HttpGet("test-insert-transaction")]
        public async Task<IActionResult> TestInsertTransaction()
        {
            DateTime DateNow = DateTime.Now;
            var transaction = new FleetCardTransaction
            {
                CardNumber = "1234567890123456",
                PlateNumber = "TEST-PLATE",
                TransactionDate = DateNow.ToString(),
                StationName = "Test Station",
                ProductName = "Test Product",
                Quantity = 10,
                UnitPrice = 20,
                TotalAmount = 200,
                Status = "TEST"
            };
            try
            {
                await _repository.InsertTransactionAsync(transaction);
                return Ok(new { message = "Transaction inserted (if no error)." });
            }
            catch (System.Exception ex)
            {
                // Insert error log into FLEET_CARD_APP_LOGS
                try
                {
                    await using var logConn = new OracleConnection(_connectionString);
                    await logConn.OpenAsync();

                    await using var logCmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"INSERT INTO EFM_FED.FLEET_CARD_APP_LOGS(LOG_TIMESTAMP, LOG_LEVEL, MESSAGE, STACK_TRACE, CONTEXT_INFO)
                        VALUES (SYSTIMESTAMP, :logLevel, :message, :stackTrace, :contextInfo)", logConn)
                    {
                        CommandType = System.Data.CommandType.Text
                    };
                    logCmd.Parameters.Add(":logLevel", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 20).Value = "ERROR";
                    logCmd.Parameters.Add(":message", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 4000).Value = ex.Message;
                    logCmd.Parameters.Add(":stackTrace", Oracle.ManagedDataAccess.Client.OracleDbType.Clob).Value = ex.ToString();
                    logCmd.Parameters.Add(":contextInfo", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 1000).Value = $"TestInsertTransaction | CardNumber: {transaction.CardNumber}";
                    
                    await logCmd.ExecuteNonQueryAsync();
                }
                catch (System.Exception logEx)
                {
                    System.Console.WriteLine($"[FATAL] Failed to write error log: {logEx.Message}");
                }
                return StatusCode(500, new { message = $"Failed to insert transaction: {ex.Message}" });
            }
        }
    }
}