using EXAT.ECM.EER.API.DAL;
using EXAT.ECM.EER.API.Models;
using EXAT.ECM.EER.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection.PortableExecutable;
using static EXAT.ECM.EER.API.Models.APIModel.ResponseModel;
using System.Text.Json;
namespace EXAT.ECM.EER.API.Services
{
    public class EERService : IEERService
    {
        private readonly OracleDbContext _oracleContext;
        private readonly IConfiguration _configuration; //AppDomain.CurrentDomain.BaseDirectory + "\\Tempfile\\";
        private readonly ILogger<EERService> _logger;

        public EERService(OracleDbContext oracleContext, IConfiguration configuration, ILogger<EERService> logger)
        {
            _oracleContext = oracleContext;
            _configuration = configuration;
            _logger = logger;
        }

        //EER SUMMARY REPORT
        public async Task<EER_HEADER_SUMMARY_REPORT> GetEERSummaryAsync(EERParameterModel request)
        {
            EER_HEADER_SUMMARY_REPORT result = new EER_HEADER_SUMMARY_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetEERSummaryAsync ");

                SuccessResponse<EER_HEADER_SUMMARY_REPORT> response = new SuccessResponse<EER_HEADER_SUMMARY_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderSummaryAsync(request);
                var detail = await GetDetailSummaryAsync(request);

                result = header.FirstOrDefault();
                result.Detail = detail;

                // Log หลังจากได้รับข้อมูลจาก Oracle
                _logger.LogInformation("Received {HeaderData} records from Oracle.", header.Count);
                _logger.LogInformation("Received {DetailData} records from Oracle.", detail.Count);

                return result;
            }
            catch (Exception ex)
            {
                // Handle the error and log it
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                //return new ErrorResponse
                //{
                //    Status = "E",
                //    StatusCode = "500",
                //    Message = "An error occurred while processing your request."
                //};
                return null;
            }
        }
        private async Task<List<EER_HEADER_SUMMARY_REPORT>> GetHeaderSummaryAsync(EERParameterModel request)
        {

            var p1 = request.p_EXPRESSWAY_ID ?? (object)DBNull.Value;
            var p2 = request.p_DIRECTION_ID ?? (object)DBNull.Value;
            var p3 = request.P_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value;
            var p4 = request.P_REQUEST_DOCDATE_TO ?? (object)DBNull.Value;
            var result = await _oracleContext
                    .Set<EER_HEADER_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_EER.SP_7001_GETDATA_SUMMARY_REPORT (
                                            :p_EXPRESSWAY_ID,
   	                                        :p_DIRECTION_ID,
   	                                        :P_REQUEST_DOCDATE_FROM,
   	                                        :P_REQUEST_DOCDATE_TO,
	                                        :DATA_HEADER
                                        );
                                    END;",
                    new OracleParameter("p_EXPRESSWAY_ID", request.p_EXPRESSWAY_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DIRECTION_ID", request.p_DIRECTION_ID ?? (object)DBNull.Value),
                    new OracleParameter("P_REQUEST_DOCDATE_FROM", request.P_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("P_REQUEST_DOCDATE_TO", request.P_REQUEST_DOCDATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("DATA_HEADER", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
            )
                .ToListAsync();
            //_logger.LogInformation("EER Summary: {@result}", result);
            _logger.LogInformation("EER Summary JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

            return result;
        }
        private async Task<List<EER_DETAIL_SUMMARY_REPORT>> GetDetailSummaryAsync(EERParameterModel request)
        {
            var p1 = request.p_EXPRESSWAY_ID ?? (object)DBNull.Value;
            var p2 = request.p_DIRECTION_ID ?? (object)DBNull.Value;
            var p3 = request.P_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value;
            var p4 = request.P_REQUEST_DOCDATE_TO ?? (object)DBNull.Value;
            var result = await _oracleContext
                    .Set<EER_DETAIL_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_EER.SP_7001_GETLIST_SUMMARY_REPORT (
                                            :p_EXPRESSWAY_ID,
   	                                        :p_DIRECTION_ID,
   	                                        :P_REQUEST_DOCDATE_FROM,
   	                                        :P_REQUEST_DOCDATE_TO,
	                                        :DATA_HEADER
                                        );
                                    END;",
                    new OracleParameter("p_EXPRESSWAY_ID", request.p_EXPRESSWAY_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DIRECTION_ID", request.p_DIRECTION_ID ?? (object)DBNull.Value),
                    new OracleParameter("P_REQUEST_DOCDATE_FROM", request.P_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("P_REQUEST_DOCDATE_TO", request.P_REQUEST_DOCDATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("DATA_HEADER", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            _logger.LogInformation("EER Summary JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

            return result;
        }

        //EER REQUEST REPORT
        public async Task<EER_HEADER_REQUEST_REPORT> GetEERRequestFormAsync(EERParameterModel request)
        {
            EER_HEADER_REQUEST_REPORT result = new EER_HEADER_REQUEST_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetEERRequestFormAsync ");

                SuccessResponse<EER_HEADER_REQUEST_REPORT> response = new SuccessResponse<EER_HEADER_REQUEST_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderRequestFormAsync(request);
                //var detail = await GetDetailRequestFormAsync(request);

                result = header.FirstOrDefault();
                //result.Detail = detail;

                // Log หลังจากได้รับข้อมูลจาก Oracle
                _logger.LogInformation("Received {HeaderData} records from Oracle.", header.Count);
                //_logger.LogInformation("Received {DetailData} records from Oracle.", detail.Count);

                return result;
            }
            catch (Exception ex)
            {
                // Handle the error and log it
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                //return new ErrorResponse
                //{
                //    Status = "E",
                //    StatusCode = "500",=
                //    Message = "An error occurred while processing your request."
                //};
                return null;
            }
        }
        private async Task<List<EER_HEADER_REQUEST_REPORT>> GetHeaderRequestFormAsync(EERParameterModel request)
        {
            var p1 = request.p_REQUEST_ID ?? (object)DBNull.Value;
            var result = await _oracleContext
                    .Set<EER_HEADER_REQUEST_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_EER.SP_7002_GETDATA_REQFORM_RPT (
                                            :p_REQUEST_ID,
                                            :p_ITEM_ID,
	                                        :p_OUTPUT
                                        );
                                    END;",
                    new OracleParameter("p_REQUEST_ID", request.p_REQUEST_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_ITEM_ID", request.p_ITEM_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTPUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
            return result;
        }

        //EER INSERT DOCUMENT TO DB
        public async Task<ImportResult> InserDocumentRequest(EERParameterModel request,string base64String)
        {
            var results = new ImportResult();
            try
            {
                var result = await _oracleContext.Database.ExecuteSqlRawAsync(@"
                    BEGIN
                        EFM_EER.SP_6001_INSERT_FILES(
                            :p_REQUEST_ID,
                            :p_ITEM_ID,
                            :p_FILE_CONTENT
                        );
                    END;",
                        new OracleParameter("p_REQUEST_ID", OracleDbType.Varchar2, request.p_REQUEST_ID ?? (object)DBNull.Value, ParameterDirection.Input),
                        new OracleParameter("p_ITEM_ID", OracleDbType.Varchar2, request.p_ITEM_ID ?? (object)DBNull.Value, ParameterDirection.Input),
                        new OracleParameter("p_FILE_CONTENT", OracleDbType.NClob, base64String ?? (object)DBNull.Value, ParameterDirection.Input)
                        );

                results.Status = "S";
                results.Message = "Success";
            }
            catch (Exception ex)
            {
                results.Status = "E";
                results.Message = ex.Message;
            }
            return results;
        }
    }
}
