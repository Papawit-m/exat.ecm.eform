using EXAT.ECM.EON.API.Models;
using EXAT.ECM.EON.API.Services.Interfaces;
using Oracle.ManagedDataAccess.Client;
using static EXAT.ECM.EON.API.Models.APIModel.ResponseModel;
using System.Data;
using EXAT.ECM.EON.API.DAL;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.EON.API.Services
{
    public class EONService : IEONService
    {
        private readonly OracleDbContext _oracleContext;
        private readonly IConfiguration _configuration; //AppDomain.CurrentDomain.BaseDirectory + "\\Tempfile\\";
        private readonly ILogger<EONService> _logger;

        public EONService(OracleDbContext oracleContext, IConfiguration configuration, ILogger<EONService> logger)
        {
            _oracleContext = oracleContext;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<List<EON_SUMMARY_REPORT>> GetEONSummaryAsync(EONParameterModel request)
        {
            List<EON_SUMMARY_REPORT> result = new List<EON_SUMMARY_REPORT>();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetEONSummaryAsync ");

                SuccessResponse<EON_SUMMARY_REPORT> response = new SuccessResponse<EON_SUMMARY_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var detail = await GetSummaryAsync(request);
                result = detail;

                // Log หลังจากได้รับข้อมูลจาก Oracle
                _logger.LogInformation("Received {HeaderData} records from Oracle.", detail.Count);

                // ส่งข้อมูลที่ได้รับ
                return result;
            }
            catch (Exception ex)
            {
                // Handle the error and log it
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                throw new Exception(ex.Message);
            }
        }
        private async Task<List<EON_SUMMARY_REPORT>> GetSummaryAsync(EONParameterModel request)
        {
            var result = await _oracleContext
                    .Set<EON_SUMMARY_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_EON.SP_7002_GETLIST_SUMMARY_REPORT (
                                            :P_DOCNO,
                                            :P_DEPARTMENT_CODE,
                                            :P_DIVISION_CODE,
                                            :P_SECTION_CODE,
                                            :p_EXPRESSWAY_ID,
                                            :p_DIRECTION_ID,
                                            :p_SPEED_ID,
                                            :p_STATUS_ID,
   	                                        :p_WORKSTART_DATE_FROM,
   	                                        :p_WORKSTART_DATE_TO,
   	                                        :p_REQUEST_DOCDATE_FROM,
   	                                        :p_REQUEST_DOCDATE_TO,
   	                                        :p_USER_AD,
	                                        :p_output
                                        );
                                    END;",
                    new OracleParameter("p_DOCNO", request.p_DOCNO ?? (object)DBNull.Value),
                    new OracleParameter("p_DEPARTMENT_CODE", request.p_DEPARTMENT_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_DIVISION_CODE", request.p_DIVISION_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_SECTION_CODE", request.p_SECTION_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_EXPRESSWAY_ID", request.p_EXPRESSWAY_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DIRECTION_ID", request.p_DIRECTION_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_SPEED_ID", request.p_SPEED_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_STATUS_ID", request.p_STATUS_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_WORKSTART_DATE_FROM", Utilities.ConvertValue<DateTime>(request.p_WORKSTART_DATE_FROM) ?? (object)DBNull.Value),
                    new OracleParameter("p_WORKSTART_DATE_TO", Utilities.ConvertValue<DateTime>(request.p_WORKSTART_DATE_TO) ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_DOCDATE_FROM", Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOCDATE_FROM) ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_DOCDATE_TO", Utilities.ConvertValue<DateTime>(request.p_REQUEST_DOCDATE_TO) ?? (object)DBNull.Value),
                    new OracleParameter("p_USER_AD", request.p_USER_AD ?? (object)DBNull.Value),
                    new OracleParameter("p_output", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            return result;
        }

        public async Task<List<EON_REQUEST_REPORT>> GetEONRequestAsync(EONParameterModel request)
        {
            List<EON_REQUEST_REPORT> result = new List<EON_REQUEST_REPORT>();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetEONRequestAsync ");

                SuccessResponse<EON_REQUEST_REPORT> response = new SuccessResponse<EON_REQUEST_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var detail = await GetRequestAsync(request);
                result = detail;

                // Log หลังจากได้รับข้อมูลจาก Oracle
                _logger.LogInformation("Received {HeaderData} records from Oracle.", detail.Count);

                // ส่งข้อมูลที่ได้รับ
                return result;
            }
            catch (Exception ex)
            {
                // Handle the error and log it
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                throw new Exception(ex.Message);
            }
        }
        private async Task<List<EON_REQUEST_REPORT>> GetRequestAsync(EONParameterModel request)
        {
            var result = await _oracleContext
                    .Set<EON_REQUEST_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_EON.SP_7003_GETDATA_REQFORM_RPT (
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
