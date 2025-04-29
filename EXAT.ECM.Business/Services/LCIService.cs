using EXAT.ECM.Business.DAL;
using EXAT.ECM.Business.Models;
using EXAT.ECM.Business.Models.LCI;
using EXAT.ECM.Business.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using static EXAT.ECM.Business.Models.APIModel.ResponseModel;

namespace EXAT.ECM.Business.Services
{
    public class LCIService : ILCIService
    {
        private readonly OracleDbContext _oracleContext;
        private readonly IConfiguration _configuration; //AppDomain.CurrentDomain.BaseDirectory + "\\Tempfile\\";
        private readonly ILogger<LCIService> _logger;

        public LCIService(OracleDbContext oracleContext, IConfiguration configuration, ILogger<LCIService> logger)
        {
            _oracleContext = oracleContext;
            _configuration = configuration;
            _logger = logger;
        }
        // LCI Summary Report
        public async Task<LCI_HEADER_SUMMARY_REPORT> GetLCISummaryAsync(LCIParameterModel request)
        {
            LCI_HEADER_SUMMARY_REPORT result = new LCI_HEADER_SUMMARY_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetLCISummaryAsync ");

                SuccessResponse<LCI_HEADER_SUMMARY_REPORT> response = new SuccessResponse<LCI_HEADER_SUMMARY_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderSummaryAsync(request);
                var detail = await GetDetailSummaryAsync(request);

                //LCI_HEADER_SUMMARY_REPORT result = header.FirstOrDefault();
                result = header.FirstOrDefault();
                result.Detail = detail;

                // Log หลังจากได้รับข้อมูลจาก Oracle
                _logger.LogInformation("Received {HeaderData} records from Oracle.", header.Count);
                _logger.LogInformation("Received {DetailData} records from Oracle.", detail.Count);

                // ส่งข้อมูลที่ได้รับ
                //response.Data = result;  // กำหนดเป็น List<DataModel> ทั้งหมด
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

        private async Task<List<LCI_HEADER_SUMMARY_REPORT>> GetHeaderSummaryAsync(LCIParameterModel request)
        {
            var result = await _oracleContext
                    .Set<LCI_HEADER_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_LCI.SP_7001_GETDATA_SUMMARY_REPORT (
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

        private async Task<List<LCI_DETAIL_SUMMARY_REPORT>> GetDetailSummaryAsync(LCIParameterModel request)
        {
            var result = await _oracleContext
                    .Set<LCI_DETAIL_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_LCI.SP_7001_GETLIST_SUMMARY_REPORT (
                                            :p_REQUEST_DOCNO, 
                                            :p_LEGALDEP_DOCNO, 
                                            :p_REQUEST_SUBJECT,
                                            :p_DIV_CODE, 
                                            :p_SEC_CODE, 
                                            :p_DEP_CODE, 
                                            :p_SECRET_ID,
                                            :p_SPEED_ID,
                                            :p_STATUS_ID,
                                            :p_REQUEST_DOC_DATE_FROM, 
                                            :p_REQUEST_DOC_DATE_TO, 
                                            :p_USER_AD,
                                            :p_output
                                        );
                                    END;",
                    new OracleParameter("p_REQUEST_DOCNO", request.p_REQUEST_DOCNO ?? (object)DBNull.Value),
                    new OracleParameter("p_LEGALDEP_DOCNO", request.p_LEGALDEP_DOCNO ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_SUBJECT", request.p_REQUEST_SUBJECT ?? (object)DBNull.Value),
                    new OracleParameter("p_DIV_CODE", request.p_DIV_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_SEC_CODE", request.p_SEC_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_DEP_CODE", request.p_DEP_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_SECRET_ID", request.p_SECRET_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_SPEED_ID", request.p_SPEED_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_STATUS_ID", request.p_STATUS_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_DOC_DATE_FROM", Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOC_DATE_FROM )?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_DOC_DATE_TO", Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOC_DATE_TO) ?? (object)DBNull.Value),
                    new OracleParameter("p_USER_AD", request.p_USER_AD ?? (object)DBNull.Value),
                    new OracleParameter("p_output", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();

            return result;
        }

        // LCI Request Report
        public async Task<LCI_HEADER_REQUEST_REPORT> GetLCIRequestAsync(LCIParameterModel request)
        {
            LCI_HEADER_REQUEST_REPORT result = new LCI_HEADER_REQUEST_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetLCISummaryAsync ");

                SuccessResponse<LCI_HEADER_REQUEST_REPORT> response = new SuccessResponse<LCI_HEADER_REQUEST_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderRequestAsync(request);
                var detail = await GetDetailRequestAsync(request);

                //LCI_HEADER_SUMMARY_REPORT result = header.FirstOrDefault();
                result = header.FirstOrDefault();
                result.Detail = detail;

                // Log หลังจากได้รับข้อมูลจาก Oracle
                _logger.LogInformation("Received {HeaderData} records from Oracle.", header.Count);
                _logger.LogInformation("Received {DetailData} records from Oracle.", detail.Count);

                // ส่งข้อมูลที่ได้รับ
                //response.Data = result;  // กำหนดเป็น List<DataModel> ทั้งหมด
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

        private async Task<List<LCI_HEADER_REQUEST_REPORT>> GetHeaderRequestAsync(LCIParameterModel request)
        {
            var result = await _oracleContext
                    .Set<LCI_HEADER_REQUEST_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_LCI.SP_7002_GETDATA_REQFORM_RPT (
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

        private async Task<List<LCI_DETAIL_REQUEST_REPORT>> GetDetailRequestAsync(LCIParameterModel request)
        {
            var result = await _oracleContext
                    .Set<LCI_DETAIL_REQUEST_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_LCI.SP_7002_GETLIST_REQFORM_RPT (
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
