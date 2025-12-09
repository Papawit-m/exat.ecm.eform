
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
using EXAT.ECM.FED.API.Services;
using System.Text;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Aspose.Pdf.Operators;
using Aspose.Words.Bibliography;
using Aspose.Pdf.AI;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
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
        private readonly IBatchInsertService _batchInsert;
        private readonly IProgressTrackingService _progressTracking;
        private readonly IServiceScopeFactory _scopeFactory;

        private static readonly Dictionary<string, string> MonthAbbr = new()
        {
            {"01","JAN"},{"02","FEB"},{"03","MAR"},{"04","APR"},{"05","MAY"},{"06","JUN"},
            {"07","JUL"},{"08","AUG"},{"09","SEP"},{"10","OCT"},{"11","NOV"},{"12","DEC"}
        };

        // Static constructor เพื่อ register encoding provider ครั้งเดียว
        static EXATFEDController()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public EXATFEDController(ILogger<EXATFEDController> logger
                                  , IOptions<AsposeOption> asposeOption
                                  , IWebHostEnvironment environment
                                  , IFEDService fedService
                                  , IConfiguration configuration
                                  , ILoggingService loggingService
                                  , IFleetCardRepository repository
                                  , IProgressTrackingService progressTracking
                                  , IBatchInsertService batchInsert
                                  , OracleDbContext oracleContext
                                  , IServiceScopeFactory scopeFactory

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
            _progressTracking = progressTracking;
            _batchInsert = batchInsert;
            _oracleContext = oracleContext;
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            //_connectionString = configuration.GetConnectionString("OracleConnection");
            _scopeFactory = scopeFactory;
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
        private string test = "DocumentTemplate/FED/test.docx";
        private string VehicleHandoverTemplate = "DocumentTemplate/FED/VehicleHandoverTemplate.docx";
        private string DailyVehicleInspectionTemplate = "DocumentTemplate/FED/DailyVehicleInspectionTemplate.docx";
        private string VehicleRepairRequestTemplate = "DocumentTemplate/FED/VehicleRepairRequestTemplate.docx";
        private string FEDFuelFleetCardBank = "DocumentTemplate/FED/FEDFuelFleetCardBankTemplate.docx";

        #region Vehicle Inspection Delivery Templates
        private string VehicleInspectionDelivery1Template = "DocumentTemplate/FED/VehicleInspectionDeliveryTemplate1.docx";
        private string VehicleInspectionDelivery2Template = "DocumentTemplate/FED/VehicleInspectionDeliveryTemplate2.docx";
        private string VehicleInspectionDelivery3Template = "DocumentTemplate/FED/VehicleInspectionDeliveryTemplate3.docx";
        private string VehicleInspectionDelivery4Template = "DocumentTemplate/FED/VehicleInspectionDeliveryTemplate4.docx";
        #endregion

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
                //// OLD 
                if (!string.IsNullOrEmpty(p_Parameter))
                {
                    p_Parameter = HttpUtility.UrlDecode(p_Parameter);
                    splitParam = p_Parameter.Split(new Char[] { '|' });
                }
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
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //// get data from oracle
                    var data = _fedService.GetVEHICLEREPAIRREQUESTFormAsync(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    // part Checkbox
                    replacWords.SetCheckboxes(document, d_header);

                    document.Save(memoryStream, p_FileName);
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
                    // part Checkbox
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
                    // part Checkbox
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
                    var d_total = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail.FirstOrDefault());
                    //var d_total = data.Result?.TOTAL == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.TOTAL);

                    _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.ReplaceNodeText(document, d_total);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    document.Save(memoryStream, p_FileName);
                }
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
                    var d_detail2 = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail.FirstOrDefault());
                    _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
                    replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.ReplaceNodeText(document, d_detail2);
                    replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
                    document.Save(memoryStream, p_FileName);
                }
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

                #region VehicleInspectionDelivery บันทึกการตรวจและส่งมอบรถพร้อมอุปกรณ์รถ
                if (p_Template == "VehicleInspectionDeliveryTemplate1") // บันทึกการตรวจและส่งมอบรถพร้อมอุปกรณ์รถ(1)
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "VehicleInspectionDelivery1");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, VehicleInspectionDelivery1Template);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //// get data from oracle
                    var data = _fedService.GetVehicleInspectionDelivery1(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.SetCheckboxes(document, d_header);

                    document.Save(memoryStream, p_FileName);
                }
                if (p_Template == "VehicleInspectionDeliveryTemplate2") // บันทึกการตรวจและส่งมอบรถพร้อมอุปกรณ์รถ(1)
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "VehicleInspectionDelivery2");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, VehicleInspectionDelivery2Template);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //// get data from oracle
                    var data = _fedService.GetVehicleInspectionDelivery2(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.SetCheckboxes(document, d_header);

                    document.Save(memoryStream, p_FileName);
                }
                if (p_Template == "VehicleInspectionDeliveryTemplate3") // บันทึกการตรวจและส่งมอบรถพร้อมอุปกรณ์รถ(1)
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "VehicleInspectionDelivery3");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, VehicleInspectionDelivery3Template);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //// get data from oracle
                    var data = _fedService.GetVehicleInspectionDelivery3(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.SetCheckboxes(document, d_header);

                    document.Save(memoryStream, p_FileName);
                }
                if (p_Template == "VehicleInspectionDeliveryTemplate4") // บันทึกการตรวจและส่งมอบรถพร้อมอุปกรณ์รถ(4)
                {
                    if (string.IsNullOrEmpty(p_FileName))
                    {
                        p_FileName = string.Format("Export_{0}.Docx", "VehicleInspectionDelivery4");
                    }
                    string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, VehicleInspectionDelivery4Template);
                    var apOption = new AsposeHelperOption(_asposeOption, _environment);
                    AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                    ReplaceWords replacWords = new ReplaceWords();

                    //// get data from oracle
                    var data = _fedService.GetVehicleInspectionDelivery4(request);
                    var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);

                    replacWords.ReplaceNodeText(document, d_header);
                    replacWords.SetCheckboxes(document, d_header);

                    document.Save(memoryStream, p_FileName);
                }
                #endregion



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
        //[HttpGet("DownloadVehiUsageFED")]
        //[ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        //[ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        
        //public async Task<IActionResult> DownloadVehiUsageFED(
        //                          [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
        //                        , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
        //                        , [FromQuery] string? p_Template    //  Template Name 
        //                        )
        //{
        //    _logger.LogInformation("Start processing DownloadPrintFormFED");

        //    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
        //    byte[] bytes = null;
        //    try
        //    {
        //        var result = new SuccessResponse()
        //        {
        //            Status = "S",
        //            StatusCode = "200",
        //            Data = string.Format("Success"),
        //        };
        //        FEDParameterModel request = new FEDParameterModel();
        //        #region set parameter
        //            string[] splitParam = new string[0];
        //        if (!string.IsNullOrEmpty(p_Parameter))
        //            splitParam = p_Parameter.Split(new Char[] { '|' });

        //        foreach (string paramItem in splitParam)
        //        {
        //            string[] paramVal = paramItem.Split('=');
        //            if (paramVal != null && paramVal.Length == 2)
        //            {
        //                switch (paramVal[0])
        //                {
        //                    case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_USAGE_ID": request.p_USAGE_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_DAILY_ID": request.p_DAILY_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_REQUEST_ID": request.p_REQUEST_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_VEHICLE_ID": request.p_VEHICLE_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_MONTH_NO": request.p_MONTH_NO = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_YEAR": request.p_YEAR = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                }
        //            }
        //        }
        //        #endregion
        //        if (p_Template == "DailyVehiuseTemplate")
        //        {
        //            if (string.IsNullOrEmpty(p_FileName))
        //            {
        //                p_FileName = string.Format("Export_{0}.Docx", "DownloadDailyVehiuseFEDRequest");
        //            }
        //            string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, DailyVehiuseTemplate);
        //            var apOption = new AsposeHelperOption(_asposeOption, _environment);
        //            AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
        //            ReplaceWords replacWords = new ReplaceWords();
        //            // get data from oracle
        //            var data = _fedService.GetDailyVehiUsageAsync(request);
        //            var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
        //            var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
        //            _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
        //            replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
        //            replacWords.ReplaceNodeText(document, d_header);
        //            replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");

        //            document.Save(memoryStream, p_FileName);
        //        }
        //        if (p_Template == "MonthlyVehiuseTemplate")
        //        {
        //            if (string.IsNullOrEmpty(p_FileName))
        //            {
        //                p_FileName = string.Format("Export_{0}.Docx", "DownloadMonthlyVehiuseFEDRequest");
        //            }
        //            string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, MonthlyVehiuseTemplate);
        //            var apOption = new AsposeHelperOption(_asposeOption, _environment);
        //            AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
        //            ReplaceWords replacWords = new ReplaceWords();
        //            // get data from oracle
        //            var data = _fedService.GetMonthlyVehiUsageAsync(request);
        //            var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
        //            var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
        //            _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
        //            replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
        //            replacWords.ReplaceNodeText(document, d_header);
        //            replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
        //            document.Save(memoryStream, p_FileName);
        //        }
        //        if (memoryStream != null)
        //            bytes = memoryStream.ToArray();

        //        return File(bytes, "application/octet-stream", p_FileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorResponse = new ErrorResponse
        //        {
        //            Status = "E",
        //            StatusCode = "500",
        //            Message = ex.Message
        //        };
        //        _logger.LogError(ex, ex.Message);
        //        return StatusCode(500, errorResponse);  // Return 500 if an error occurs
        //    }
        //}
        // GET: api/DownloadDriverUsageVehicleFED
        //[HttpGet("DownloadDriverUsageVehicleFED")]
        //[ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        //[ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        //public async Task<IActionResult> DownloadDriverUsageVehicleFED(
        //                          [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
        //                        , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
        //                        , [FromQuery] string? p_Template    //  Template Name 
        //                        )
        //{
        //    _logger.LogInformation("Start processing DownloadPrintFormFED");

        //    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
        //    byte[] bytes = null;
        //    try
        //    {
        //        var result = new SuccessResponse()
        //        {
        //            Status = "S",
        //            StatusCode = "200",
        //            Data = string.Format("Success"),
        //        };

        //        FEDParameterModel request = new FEDParameterModel();
        //        #region set parameter
        //        string[] splitParam = new string[0];
        //        if (!string.IsNullOrEmpty(p_Parameter))
        //            splitParam = p_Parameter.Split(new Char[] { '|' });

        //        foreach (string paramItem in splitParam)
        //        {
        //            string[] paramVal = paramItem.Split('=');
        //            if (paramVal != null && paramVal.Length == 2)
        //            {
        //                switch (paramVal[0])
        //                {
        //                    case "p_DEPT_CODE": request.p_DEPT_CODE= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_VEHICLE_TYPE": request.p_VEHICLE_TYPE= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_VEHICLE_ID": request.p_VEHICLE_ID= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_DRIVER_BY": request.p_DRIVER_BY= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_MONTH_NO": request.p_MONTH_NO= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_YEAR": request.p_YEAR= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_START_DATE": request.p_START_DATE= string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_END_DATE": request.p_END_DATE = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                }
        //            }
        //        }
        //        #endregion
        //        if (p_Template == "DriverUsageVehicle")
        //        {
        //            if (string.IsNullOrEmpty(p_FileName))
        //            {
        //                p_FileName = string.Format("Export_{0}.Docx", "DownloadDriverUsageVehicleRequest");
        //            }
        //            string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, DriverUsageVehicle);
        //            var apOption = new AsposeHelperOption(_asposeOption, _environment);
        //            AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
        //            ReplaceWords replacWords = new ReplaceWords();

        //            //get data from oracle
        //            var data = _fedService.GetDriverUsageVehicleAsync(request);
        //            var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
        //            var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
        //            _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
        //            replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
        //            replacWords.ReplaceNodeText(document, d_header);
        //            replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
        //            document.Save(memoryStream, p_FileName);
        //        }

        //        if (memoryStream != null)
        //            bytes = memoryStream.ToArray();

        //        return File(bytes, "application/octet-stream", p_FileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorResponse = new ErrorResponse
        //        {
        //            Status = "E",
        //            StatusCode = "500",
        //            Message = ex.Message
        //        };
        //        _logger.LogError(ex, ex.Message);
        //        return StatusCode(500, errorResponse);  // Return 500 if an error occurs
        //    }
        //}
        // GET: api/DownloadMachinerUseFED
        //[HttpGet("DownloadMachinerUseFED")]
        //[ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        //[ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        //public async Task<IActionResult> DownloadMachinerUseFED(
        //                          [FromQuery] string? p_Parameter   //  Param ที่ใช้ใน store procedure, Concat param+val with | (pipe) ex. param1=aaa|param2=bbb
        //                        , [FromQuery] string? p_FileName    //  File name ที่จะ export ออกไป
        //                        , [FromQuery] string? p_Template    //  Template Name 
        //                        )
        //{
        //    _logger.LogInformation("Start processing DownloadPrintFormFED");

        //    System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
        //    byte[] bytes = null;
        //    try
        //    {
        //        var result = new SuccessResponse()
        //        {
        //            Status = "S",
        //            StatusCode = "200",
        //            Data = string.Format("Success"),
        //        };

        //        FEDParameterModel request = new FEDParameterModel();
        //        #region set parameter
        //        string[] splitParam = new string[0];
        //        if (!string.IsNullOrEmpty(p_Parameter))
        //            splitParam = p_Parameter.Split(new Char[] { '|' });
        //        foreach (string paramItem in splitParam)
        //        {
        //            string[] paramVal = paramItem.Split('=');
        //            if (paramVal != null && paramVal.Length == 2)
        //            {
        //                switch (paramVal[0])
        //                {
        //                    case "p_HEADER_ID": request.p_HEADER_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_REQUEST_ID": request.p_REQUEST_ID = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                    case "p_DATA": request.p_DATA = string.IsNullOrEmpty(paramVal[1]) ? null : Utilities.CleansingData(paramVal[1]); break;
        //                }
        //            }
        //        }
        //        #endregion
        //        if (p_Template == "MachinerUseTemplate")
        //        {
        //            if (string.IsNullOrEmpty(p_FileName))
        //            {
        //                p_FileName = string.Format("Export_{0}.Docx", "DownloadPrintFormMachinerUse");
        //            }
        //            string contentPath = string.Format("{0}/{1}", this._environment.ContentRootPath, MachineUsage);
        //            var apOption = new AsposeHelperOption(_asposeOption, _environment);
        //            AsposeHelper document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
        //            ReplaceWords replacWords = new ReplaceWords();

        //            // get data from oracle
        //            var data = _fedService.GetMachineUseAsync(request);
        //            var d_header = data.Result == null ? null : replacWords.ConvertDataToReplaceObject(data.Result);
        //            var d_detail = data.Result?.Detail == null ? null : replacWords.ConvertDataToReplaceObject(data.Result.Detail);
        //            _logger.LogInformation("Detail JSON: {result}", JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));
        //            replacWords.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
        //            replacWords.ReplaceNodeText(document, d_header);
        //            replacWords.RemoveRowWithSpecificBookmark(document, "bmDataRow");
        //            document.Save(memoryStream, p_FileName);
        //        }

        //        if (memoryStream != null)
        //            bytes = memoryStream.ToArray();

        //        return File(bytes, "application/octet-stream", p_FileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        var errorResponse = new ErrorResponse
        //        {
        //            Status = "E",
        //            StatusCode = "500",
        //            Message = ex.Message
        //        };
        //        _logger.LogError(ex, ex.Message);
        //        return StatusCode(500, errorResponse);  // Return 500 if an error occurs
        //    }
        //}
        
        
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

                logMessage = $"SUCCESS: Get {configs.Count} rows from EFM_FED.FLEET_CARD_IMPORT_CONFIGS";
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


        // Section New Import Bank Feed Crad TxT

        /// โหลด Delimiter จาก FLEET_CARD_TEMPLATE_NAME
        /// </summary>
        private async Task<string> LoadTemplateDelimiterAsync(string templateName)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT DELIMITER
                    FROM FLEET_CARD_TEMPLATE_NAME
                    WHERE TEMPLATE_NAME = :templateName
                    AND IS_ACTIVE = 1";

                var param = cmd.CreateParameter();
                param.ParameterName = "templateName";
                param.Value = templateName;
                cmd.Parameters.Add(param);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString() ?? "|"; // Default to pipe if not found
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, "Failed to load delimiter", $"Template: {templateName}");
                return "|"; // Default to pipe on error
            }
        }

        /// <summary>
        /// โหลด Template Config จาก FLEET_CARD_IMPORT_CONFIGS
        /// </summary>
        private async Task<List<TemplateFieldConfig>> LoadTemplateConfigAsync(string templateName)
        {
            var configs = new List<TemplateFieldConfig>();

            try
            {
                using var connection = new OracleConnection(_connectionString);
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        FIELD_NAME, 
                        SOURCE_COLUMN_NAME, 
                        SOURCE_COLUMN_INDEX,
                        IS_REQUIRED,
                        FORMAT_DATE,
                        YEAR_TYPE
                    FROM EFM_FED.FLEET_CARD_IMPORT_CONFIGS
                    WHERE TEMPLATE_NAME = :templateName
                    ORDER BY CONFIG_ID";

                var param = cmd.CreateParameter();
                param.ParameterName = "templateName";
                param.Value = templateName;
                cmd.Parameters.Add(param);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    configs.Add(new TemplateFieldConfig
                    {
                        FieldName = reader.GetString(0),
                        SourceColumnName = reader.IsDBNull(1) ? null : reader.GetString(1),
                        SourceColumnIndex = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                        IsRequired = reader.GetInt32(3) == 1,
                        FormatDate = reader.IsDBNull(4) ? null : reader.GetString(4),
                        YearType = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, "Failed to load template config", $"Template: {templateName}");
                throw;
            }

            return configs;
        }

        /// <summary>
        /// Validate row ตาม config
        /// </summary>
        private (bool isValid, string? errorMessage) ValidateRow(string[] headers, string[] row, List<TemplateFieldConfig> configs)
        {
            foreach (var config in configs.Where(c => c.IsRequired))
            {
                var value = GetColumnValue(headers, row, config);
                if (string.IsNullOrWhiteSpace(value))
                {
                    return (false, $"Required field '{config.FieldName}' is missing or empty");
                }
            }
            return (true, null);
        }

        /// <summary>
        /// ดึงค่า column จาก row ตาม config
        /// </summary>
        private string? GetColumnValue(string[] headers, string[] row, TemplateFieldConfig config)
        {
            // ถ้ามี SOURCE_COLUMN_INDEX ใช้ index
            if (config.SourceColumnIndex.HasValue && config.SourceColumnIndex.Value < row.Length)
            {
                return row[config.SourceColumnIndex.Value];
            }

            // ถ้ามี SOURCE_COLUMN_NAME ค้นหา index จาก header
            if (!string.IsNullOrEmpty(config.SourceColumnName))
            {
                var index = Array.FindIndex(headers, h => h.Equals(config.SourceColumnName, StringComparison.OrdinalIgnoreCase));
                if (index >= 0 && index < row.Length)
                {
                    return row[index];
                }
            }

            // ถ้าไม่มีทั้งคู่ ให้ค้นหาจาก FIELD_NAME
            var fieldIndex = Array.FindIndex(headers, h => h.Equals(config.FieldName, StringComparison.OrdinalIgnoreCase));
            if (fieldIndex >= 0 && fieldIndex < row.Length)
            {
                return row[fieldIndex];
            }

            return null;
        }

        /// <summary>
        /// Config class for template fields
        /// </summary>
        private class TemplateFieldConfig
        {
            public string FieldName { get; set; } = string.Empty;
            public string? SourceColumnName { get; set; }
            public int? SourceColumnIndex { get; set; }
            public bool IsRequired { get; set; }
            public string? FormatDate { get; set; }
            public string? YearType { get; set; }
        }


        private async Task<ImportResult> ReadTextFileInternal(string? fileName, string? p_Parameter, string templateName)
        {
            var result = new ImportResult();
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
                
                var FileList = await GetFileFromDatabase(request);
                var File = FileList?.FirstOrDefault();

                if (File == null || string.IsNullOrEmpty(File.CONTENT_VALUE))
                {
                    
                    result.Status = "E";
                    result.Message = "ไม่พบไฟล์ที่เกี่ยวข้อง";
                    return result;
                }

                if (!File.FILE_NAME.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = "E";
                    result.Message = "Only .txt files are supported";
                    return result;
                }

                // Save to temp file
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);
                var filePath = Path.Combine(tempDir, File.FILE_NAME);
                var fileBytes = Convert.FromBase64String(File.CONTENT_VALUE);

                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{
                //    await FileExcel.FILE_CONTENT.CopyToAsync(stream);
                //}                

                // อ่านไฟล์ด้วย Windows-874 (TIS-620) เพื่อรองรับภาษาไทย
                var encoding = Encoding.GetEncoding(874);
                // เขียนเนื้อไฟล์ (string) ลงไฟล์ชั่วคราว
                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                var lines = await System.IO.File.ReadAllLinesAsync(filePath, encoding);
                var records = new List<Dictionary<string, string>>();

                // โหลด delimiter จาก template
                var delimiter = await LoadTemplateDelimiterAsync(templateName);

                if (lines.Length == 0)
                {
                    result.Status = "S";
                    result.Message = "File is empty";
                    result.RecordCount = "0";
                    result.Data = records.ToString();
                    return result;
                }

                // อ่าน header จาก row แรก
                var headers = lines[0].Split(delimiter).Select(h => h.Trim()).ToArray();

                // อ่านข้อมูลจาก row 2 เป็นต้นไป
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(delimiter).Select(v => v.Trim()).ToArray();

                    if (values.Length != headers.Length)
                    {
                        continue; // ข้าม row ที่จำนวน column ไม่ตรง
                    }

                    var record = new Dictionary<string, string>();
                    for (int j = 0; j < headers.Length; j++)
                    {
                        record[headers[j]] = values[j];
                    }
                    records.Add(record);
                }

                result.Status = "S";
                result.FileName = File.FILE_NAME;
                result.RecordCount = "0";
                result.Data = records.ToString();
                

                // Cleanup temp file
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(filePath);
                        if (Directory.Exists(dir)) Directory.Delete(dir, true);
                    }
                    catch { /* Ignore cleanup errors */ }
                }

                return result;
            }
            catch (System.Exception ex)
            {

                result.Status = "E";
                result.Message = ex.Message;
                result.Data = ex.StackTrace;
                return result;
            }
        }

        private async Task ProcessZipInBackgroundAsync(string progressId, string[] textFiles, string zipFileName, string importBatchId, string tempDir, string? headerId , string templateName ,ILoggingService loggingService, IBatchInsertService batchInsert, IProgressTrackingService progressTracking)
        {
            try
            {
                    await loggingService.LogInfoAsync("INFO", $"[{progressId}] ZIP import started: {zipFileName} ({textFiles.Length} files)");

                    var templateConfigs = await LoadTemplateConfigAsync(templateName);
                    if (templateConfigs.Count == 0)
                    {
                        progressTracking.SetError(progressId, $"Template config '{templateName}' not found");
                        await loggingService.LogErrorAsync("ERROR", new Exception("Template config not found"), $"[{progressId}] Template config not found", templateName);
                        return;
                    }

                    var encoding = Encoding.GetEncoding(874);
                    var totalProcessed = 0;
                    var totalInserted = 0;
                    var totalFailed = 0;
                    var allErrors = new List<object>();

                    // โหลด delimiter จาก template
                    var delimiter = await LoadTemplateDelimiterAsync(templateName);

                foreach (var txtFilePath in textFiles)
                    {
                        var fileName = Path.GetFileName(txtFilePath);
                        await loggingService.LogInfoAsync("INFO", $"[{progressId}] Processing file: {fileName}");

                        using var reader = new StreamReader(txtFilePath, encoding);
                        var headerLine = await reader.ReadLineAsync();
                        if (string.IsNullOrEmpty(headerLine))
                        {
                            await loggingService.LogInfoAsync("WARNING", $"[{progressId}] Empty file: {fileName}");
                            continue;
                        }

                        var headers = headerLine.Split(delimiter).Select(h => h.Trim()).ToArray();
                        var fileInfo = new FileInfo(txtFilePath);
                        var fileMeta = new
                        {
                            FileName = fileName,
                            FileSize = fileInfo.Length,
                            CreatedDate = fileInfo.CreationTime,
                            ModifiedDate = fileInfo.LastWriteTime
                        };

                        var batch = new List<string[]>(1000);
                        var fileProcessed = 0;

                        string? line;
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            var row = line.Split(delimiter).Select(v => v.Trim()).ToArray();

                            var (isValid, validationError) = ValidateRow(headers, row, templateConfigs);
                            if (!isValid)
                            {
                                allErrors.Add(new { file = fileName, row = fileProcessed + 1, error = validationError });
                                await _loggingService.LogErrorAsync(
                                    "ERROR",
                                    new Exception(validationError ?? "Validation failed"),
                                    "Validation failed",
                                    $"File={fileName};Row={fileProcessed + 1};Template={templateName};Batch={importBatchId};ProgressId={progressId}");
                            fileProcessed++;
                                totalProcessed++;
                                continue;
                            }

                            batch.Add(row);
                            fileProcessed++;
                            totalProcessed++;

                            if (batch.Count >= 1000)
                            {
                                try
                                {
                                    var (inserted, failed, batchErrors) = await batchInsert.InsertBatchAsync(
                                        "EFM_FED.FLEET_CARD_TEMP_TRANS_TEXT",
                                        headers,
                                        batch,
                                        importBatchId,
                                        fileMeta,
                                        (h, r) =>
                                        {
                                            var cardNoIndex = Array.FindIndex(h, x => x.Equals("CARD_NO", StringComparison.OrdinalIgnoreCase));
                                            if (cardNoIndex >= 0 && cardNoIndex < r.Length && r[cardNoIndex].Length > 50)
                                                return (false, $"CARD_NO too long ({r[cardNoIndex].Length} chars)");
                                            return (true, null);
                                        },
                                        headerId,
                                        templateName
                                    );
                                    totalInserted += inserted;
                                    totalFailed += failed;
                                    allErrors.AddRange(batchErrors);
                                    progressTracking.UpdateProgress(progressId, totalProcessed);
                                }
                                catch (Exception ex)
                                {
                                    await loggingService.LogErrorAsync("ERROR", ex, $"[{progressId}] Batch insert failed - {fileName}");
                                    allErrors.Add(new { file = fileName, row = fileProcessed, error = ex.Message });
                                }
                                batch.Clear();
                            }
                        }

                        if (batch.Count > 0)
                        {
                            try
                            {
                                var (inserted, failed, batchErrors) = await batchInsert.InsertBatchAsync(
                                    "EFM_FED.FLEET_CARD_TEMP_TRANS_TEXT",
                                    headers,
                                    batch,
                                    importBatchId,
                                    fileMeta,
                                    (h, r) =>
                                    {
                                        var cardNoIndex = Array.FindIndex(h, x => x.Equals("CARD_NO", StringComparison.OrdinalIgnoreCase));
                                        if (cardNoIndex >= 0 && cardNoIndex < r.Length && r[cardNoIndex].Length > 50)
                                            return (false, $"CARD_NO too long ({r[cardNoIndex].Length} chars)");
                                        return (true, null);
                                    },
                                    headerId,
                                    templateName
                                );
                                totalInserted += inserted;
                                totalFailed += failed;
                                allErrors.AddRange(batchErrors);
                                progressTracking.UpdateProgress(progressId, totalProcessed);
                            }
                            catch (Exception ex)
                            {
                                await loggingService.LogErrorAsync("ERROR", ex, $"[{progressId}] Final batch failed - {fileName}");
                                allErrors.Add(new { file = fileName, error = ex.Message });
                            }
                        }

                        await loggingService.LogInfoAsync("INFO", $"[{progressId}] Completed file: {fileName} ({fileProcessed} rows)");
                    }

                    progressTracking.CompleteProgress(progressId, totalInserted, totalFailed, allErrors);
                    await loggingService.LogInfoAsync("INFO", $"[{progressId}] ZIP import completed: {zipFileName} - {totalProcessed} rows processed");
                }
                catch (Exception ex)
                {
                    await loggingService.LogErrorAsync("ERROR", ex, $"[{progressId}] ProcessZipInBackgroundAsync failed");
                    progressTracking.SetError(progressId, ex.Message);
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(tempDir))
                        {
                            Directory.Delete(tempDir, true);
                            await loggingService.LogInfoAsync("INFO", $"[{progressId}] Temp directory cleaned up: {tempDir}");
                        }
                    }
                    catch (Exception ex)
                    {
                        await loggingService.LogInfoAsync("WARNING", $"[{progressId}] Failed to cleanup temp directory: {ex.Message}");
                    }
                }
        }

        private async Task ProcessTextInBackgroundAsync(string progressId, string filePath, string fileName, string? importBatchId, string? headerId = null, string templateName = "TEXT_TEMPLATE")
        {
            try
            {
                await _loggingService.LogInfoAsync("INFO", $"[{progressId}] Text import started: {fileName}");

                // Load template config
                var templateConfigs = await LoadTemplateConfigAsync(templateName);
                if (templateConfigs.Count == 0)
                {
                    _progressTracking.SetError(progressId, $"Template config '{templateName}' not found");
                    await _loggingService.LogErrorAsync("ERROR", new Exception("Template config not found"), $"[{progressId}] Template config not found", templateName);
                    return;
                }

                var encoding = Encoding.GetEncoding(874);

                // โหลด delimiter จาก template
                var delimiter = await LoadTemplateDelimiterAsync(templateName);

                using var reader = new StreamReader(filePath, encoding);
                var headerLine = await reader.ReadLineAsync();
                if (string.IsNullOrEmpty(headerLine))
                {
                    _progressTracking.SetError(progressId, "Empty file or missing header");
                    return;
                }

                var headers = headerLine.Split(delimiter).Select(h => h.Trim()).ToArray();
                var fileInfo = new FileInfo(filePath);
                var fileMeta = new
                {
                    FileName = fileName,
                    FileSize = fileInfo.Length,
                    CreatedDate = fileInfo.CreationTime,
                    ModifiedDate = fileInfo.LastWriteTime
                };

                var batch = new List<string[]>(1000);
                var processedRows = 0;
                var errorList = new List<object>();

                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var row = line.Split(delimiter).Select(v => v.Trim()).ToArray();

                    // Validate row ตาม config
                    var (isValid, validationError) = ValidateRow(headers, row, templateConfigs);
                    if (!isValid)
                    {
                        errorList.Add(new { row = processedRows + 1, error = validationError });
                        await _loggingService.LogErrorAsync(
                            "ERROR",
                            new Exception(validationError ?? "Validation failed"),
                            "Validation failed",
                            $"File={fileName};Row={processedRows + 1};Template={templateName};Batch={importBatchId};ProgressId={progressId}");
                        processedRows++;
                        continue;
                    }

                    batch.Add(row);
                    processedRows++;

                    if (batch.Count >= 1000)
                    {
                        try
                        {
                            var (batchInserted, batchFailed, batchErrors) = await _batchInsert.InsertBatchAsync(
                                "EFM_FED.FLEET_CARD_TEMP_TRANS_TEXT",
                                headers,
                                batch,
                                importBatchId ?? $"BATCH_{DateTime.Now:yyyyMMddHHmmss}",
                                fileMeta,
                                (h, r) =>
                                {
                                    // Additional validation
                                    var cardNoIndex = Array.FindIndex(h, x => x.Equals("CARD_NO", StringComparison.OrdinalIgnoreCase));
                                    if (cardNoIndex >= 0 && cardNoIndex < r.Length && r[cardNoIndex].Length > 50)
                                    {
                                        return (false, $"CARD_NO too long ({r[cardNoIndex].Length} chars)");
                                    }
                                    return (true, null);
                                },
                                headerId,
                                templateName
                            );
                            errorList.AddRange(batchErrors);
                            _progressTracking.UpdateProgress(progressId, processedRows);
                        }
                        catch (Exception ex)
                        {
                            await _loggingService.LogErrorAsync("ERROR", ex, $"[{progressId}] Batch insert failed at row {processedRows}");
                            errorList.Add(new { row = processedRows, error = ex.Message });
                        }
                        batch.Clear();
                    }
                }

                // Insert remaining rows
                if (batch.Count > 0)
                {
                    try
                    {
                        var (batchInserted2, batchFailed2, batchErrors2) = await _batchInsert.InsertBatchAsync(
                            "EFM_FED.FLEET_CARD_TEMP_TRANS_TEXT",
                            headers,
                            batch,
                            importBatchId ?? $"BATCH_{DateTime.Now:yyyyMMddHHmmss}",
                            fileMeta,
                            (h, r) =>
                            {
                                var cardNoIndex = Array.FindIndex(h, x => x.Equals("CARD_NO", StringComparison.OrdinalIgnoreCase));
                                if (cardNoIndex >= 0 && cardNoIndex < r.Length && r[cardNoIndex].Length > 50)
                                {
                                    return (false, $"CARD_NO too long ({r[cardNoIndex].Length} chars)");
                                }
                                return (true, null);
                            },
                            headerId,
                            templateName
                        );
                        errorList.AddRange(batchErrors2);
                        _progressTracking.UpdateProgress(progressId, processedRows);
                    }
                    catch (Exception ex)
                    {
                        await _loggingService.LogErrorAsync("ERROR", ex, $"[{progressId}] Final batch insert failed");
                        errorList.Add(new { row = processedRows, error = ex.Message });
                    }
                }

                var totalErrors = errorList.Count;
                var inserted = processedRows - totalErrors;
                _progressTracking.CompleteProgress(progressId, inserted, totalErrors, errorList);
                await _loggingService.LogInfoAsync("INFO", $"[{progressId}] Text import completed: {processedRows} rows processed");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, $"[{progressId}] ProcessTextInBackgroundAsync failed");
                _progressTracking.SetError(progressId, ex.Message);
            }
        }

        private static string NormalizeHeader(string header)
        {
            // แปลง SPENDING_01_MM_YYYY_TO_DD_MM_YYYY → SPEND_MMM_YYYY
            var match = Regex.Match(header, @"^SPENDING_01_(\d{2})_(\d{4})_TO_\d{2}_\d{2}_\d{4}$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var monthNum = match.Groups[1].Value;
                var year = match.Groups[2].Value;
                if (MonthAbbr.TryGetValue(monthNum, out var monthAbbr))
                {
                    return $"SPEND_{monthAbbr}_{year}";
                }
            }
            return header.Trim().ToUpperInvariant();
        }

        private async Task<List<T_TEMP_FED_FILE>> GetFileFromDatabase(FEDParameterModel request)
        {

            var p1 = request.p_TEMP_ID ?? (object)DBNull.Value;

            var result = await _oracleContext
                    .Set<T_TEMP_FED_FILE>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7404_SP_GET_FILE_EXCEL (
                                            :P_TEMP_ID,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_TEMP_ID", request.p_TEMP_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
            return result;

        }

        private static (bool isValid, string? errorMessage) ValidateRow(string[] headers, string[] values)
        {
            // หา index ของ CARD_NO
            var cardNoIndex = Array.FindIndex(headers, h =>
                string.Equals(h.Trim(), "CARD_NO", StringComparison.OrdinalIgnoreCase));

            if (cardNoIndex >= 0 && cardNoIndex < values.Length)
            {
                var cardNo = values[cardNoIndex]?.Trim();
                if (!string.IsNullOrEmpty(cardNo) && cardNo.Length != 16)
                {
                    return (false, $"CARD_NO must be 16 digits, got: {cardNo.Length} digits");
                }
            }

            return (true, null);
        }

        private async Task<(int inserted, int failed, List<object> errors, string importBatchId, string? headerId)> InsertRowsAsync(
            string tableName,
            string[] headers,
            List<string[]> rows,
            string importBatchId,
            dynamic fileMeta,
            string? headerId = null,
            string templateName = "TEXT_TEMPLATE")
        {
            using var conn = new OracleConnection(_connectionString);

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
            }
            // Load template config
            var templateConfigs = await LoadTemplateConfigAsync(templateName);
            if (templateConfigs.Count == 0)
            {
                throw new Exception($"Template config '{templateName}' not found");
            }

            // Load actual table columns (uppercase)
            var validColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmdCols = conn.CreateCommand())
            {
                cmdCols.CommandText = "SELECT COLUMN_NAME FROM USER_TAB_COLUMNS WHERE TABLE_NAME = :t";
                var p = cmdCols.CreateParameter();
                p.ParameterName = ":t";
                p.Value = tableName.ToUpperInvariant();
                cmdCols.Parameters.Add(p);
                using var reader = await cmdCols.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    validColumns.Add(reader.GetString(0));
                }
            }

            // Build column mapping from template config: FIELD_NAME -> file column index
            var columnMapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var config in templateConfigs)
            {
                if (string.IsNullOrEmpty(config.SourceColumnName)) continue;

                // Find header index matching SOURCE_COLUMN_NAME
                var idx = Array.FindIndex(headers, h =>
                    h.Equals(config.SourceColumnName, StringComparison.OrdinalIgnoreCase));

                if (idx >= 0 && validColumns.Contains(config.FieldName))
                {
                    columnMapping[config.FieldName] = idx;
                }
            }

            var headerCols = columnMapping.Keys.ToList();

            // Optional metadata columns if exist
            bool hasFileName = validColumns.Contains("FILE_NAME");
            bool hasFileSize = validColumns.Contains("FILE_SIZE");
            bool hasCreated = validColumns.Contains("FILE_CREATED_DATE");
            bool hasModified = validColumns.Contains("FILE_LAST_MODIFIED_DATE");
            bool hasBatch = validColumns.Contains("IMPORT_BATCH_ID");
            bool hasHeaderId = validColumns.Contains("HEADER_ID");
            bool hasTemplateName = validColumns.Contains("TEMPLATE_NAME");

            var metaCols = new List<string>();
            if (hasBatch) metaCols.Add("IMPORT_BATCH_ID");
            if (hasHeaderId) metaCols.Add("HEADER_ID");
            if (hasTemplateName) metaCols.Add("TEMPLATE_NAME");
            if (hasFileName) metaCols.Add("FILE_NAME");
            if (hasFileSize) metaCols.Add("FILE_SIZE");
            if (hasCreated) metaCols.Add("FILE_CREATED_DATE");
            if (hasModified) metaCols.Add("FILE_LAST_MODIFIED_DATE");

            int inserted = 0;
            int failed = 0;
            var errors = new List<object>();

            foreach (var row in rows)
            {
                // Validate row ตาม config
                var (isValid, validationError) = ValidateRow(headers, row, templateConfigs);
                if (!isValid)
                {
                    failed++;
                    if (errors.Count < 20)
                    {
                        errors.Add(new { row = inserted + failed, error = validationError });
                    }
                    continue;
                }

                try
                {
                    using var cmd = conn.CreateCommand();

                    var allCols = metaCols.Concat(headerCols).ToList();
                    var paramNames = new List<string>();

                    foreach (var col in allCols)
                    {
                        var param = cmd.CreateParameter();
                        param.ParameterName = ":" + col;
                        if (col == "IMPORT_BATCH_ID") param.Value = importBatchId;
                        else if (col == "HEADER_ID") param.Value = !string.IsNullOrEmpty(headerId) ? (object)headerId : DBNull.Value;
                        else if (col == "TEMPLATE_NAME") param.Value = templateName;
                        else if (col == "FILE_NAME") param.Value = (object)fileMeta.FileName ?? DBNull.Value;
                        else if (col == "FILE_SIZE") param.Value = (object)Convert.ToDecimal(fileMeta.FileSize) ?? DBNull.Value;
                        else if (col == "FILE_CREATED_DATE") param.Value = (object)(DateTime)fileMeta.FileCreatedDate;
                        else if (col == "FILE_LAST_MODIFIED_DATE") param.Value = (object)(DateTime)fileMeta.FileLastModifiedDate;
                        else
                        {
                            // Use columnMapping to get file column index from FIELD_NAME
                            if (columnMapping.TryGetValue(col, out int idx))
                            {
                                var rawValue = idx < row.Length ? row[idx] : null;

                                // Check if this field has date format config
                                var fieldConfig = templateConfigs.FirstOrDefault(c =>
                                    c.FieldName.Equals(col, StringComparison.OrdinalIgnoreCase));

                                if (fieldConfig != null && !string.IsNullOrWhiteSpace(fieldConfig.FormatDate) && !string.IsNullOrWhiteSpace(rawValue))
                                {
                                    try
                                    {
                                        // Parse date according to FORMAT_DATE
                                        var dateValue = DateTime.ParseExact(rawValue, fieldConfig.FormatDate, System.Globalization.CultureInfo.InvariantCulture);

                                        // Convert BE (Buddhist Era) to AD (Christian Era)
                                        if (fieldConfig.YearType?.Equals("BE", StringComparison.OrdinalIgnoreCase) == true)
                                        {
                                            dateValue = dateValue.AddYears(-543);
                                        }

                                        param.Value = dateValue;
                                    }
                                    catch
                                    {
                                        // If parsing fails, store as string
                                        param.Value = (object)rawValue ?? DBNull.Value;
                                    }
                                }
                                else
                                {
                                    param.Value = (object)rawValue ?? DBNull.Value;
                                }
                            }
                            else
                            {
                                param.Value = DBNull.Value;
                            }
                        }
                        cmd.Parameters.Add(param);
                        paramNames.Add(param.ParameterName);
                    }

                    cmd.CommandText = $"INSERT INTO {tableName} (" + string.Join(",", allCols) + ") VALUES (" + string.Join(",", paramNames) + ")";
                    await cmd.ExecuteNonQueryAsync();
                    inserted++;
                }
                catch (Exception ex)
                {
                    failed++;
                    if (errors.Count < 20)
                    {
                        errors.Add(new { row = inserted + failed, error = ex.Message });
                    }
                    await _loggingService.LogErrorAsync(
                        "ERROR",
                        new Exception(validationError ?? "Validation failed"),
                        "Validation failed",
                        $"File={fileMeta.FileName};Row={inserted + failed};Template={templateName};Batch={importBatchId};HeaderId={headerId ?? ""}");
                    continue;
                }
            }

            return (inserted, failed, errors, importBatchId, headerId);
        }

        /// <summary>
        /// อ่านไฟล์ TEXT และแปลงเป็น JSON
        /// POST: api/TextFileReader/read (upload file via form-data)
        /// </summary>
        [HttpPost("ReadTextFile")]
        public async Task<ImportResult> ReadTextFile([FromQuery] string? p_Parameter, [FromQuery] string p_templateName = null)
        {
            p_templateName = p_templateName ?? "TEXT_TEMPLATE";
            //[FromForm] IFormFile? file
            var result = await ReadTextFileInternal(null, p_Parameter, p_templateName);
            return result;
        }

        /// <summary>
        /// อ่านไฟล์ TEXT จาก Base64 string และแปลงเป็น JSON
        /// POST: api/TextFileReader/read/base64
        /// </summary>
        /// <param name="base64File">Base64 encoded string ของไฟล์</param>
        /// <param name="fileName">ชื่อไฟล์ (ต้องลงท้ายด้วย .txt)</param>
        [HttpPost("ReadTextFileBase64")]
        public async Task<ImportResult> ReadTextFileBase64([FromQuery] string? p_Parameter, [FromQuery] string p_templateName = null)
        {
            p_templateName = p_templateName ?? "TEXT_TEMPLATE";
            var result = new ImportResult();
           
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

                var FileList = await GetFileFromDatabase(request);
                var File = FileList?.FirstOrDefault();

                if (File == null || string.IsNullOrEmpty(File.CONTENT_VALUE))
                {

                    result.Status = "E";
                    result.Message = "ไม่พบไฟล์ที่เกี่ยวข้อง";
                    return result;
                }

                if (!File.FILE_NAME.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = "E";
                    result.Message = "Only .txt files are supported";
                    return result;
                }
                                
                // Decode base64 to bytes
                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(File.CONTENT_VALUE);
                }
                catch (FormatException)
                {
                    result.Status = "E";
                    result.Message = "Invalid base64 format";
                    return result;
                }

                // Save to temp file
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);
                var filePath = Path.Combine(tempDir, File.FILE_NAME);

                await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                var records = new List<Dictionary<string, string>>();

                // โหลด delimiter จาก template
                var delimiter = await LoadTemplateDelimiterAsync(p_templateName);


                // อ่านไฟล์ด้วย Windows-874 (TIS-620) เพื่อรองรับภาษาไทย
                var encoding = Encoding.GetEncoding(874);
                var lines = await System.IO.File.ReadAllLinesAsync(filePath, encoding);

                if (lines.Length == 0)
                {
                    result.Status = "S";
                    result.Message = "File is empty";
                    result.RecordCount = "0";
                    result.Data = records.ToString();
                    return result;
                }

                // อ่าน header จาก row แรก
                var headers = lines[0].Split(delimiter).Select(h => h.Trim()).ToArray();

                // อ่านข้อมูลจาก row 2 เป็นต้นไป
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(delimiter).Select(v => v.Trim()).ToArray();

                    if (values.Length != headers.Length)
                    {
                        continue; // ข้าม row ที่จำนวน column ไม่ตรง
                    }

                    var record = new Dictionary<string, string>();
                    for (int j = 0; j < headers.Length; j++)
                    {
                        record[headers[j]] = values[j];
                    }
                    records.Add(record);
                }
                result.Status = "S";
                result.FileName = File.FILE_NAME;
                result.ColumnCount = headers.Length.ToString();
                result.Headers = headers.ToString();
                result.RecordCount = records.Count.ToString();
                result.Data = records.ToString();
                

                // Cleanup temp file
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(filePath);
                        if (Directory.Exists(dir)) Directory.Delete(dir, true);
                    }
                    catch { /* Ignore cleanup errors */ }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Status = "E";
                result.Message = ex.Message;
                result.Data = ex.StackTrace;
                return result;
            }
        }

        /// <summary>
        /// อ่านไฟล์ ZIP ที่มี TEXT files หลายไฟล์และแปลงเป็น JSON
        /// POST: api/TextFileReader/read-zip (upload ZIP file via form-data)
        /// </summary>
        [HttpPost("read-zip")]
        public async Task<IActionResult> ReadZipFile([FromForm] IFormFile? zipFile, [FromQuery] string p_templateName = null)
        {
            p_templateName = p_templateName ?? "TEXT_TEMPLATE";

            try
            {
                if (zipFile == null || zipFile.Length == 0)
                {
                    return BadRequest(new { success = false, message = "ZIP file is required" });
                }

                if (!zipFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { success = false, message = "Only ZIP files are supported" });
                }

                // สร้าง temp directory สำหรับ extract
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                try
                {
                    // Save ZIP file to temp
                    var tempZipPath = Path.Combine(tempDir, zipFile.FileName);
                    using (var stream = new FileStream(tempZipPath, FileMode.Create))
                    {
                        await zipFile.CopyToAsync(stream);
                    }

                    // Extract ZIP
                    var extractDir = Path.Combine(tempDir, "extracted");
                    ZipFile.ExtractToDirectory(tempZipPath, extractDir);

                    // หา TEXT files ทั้งหมดใน ZIP
                    var textFiles = Directory.GetFiles(extractDir, "*.txt", SearchOption.AllDirectories);

                    if (textFiles.Length == 0)
                    {
                        return BadRequest(new { success = false, message = "No TEXT files found in ZIP" });
                    }

                    var encoding = Encoding.GetEncoding(874);
                    var allFilesData = new List<object>();

                    // โหลด delimiter จาก template
                    var delimiter = await LoadTemplateDelimiterAsync(p_templateName);

                    foreach (var txtFilePath in textFiles)
                    {
                        var fileName = Path.GetFileName(txtFilePath);
                        var lines = await System.IO.File.ReadAllLinesAsync(txtFilePath, encoding);

                        if (lines.Length == 0)
                        {
                            allFilesData.Add(new
                            {
                                fileName = fileName,
                                success = true,
                                message = "File is empty",
                                recordCount = 0,
                                data = new List<object>()
                            });
                            continue;
                        }

                        var headers = lines[0].Split(delimiter).Select(h => h.Trim()).ToArray();
                        var records = new List<Dictionary<string, string>>();

                        for (int i = 1; i < lines.Length; i++)
                        {
                            var values = lines[i].Split(delimiter).Select(v => v.Trim()).ToArray();
                            if (values.Length != headers.Length) continue;

                            var record = new Dictionary<string, string>();
                            for (int j = 0; j < headers.Length; j++)
                            {
                                record[headers[j]] = values[j];
                            }
                            records.Add(record);
                        }

                        allFilesData.Add(new
                        {
                            fileName = fileName,
                            success = true,
                            columnCount = headers.Length,
                            headers = headers,
                            recordCount = records.Count,
                            data = records
                        });
                    }

                    var result = new
                    {
                        success = true,
                        zipFileName = zipFile.FileName,
                        filesCount = textFiles.Length,
                        files = allFilesData
                    };

                    // Cleanup temp directory
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    // Cleanup on error
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// อ่านไฟล์ ZIP จาก Base64 string ที่มี TEXT files หลายไฟล์
        /// POST: api/TextFileReader/read-zip/base64
        /// </summary>
        [HttpPost("ReadZipFileBase64")]
        public async Task<ImportResult> ReadZipFileBase64([FromQuery] string? p_Parameter, [FromQuery] string p_templateName = null)
        {
            p_templateName = p_templateName ?? "TEXT_TEMPLATE";
            var result = new ImportResult();
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

                var FileList = await GetFileFromDatabase(request);
                var File = FileList?.FirstOrDefault();

                if (File == null || string.IsNullOrEmpty(File.CONTENT_VALUE))
                {

                    result.Status = "E";
                    result.Message = "ไม่พบไฟล์ที่เกี่ยวข้อง";
                    return result;
                }

                if (!File.FILE_NAME.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = "E";
                    result.Message = "Only .zip files are supported";
                    return result;
                }
                

                // Decode base64 to bytes
                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(File.CONTENT_VALUE);
                }
                catch (FormatException)
                {
                    result.Status = "E";
                    result.Message = "Invalid base64 format";
                    return result;
                }

                // สร้าง temp directory
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                try
                {
                    // Save ZIP file
                    var tempZipPath = Path.Combine(tempDir, File.FILE_NAME);
                    await System.IO.File.WriteAllBytesAsync(tempZipPath, fileBytes);

                    // Extract ZIP
                    var extractDir = Path.Combine(tempDir, "extracted");
                    ZipFile.ExtractToDirectory(tempZipPath, extractDir);

                    // หา TEXT files ทั้งหมดใน ZIP
                    var textFiles = Directory.GetFiles(extractDir, "*.txt", SearchOption.AllDirectories);

                    if (textFiles.Length == 0)
                    {
                        result.Status = "E";
                        result.Message = "No TEXT files found in ZIP";
                        return result;
                    }

                    var encoding = Encoding.GetEncoding(874);
                    var allFilesData = new List<object>();

                    // โหลด delimiter จาก template
                    var delimiter = await LoadTemplateDelimiterAsync(p_templateName);

                    foreach (var txtFilePath in textFiles)
                    {
                        var txtFileName = Path.GetFileName(txtFilePath);
                        var lines = await System.IO.File.ReadAllLinesAsync(txtFilePath, encoding);

                        if (lines.Length == 0)
                        {
                            allFilesData.Add(new
                            {
                                fileName = txtFileName,
                                success = true,
                                message = "File is empty",
                                recordCount = 0,
                                data = new List<object>()
                            });
                            continue;
                        }

                        var headers = lines[0].Split(delimiter).Select(h => h.Trim()).ToArray();
                        var records = new List<Dictionary<string, string>>();

                        for (int i = 1; i < lines.Length; i++)
                        {
                            var values = lines[i].Split(delimiter).Select(v => v.Trim()).ToArray();
                            if (values.Length != headers.Length) continue;

                            var record = new Dictionary<string, string>();
                            for (int j = 0; j < headers.Length; j++)
                            {
                                record[headers[j]] = values[j];
                            }
                            records.Add(record);
                        }

                        allFilesData.Add(new
                        {
                            fileName = txtFileName,
                            success = true,
                            columnCount = headers.Length,
                            headers = headers,
                            recordCount = records.Count,
                            data = records
                        });
                    }
                    result.Status = "S";
                    result.FileName = File.FILE_NAME;
                    result.FilesCount = textFiles.Length.ToString();
                    result.Data = allFilesData.ToString();
                   

                    // Cleanup temp directory
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    // Cleanup on error
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                    result.Status = "E";
                    result.Message = ex.Message;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Status = "E";
                result.Message = ex.Message;
                result.Data = ex.StackTrace;
                return result;
                
            }
        }

        /// <summary>
        /// Import TEXT file แบบ async สำหรับไฟล์ขนาดใหญ่
        /// </summary>
        /// <param name="fileName">ชื่อไฟล์ เช่น myfile.txt</param>
        /// <param name="file">ไฟล์ที่ต้องการ upload (optional)</param>
        /// <param name="importBatchId">Batch ID (optional) เช่น BATCH_2025_001</param>
        /// <param name="headerId">Header ID (optional) รองรับ VARCHAR2(36) เช่น 550e8400-e29b-41d4-a716-446655440000 หรือ HDR-001</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/TextFileReader/import-async?fileName=test.txt&headerId=550e8400-e29b-41d4-a716-446655440000
        ///     
        /// หรือ
        ///     
        ///     POST /api/TextFileReader/import-async?fileName=test.txt&headerId=BATCH-2025-001
        ///     
        /// </remarks>
        [HttpPost("import-async")]
        public async Task<IActionResult> ImportTextAsync(
            [FromForm] string? fileName,
            [FromForm] IFormFile? file,
            [FromForm] string? importBatchId = null,
            [FromForm] string? headerId = null,
            [FromForm] string templateName = "TEXT_TEMPLATE")
        {
            try
            {
                string filePath;
                string actualFileName;
                bool isUploadedFile = false;

                if (file != null)
                {
                    // Upload mode
                    if (file.Length == 0)
                    {
                        return BadRequest(new { success = false, message = "Uploaded file is empty" });
                    }

                    if (!file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        return BadRequest(new { success = false, message = "Only .txt files are supported" });
                    }

                    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);
                    filePath = Path.Combine(tempDir, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    actualFileName = file.FileName;
                    isUploadedFile = true;
                }
                else if (!string.IsNullOrEmpty(fileName))
                {
                    // File path mode
                    var basePath = Path.Combine(Directory.GetCurrentDirectory(), "SimpleFile", "TextFile");
                    filePath = Path.Combine(basePath, fileName);
                    actualFileName = fileName;

                    if (!System.IO.File.Exists(filePath))
                    {
                        return NotFound(new { success = false, message = "File not found", path = filePath });
                    }
                }
                else
                {
                    return BadRequest(new { success = false, message = "Either fileName parameter or file upload is required" });
                }

                // นับจำนวนแถวทั้งหมดก่อน (เพื่อแสดง progress)
                var totalRows = 0;
                var encoding = Encoding.GetEncoding(874);
                using (var counter = new StreamReader(filePath, encoding))
                {
                    var header = await counter.ReadLineAsync();
                    while (await counter.ReadLineAsync() != null)
                    {
                        totalRows++;
                    }
                }

                // สร้าง progressId และ initialize progress
                var progressId = Guid.NewGuid().ToString();
                _progressTracking.InitializeProgress(progressId, totalRows);

                // Start background processing
                var batchId = importBatchId ?? Guid.NewGuid().ToString();
                _ = Task.Run(async () =>
                {
                    await ProcessTextInBackgroundAsync(progressId, filePath, actualFileName, batchId, headerId, templateName);

                    // Cleanup temp file if uploaded
                    if (isUploadedFile && System.IO.File.Exists(filePath))
                    {
                        try
                        {
                            var dir = Path.GetDirectoryName(filePath);
                            if (Directory.Exists(dir)) Directory.Delete(dir, true);
                        }
                        catch { /* Ignore cleanup errors */ }
                    }
                });

                return Ok(new
                {
                    success = true,
                    message = "Import started",
                    progressId = progressId,
                    totalRows = totalRows,
                    fileName = actualFileName,
                    importBatchId = batchId,
                    headerId = headerId,
                    source = isUploadedFile ? "uploaded" : "local"
                });
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, "ImportTextAsync failed", $"File: {fileName}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Import TEXT file จาก Base64 string แบบ async
        /// </summary>
        /// <param name="base64File">Base64 encoded TEXT file</param>
        /// <param name="fileName">ชื่อไฟล์</param>
        /// <param name="importBatchId">Batch ID (optional)</param>
        /// <param name="headerId">Header ID (optional) รองรับ VARCHAR2(36)</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/TextFileReader/import-async/base64
        ///     Content-Type: multipart/form-data
        ///     base64File: UE9TVCAvYXBpL1RleHRGaWxlUmVhZGVyL2ltcG9ydC1hc3luYy9iYXNlNjQ=
        ///     fileName: test.txt
        ///     importBatchId: BATCH-001 (optional)
        ///     headerId: 550e8400-e29b-41d4-a716-446655440000 (optional)
        ///     
        /// </remarks>
        [HttpPost("ImportTextBase64Async")]
        public async Task<ImportResult> ImportTextBase64Async(
            [FromQuery] string? p_Parameter,
            [FromQuery] string? p_importBatchId = null,
            [FromQuery] string? p_headerId = null,
            [FromQuery] string p_templateName = null)
        {
            var result = new ImportResult();
            string fileName ="";
            p_templateName = p_templateName ?? "TEXT_TEMPLATE";

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

                var FileList = await GetFileFromDatabase(request);
                var File = FileList?.FirstOrDefault();
                fileName = File.FILE_NAME;

                if (File == null || string.IsNullOrEmpty(File.CONTENT_VALUE))
                {

                    result.Status = "E";
                    result.Message = "ไม่พบไฟล์ที่เกี่ยวข้อง";
                    return result;
                }

                if (!File.FILE_NAME.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = "E";
                    result.Message = "Only .txt files are supported";
                    return result;
                }
                
                // Decode Base64
                byte[] fileBytes;
                try
                {
                    fileBytes = Convert.FromBase64String(File.CONTENT_VALUE);
                }
                catch (FormatException)
                {
                    result.Status = "E";
                    result.Message = "Invalid Base64 string";
                    return result;
                }

                if (fileBytes.Length == 0)
                {
                    result.Status = "E";
                    result.Message = "Decoded file is empty";
                    return result;
                }

                // Create temp directory and save file
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);
                var filePath = Path.Combine(tempDir, File.FILE_NAME);

                try
                {
                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    // นับจำนวนแถวทั้งหมดก่อน
                    var totalRows = 0;
                    var encoding = Encoding.GetEncoding(874);
                    using (var counter = new StreamReader(filePath, encoding))
                    {
                        var header = await counter.ReadLineAsync();
                        while (await counter.ReadLineAsync() != null)
                        {
                            totalRows++;
                        }
                    }

                    // สร้าง progressId และ initialize progress
                    var progressId = Guid.NewGuid().ToString();
                    _progressTracking.InitializeProgress(progressId, totalRows);

                    // Start background processing
                    var batchId = p_importBatchId ?? Guid.NewGuid().ToString();
                    _ = Task.Run(async () =>
                    {
                        await ProcessTextInBackgroundAsync(progressId, filePath, File.FILE_NAME, batchId, p_headerId, p_templateName);

                        // Cleanup temp file
                        if (System.IO.File.Exists(filePath))
                        {
                            try
                            {
                                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                            }
                            catch { /* Ignore cleanup errors */ }
                        }
                    });

                    result.Status = "S";
                    result.Message = "Import started";
                    result.ProgressId = progressId;
                    result.TotalRows = totalRows.ToString();
                    result.FileName = File.FILE_NAME;
                    result.ImportBatchId = batchId;
                    result.Headers = p_headerId;
                    result.Source = "base64";

                }
                catch (Exception ex)
                {
                    // Cleanup on error
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                    result.Status = "E";
                    result.Message = ex.Message;
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, "ImportTextBase64Async failed", $"File: {fileName}");
                result.Status = "E";
                result.Message = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Upload และ Import ZIP file ที่มี TEXT files หลายไฟล์
        /// </summary>
        /// <param name="zipFile">ไฟล์ ZIP ที่มีไฟล์ TEXT หลายไฟล์</param>
        /// <param name="importBatchId">Batch ID (optional)</param>
        /// <param name="headerId">Header ID (optional) รองรับ VARCHAR2(36)</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/TextFileReader/import-zip?headerId=550e8400-e29b-41d4-a716-446655440000
        ///     Content-Type: multipart/form-data
        ///     zipFile: [ZIP file containing multiple TEXT files]
        ///     
        /// headerId Format Examples:
        /// - UUID: 550e8400-e29b-41d4-a716-446655440000
        /// - Custom: BATCH-2025-001, HDR-12345, IMPORT-001
        /// - Max length: 36 characters
        ///     
        /// </remarks>
        [HttpPost("import-zip")]
        public async Task<IActionResult> ImportZipAsync(
            [FromForm] IFormFile zipFile,
            [FromForm] string? importBatchId = null,
            [FromForm] string? headerId = null,
            [FromForm] string templateName = "TEXT_TEMPLATE")
        {
            try
            {
                if (zipFile == null || zipFile.Length == 0)
                {
                    return BadRequest(new { success = false, message = "ZIP file is required" });
                }

                if (!zipFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { success = false, message = "Only ZIP files are supported" });
                }

                // สร้าง temp directory สำหรับ extract
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                try
                {
                    // Save ZIP file to temp
                    var tempZipPath = Path.Combine(tempDir, zipFile.FileName);
                    using (var stream = new FileStream(tempZipPath, FileMode.Create))
                    {
                        await zipFile.CopyToAsync(stream);
                    }

                    // Extract ZIP
                    var extractDir = Path.Combine(tempDir, "extracted");
                    ZipFile.ExtractToDirectory(tempZipPath, extractDir);

                    // หา TEXT files ทั้งหมดใน ZIP
                    var textFiles = Directory.GetFiles(extractDir, "*.txt", SearchOption.AllDirectories);

                    if (textFiles.Length == 0)
                    {
                        return BadRequest(new { success = false, message = "No TEXT files found in ZIP" });
                    }

                    // นับ total rows จากทุกไฟล์
                    var totalRows = 0;
                    var encoding = Encoding.GetEncoding(874);
                    var fileDetails = new List<object>();

                    foreach (var txtFile in textFiles)
                    {
                        var fileName = Path.GetFileName(txtFile);
                        var rowCount = 0;
                        using (var counter = new StreamReader(txtFile, encoding))
                        {
                            await counter.ReadLineAsync(); // skip header
                            while (await counter.ReadLineAsync() != null)
                            {
                                rowCount++;
                            }
                        }
                        totalRows += rowCount;
                        fileDetails.Add(new { fileName, rows = rowCount });
                    }

                    // สร้าง progressId และ initialize
                    var progressId = Guid.NewGuid().ToString();
                    _progressTracking.InitializeProgress(progressId, totalRows);

                    // Start background processing
                    var batchId = importBatchId ?? $"ZIP_{DateTime.Now:yyyyMMddHHmmss}";
                    _ = Task.Run(async () =>
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
                        var batchInsertService = scope.ServiceProvider.GetRequiredService<IBatchInsertService>();
                        var progressTracking = scope.ServiceProvider.GetRequiredService<IProgressTrackingService>();

                        await ProcessZipInBackgroundAsync(
                                progressId,
                                textFiles,
                                zipFile.FileName,
                                batchId,
                                tempDir,
                                headerId,
                                templateName,
                                loggingService: loggingService,
                                batchInsert: batchInsertService,
                                progressTracking: progressTracking
                            );
                        //await ProcessZipInBackgroundAsync(progressId, textFiles, zipFile.FileName, batchId, tempDir, headerId, templateName,loggingService: loggingService);
                    });

                    return Ok(new
                    {
                        success = true,
                        message = "ZIP import started",
                        progressId = progressId,
                        zipFileName = zipFile.FileName,
                        filesFound = textFiles.Length,
                        totalRows = totalRows,
                        importBatchId = batchId,
                        headerId = headerId,
                        files = fileDetails
                    });
                }
                catch (Exception ex)
                {
                    // Cleanup on error
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, "ImportZipAsync failed", $"File: {zipFile?.FileName}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// ตรวจสอบสถานะการ import
        /// GET: api/TextFileReader/import-progress/{progressId}
        /// </summary>
        [HttpGet("ImportProgress/{progressId}")]
        public IActionResult GetImportProgress(string progressId)
        {
            var progress = _progressTracking.GetProgress(progressId);
            if (progress == null)
            {
                return NotFound(new { success = false, message = "Progress not found" });
            }
            return Ok(progress);
        }

        /// <summary>
        /// Import ZIP file จาก Base64 string ที่มี TEXT files หลายไฟล์
        /// POST: api/TextFileReader/import-zip/base64
        /// </summary>
        /// <param name="base64File">Base64 encoded ZIP file</param>
        /// <param name="fileName">ชื่อไฟล์ ZIP</param>
        /// <param name="importBatchId">Batch ID (optional)</param>
        /// <param name="headerId">Header ID (optional)</param>
        [HttpPost("ImportZipBase64Async")]
        public async Task<ImportResult> ImportZipBase64Async(
            [FromQuery] string? p_Parameter,
            [FromQuery] string? p_importBatchId = null,
            [FromQuery] string? p_headerId = null,
            [FromQuery] string p_templateName = null)
        {
            var result = new ImportResult();
            string fileName = "";
            p_templateName = p_templateName ?? "TEXT_TEMPLATE";

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

                var FileList = await GetFileFromDatabase(request);
                var File = FileList?.FirstOrDefault();
                fileName = File.FILE_NAME;

                if (File == null || string.IsNullOrEmpty(File.CONTENT_VALUE))
                {

                    result.Status = "E";
                    result.Message = "ไม่พบไฟล์ที่เกี่ยวข้อง";
                    return result;
                }

                if (!File.FILE_NAME.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = "E";
                    result.Message = "Only .zip files are supported";
                    return result;
                }

                // Decode Base64 to bytes
                byte[] zipBytes;
                try
                {
                    zipBytes = Convert.FromBase64String(File.CONTENT_VALUE);
                }
                catch (FormatException)
                {
                    result.Status = "E";
                    result.Message = "Invalid Base64 string";
                    return result;
                }

                // สร้าง temp directory สำหรับ extract
                var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                try
                {
                    // Save ZIP file to temp
                    var tempZipPath = Path.Combine(tempDir, fileName);
                    await System.IO.File.WriteAllBytesAsync(tempZipPath, zipBytes);

                    // Extract ZIP
                    var extractDir = Path.Combine(tempDir, "extracted");
                    ZipFile.ExtractToDirectory(tempZipPath, extractDir);

                    // หา TEXT files ทั้งหมดใน ZIP
                    var textFiles = Directory.GetFiles(extractDir, "*.txt", SearchOption.AllDirectories);

                    if (textFiles.Length == 0)
                    {
                        result.Status = "E";
                        result.Message = "No TEXT files found in ZIPg";
                        return result;
                    }

                    // นับ total rows จากทุกไฟล์
                    var totalRows = 0;
                    var encoding = Encoding.GetEncoding(874);
                    var fileDetails = new List<object>();

                    foreach (var txtFile in textFiles)
                    {
                        var txtFileName = Path.GetFileName(txtFile);
                        var rowCount = 0;
                        using (var counter = new StreamReader(txtFile, encoding))
                        {
                            await counter.ReadLineAsync(); // skip header
                            while (await counter.ReadLineAsync() != null)
                            {
                                rowCount++;
                            }
                        }
                        totalRows += rowCount;
                        fileDetails.Add(new { fileName = txtFileName, rows = rowCount });
                    }

                    // สร้าง progressId และ initialize
                    var progressId = Guid.NewGuid().ToString();
                    _progressTracking.InitializeProgress(progressId, totalRows);

                    // Start background processing
                    var batchId = p_importBatchId ?? $"ZIP_{DateTime.Now:yyyyMMddHHmmss}";
                    
                        using var scope = _scopeFactory.CreateScope();
                        var loggingService = scope.ServiceProvider.GetRequiredService<ILoggingService>();
                        var batchInsertService = scope.ServiceProvider.GetRequiredService<IBatchInsertService>();
                        var progressTracking = scope.ServiceProvider.GetRequiredService<IProgressTrackingService>();

                        //await ProcessZipInBackgroundAsync(progressId, textFiles, fileName, batchId, tempDir, p_headerId, p_templateName);
                        await ProcessZipInBackgroundAsync(
                                progressId,
                                textFiles,
                                fileName,
                                batchId,
                                tempDir,
                                headerId: p_headerId,
                                templateName: p_templateName,
                                loggingService: loggingService,
                                batchInsert: batchInsertService,
                                progressTracking: progressTracking
                            );
                   

                    result.Status = "S";
                    result.Message = "ZIP import started";
                    result.ProgressId = progressId;
                    result.FileName = fileName;
                    result.FilesFound = textFiles.Length.ToString();
                    result.TotalRows = totalRows.ToString();
                    result.ImportBatchId = batchId;
                    result.Headers = p_headerId;
                    result.FileDetails = fileDetails.ToString();
                }
                catch (Exception ex)
                {
                    // Cleanup on error
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                    throw;
                }
                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, "ImportZipBase64Async failed", $"File: {fileName}");
                result.Status = "E";
                result.Message = ex.Message;

                return result;
            }
        }

        /// <summary>
        /// Import TEXT file rows into staging table FLEET_CARD_TEMP_TRANS_TEXT
        /// POST: api/TextFileReader/import (all parameters via form-data)
        /// </summary>
        [HttpPost("ImportTextToStaging")]
        public async Task<ImportResult> ImportTextToStaging(
            [FromQuery] string? p_Parameter,
            [FromQuery] string? p_importBatchId = null,
            [FromQuery] string? p_headerId = null,
            [FromQuery] string p_templateName = null)
        {
            var result = new ImportResult();
            string fileName = "";
            p_templateName = p_templateName ?? "TEXT_TEMPLATE";

            try
            {
                string filePath;
                string actualFileName;
                bool isUploadedFile = false;

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

                var FileList = await GetFileFromDatabase(request);
                var File = FileList?.FirstOrDefault();
                fileName = File.FILE_NAME;

                if (File == null || string.IsNullOrEmpty(File.CONTENT_VALUE))
                {

                    result.Status = "E";
                    result.Message = "ไม่พบไฟล์ที่เกี่ยวข้อง";
                    return result;
                }

                if (!File.FILE_NAME.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    result.Status = "E";
                    result.Message = "Only .txt files are supported";
                    return result;
                }

                if (File != null)
                {
                    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                    Directory.CreateDirectory(tempDir);
                    filePath = Path.Combine(tempDir, File.FILE_NAME);
                    var fileBytes = Convert.FromBase64String(File.CONTENT_VALUE);

                    await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

                    actualFileName = File.FILE_NAME;
                    isUploadedFile = true;
                }
                else if (!string.IsNullOrWhiteSpace(fileName))
                {
                    var basePath = Path.Combine(Directory.GetCurrentDirectory(), "SimpleFile", "TextFile");
                    filePath = Path.Combine(basePath, fileName);
                    actualFileName = fileName;

                    if (!System.IO.File.Exists(filePath))
                    {
                        result.Status = "E";
                        result.Message = $"File not found: {fileName}";
                        result.Data = basePath;
                        return result;
                    }
                }
                else
                {
                    result.Status = "E";
                    result.Message = "Either fileName parameter or file upload is required";
                    return result;
                }

                // Read file
                var encoding = Encoding.GetEncoding(874);

                // โหลด delimiter (default: TEXT_TEMPLATE)
                var delimiter = await LoadTemplateDelimiterAsync(p_templateName);

                var lines = await System.IO.File.ReadAllLinesAsync(filePath, encoding);
                if (lines.Length <= 1)
                {
                    result.Status = "E";
                    result.Message = "No data rows found";
                    return result;
                }

                var headers = lines[0].Split(delimiter).Select(h => h.Trim()).ToArray();
                var dataRows = new List<string[]>();
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(delimiter).Select(v => v.Trim()).ToArray();
                    if (values.Length == headers.Length)
                    {
                        dataRows.Add(values);
                    }
                }

                // File metadata
                var fi = new FileInfo(filePath);
                var meta = new
                {
                    FileName = fi.Name,
                    FileSize = fi.Length,
                    FileCreatedDate = System.IO.File.GetCreationTime(filePath),
                    FileLastModifiedDate = System.IO.File.GetLastWriteTime(filePath)
                };

                // Insert
                await _loggingService.LogInfoAsync($"TEXT import start: {actualFileName}", $"Batch={p_importBatchId ?? "(auto)"}; Path={filePath}; Rows={dataRows.Count}; Source={(isUploadedFile ? "uploaded" : "local")}; Template={p_templateName}");

                var resultInsertRow = await InsertRowsAsync(
                    tableName: "EFM_FED.FLEET_CARD_TEMP_TRANS_TEXT",
                    headers: headers,
                    rows: dataRows,
                    importBatchId: string.IsNullOrWhiteSpace(p_importBatchId) ? Guid.NewGuid().ToString() : p_importBatchId!,
                    fileMeta: meta,
                    headerId: p_headerId,
                    templateName: p_templateName
                );

                await _loggingService.LogInfoAsync($"TEXT import complete: {actualFileName}", $"Batch={resultInsertRow.importBatchId}; Inserted={resultInsertRow.inserted}; Failed={resultInsertRow.failed}");

                result.Status = "S";
                result.Message = "EFM_FED.FLEET_CARD_TEMP_TRANS_TEXT";
                result.FileName = meta.FileName;
                result.ImportBatchId = resultInsertRow.importBatchId;
                result.Headers = resultInsertRow.headerId;
                result.Total = dataRows.Count.ToString();
                result.Inserted = resultInsertRow.inserted.ToString();
                result.Failed = resultInsertRow.failed.ToString();
                result.Errors = resultInsertRow.errors.ToString();
                result.Source = isUploadedFile ? "uploaded" : "local";

               
                // Cleanup temp file if uploaded
                if (isUploadedFile && System.IO.File.Exists(filePath))
                {
                    try
                    {
                        var dir = Path.GetDirectoryName(filePath);
                        if (Directory.Exists(dir)) Directory.Delete(dir, true);
                    }
                    catch { /* Ignore cleanup errors */ }
                }

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, "TEXT import failed", $"File={fileName}; Batch={p_importBatchId}");
                result.Status = "E";
                result.Message = ex.Message;
                result.Data = ex.StackTrace;
                return result;
            }
        }

        /// <summary>
        /// สรุปผลการนำเข้าตาม Batch ID
        /// GET: api/TextFileReader/import/summary?batchId=xxx&headerId=xxx
        /// </summary>
        [HttpGet("ImportSummary")]
        public async Task<IActionResult> GetImportSummary(
            [FromQuery] string? batchId = null,
            [FromQuery] string? importBatchId = null,
            [FromQuery] string? headerId = null)
        {
            try
            {
                // Support both parameter names for backward compatibility
                var actualBatchId = batchId ?? importBatchId;

                if (string.IsNullOrWhiteSpace(actualBatchId) && string.IsNullOrWhiteSpace(headerId))
                {
                    return BadRequest(new { success = false, message = "batchId or headerId parameter is required" });
                }

                using var conn = new OracleConnection(_connectionString);

                if (conn.State != ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }

                using var cmd = conn.CreateCommand();

                var whereConditions = new List<string>();
                if (!string.IsNullOrWhiteSpace(actualBatchId))
                    whereConditions.Add("IMPORT_BATCH_ID = :batchId");
                if (!string.IsNullOrWhiteSpace(headerId))
                    whereConditions.Add("HEADER_ID = :headerId");

                cmd.CommandText = $@"
                    SELECT 
                        IMPORT_BATCH_ID,
                        HEADER_ID,
                        MIN(FILE_NAME) AS FILE_NAME,
                        MIN(FILE_SIZE) AS FILE_SIZE,
                        MIN(IMPORT_DATE) AS IMPORT_DATE,
                        PROCESSING_STATUS,
                        COUNT(*) AS RECORD_COUNT
                    FROM EFM_FED.FLEET_CARD_TEMP_TRANS_TEXT
                    WHERE {string.Join(" AND ", whereConditions)}
                    GROUP BY IMPORT_BATCH_ID, HEADER_ID, PROCESSING_STATUS
                    ORDER BY PROCESSING_STATUS";

                if (!string.IsNullOrWhiteSpace(actualBatchId))
                {
                    var paramBatch = cmd.CreateParameter();
                    paramBatch.ParameterName = ":batchId";
                    paramBatch.Value = actualBatchId;
                    cmd.Parameters.Add(paramBatch);
                }

                if (!string.IsNullOrWhiteSpace(headerId))
                {
                    var paramHeader = cmd.CreateParameter();
                    paramHeader.ParameterName = ":headerId";
                    paramHeader.Value = headerId;
                    cmd.Parameters.Add(paramHeader);
                }

                var summary = new List<object>();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    summary.Add(new
                    {
                        importBatchId = reader.GetString(0),
                        headerId = reader.IsDBNull(1) ? null : reader.GetString(1),
                        fileName = reader.IsDBNull(2) ? null : reader.GetString(2),
                        fileSize = reader.IsDBNull(3) ? (long?)null : Convert.ToInt64(reader.GetValue(3)),
                        importDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                        processingStatus = reader.IsDBNull(5) ? null : reader.GetString(5),
                        recordCount = Convert.ToInt32(reader.GetValue(6))
                    });
                }

                if (summary.Count == 0)
                {
                    return NotFound(new { success = false, message = $"No records found for batch: {actualBatchId ?? "(none)"}, header: {headerId ?? "(none)"}" });
                }

                return Ok(new { success = true, importBatchId = actualBatchId, headerId = headerId, summary });
            }
            catch (Exception ex)
            {
                var actualBatchId = batchId ?? importBatchId;
                await _loggingService.LogErrorAsync("ERROR", ex, "Failed to retrieve import summary", $"Batch={actualBatchId}");
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }
    }
}