using EXAT.ECM.PRS.API.DAL;
using EXAT.ECM.PRS.API.Models;
using EXAT.ECM.PRS.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using static EXAT.ECM.PRS.API.Models.APIModel.ResponseModel;

namespace EXAT.ECM.PRS.API.Services
{
    public class PRSService : IPRSService
    {
        private readonly OracleDbContext _oracleContext;
        private readonly IConfiguration _configuration; //AppDomain.CurrentDomain.BaseDirectory + "\\Tempfile\\";
        private readonly ILogger<PRSService> _logger;

        public PRSService(OracleDbContext oracleContext, IConfiguration configuration, ILogger<PRSService> logger)
        {
            _oracleContext = oracleContext;
            _configuration = configuration;
            _logger = logger;
        }
        //PRS SUMMARY REPORT
        public async Task<PRS_HEADER_SUMMARY_REPORT> GetPRSSummaryAsync(PRSParameterModel request)
        {
            PRS_HEADER_SUMMARY_REPORT result = new PRS_HEADER_SUMMARY_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetPRSSummaryAsync ");

                SuccessResponse<PRS_HEADER_SUMMARY_REPORT> response = new SuccessResponse<PRS_HEADER_SUMMARY_REPORT>()
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
        private async Task<List<PRS_HEADER_SUMMARY_REPORT>> GetHeaderSummaryAsync(PRSParameterModel request)
        {
            _logger.LogInformation("Start Function GetHeaderSummaryAsync ");

            var p1 = request.p_REQUEST_DOCNO ?? (object)DBNull.Value;
            var p2 = request.p_REQUEST_SUBJECT ?? (object)DBNull.Value;
            var p3 = request.p_DIV_CODE ?? (object)DBNull.Value;
            var p4 = request.p_SEC_CODE ?? (object)DBNull.Value;
            var p5 = request.p_DEP_CODE ?? (object)DBNull.Value;
            var p6 = Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOC_DATE_FROM) ?? (object)DBNull.Value;
            var p7 = Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOC_DATE_TO) ?? (object)DBNull.Value;

            _logger.LogInformation("Call Store procedure ");

            var result = await _oracleContext
                    .Set<PRS_HEADER_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_PRS.SP_7001_GETDATA_HEADER_RPT (
                                            :p_REQUEST_DOCNO,
   	                                        :p_REQUEST_SUBJECT,
   	                                        :p_DIV_CODE,
   	                                        :p_SEC_CODE,
   	                                        :p_DEP_CODE,
   	                                        :p_STATUS_ID,
   	                                        :p_REQUEST_DOC_DATE_FROM,
   	                                        :p_REQUEST_DOC_DATE_TO,
   	                                        :p_USER_AD,
	                                        :DATA_HEADER
                                        );
                                    END;",
                    new OracleParameter("p_REQUEST_DOCNO", request.p_REQUEST_DOCNO ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_SUBJECT", request.p_REQUEST_SUBJECT ?? (object)DBNull.Value),
                    new OracleParameter("p_DIV_CODE", request.p_DIV_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_SEC_CODE", request.p_SEC_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_DEP_CODE", request.p_DEP_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_STATUS_ID", request.p_STATUS_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_DOC_DATE_FROM", Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOC_DATE_FROM) ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_DOC_DATE_TO", Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOC_DATE_TO) ?? (object)DBNull.Value),
                    new OracleParameter("p_USER_AD", request.p_USER_AD ?? (object)DBNull.Value),
                    new OracleParameter("DATA_HEADER", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
            return result;
        }
        private async Task<List<PRS_DETAIL_SUMMARY_REPORT>> GetDetailSummaryAsync(PRSParameterModel request)
        {
            var result = await _oracleContext
                    .Set<PRS_DETAIL_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_PRS.SP_7001_GETLIST_SUMMARY_REPORT (
                                            :p_REQUEST_DOCNO,
   	                                        :p_REQUEST_SUBJECT,
   	                                        :p_DIV_CODE,
   	                                        :p_SEC_CODE,
   	                                        :p_DEP_CODE,
   	                                        :p_STATUS_ID,
   	                                        :p_REQUEST_DOC_DATE_FROM,
   	                                        :p_REQUEST_DOC_DATE_TO,
   	                                        :p_USER_AD,
                                            :p_output
                                        );
                                    END;",
                    new OracleParameter("p_REQUEST_DOCNO", request.p_REQUEST_DOCNO ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_SUBJECT", request.p_REQUEST_SUBJECT ?? (object)DBNull.Value),
                    new OracleParameter("p_DIV_CODE", request.p_DIV_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_SEC_CODE", request.p_SEC_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_DEP_CODE", request.p_DEP_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_STATUS_ID", request.p_STATUS_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_DOC_DATE_FROM", Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOC_DATE_FROM) ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_DOC_DATE_TO", Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOC_DATE_TO) ?? (object)DBNull.Value),
                    new OracleParameter("p_USER_AD", request.p_USER_AD ?? (object)DBNull.Value),
                    new OracleParameter("p_output", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            return result;
        }

        //PRS REQUEST REPORT
        public async Task<PRS_HEADER_REQUEST_REPORT> GetPRSRequestFormAsync(PRSParameterModel request)
        {
            PRS_HEADER_REQUEST_REPORT result = new PRS_HEADER_REQUEST_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetPRSRequestFormAsync ");

                SuccessResponse<PRS_HEADER_REQUEST_REPORT> response = new SuccessResponse<PRS_HEADER_REQUEST_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderRequestFormAsync(request);
                var detail = await GetDetailRequestFormAsync(request);

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
                //    StatusCode = "500",=
                //    Message = "An error occurred while processing your request."
                //};
                return null;
            }
        }
        private async Task<List<PRS_HEADER_REQUEST_REPORT>> GetHeaderRequestFormAsync(PRSParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;

            var result = await _oracleContext
                    .Set<PRS_HEADER_REQUEST_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_PRS.SP_7002_GETDATA_REQFORM_RPT (
                                            :p_HEADER_ID,
	                                        :p_OUTPUT
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTPUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
            return result;
        }
        private async Task<List<PRS_DETAIL_REQUEST_REPORT>> GetDetailRequestFormAsync(PRSParameterModel request)
        {
            var result = await _oracleContext
                    .Set<PRS_DETAIL_REQUEST_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_PRS.SP_7002_GETLIST_REQFORM_RPT (
                                            :p_HEADER_ID,
                                            :p_OUTPUT
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTPUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            return result;
        }


    }
}
