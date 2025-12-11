using EXAT.ECM.FED.API.DAL;
using EXAT.ECM.FED.API.Models;
using EXAT.ECM.FED.API.Services.Interfaces;
using EXAT.ECM.FED.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection.PortableExecutable;
using static EXAT.ECM.FED.API.Models.APIModel.ResponseModel;
using System.Text.Json;
using EXAT.ECM.FED.API.Models.IMPORT;
using OfficeOpenXml;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;
using SkiaSharp;
using System.Reflection;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using EXAT.ECM.FED.API.Helper;
using static EXAT.ECM.FED.API.Models.Utilities;
using System.Globalization;
using System.Threading;
using System.Text;



namespace EXAT.ECM.FED.API.Services
{
    public class FEDService : IFEDService  
    {
        private readonly OracleDbContext _oracleContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FEDService> _logger;
        private readonly string _connectionString;
        private readonly IConfigService _configService;
        private readonly ILoggingService _loggingService; 
        private readonly IFleetCardRepository _repository;

        

        public FEDService(
            OracleDbContext oracleContext, 
            IConfiguration configuration, 
            ILogger<FEDService> logger, 
            IConfigService configService,
            IFleetCardRepository repository, 
            ILoggingService loggingService)
        {
            _oracleContext = oracleContext;
            _configuration = configuration;
            _logger = logger;
            _configService = configService;
            _loggingService = loggingService;
            _repository = repository;
            _connectionString = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
            //_connectionString = configuration.GetConnectionString("OracleConnection");
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

                var headerItem = header?.FirstOrDefault();

                if (headerItem != null)
                {
                    result = headerItem;
                }

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
                //_logger.LogInformation("Received {HeaderData} records from Oracle.", header.Count);
                //_logger.LogInformation("Received {DetailData} records from Oracle.", detail.Count);

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
            try
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
            catch (Exception ex)
            {
                return null;
            }
            
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
                                            :p_ORG_CODE,
                                            :p_MONTH_NO,
                                            :p_YEAR,
	                                        :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
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
                                            :p_ORG_CODE,
                                            :p_VEHICLE_ID,
                                            :p_MONTH,
                                            :p_YEAR,
                                            :p_DOC_DATE_FROM,
                                            :p_DOC_DATE_TO,
                                            :p_USER_AD,
                                            :p_DATA
                                        );
                                    END;",
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DOC_DATE_FROM", request.p_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DOC_DATE_TO", request.p_REQUEST_DOCDATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_USER_AD", request.p_USER_AD ?? (object)DBNull.Value),
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
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<DETAIL_FED_INCOMPT_FUEL_TAXINV>> GetDetailIncomptFuelTaxinvFormAsync(FEDParameterModel request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }

