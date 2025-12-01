using EXAT.ECM.Business.DAL;
using EXAT.ECM.Business.Models;
using EXAT.ECM.Business.Models.FED;
using EXAT.ECM.Business.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection.PortableExecutable;
using static EXAT.ECM.Business.Models.APIModel.ResponseModel;
using System.Text.Json; // ต้องเพิ่มที่ด้านบนของไฟล์
namespace EXAT.ECM.Business.Services
{
    public class FEDService : IFEDService
    {
        private readonly OracleDbContext _oracleContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FEDService> _logger;

        public FEDService(OracleDbContext oracleContext, IConfiguration configuration, ILogger<FEDService> logger)
        {
            _oracleContext = oracleContext;
            _configuration = configuration;
            _logger = logger;
        }
        //FED VEHICLE REPORT
        public async Task<FED_VEHICLE_REPORT> GetVEHICLEAsync(FEDParameterModel request)
        {
            FED_VEHICLE_REPORT result = new FED_VEHICLE_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetVEHICLEAsync ");

                SuccessResponse<FED_VEHICLE_REPORT> response = new SuccessResponse<FED_VEHICLE_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderVEHICLEAsync(request);

                result = header.FirstOrDefault();

                _logger.LogInformation("Received {HeaderData} records from Oracle.", header.Count);

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
        private async Task<List<FED_VEHICLE_REPORT>> GetHeaderVEHICLEAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            var result = await _oracleContext
                    .Set<FED_VEHICLE_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7101_GETDATA_VEHICLEREQUEST (
                                            :p_HEADER_ID,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", OracleDbType.Varchar2) { Value = request.p_HEADER_ID ?? (object)DBNull.Value,
                        Direction = ParameterDirection.Input
                    },
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) {
                        Direction = ParameterDirection.Output
                    }
            )
                .ToListAsync();
            _logger.LogInformation("EER VEHICLE JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));

            return result;
        }
        public async Task<FED_HEADER_DAILYVEHIUSE_REPORT> GetDailyVehiUsageAsync(FEDParameterModel request)
        {
            FED_HEADER_DAILYVEHIUSE_REPORT result = new FED_HEADER_DAILYVEHIUSE_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetEERSummaryAsync ");

                SuccessResponse<FED_HEADER_DAILYVEHIUSE_REPORT> response = new SuccessResponse<FED_HEADER_DAILYVEHIUSE_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderDailyVehiUsageAsync(request);
                var detail = await GetDetailDailyVehiUsageAsync(request);

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
                return null;
            }
        }
        public async Task<List<FED_HEADER_DAILYVEHIUSE_REPORT>> GetHeaderDailyVehiUsageAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                    .Set<FED_HEADER_DAILYVEHIUSE_REPORT>()
                    .FromSqlRaw(@"
                            BEGIN 
                                EFM_FED.SP_7102_GETDATA_DAILYVEHIUSE (
                                    :p_HEADER_ID,
                                    :p_REQUEST_ID,
                                    :p_DATA
                                );
                            END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_ID", request.p_REQUEST_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
            )
                .ToListAsync(); // ✅ คืนค่าเป็น List<FED_HEADER_DAILYVEHIUSE_REPORT>

            _logger.LogInformation("EER FED_HEADER_DAILYVEHIUSE_REPORT JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return result;
        }
        public async Task<List<FED_DETAIL_DAILYVEHIUSE_REPORT>> GetDetailDailyVehiUsageAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                   .Set<FED_DETAIL_DAILYVEHIUSE_REPORT>()
                   .FromSqlRaw(@"
                            BEGIN 
                                EFM_FED.SP_7102_GETLIST_DAILYVEHIUSE (
                                    :p_HEADER_ID,
                                    :p_USAGE_ID,
                                    :p_DAILY_ID,
                                    :p_DATA
                                );
                            END;",
                   new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                   new OracleParameter("p_USAGE_ID", request.p_USAGE_ID ?? (object)DBNull.Value),
                   new OracleParameter("p_DAILY_ID", request.p_DAILY_ID ?? (object)DBNull.Value),
                   new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
           )
               .ToListAsync(); // ✅ คืนค่าเป็น List<FED_HEADER_DAILYVEHIUSE_REPORT>

            _logger.LogInformation("EER FED_DETAIL_DAILYVEHIUSE_REPORT JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return result;
            
        }
        public async Task<FED_HEADER_MONTHLYVEHIUSE_REPORT> GetMonthlyVehiUsageAsync(FEDParameterModel request)
        {
            FED_HEADER_MONTHLYVEHIUSE_REPORT result = new FED_HEADER_MONTHLYVEHIUSE_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetEERSummaryAsync ");

                SuccessResponse<FED_HEADER_MONTHLYVEHIUSE_REPORT> response = new SuccessResponse<FED_HEADER_MONTHLYVEHIUSE_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderMonthlyVehiUsageAsync(request);
                var detail = await GetDetailMonthlyVehiUsageAsync(request);

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
                return null;
            }
        }
        public async Task<List<FED_HEADER_MONTHLYVEHIUSE_REPORT>> GetHeaderMonthlyVehiUsageAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                    .Set<FED_HEADER_MONTHLYVEHIUSE_REPORT>()
                    .FromSqlRaw(@"
                            BEGIN 
                                EFM_FED.SP_7104_GETDATA_MONTHLYVEHIUSE (
                                    :p_VEHICLE_ID,
                                    :p_MONTH_NO,
                                    :p_YEAR,
                                    :p_DATA
                                );
                            END;",
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
            )
                .ToListAsync(); // ✅ คืนค่าเป็น List<FED_HEADER_DAILYVEHIUSE_REPORT>

            _logger.LogInformation("EER VEHICLE JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return result;
        }
        public async Task<List<FED_DETAIL_MONTHLYVEHIUSE_REPORT>> GetDetailMonthlyVehiUsageAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                   .Set<FED_DETAIL_MONTHLYVEHIUSE_REPORT>()
                   .FromSqlRaw(@"
                            BEGIN 
                                EFM_FED.SP_7104_GETLIST_MONTHLYVEHIUSE (
                                    :p_VEHICLE_ID,
                                    :p_MONTH_NO,
                                    :p_YEAR,
                                    :p_DATA
                                );
                            END;",
                   new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                   new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                   new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                   new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
           )
               .ToListAsync(); // ✅ คืนค่าเป็น List<FED_HEADER_DAILYVEHIUSE_REPORT>
            _logger.LogInformation("EER VEHICLE JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return result;
        }
        public async Task<FED_HEADER_DriverUsageVehicle_REPORT> GetDriverUsageVehicleAsync(FEDParameterModel request)
        {
            FED_HEADER_DriverUsageVehicle_REPORT result = new FED_HEADER_DriverUsageVehicle_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetEERSummaryAsync ");

                SuccessResponse<FED_HEADER_DAILYVEHIUSE_REPORT> response = new SuccessResponse<FED_HEADER_DAILYVEHIUSE_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderDriverUsageVehicleAsync(request);
                var detail = await GetDetailDriverUsageVehicleAsync(request);

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
                return null;
            }
        }
        private async Task<List<FED_HEADER_DriverUsageVehicle_REPORT>> GetHeaderDriverUsageVehicleAsync(FEDParameterModel request)
        {
            var p1 = request.p_DEPT_CODE ?? (object)DBNull.Value;
            var p2 = request.p_VEHICLE_TYPE ?? (object)DBNull.Value;
            var p3 = request.p_VEHICLE_ID ?? (object)DBNull.Value;
            var p4 = request.p_DRIVER_BY ?? (object)DBNull.Value;
            var p5 = request.p_MONTH_NO ?? (object)DBNull.Value;
            var p6 = request.p_YEAR ?? (object)DBNull.Value;
            var p7 = Utilities.ConvertValue<DateTime>(request.p_START_DATE) ?? (object)DBNull.Value;
            var p8 = Utilities.ConvertValue<DateTime>(request.p_END_DATE) ?? (object)DBNull.Value;
            var result = await _oracleContext
                    .Set<FED_HEADER_DriverUsageVehicle_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7105_GETDATA_DRIVERUSEVEHI (
                                            :p_DEPT_CODE,
                                            :p_VEHICLE_TYPE,
                                            :p_VEHICLE_ID,
                                            :p_DRIVER_BY,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_START_DATE,
                                            :p_END_DATE,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_DEPT_CODE", request.p_DEPT_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_TYPE", request.p_VEHICLE_TYPE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DRIVER_BY", request.p_DRIVER_BY ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_START_DATE", Utilities.ConvertValue<DateTime>(request.p_START_DATE) ?? (object)DBNull.Value),
                    new OracleParameter("p_END_DATE", Utilities.ConvertValue<DateTime>(request.p_END_DATE) ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
            )
                .ToListAsync();
            _logger.LogInformation("EER VEHICLE JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return result;
        }
        private async Task<List<FED_DETAIL_DriverUsageVehicle_REPORT>> GetDetailDriverUsageVehicleAsync(FEDParameterModel request)
        {
            var p1 = request.p_DEPT_CODE ?? (object)DBNull.Value;
            var p2 = request.p_VEHICLE_TYPE ?? (object)DBNull.Value;
            var p3 = request.p_VEHICLE_ID ?? (object)DBNull.Value;
            var p4 = request.p_DRIVER_BY ?? (object)DBNull.Value;
            var p5 = request.p_MONTH_NO ?? (object)DBNull.Value;
            var p6 = request.p_YEAR ?? (object)DBNull.Value;
            var p7 = Utilities.ConvertValue<DateTime>(request.p_START_DATE) ?? (object)DBNull.Value;
            var p8 = Utilities.ConvertValue<DateTime>(request.p_END_DATE) ?? (object)DBNull.Value;
            var result = await _oracleContext
                    .Set<FED_DETAIL_DriverUsageVehicle_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7105_GETLIST_DRIVERUSEVEHI (
                                            :p_DEPT_CODE,
                                            :p_VEHICLE_TYPE,
                                            :p_VEHICLE_ID,
                                            :p_DRIVER_BY,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_START_DATE,
                                            :p_END_DATE,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_DEPT_CODE", request.p_DEPT_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_TYPE", request.p_VEHICLE_TYPE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DRIVER_BY", request.p_DRIVER_BY ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_START_DATE", Utilities.ConvertValue<DateTime>(request.p_START_DATE) ?? (object)DBNull.Value),
                    new OracleParameter("p_END_DATE", Utilities.ConvertValue<DateTime>(request.p_END_DATE) ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
            )
                .ToListAsync();
            _logger.LogInformation("EER VEHICLE JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return result;
        }
        public async Task<FED_HEADER_MachineUse_REPORT> GetMachineUseAsync(FEDParameterModel request)
        {
            FED_HEADER_MachineUse_REPORT result = new FED_HEADER_MachineUse_REPORT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetEERSummaryAsync ");

                SuccessResponse<FED_HEADER_MachineUse_REPORT> response = new SuccessResponse<FED_HEADER_MachineUse_REPORT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderMachineUseAsync(request);
                var detail = await GetDetailMachineUseAsync(request);

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
                return null;
            }
        }

        private async Task<List<FED_HEADER_MachineUse_REPORT>> GetHeaderMachineUseAsync(FEDParameterModel request)
        {
            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            var p2 = request.p_REQUEST_ID ?? (object)DBNull.Value;
            var result = await _oracleContext
                    .Set<FED_HEADER_MachineUse_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7103_GETDATA_MACHINERYUSE (
                                            :p_HEADER_ID,
                                            :p_REQUEST_ID,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_ID", request.p_REQUEST_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
            )
                .ToListAsync();
            _logger.LogInformation("EER VEHICLE JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return result;
        }
        private async Task<List<FED_DETAIL_MachineUse_REPORT>> GetDetailMachineUseAsync(FEDParameterModel request)
        {
            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            var p2 = request.p_REQUEST_ID ?? (object)DBNull.Value;
            var p3 = request.p_DAILY_ID ?? (object)DBNull.Value;
            var result = await _oracleContext
                    .Set<FED_DETAIL_MachineUse_REPORT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7103_GETLIST_MACHINERYUSE (
                                            :p_HEADER_ID,
                                            :p_REQUEST_ID,
                                            :p_DAILY_ID,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_REQUEST_ID", request.p_REQUEST_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DAILY_ID", request.p_DAILY_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
            )
                .ToListAsync();
            _logger.LogInformation("EER VEHICLE JSON: {result}", JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
            return result;
        }


        //FUELEXPENSE REQ REPORT
        public async Task<FUELEXPENSEREQ> GetFuelexpenseRequestFormAsync(FEDParameterModel request)
        {
            FUELEXPENSEREQ result = new FUELEXPENSEREQ();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetFuelexpenseRequestFormAsync ");

                SuccessResponse<FUELEXPENSEREQ> response = new SuccessResponse<FUELEXPENSEREQ>()
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
        private async Task<List<FUELEXPENSEREQ>> GetHeaderRequestFormAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;

            var result = await _oracleContext
                    .Set<FUELEXPENSEREQ>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7301_GETDATA_FUELEXPENSEREQ (
                                            :p_HEADER_ID,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
            return result;
        }
        private async Task<List<DETAIL_FUELEXPENSEREQ>> GetDetailRequestFormAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                    .Set<DETAIL_FUELEXPENSEREQ>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7301_GETLIST_FUELEXPENSEREQ (
                                            :p_HEADER_ID,
                                            :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            return result;
        }

        //POLFUELEXCEED REQ REPORT
        public async Task<PoliceFuelExceed> GetPolicefuelExceedRequestFormAsync(FEDParameterModel request)
        {
            PoliceFuelExceed result = new PoliceFuelExceed();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetPolicefuelExceedRequestFormAsync ");

                SuccessResponse<PoliceFuelExceed> response = new SuccessResponse<PoliceFuelExceed>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderPolicefuelExceedFormAsync(request);
                var detail = await GetDetailPolicefuelExceedFormAsync(request);

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
        private async Task<List<PoliceFuelExceed>> GetHeaderPolicefuelExceedFormAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;

            var result = await _oracleContext
                    .Set<PoliceFuelExceed>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7401_GETDATA_POLFUELEXCEED (
                                            :p_MONTH_NO,
                                            :p_YEAR,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
            return result;
        }
        private async Task<List<DETAIL_PoliceFuelExceed>> GetDetailPolicefuelExceedFormAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                    .Set<DETAIL_PoliceFuelExceed>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7401_GETLIST_POLFUELEXCEED (
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            return result;
        }

        //INCOMPTFUELTAXINV REPORT
        public async Task<FED_INCOMPT_FUEL_TAXINV> GetIncomptFuelTaxinvFormAsync(FEDParameterModel request)
        {
            FED_INCOMPT_FUEL_TAXINV result = new FED_INCOMPT_FUEL_TAXINV();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetIncomptFuelTaxinvFormAsync ");

                SuccessResponse<FED_INCOMPT_FUEL_TAXINV> response = new SuccessResponse<FED_INCOMPT_FUEL_TAXINV>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderIncomptFuelTaxinvFormAsync(request);
                var detail = await GetDetailIncomptFuelTaxinvFormAsync(request);

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
        private async Task<List<FED_INCOMPT_FUEL_TAXINV>> GetHeaderIncomptFuelTaxinvFormAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;

            var result = await _oracleContext
                    .Set<FED_INCOMPT_FUEL_TAXINV>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7402_GETDATA_FUELTAXINCOMPT (
                                            :p_ORG_CODE,
                                            :p_VEHICLE_ID,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_TAX_INVOICE,
                                            :p_DATE_FROM,
                                            :p_DATE_TO,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_TAX_INVOICE", request.p_TAX_INVOICE ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_FROM", request.p_DATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_TO", request.p_DATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
            return result;
        }
        private async Task<List<DETAIL_FED_INCOMPT_FUEL_TAXINV>> GetDetailIncomptFuelTaxinvFormAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                    .Set<DETAIL_FED_INCOMPT_FUEL_TAXINV>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7402_GETLIST_FUELTAXINCOMPT (
                                            :p_ORG_CODE,
                                            :p_VEHICLE_ID,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_TAX_INVOICE,
                                            :p_DATE_FROM,
                                            :p_DATE_TO,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_TAX_INVOICE", request.p_TAX_INVOICE ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_FROM", request.p_DATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_TO", request.p_DATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_DATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            return result;
        }

        //FUELFLEETCARD REPORT  
        public async Task<FuelFleetCard> GetFuelFleetCardFormAsync(FEDParameterModel request)
        {
            FuelFleetCard result = new FuelFleetCard();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetFuelFleetCardFormAsync ");

                SuccessResponse<FuelFleetCard> response = new SuccessResponse<FuelFleetCard>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderFuelFleetCardFormAsync(request);
                var detail = await GetDetailFuelFleetCardFormAsync(request);
                var detail2 = await GetDetail2FuelFleetCardFormAsync(request);

                result = header.FirstOrDefault();
                result.Detail = detail;
                result.Detail2 = detail2;

                // Log หลังจากได้รับข้อมูลจาก Oracle
                _logger.LogInformation("Received {HeaderData} records from Oracle.", header.Count);
                _logger.LogInformation("Received {DetailData} records from Oracle.", detail.Count);
                _logger.LogInformation("Received {DetailData} records from Oracle.", detail2.Count);
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
        private async Task<List<FuelFleetCard>> GetHeaderFuelFleetCardFormAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;

            var result = await _oracleContext
                    .Set<FuelFleetCard>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7403_GETDATA_FLEETCARD (
                                            :p_ORG_CODE,
                                            :p_VEHICLE_ID,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_DATE_FROM,
                                            :p_DATE_TO,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_FROM", request.p_DATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_TO", request.p_DATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
            return result;
        }
        private async Task<List<DETAIL_FuelFleetCard>> GetDetailFuelFleetCardFormAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                    .Set<DETAIL_FuelFleetCard>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7403_GETLIST_FLEETCARD (
                                            :p_ORG_CODE,
                                            :p_VEHICLE_ID,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_DATE_FROM,
                                            :p_DATE_TO,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_FROM", request.p_DATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_TO", request.p_DATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            return result;
        }
        private async Task<List<DETAIL2_FuelFleetCard>> GetDetail2FuelFleetCardFormAsync(FEDParameterModel request)
        {
            var result = await _oracleContext
                    .Set<DETAIL2_FuelFleetCard>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7403_GETLIST_FLEETCARD_PEND (
                                            :p_ORG_CODE,
                                            :p_VEHICLE_ID,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_TAX_INVOICE,
                                            :p_DATE_FROM,
                                            :p_DATE_TO,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_TAX_INVOICE", request.p_TAX_INVOICE ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_FROM", request.p_DATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_TO", request.p_DATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
            return result;
        }
    }
}
