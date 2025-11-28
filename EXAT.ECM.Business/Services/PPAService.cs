using EXAT.ECM.Business.DAL;
using EXAT.ECM.Business.Models;
using EXAT.ECM.Business.Models.PPA;
using EXAT.ECM.Business.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Security.Policy;
using static EXAT.ECM.Business.Models.APIModel.ResponseModel;

namespace EXAT.ECM.Business.Services
{
    public class PPAService : IPPAService
    {
        private readonly OracleDbContext _oracleContext;
        private readonly IConfiguration _configuration; //AppDomain.CurrentDomain.BaseDirectory + "\\Tempfile\\";
        private readonly ILogger<PPAService> _logger;

        public PPAService(OracleDbContext oracleContext, IConfiguration configuration, ILogger<PPAService> logger)
        {
            _oracleContext = oracleContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<PPA_HEADER_SUMMARY_REPORT> GetPPASummaryAsync(PPAParameterModel request)
        {
            PPA_HEADER_SUMMARY_REPORT result = new PPA_HEADER_SUMMARY_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetPPASummaryAsync ");

                SuccessResponse<PPA_HEADER_SUMMARY_REPORT> response = new SuccessResponse<PPA_HEADER_SUMMARY_REPORT>()
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

                // ส่งข้อมูลที่ได้รับ
                //response.Data = result;  // กำหนดเป็น List<DataModel> ทั้งหมด
                return result;
            }
            catch (Exception ex)
            {
                // Handle the error and log it
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }

        private async Task<List<PPA_HEADER_SUMMARY_REPORT>> GetHeaderSummaryAsync(PPAParameterModel request)
        {
            var result = await _oracleContext
                    .Set<PPA_HEADER_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_PPA.SP_7003_GETDATA_HEADER_SUMMARY_REPORT (
                                            :p_DOCNO, 
                                            :p_PROJECT_NAME, 
                                            :p_PROJECTSECRETARY_BY, 
                                            :p_CONTRACT_NUMBER, 
                                            :p_PROJECT_START_DATE, 
                                            :p_PROJECT_END_DATE, 
                                            :p_STATUS_ID,
                                            :p_USER_AD,
                                            :DATA_HEADER
                                        );
                                    END;",
                    new OracleParameter("p_DOCNO", request.p_DOCNO ?? (object)DBNull.Value),
                    new OracleParameter("p_PROJECT_NAME", request.p_PROJECT_NAME ?? (object)DBNull.Value),
                    new OracleParameter("p_PROJECTSECRETARY_BY", request.p_PROJECTSECRETARY_BY ?? (object)DBNull.Value),
                    new OracleParameter("p_CONTRACT_NUMBER", request.p_CONTRACT_NUMBER ?? (object)DBNull.Value),
                    new OracleParameter("p_PROJECT_START_DATE", Utilities.ConvertValue<DateTime>(request.p_PROJECT_START_DATE) ?? (object)DBNull.Value),
                    new OracleParameter("p_PROJECT_END_DATE", Utilities.ConvertValue<DateTime>(request.p_PROJECT_END_DATE) ?? (object)DBNull.Value),
                    new OracleParameter("p_STATUS_ID", request.p_STATUS_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_USER_AD", request.p_USER_AD ?? (object)DBNull.Value),
                    new OracleParameter("DATA_HEADER", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();

            return result;
        }

        private async Task<List<PPA_DETAIL_SUMMARY_REPORT>> GetDetailSummaryAsync(PPAParameterModel request)
        {
            var result = await _oracleContext
                    .Set<PPA_DETAIL_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_PPA.SP_7003_GETLIST_SUMMARY_REPORT (
                                            :p_DOCNO, 
                                            :p_PROJECT_NAME, 
                                            :p_PROJECTSECRETARY_BY, 
                                            :p_CONTRACT_NUMBER, 
                                            :p_PROJECT_START_DATE, 
                                            :p_PROJECT_END_DATE, 
                                            :p_STATUS_ID,
                                            :p_USER_AD,
                                            :p_OUTPUT
                                        );
                                    END;",
                     new OracleParameter("p_DOCNO", request.p_DOCNO ?? (object)DBNull.Value),
                    new OracleParameter("p_PROJECT_NAME", request.p_PROJECT_NAME ?? (object)DBNull.Value),
                    new OracleParameter("p_PROJECTSECRETARY_BY", request.p_PROJECTSECRETARY_BY ?? (object)DBNull.Value),
                    new OracleParameter("p_CONTRACT_NUMBER", request.p_CONTRACT_NUMBER ?? (object)DBNull.Value),
                    new OracleParameter("p_PROJECT_START_DATE", Utilities.ConvertValue<DateTime>(request.p_PROJECT_START_DATE) ?? (object)DBNull.Value),
                    new OracleParameter("p_PROJECT_END_DATE", Utilities.ConvertValue<DateTime>(request.p_PROJECT_END_DATE) ?? (object)DBNull.Value),
                    new OracleParameter("p_STATUS_ID", request.p_STATUS_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_USER_AD", request.p_USER_AD ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTPUT", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();

            return result;
        }

    }
}