        //FUELFLEETCARD REPORT
        public async Task<List<Header_Report>> GetLIST_HEADER_REPORTFormAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
                var result = await _oracleContext
                    .Set<Header_Report>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7403_GETLIST_HEADER_REPORT (
                                            :p_HEADER_ID,
                                            :p_ORG_CODE,
                                            :p_VEHICLE_ID,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_DATE_FROM,
                                            :p_DATE_TO,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_FROM", request.p_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_TO", request.p_REQUEST_DOCDATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        public async Task<FuelFleetCard> GetFuelFleetCardFormAsync(ParameterHEADER_REPORT request)
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
                //_logger.LogInformation("Received {HeaderData} records from Oracle.", header.Count);
                //_logger.LogInformation("Received {DetailData} records from Oracle.", detail.Count);
                //_logger.LogInformation("Received {DetailData} records from Oracle.", detail2.Count);
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
        private async Task<List<FuelFleetCard>> GetHeaderFuelFleetCardFormAsync(ParameterHEADER_REPORT request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
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
                                            :p_HEADER_ID,
                                            :p_CATEGORY_ID,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_FROM", request.p_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_TO", request.p_REQUEST_DOCDATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_CATEGORY_ID", request.p_CATEGORY_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<DETAIL_FuelFleetCard>> GetDetailFuelFleetCardFormAsync(ParameterHEADER_REPORT request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
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
                                            :p_HEADER_ID,
                                            :p_CATEGORY_ID,
	                                        :p_OUTDATA
                                        );
                                    END;",
                   new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                   new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                   new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                   new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                   new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                   new OracleParameter("p_DATE_FROM", request.p_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value),
                   new OracleParameter("p_DATE_TO", request.p_REQUEST_DOCDATE_TO ?? (object)DBNull.Value),
                   new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                   new OracleParameter("p_CATEGORY_ID", request.p_CATEGORY_ID ?? (object)DBNull.Value),
                   new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
               .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<DETAIL2_FuelFleetCard>> GetDetail2FuelFleetCardFormAsync(ParameterHEADER_REPORT request)
        {
            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
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
                                            :p_DATE_FROM,
                                            :p_DATE_TO,
                                            :p_HEADER_ID,
                                            :p_CATEGORY_ID,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_ORG_CODE", request.p_ORG_CODE ?? (object)DBNull.Value),
                    new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_MONTH_NO", request.p_MONTH_NO ?? (object)DBNull.Value),
                    new OracleParameter("p_YEAR", request.p_YEAR ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_FROM", request.p_REQUEST_DOCDATE_FROM ?? (object)DBNull.Value),
                    new OracleParameter("p_DATE_TO", request.p_REQUEST_DOCDATE_TO ?? (object)DBNull.Value),
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_CATEGORY_ID", request.p_CATEGORY_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
                )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }

        // FuelFleetCardBank
        public async Task<FED_Header_FUEL_FLEET_CARD_BANK> GetFuelFleetCardBank (FEDParameterModel request)
        {
            FED_Header_FUEL_FLEET_CARD_BANK result = new FED_Header_FUEL_FLEET_CARD_BANK();
            try
            {
                _logger.LogInformation("Starting GetFuelFleetCardFormAsync ");
                SuccessResponse<FuelFleetCard> response = new SuccessResponse<FuelFleetCard>()
                {
                    Status = "S",
                    StatusCode = "200"
                };
                _logger.LogInformation("Calling Oracle ");
                var header = await GetDATAFuelFleetCardBank(request);
                var detail = await GetDetailFuelFleetCardBank(request);
                var first = detail.FirstOrDefault();
                result = new FED_Header_FUEL_FLEET_CARD_BANK
                {
                    TOTAL_EXCL_VAT_AMT = first.TOTAL_EXCL_VAT_AMT,
                    TOTAL_VAT_AMT = first.TOTAL_VAT_AMT,
                    GRAND_TOTAL_AMT = first.GRAND_TOTAL_AMT,
                    TOTAL_VOLUME_LITERS = first.TOTAL_VOLUME_LITERS,
                    TOTAL_PRICE_PER_LITER = first.TOTAL_PRICE_PER_LITER,
                    Detail = detail,
                    header_data = header.FirstOrDefault()
                };
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                //throw new ErrorResponse
                //{
                //    Status = "E",
                //    StatusCode = "500",
                //    Message = "An error occurred while processing your request."
                //};
                return null;
            }
        }
        public async Task<List<FED_DATA_FUEL_FLEET_CARD_BANK>> GetDATAFuelFleetCardBank(FEDParameterModel request)
        {
            try
            {
                var dateFrom = string.IsNullOrEmpty(request.p_DATE_FROM)
                ? (object)DBNull.Value
                : DateTime.ParseExact(request.p_DATE_FROM, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                var dateTo = string.IsNullOrEmpty(request.p_DATE_TO)
                    ? (object)DBNull.Value
                    : DateTime.ParseExact(request.p_DATE_TO, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var result = await _oracleContext
                   .Set<FED_DATA_FUEL_FLEET_CARD_BANK>()
                  .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7406_GETDATA_FLEETCARD_BANK (
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
                   new OracleParameter("p_DATE_FROM", OracleDbType.Date) { Value = dateFrom },
                   new OracleParameter("p_DATE_TO", OracleDbType.Date) { Value = dateTo },
                   new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
               .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        public async Task<List<FED_DETAIL_FUEL_FLEET_CARD_BANK>> GetDetailFuelFleetCardBank(FEDParameterModel request)
        {
            try
            {
                var dateFrom = string.IsNullOrEmpty(request.p_DATE_FROM)
                ? (object)DBNull.Value
                : DateTime.ParseExact(request.p_DATE_FROM, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                var dateTo = string.IsNullOrEmpty(request.p_DATE_TO)
                    ? (object)DBNull.Value
                    : DateTime.ParseExact(request.p_DATE_TO, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                var result = await _oracleContext
                   .Set<FED_DETAIL_FUEL_FLEET_CARD_BANK>()
                  .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7406_GETLIST_FLEETCARD_BANK (
                                            :p_ORG_CODE,
                                            :p_VEHICLE_ID,
                                            :p_MONTH_NO,
                                            :p_YEAR,
                                            :p_DATE_FROM,
                                            :p_DATE_TO,
	                                        :p_OUTDATA
                                        );
                                    END;",
                   new OracleParameter("p_ORG_CODE", request.p_ORG_CODE?? (object)DBNull.Value),
                   new OracleParameter("p_VEHICLE_ID", request.p_VEHICLE_ID?? (object)DBNull.Value),
                   new OracleParameter("p_MONTH_NO", request.p_MONTH_NO?? (object)DBNull.Value),
                   new OracleParameter("p_YEAR", request.p_YEAR?? (object)DBNull.Value),
                   new OracleParameter("p_DATE_FROM", OracleDbType.Date) { Value = dateFrom },
                   new OracleParameter("p_DATE_TO", OracleDbType.Date){ Value = dateTo },
                   new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
               .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }

        //VEHICLEHANDOVER
        public async Task<FED_HEADER_VEHICLEHANDOVER> GetVEHICLEHANDOVERFormAsync(FEDParameterModel request)
        {
            FED_HEADER_VEHICLEHANDOVER result = new FED_HEADER_VEHICLEHANDOVER();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetVEHICLEHANDOVERFormAsync ");
                SuccessResponse<FED_HEADER_VEHICLEHANDOVER> response = new SuccessResponse<FED_HEADER_VEHICLEHANDOVER>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");
                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderVEHICLEHANDOVERFormAsync(request);

                result = header.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<FED_HEADER_VEHICLEHANDOVER>> GetHeaderVEHICLEHANDOVERFormAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
                var result = await _oracleContext
                    .Set<FED_HEADER_VEHICLEHANDOVER>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7109_GETDATA_HANDOVER_TOOLS (
                                            :P_HEADER_ID,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        //VEHICLEREPAIRREQUEST
        public async Task<FED_HEADER_VEHICLEREPAIRREQUEST> GetVEHICLEREPAIRREQUESTFormAsync(FEDParameterModel request)
        {
            FED_HEADER_VEHICLEREPAIRREQUEST result = new FED_HEADER_VEHICLEREPAIRREQUEST();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetVEHICLEHANDOVERFormAsync ");
                SuccessResponse<FED_HEADER_VEHICLEREPAIRREQUEST> response = new SuccessResponse<FED_HEADER_VEHICLEREPAIRREQUEST>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");
                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderVEHICLEREPAIRREQUESTFormAsync(request);

                result = header.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<FED_HEADER_VEHICLEREPAIRREQUEST>> GetHeaderVEHICLEREPAIRREQUESTFormAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
                var result = await _oracleContext
                    .Set<FED_HEADER_VEHICLEREPAIRREQUEST>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7201_GETDATA_REPAIRE_ITEM (
                                            :P_HEADER_ID,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        // DailyVehicleInspection
        public async Task<FED_HEADER_INSPT_EQUIPMNT> GetDailyVehicleInspectionFormAsync(FEDParameterModel request)
        {
            FED_HEADER_INSPT_EQUIPMNT result = new FED_HEADER_INSPT_EQUIPMNT();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetVEHICLEHANDOVERFormAsync ");
                SuccessResponse<FED_HEADER_INSPT_EQUIPMNT> response = new SuccessResponse<FED_HEADER_INSPT_EQUIPMNT>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");
                // เรียกข้อมูลจาก Oracle
                var header = await GetHeaderDailyVehicleInspectionFormAsync(request);

                result = header.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<FED_HEADER_INSPT_EQUIPMNT>> GetHeaderDailyVehicleInspectionFormAsync(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
                var result = await _oracleContext
                    .Set<FED_HEADER_INSPT_EQUIPMNT>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7106_GETDATA_INSPT_EQUIPMNT (
                                            :P_HEADER_ID,
	                                        :p_OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("p_OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }

        //Import Feed card
        public async Task<ImportResult> ImportFileExcelFED(FEDParameterModel request)
        {
            var p1 = request.p_TEMP_ID ?? (object)DBNull.Value;

            string conectdb = "";
            string CheckFileExcel = "";
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

                    conectdb = "Step 1. Check Connect : ✅ Connection Successful! - database = " + dbName;
                }
            }
            catch (Exception ex)
            {
                conectdb = "Step 1. Check Connect : ❌ Connection Failed - error = " + ex.Message;
            }

            var result = new ImportResult();
            try
            {
                
                // Step 1: Get file from SP_GET_FILE_EXCEL
                var FileExcelList = await GetFileFromDatabase(request);
                var FileExcel = FileExcelList?.FirstOrDefault();
                
                if (FileExcel == null || string.IsNullOrEmpty(FileExcel.CONTENT_VALUE))
                {
                    CheckFileExcel = "Step 2. Check FileExcel : ไม่พบไฟล์ที่เกี่ยวข้อง";

                    result.Status = "E";
                    result.Message = conectdb + ";" + CheckFileExcel;
                    //await InsertLog(p_Parameter, "E", result.Message);
                    return result;
                }

                if(FileExcel != null && !string.IsNullOrEmpty(FileExcel.CONTENT_VALUE))
                {
                    // Step 2: Decode + read to DataTable

                    var tbl = ReadExcelBase64(FileExcel.CONTENT_VALUE);
                    if (tbl.Rows.Count == 0 || string.IsNullOrEmpty(FileExcel.CONTENT_VALUE))
                    {
                        CheckFileExcel = "Step 3. Check Data File Excel : ไม่พบข้อมูลในไฟล์ Excel";

                        result.Status = "E";
                        result.Message = conectdb + ";" + CheckFileExcel;
                        //await InsertLog(p_Parameter, "E", result.Message);
                        return result;
                    }
                    if (tbl.Rows.Count != 0)
                    {
                        // Step 3: Call SP_VALIDATE_EXCEL_TEMP_ID
                        var tempList = ConvertDataTableToTemp(tbl, request.p_TEMP_ID);
                        using (var conn = _oracleContext.GetOracleConnection())

                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = @"INSERT INTO EFM_FED.T_TEMP_FED_IMPORT_FLEETCARD (
                                                ITEM_ID
	                                            , IMPORT_DATE
	                                            , ACCOUNT_NO
	                                            , CREDIT_LINE
	                                            , FROM_DATE
	                                            , TODATE
	                                            , DEPARTMENT
	                                            , COST_CENTER
	                                            , CARD_NO
	                                            , PLATE_NO
	                                            , TRANSACTION_DATE
	                                            , MERCHANT_ID
	                                            , TAX_ID
	                                            , MERCHANT_NAME
	                                            , LOCATION
	                                            , ADDRESS_ACCORDING
	                                            , BRANCH_NUMBER
	                                            , INVOICE_NO
	                                            , PRODUCT
	                                            , QUANTITY_LITRE
	                                            , QUANTITY_KM
	                                            , EXCLUDE_VAT_AMOUNT
	                                            , VAT_AMOUNT
	                                            , AMOUNT
	                                            , UNIT_PRICE
	                                            , ODOMETER
	                                            , DISTANCE_KM
	                                            , FUEL_CONS_KM_LITRE
	                                            , FUEL_CONS_BAHT_KM
	                                            , NGV_CONS_KM_KG
	                                            , NGV_CONS_BAHT_KM
	                                            , LPG_CONS_KM_LITRE
	                                            , LPG_CONS_BAHT_KM
	                                            , FUEL_CONS_KM_LITRE2
	                                            , TEMP_GROUP_ID
                                        ) VALUES (
                                                  :ITEM_ID
	                                            , :IMPORT_DATE
	                                            , :ACCOUNT_NO
	                                            , :CREDIT_LINE
	                                            , :FROM_DATE
	                                            , :TODATE
	                                            , :DEPARTMENT
	                                            , :COST_CENTER
	                                            , :CARD_NO
	                                            , :PLATE_NO
	                                            , :TRANSACTION_DATE
	                                            , :MERCHANT_ID
	                                            , :TAX_ID
	                                            , :MERCHANT_NAME
	                                            , :LOCATION
	                                            , :ADDRESS_ACCORDING
	                                            , :BRANCH_NUMBER
	                                            , :INVOICE_NO
	                                            , :PRODUCT
	                                            , :QUANTITY_LITRE
	                                            , :QUANTITY_KM
	                                            , :EXCLUDE_VAT_AMOUNT
	                                            , :VAT_AMOUNT
	                                            , :AMOUNT
	                                            , :UNIT_PRICE
	                                            , :ODOMETER
	                                            , :DISTANCE_KM
	                                            , :FUEL_CONS_KM_LITRE
	                                            , :FUEL_CONS_BAHT_KM
	                                            , :NGV_CONS_KM_KG
	                                            , :NGV_CONS_BAHT_KM
	                                            , :LPG_CONS_KM_LITRE
	                                            , :LPG_CONS_BAHT_KM
	                                            , :FUEL_CONS_KM_LITRE2
	                                            , :TEMP_GROUP_ID
                                                           )";
                            cmd.ArrayBindCount = tempList.Count;

                            cmd.Parameters.Add(":TEMP_ID", OracleDbType.Varchar2, tempList.Select(x => x.ITEM_ID).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":IMPORT_DATE", OracleDbType.Date, tempList.Select(x => x.INSERT_DATE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":ACCOUNT_NO", OracleDbType.Varchar2, tempList.Select(x => x.ACCOUNT_NO).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":CREDIT_LINE", OracleDbType.Varchar2, tempList.Select(x => x.CREDIT_LINE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":FROM_DATE", OracleDbType.Varchar2, tempList.Select(x => x.FROM_DATE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":TODATE", OracleDbType.Varchar2, tempList.Select(x => x.TODATE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":DEPARTMENT", OracleDbType.Varchar2, tempList.Select(x => x.DEPARTMENT).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":COST_CENTER", OracleDbType.Varchar2, tempList.Select(x => x.COST_CENTER).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":CARD_NO", OracleDbType.Varchar2, tempList.Select(x => x.CARD_NO).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":PLATE_NO", OracleDbType.Varchar2, tempList.Select(x => x.PLATE_NO).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":TRANSACTION_DATE", OracleDbType.Varchar2, tempList.Select(x => x.TRANSACTION_DATE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":MERCHANT_ID", OracleDbType.Varchar2, tempList.Select(x => x.MERCHANT_ID).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":TAX_ID", OracleDbType.Varchar2, tempList.Select(x => x.TAX_ID).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":MERCHANT_NAME", OracleDbType.Varchar2, tempList.Select(x => x.MERCHANT_NAME).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":LOCATION", OracleDbType.Varchar2, tempList.Select(x => x.LOCATION).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":ADDRESS_ACCORDING", OracleDbType.Varchar2, tempList.Select(x => x.ADDRESS_ACCORDING).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":BRANCH_NUMBER", OracleDbType.Varchar2, tempList.Select(x => x.BRANCH_NUMBER).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":INVOICE_NO", OracleDbType.Varchar2, tempList.Select(x => x.INVOICE_NO).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":PRODUCT", OracleDbType.Varchar2, tempList.Select(x => x.PRODUCT).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":QUANTITY_LITRE", OracleDbType.Varchar2, tempList.Select(x => x.QUANTITY_LITRE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":QUANTITY_KM", OracleDbType.Varchar2, tempList.Select(x => x.QUANTITY_KM).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":EXCLUDE_VAT_AMOUNT", OracleDbType.Varchar2, tempList.Select(x => x.EXCLUDE_VAT_AMOUNT).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":VAT_AMOUNT", OracleDbType.Varchar2, tempList.Select(x => x.VAT_AMOUNT).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":AMOUNT", OracleDbType.Varchar2, tempList.Select(x => x.AMOUNT).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":UNIT_PRICE", OracleDbType.Varchar2, tempList.Select(x => x.UNIT_PRICE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":ODOMETER", OracleDbType.Varchar2, tempList.Select(x => x.ODOMETER).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":DISTANCE_KM", OracleDbType.Varchar2, tempList.Select(x => x.DISTANCE_KM).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":FUEL_CONS_KM_LITRE", OracleDbType.Varchar2, tempList.Select(x => x.FUEL_CONS_KM_LITRE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":FUEL_CONS_BAHT_KM", OracleDbType.Varchar2, tempList.Select(x => x.FUEL_CONS_BAHT_KM).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":NGV_CONS_KM_KG", OracleDbType.Varchar2, tempList.Select(x => x.NGV_CONS_KM_KG).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":NGV_CONS_BAHT_KM", OracleDbType.Varchar2, tempList.Select(x => x.NGV_CONS_BAHT_KM).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":LPG_CONS_KM_LITRE", OracleDbType.Varchar2, tempList.Select(x => x.LPG_CONS_KM_LITRE).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":LPG_CONS_BAHT_KM", OracleDbType.Varchar2, tempList.Select(x => x.LPG_CONS_BAHT_KM).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":FUEL_CONS_KM_LITRE2", OracleDbType.Varchar2, tempList.Select(x => x.FUEL_CONS_KM_LITRE2).ToArray(), ParameterDirection.Input);
                            cmd.Parameters.Add(":TEMP_GROUP_ID", OracleDbType.Varchar2, tempList.Select(x => x.TEMP_GROUP_ID).ToArray(), ParameterDirection.Input);

                            await cmd.ExecuteNonQueryAsync();

                            // Step 4: Insert to Temp Table
                            var isValid = await ValidateExcel(request);
                            var statusvalidate = isValid?.FirstOrDefault();

                            if (statusvalidate.Status == "E")
                            {
                                //string url = await GenerateErrorFile(p_Parameter);
                                result.Status = "EU";
                                result.Message = "ข้อมูลบางรายการไม่ถูกต้อง";
                                //result.ErrorFileUrl = url;
                                //await InsertLog(p_Parameter, "S", result.Message);
                                return result;
                            }
                            if (statusvalidate.Status == "S")
                            {
                                //string url = await GenerateErrorFile(p_Parameter);
                                result.Status = "S";
                                result.Message = "Success";
                                //result.ErrorFileUrl = url;
                                //await InsertLog(p_Parameter, "S", result.Message);
                                return result;
                            }

                        }
                        
                    }
                }
                //await InsertLog(p_Parameter, "S", result.Message);
                return result;
            }
            catch (Exception ex)
            {
                result.Status = "E";
                result.Message = ex.Message;
                //await InsertLog(p_Parameter, "E", ex.Message);
                //return StatusCode(500, result);
                return result;
            }
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
        private DataTable ReadExcelBase64(string base64)
        {
            var tbl = new DataTable();

            byte[] bytes;

            string wrappedXml = $"<root>{base64}</root>";
            var doc = XDocument.Parse(wrappedXml);
            var node = doc.Descendants("content").FirstOrDefault()?.Value;

            try
            {
                bytes = Convert.FromBase64String(node);
            }
            catch (FormatException ex)
            {
                throw new Exception("ข้อมูลไฟล์ไม่ถูกต้องหรือไม่ได้อยู่ในรูปแบบ base64", ex);
            }
           

            using var stream = new MemoryStream(bytes);
            using var excel = new ExcelPackage(stream);
            var ws = excel.Workbook.Worksheets.First();

            foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                tbl.Columns.Add(firstRowCell.Text);

            int startRow = 2;
            for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                DataRow row = tbl.NewRow();
                foreach (var cell in wsRow)
                    row[cell.Start.Column - 1] = cell.Text;
                tbl.Rows.Add(row);
            }

            return tbl;
        }
        private List<T_TEMP_FED_IMPORT_FLEETCARD> ConvertDataTableToTemp(DataTable tbl, string? tempId)
        {
            var listData = new List<T_TEMP_FED_IMPORT_FLEETCARD>();
            var date = DateTime.Now;
            for (int i = 0; i < tbl.Rows.Count; i++)
            {

                var item = new T_TEMP_FED_IMPORT_FLEETCARD
                {
                    ITEM_ID = Guid.NewGuid(),
                    TEMP_GROUP_ID = tempId,
                    INSERT_DATE = date
                };
                for (int j = 0; j < tbl.Columns.Count; j++)
                {
                    string columnName = tbl.Columns[j].ColumnName;
                    string value = tbl.Rows[i][j]?.ToString();
                    SetColumnValue(item, columnName, value);
                }
                listData.Add(item);
            }
            return listData;
        }
        private void SetColumnValue(T_TEMP_FED_IMPORT_FLEETCARD item, string columnName, string value)
        {
            string cleanValue = !string.IsNullOrEmpty(value) ? value.Trim() : null;
            switch (columnName)
            {
                case "IMPORT_DATE": item.IMPORT_DATE = cleanValue; break;
                case "ACCOUNT_NO": item.ACCOUNT_NO = cleanValue; break;
                case "CREDIT_LINE": item.CREDIT_LINE = cleanValue; break;
                case "FROM_DATE": item.FROM_DATE = cleanValue; break;
                case "TODATE": item.TODATE = cleanValue; break;
                case "DEPARTMENT": item.DEPARTMENT = cleanValue; break;
                case "COST_CENTER": item.COST_CENTER = cleanValue; break;
                case "CARD_NO": item.CARD_NO = cleanValue; break;
                case "PLATE_NO": item.PLATE_NO = cleanValue; break;
                case "TRANSACTION_DATE": item.TRANSACTION_DATE = cleanValue; break;
                case "MERCHANT_ID": item.MERCHANT_ID = cleanValue; break;
                case "TAX_ID": item.TAX_ID = cleanValue; break;
                case "MERCHANT_NAME": item.MERCHANT_NAME = cleanValue; break;
                case "LOCATION": item.LOCATION = cleanValue; break;
                case "ADDRESS_ACCORDING": item.ADDRESS_ACCORDING = cleanValue; break;
                case "BRANCH_NUMBER": item.BRANCH_NUMBER = cleanValue; break;
                case "INVOICE_NO": item.INVOICE_NO = cleanValue; break;
                case "PRODUCT": item.PRODUCT = cleanValue; break;
                case "QUANTITY_LITRE": item.QUANTITY_LITRE = cleanValue; break;
                case "QUANTITY_KM": item.QUANTITY_KM = cleanValue; break;
                case "EXCLUDE_VAT_AMOUNT": item.EXCLUDE_VAT_AMOUNT = cleanValue; break;
                case "VAT_AMOUNT": item.VAT_AMOUNT = cleanValue; break;
                case "AMOUNT": item.AMOUNT = cleanValue; break;
                case "UNIT_PRICE": item.UNIT_PRICE = cleanValue; break;
                case "ODOMETER": item.ODOMETER = cleanValue; break;
                case "DISTANCE_KM": item.DISTANCE_KM = cleanValue; break;
                case "FUEL_CONSUMPTION_KM_LITRE": item.FUEL_CONS_KM_LITRE = cleanValue; break;
                case "FUEL_CONSUMPTION_BAHT_KM": item.FUEL_CONS_BAHT_KM = cleanValue; break;
                case "NGV_CONSUMPTION_KM_KG": item.NGV_CONS_KM_KG = cleanValue; break;
                case "NGV_CONSUMPTION_BAHT_KM": item.NGV_CONS_BAHT_KM = cleanValue; break;
                case "LPG_CONSUMPTION_KM_LITRE": item.LPG_CONS_KM_LITRE = cleanValue; break;
                case "LPG_CONSUMPTION_BAHT_KM": item.LPG_CONS_BAHT_KM = cleanValue; break;
                case "FUEL_CONSUMPTION_KM__LITRE": item.FUEL_CONS_KM_LITRE2 = cleanValue; break;
            }
        }
        private async Task<List<T_VALIDATE_EXCEL>> ValidateExcel(FEDParameterModel request)
        {
            var p1 = request.p_TEMP_ID ?? (object)DBNull.Value;

            var result = await _oracleContext
                    .Set<T_VALIDATE_EXCEL>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7404_VALIDATE_EXCEL (
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

        public async Task<T_TEMP_FED_IMPORT_FLEETCARD_ERROR> DownloadErrorExcel(FEDParameterModel request)
        {
            T_TEMP_FED_IMPORT_FLEETCARD_ERROR result = new T_TEMP_FED_IMPORT_FLEETCARD_ERROR();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetFuelFleetCardFormAsync ");

                SuccessResponse<T_TEMP_FED_IMPORT_FLEETCARD_ERROR> response = new SuccessResponse<T_TEMP_FED_IMPORT_FLEETCARD_ERROR>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                // Log ก่อนเรียกข้อมูลจาก Oracle
                _logger.LogInformation("Calling Oracle ");

                // เรียกข้อมูลจาก Oracle
                
                var detail = await GetFileExcelError(request);
                result.Detail = detail;

                // Log หลังจากได้รับข้อมูลจาก Oracle
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

        private async Task<List<T_TEMP_FED_IMPORT_FLEETCARD_ERROR_LIST>> GetFileExcelError(FEDParameterModel request)
        {

            var p1 = request.p_TEMP_ID ?? (object)DBNull.Value;

            var result = await _oracleContext
                    .Set<T_TEMP_FED_IMPORT_FLEETCARD_ERROR_LIST>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7404_GET_FILE_EXCEL_ERROR (
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

        public async Task<ImportResultBankFED<Dictionary<string, string?>>> ImportTransactionsAsync(FEDParameterModel request)
        {

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

            var result = new ImportResultBankFED<Dictionary<string, string?>>
            {
                HeaderId = null
            };

            // 0) Check DB connection (optional health check)
            string connectDbNote;
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                using var command = new OracleCommand("SELECT sys_context('USERENV', 'DB_NAME') FROM dual", connection);
                var dbName = (await command.ExecuteScalarAsync())?.ToString();
                connectDbNote = $"Step 1. Check Connect : ✅ Connection Successful! - database = {dbName}";
            }
            catch (Exception ex)
            {
                connectDbNote = $"Step 1. Check Connect : ❌ Connection Failed - error = {ex.Message}";
            }

            // 1) ดึงไฟล์จาก DB
            var fileExcelList = await GetFileFromDatabase(request);
            var fileExcel = fileExcelList?.FirstOrDefault();

            if (fileExcel == null || string.IsNullOrWhiteSpace(fileExcel.CONTENT_VALUE))
            {
                var msg = "Step 2. Check FileExcel : ไม่พบไฟล์ที่เกี่ยวข้อง";
                result.Status = "E";
                result.Message = $"{connectDbNote};{msg}";
                result.FailureDetails.Add(new ImportFailureDetail
                {
                    RowNumber = 0,
                    RawData = "",
                    ErrorMessage = $"{connectDbNote};เกิดข้อผิดพลาดในการอ่านไฟล์: {msg}"
                });
                return result;
            }

            // ตั้งค่าพื้นฐานจากไฟล์
            result.HeaderId = fileExcel.HEADER_ID;
            var fileName = fileExcel.FILE_NAME ?? "UNKNOWN";
            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            // 2) Template name -> config
            string determinedTemplateName = DetermineTemplateNameFromFileName(fileName);
            var config = await _configService.GetTemplateConfigAsync(determinedTemplateName);
            if (!config.Any())
                throw new Exception($"Configuration for template '{determinedTemplateName}' not found in database.");

            // 3) Allow only .xlsx (EPPlus limitation)
            if (ext == ".xls")
            {
                // ถ้าต้องรองรับ .xls จริง ๆ แนะนำเปลี่ยนไปใช้ NPOI/Aspose.Cells แทน EPPlus
                result.Status = "E";
                result.Message = "ไฟล์ .xls ไม่รองรับโดย EPPlus กรุณาแปลงเป็น .xlsx ก่อนนำเข้า (หรือเปลี่ยนไปใช้ NPOI/Aspose.Cells สำหรับ .xls)";
                result.FailureDetails.Add(new ImportFailureDetail
                {
                    RowNumber = 0,
                    RawData = "",
                    ErrorMessage = result.Message
                });
                return result;
            }

            try
            {
                // 4) แปลง CLOB -> bytes (พยายามถอด Base64 แบบยืดหยุ่น)
                //if (!ClobBinaryDecoder.TryDecodeBase64Flexible(fileExcel.CONTENT_VALUE, out var fileBytes, out var decodeWhy))
                //{
                //    throw new Exception($"ถอดข้อมูลไฟล์จาก CLOB ไม่สำเร็จ: {decodeWhy}");
                //}
                var fileBytes = Convert.FromBase64String(fileExcel.CONTENT_VALUE);

                using var stream = new MemoryStream(fileBytes);
                using var package = new ExcelPackage(stream);

                var ws = package.Workbook.Worksheets.FirstOrDefault()
                         ?? throw new Exception("No worksheet found in Excel file.");
                if (ws.Dimension == null)
                    throw new Exception("Worksheet has no used range.");

                // 5) อ่าน header ส่วนบน
                var headerInfo = ImportParsingHelpers.ReadReportHeader(ws, config, headerRow: 7, processBlockRow: 6);

                // 6) วนอ่านแถวข้อมูลจริง
                var state = ContinuationState.Empty;
                int rowStart = 19; // ทำ dynamic ได้ภายหลัง
                int rowCount = ws.Dimension.End.Row;
                int totalRowsInFile = 0;
                DateTime DateNow = DateTime.Now;

                for (int row = rowStart; row <= rowCount; row++)
                {
                    // 6.1 Department row
                    if (ImportParsingHelpers.TryReadDepartmentRow(ws, row, out var dept, out var costCenter))
                    {
                        state = state.With(department: dept, costCenter: costCenter);
                        continue;
                    }

                    // 6.2 Card row
                    if (ImportParsingHelpers.TryReadCardRow(ws, row, out var cardNo, out var plateNo))
                    {
                        if (!string.IsNullOrEmpty(cardNo) || !string.IsNullOrEmpty(plateNo))
                            state = state.With(cardNumber: cardNo, plateNumber: plateNo);
                        continue;
                    }

                    // 6.3 Transaction row
                    ImportParsingHelpers.IsTransactionRow(ws, row, out var rawDateText);
                    var check = rawDateText;
                    if (check != null && check != "")
                    {
                        totalRowsInFile++;

                        var (ok, txDate, err) = ImportParsingHelpers.TryParseTransactionDate(rawDateText);
                        if (!ok)
                        {
                            var failureDetail = new ImportFailureDetail
                            {
                                RowNumber = row,
                                RawData = string.Join(" | ", Enumerable.Range(1, ws.Dimension.End.Column).Select(col => ws.Cells[row, col].Text)),
                                ErrorMessage = $"TransactionDate '{rawDateText}' is invalid or missing. Use DateTime.Now instead. ({err})"
                            };
                            result.FailureDetails.Add(failureDetail);

                            await _loggingService.LogErrorAsync(
                                "WARN",
                                new Exception("Invalid TransactionDate"),
                                failureDetail.ErrorMessage,
                                $"File: {fileName}, Row: {row}, Col: 1");

                            txDate = DateNow.ToString();
                        }

                        var rowData = ImportParsingHelpers.BuildRowDataFromConfig(ws, row, config, state);
                        var entity = ImportParsingHelpers.MapToEntity(rowData, headerInfo, result.HeaderId!, txDate);

                        try
                        {
                            await _repository.InsertTransactionAsync(entity);
                            result.SuccessDetails.Add(rowData);
                        }
                        catch (Exception ex)
                        {
                            var failureDetail = new ImportFailureDetail
                            {
                                RowNumber = row,
                                RawData = string.Join(" | ", Enumerable.Range(1, ws.Dimension.End.Column).Select(col => ws.Cells[row, col].Text)),
                                ErrorMessage = ex.Message
                            };
                            result.FailureDetails.Add(failureDetail);

                            await _loggingService.LogErrorAsync("ERROR", ex, "Failed to insert transaction.",
                                $"File: {fileName}, Row: {row}");
                        }
                    }
                }

                // 7) สรุปผล
                result.TotalRowsInFile = totalRowsInFile;
                result.SuccessfulRows = result.SuccessDetails.Count;
                result.FailedRows = result.FailureDetails.Count;

                if (totalRowsInFile == 0)
                {
                    var ex = new Exception("No data rows found in import file.");
                    await _loggingService.LogErrorAsync("ERROR", ex, "No data rows", $"File: {fileName}");
                    result.FailureDetails.Add(new ImportFailureDetail
                    {
                        RowNumber = 0,
                        RawData = "",
                        ErrorMessage = "ไม่พบข้อมูลในไฟล์ที่นำเข้า"
                    });
                }

                result.Status = result.FailedRows == 0 ? "S" : "E";
                result.Message = (result.Status == "S")
                    ? "นำเข้าข้อมูลสำเร็จ"
                    : $"นำเข้าบางรายการไม่สำเร็จ (สำเร็จ {result.SuccessfulRows} / ล้มเหลว {result.FailedRows})";
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("ERROR", ex, "Error reading import file", $"File: {fileName}");
                result.Status = "E";
                result.Message = ex.Message;
                result.FailureDetails.Add(new ImportFailureDetail
                {
                    RowNumber = 0,
                    RawData = "",
                    ErrorMessage = "เกิดข้อผิดพลาดในการอ่านไฟล์: " + ex.Message
                });
            }

            return result;
        }

        /// <summary>
        /// เมธอด Helper สำหรับวิเคราะห์ชื่อไฟล์เพื่อหา Template
        /// </summary>
        private string DetermineTemplateNameFromFileName(string fileName)
        {
            var upperFileName = fileName.ToUpperInvariant();
            if (upperFileName.StartsWith("VAT_") || upperFileName.StartsWith("NOVAT_"))
            {
                return "ORPT_MONTHLY_REPORT";
            }
            throw new ArgumentException("Invalid filename format. Only files starting with VAT_ or NOVAT_ are supported.");
        }


        #region VehicleInspectionDelivery
        public async Task<FED_HEADER_VehicleInspectionDelivery1> GetVehicleInspectionDelivery1(FEDParameterModel request)
        {
            FED_HEADER_VehicleInspectionDelivery1 result = new FED_HEADER_VehicleInspectionDelivery1();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetVehicleInspectionDelivery1 ");
                SuccessResponse<FED_HEADER_VehicleInspectionDelivery1> response = new SuccessResponse<FED_HEADER_VehicleInspectionDelivery1>()
                {
                    Status = "S",
                    StatusCode = "200"
                };
                 
                _logger.LogInformation("Calling Oracle "); 
                var header = await GetVehicleInspectionDeliveryData1(request);

                result = header.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<FED_HEADER_VehicleInspectionDelivery1>> GetVehicleInspectionDeliveryData1(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
                var result = await _oracleContext
                    .Set<FED_HEADER_VehicleInspectionDelivery1>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7108_GETDATA_DLY_N_TOOLS_1 (
                                            :P_HEADER_ID,
	                                        :OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }

        public async Task<FED_HEADER_VehicleInspectionDelivery2> GetVehicleInspectionDelivery2(FEDParameterModel request)
        {
            FED_HEADER_VehicleInspectionDelivery2 result = new FED_HEADER_VehicleInspectionDelivery2();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetVehicleInspectionDelivery2 ");
                SuccessResponse<FED_HEADER_VehicleInspectionDelivery2> response = new SuccessResponse<FED_HEADER_VehicleInspectionDelivery2>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                _logger.LogInformation("Calling Oracle ");
                var header = await GetVehicleInspectionDeliveryData2(request);

                result = header.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<FED_HEADER_VehicleInspectionDelivery2>> GetVehicleInspectionDeliveryData2(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
                var result = await _oracleContext
                    .Set<FED_HEADER_VehicleInspectionDelivery2>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7108_GETDATA_DLY_N_TOOLS_2 (
                                            :P_HEADER_ID,
	                                        :OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }

        public async Task<FED_HEADER_VehicleInspectionDelivery3> GetVehicleInspectionDelivery3(FEDParameterModel request)
        {
            FED_HEADER_VehicleInspectionDelivery3 result = new FED_HEADER_VehicleInspectionDelivery3();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetVehicleInspectionDelivery3 ");
                SuccessResponse<FED_HEADER_VehicleInspectionDelivery3> response = new SuccessResponse<FED_HEADER_VehicleInspectionDelivery3>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                _logger.LogInformation("Calling Oracle ");
                var header = await GetVehicleInspectionDeliveryData3(request);

                result = header.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<FED_HEADER_VehicleInspectionDelivery3>> GetVehicleInspectionDeliveryData3(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
                var result = await _oracleContext
                    .Set<FED_HEADER_VehicleInspectionDelivery3>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7108_GETDATA_DLY_N_TOOLS_3 (
                                            :P_HEADER_ID,
	                                        :OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }

        public async Task<FED_HEADER_VehicleInspectionDelivery4> GetVehicleInspectionDelivery4(FEDParameterModel request)
        {
            FED_HEADER_VehicleInspectionDelivery4 result = new FED_HEADER_VehicleInspectionDelivery4();
            try
            {
                // เริ่มต้น Log เมื่อเริ่มกระบวนการ
                _logger.LogInformation("Starting GetVehicleInspectionDelivery4 ");
                SuccessResponse<FED_HEADER_VehicleInspectionDelivery4> response = new SuccessResponse<FED_HEADER_VehicleInspectionDelivery4>()
                {
                    Status = "S",
                    StatusCode = "200"
                };

                _logger.LogInformation("Calling Oracle ");
                var header = await GetVehicleInspectionDeliveryData4(request);

                result = header.FirstOrDefault();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        private async Task<List<FED_HEADER_VehicleInspectionDelivery4>> GetVehicleInspectionDeliveryData4(FEDParameterModel request)
        {

            var p1 = request.p_HEADER_ID ?? (object)DBNull.Value;
            try
            {
                var result = await _oracleContext
                    .Set<FED_HEADER_VehicleInspectionDelivery4>()
                   .FromSqlRaw(@"
                                    BEGIN 
                                        EFM_FED.SP_7108_GETDATA_DLY_N_TOOLS_4 (
                                            :P_HEADER_ID,
	                                        :OUTDATA
                                        );
                                    END;",
                    new OracleParameter("p_HEADER_ID", request.p_HEADER_ID ?? (object)DBNull.Value),
                    new OracleParameter("OUTDATA", OracleDbType.RefCursor) { Direction = ParameterDirection.Output }
               )
                .ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching and joining data.");
                return null;
            }
        }
        #endregion
    }
}
