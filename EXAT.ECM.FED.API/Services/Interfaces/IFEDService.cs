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
        Task<FuelFleetCard> GetFuelFleetCardFormAsync(ParameterHEADER_REPORT request);
        Task<List<Header_Report>> GetLIST_HEADER_REPORTFormAsync(FEDParameterModel request);

        //Import Feed card
        Task<ImportResult> ImportFileExcelFED(FEDParameterModel request);

        // FuelFleetCardBank
        Task<FED_Header_FUEL_FLEET_CARD_BANK> GetFuelFleetCardBank(FEDParameterModel request);
        //VehicleHandover REPORT
        Task<FED_HEADER_VEHICLEHANDOVER> GetVEHICLEHANDOVERFormAsync(FEDParameterModel request);
        //VEHICLEREPAIRREQUEST REPORT
        Task<FED_HEADER_VEHICLEREPAIRREQUEST> GetVEHICLEREPAIRREQUESTFormAsync(FEDParameterModel request);
        // DailyVehicleInspection REPORT
        Task<FED_HEADER_INSPT_EQUIPMNT> GetDailyVehicleInspectionFormAsync(FEDParameterModel request);

        //DownloadErrorExcel

        //


        Task<T_TEMP_FED_IMPORT_FLEETCARD_ERROR> DownloadErrorExcel(FEDParameterModel request);

        Task<ImportResultBankFED<Dictionary<string, string?>>> ImportTransactionsAsync(FEDParameterModel request);

        #region VehicleInspectionDelivery
        Task<FED_HEADER_VehicleInspectionDelivery1> GetVehicleInspectionDelivery1(FEDParameterModel request);
        Task<FED_HEADER_VehicleInspectionDelivery2> GetVehicleInspectionDelivery2(FEDParameterModel request);
        Task<FED_HEADER_VehicleInspectionDelivery3> GetVehicleInspectionDelivery3(FEDParameterModel request);
        Task<FED_HEADER_VehicleInspectionDelivery4> GetVehicleInspectionDelivery4(FEDParameterModel request);
        #endregion

    }
}
