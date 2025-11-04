using EXAT.ECM.FED.API.Models;
using EXAT.ECM.FED.API.Models.IMPORT;
namespace EXAT.ECM.FED.API.Services.Interfaces
{
    public interface IFEDService
    {
        Task<FED_VEHICLE_REPORT> GetVEHICLEAsync(FEDParameterModel request);
        Task<FED_HEADER_DAILYVEHIUSE_REPORT> GetDailyVehiUsageAsync(FEDParameterModel request);
        Task<FED_HEADER_MONTHLYVEHIUSE_REPORT> GetMonthlyVehiUsageAsync(FEDParameterModel request);
        Task<FED_HEADER_DriverUsageVehicle_REPORT> GetDriverUsageVehicleAsync(FEDParameterModel request);
        Task<FED_HEADER_MachineUse_REPORT> GetMachineUseAsync(FEDParameterModel request);

        //FUELEXPENSE REQ REPORT
        Task<FUELEXPENSEREQ> GetFuelexpenseRequestFormAsync(FEDParameterModel request);
        //POLFUELEXCEED REQ REPORT
        Task<PoliceFuelExceed> GetPolicefuelExceedRequestFormAsync(FEDParameterModel request);
        //INCOMPTFUELTAXINV REPORT
        Task<FED_INCOMPT_FUEL_TAXINV> GetIncomptFuelTaxinvFormAsync(FEDParameterModel request);
        //FUELFLEETCARD REPORT
        Task<FuelFleetCard> GetFuelFleetCardFormAsync(FEDParameterModel request);
        //Import Feed card
        Task<ImportResult> ImportFileExcelFED(FEDParameterModel request);
        //DownloadErrorExcel
        Task<T_TEMP_FED_IMPORT_FLEETCARD_ERROR> DownloadErrorExcel(FEDParameterModel request);

        Task<ImportResultBankFED<Dictionary<string, string?>>> ImportTransactionsAsync(FEDParameterModel request);
    }
}
