
using EXAT.ECM.EER.API.Configurations;
using EXAT.ECM.EER.API.Helper;
using EXAT.ECM.EER.API.Models;
using EXAT.ECM.EER.API.Models.APIModel;
using EXAT.ECM.EER.API.Services;
using EXAT.ECM.EER.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text.Json;
using static EXAT.ECM.EER.API.Models.APIModel.ResponseModel;

namespace EXAT.ECM.EER.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EXATEERController : Controller
    {
        private readonly ILogger<EXATEERController> _logger;
        private readonly AsposeOption _asposeOption;
        private readonly IWebHostEnvironment _environment;
        private readonly IEERService _eerService;
        private readonly string _connectionString;

        private string EERSummaryReportTemplate = "DocumentTemplate/EER/EERSummaryReportTemplate.docx";
        private string EERRequestFormTemplate = "DocumentTemplate/EER/EERFormReqPermissionEntryExit.docx";

        public EXATEERController(ILogger<EXATEERController> logger
                                  , IOptions<AsposeOption> asposeOption
                                  , IWebHostEnvironment environment
                                  , IEERService eerService
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
            _eerService = eerService;
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            //_connectionString = configuration.GetConnectionString("OracleConnection");
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

        // GET: api/DownloadPrintFormEER
        [HttpGet("DownloadPrintFormEER")]
        [ProducesResponseType(typeof(ResponseModel.SuccessResponse), 200)]
        [ProducesResponseType(typeof(ResponseModel.ErrorResponse), 500)]
        public async Task<IActionResult> DownloadPrintFormEER(
              [FromQuery] string? p_Parameter,
              [FromQuery] string? p_FileName,
              [FromQuery] string? p_Template)
        {
            _logger.LogInformation("Start processing DownloadPrintFormEER");

            try
            {
                // ---------- Parse p_Parameter -> EERParameterModel ----------
                var request = new EERParameterModel();
                if (!string.IsNullOrEmpty(p_Parameter))
                {
                    var parts = p_Parameter.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var kv = part.Split('=', 2); // กันค่าที่มี '=' ภายใน
                        if (kv.Length != 2) continue;

                        var key = kv[0];
                        var val = string.IsNullOrWhiteSpace(kv[1]) ? null : Utilities.CleansingData(kv[1]);

                        switch (key)
                        {
                            case "p_EXPRESSWAY_ID": request.p_EXPRESSWAY_ID = val; break;
                            case "p_DIRECTION_ID": request.p_DIRECTION_ID = val; break;
                            case "P_REQUEST_DOCDATE_FROM": request.P_REQUEST_DOCDATE_FROM = val; break;
                            case "P_REQUEST_DOCDATE_TO": request.P_REQUEST_DOCDATE_TO = val; break;
                            case "p_REQUEST_ID": request.p_REQUEST_ID = val; break;
                            case "p_ITEM_ID": request.p_ITEM_ID = val; break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(p_Template))
                    return BadRequest("p_Template is required.");

                p_FileName ??= $"Export_{p_Template}_{DateTime.Now:yyyyMMddHHmmss}.docx";

                using var memoryStream = new System.IO.MemoryStream();
                var apOption = new AsposeHelperOption(_asposeOption, _environment);

                switch (p_Template)
                {
                    // -------------------- 1) SUMMARY --------------------
                    case "EERSummaryReportTemplate":
                        {
                            var contentPath = Path.Combine(_environment.ContentRootPath, EERSummaryReportTemplate);
                            var document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                            var replacer = new ReplaceWords();

                            var data = await _eerService.GetEERSummaryAsync(request);
                            var d_header = (data == null) ? null : replacer.ConvertDataToReplaceObject(data);
                            var d_detail = (data?.Detail == null) ? null : replacer.ConvertDataToReplaceObject(data.Detail);

                            _logger.LogInformation("Detail JSON: {result}",
                                JsonSerializer.Serialize(d_detail, new JsonSerializerOptions { WriteIndented = true }));

                            replacer.ReplaceNodeDataRow(document, "bmDataRow", d_detail);
                            replacer.ReplaceNodeText(document, d_header);
                            replacer.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                            document.Save(memoryStream, p_FileName);
                            var bytes = memoryStream.ToArray();

                            // ใช้ MIME ของ DOCX จะเหมาะกว่า octet-stream
                            const string docxMime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                            return File(bytes, docxMime, p_FileName);
                        }

                    // -------------------- 2) REQUEST FORM --------------------
                    case "EERRequestFormTemplate":
                        {
                            var contentPath = Path.Combine(_environment.ContentRootPath, EERRequestFormTemplate);
                            var document = new AsposeHelper(contentPath, _asposeOption, _environment, apOption.option());
                            var replacer = new ReplaceWords();

                            var data = await _eerService.GetEERRequestFormAsync(request);
                            var d_header = (data == null) ? null : replacer.ConvertDataToReplaceObject(data);

                            replacer.ReplaceNodeText(document, d_header);
                            replacer.RemoveRowWithSpecificBookmark(document, "bmDataRow");

                            document.Save(memoryStream, p_FileName);
                            var bytes = memoryStream.ToArray();

                            // อัพโหลดเข้า DB ตามรูปแบบ <file>...</file>
                            var base64 = Convert.ToBase64String(bytes);
                            var payload = $"<file><name>{p_FileName}</name><content>{base64}</content></file>";

                            var result = await _eerService.InserDocumentRequest(request, payload);
                            return Ok(result);
                        }

                    default:
                        return BadRequest($"Unknown template: {p_Template}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                var errorResponse = new ErrorResponse
                {
                    Status = "E",
                    StatusCode = "500",
                    Message = ex.Message
                };
                return StatusCode(500, errorResponse);
            }
        }                
    }
}