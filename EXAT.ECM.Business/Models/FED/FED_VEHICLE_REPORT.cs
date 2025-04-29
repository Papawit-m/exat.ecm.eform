using EXAT.ECM.Business.Models.EER;

namespace EXAT.ECM.Business.Models.FED
{
    public class FED_VEHICLE_REPORT
    {
        public string? REQUEST_DATE { get; set; }
        public string? DEP_NAME { get; set; }
        public string? SEC_NAME { get; set; }
        public string? DIV_NAME { get; set; }
        public string? DEAR { get; set; }
        public string? REQUEST_NAME { get; set; }
        public string? PURPOSE { get; set; }
        public string? DESTINATION { get; set; }
        public string? PASSENGER { get; set; }
        public string? START_DATE { get; set; }
        public string? START_TIME { get; set; }
        public string? URGENCY_STATUS { get; set; }
        public string? REQUEST_SIGN_1 { get; set; }
        public string? REQUEST_SIGN_2 { get; set; }
        public string? REQUEST_POS_NAME { get; set; }
        public string? APPROVER_SIGN_1 { get; set; }
        public string? APPROVER_SIGN_2 { get; set; }
        public string? APPROVER_POS_NAME { get; set; }
        public string? APPROVER_DATE { get; set; }
        public string? DRIVER_NAME { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? CONTROLLER_SIGN_1 { get; set; }
        public string? CONTROLLER_SIGN_2 { get; set; }
        public string? CONTROLLER_POS_NAME { get; set; }
        public string? CONTROLLER_DATE { get; set; }
    }
    public class FED_HEADER_DAILYVEHIUSE_REPORT
    {
        public string? USAGE_DATE { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? VEHICLE_DETAIL { get; set; }
        public string? DRIVER_NAME { get; set; }
        public string? VEHICLE_TYPE_NAME { get; set; }
        public string? REQUEST_ORG_NAME { get; set; }
        public string? PURPOSE { get; set; }
        public string? LAST_REFUEL_VOLUME { get; set; }
        public string? LAST_REFUEL_VOLUME_DATE { get; set; }
        public string? LAST_REFUEL_MILEAGE { get; set; }
        public string? START_MILEAGE { get; set; }
        public string? END_MILEAGE { get; set; }
        public string? LAST_REFUEL_TO_TODAY_KM { get; set; }
        public string? TOTAL_KM_SINCE_LAST_REFUEL { get; set; }
        public string? MAX_VOLUME { get; set; }
        public string? END_DATE { get; set; }
        public string? CURRENT_MILEAGE { get; set; }
        public string? OVERTIME_TYPE { get; set; }
        public string? OVERTIME_START_END { get; set; }
        public string? OVERTIME_TOTAL_HOURS { get; set; }
        public string? EASY_PASS_DETAILS { get; set; }
        public string? DRIVER_SIGN_1 { get; set; }
        public string? DRIVER_SIGN_2 { get; set; }
        public string? CONTROLLER_SIGN_1 { get; set; }
        public string? CONTROLLER_SIGN_2 { get; set; }
        public List<FED_DETAIL_DAILYVEHIUSE_REPORT> Detail { get; set; } = new List<FED_DETAIL_DAILYVEHIUSE_REPORT>();
    }
    public class FED_DETAIL_DAILYVEHIUSE_REPORT
    {
        public string? START_LOCATION { get; set; }
        public string? START_TIME { get; set; }
        public string? START_MILEAGE { get; set; }
        public string? END_LOCATION { get; set; }
        public string? END_TIME { get; set; }
        public string? END_MILEAGE { get; set; }
        public string? DISTANCE_KM { get; set; }
        public string? REQUEST_NAME { get; set; }
        public string? NOTES { get; set; }
        public string? TOTAL_DISTANCE_KM { get; set; }
    }
    public class FED_HEADER_MONTHLYVEHIUSE_REPORT
    {
        public string? VEHICLE_TYPE_NAME { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? MONTH_NAME { get; set; }
        public string? YEAR { get; set; }
        public List<FED_DETAIL_MONTHLYVEHIUSE_REPORT> Detail { get; set; } = new List<FED_DETAIL_MONTHLYVEHIUSE_REPORT>();

    }
    public class FED_DETAIL_MONTHLYVEHIUSE_REPORT
    {
        public string? DATE_USAGE { get; set; }
        public string? KM_START { get; set; }
        public string? KM_END { get; set; }
        public string? DIST_KM { get; set; }
        public string? FUEL_REQ_NO { get; set; }
        public string? FUEL_BENZ_LITER { get; set; }
        public string? FUEL_DIESEL_LITER { get; set; }
        public string? LUBE_REQ_NO { get; set; }
        public string? LUBE_LITER { get; set; }
        public string? PRICE_PER_LITER { get; set; }
        public string? AMOUNT { get; set; }
        public string? REMARKS { get; set; }
        public string? TOTAL_DIST_KM { get; set; }
        public string? TOTAL_BENZ_LITER { get; set; }
        public string? TOTAL_DIESEL_LITER { get; set; }
        public string? TOTAL_LUBE_LITER { get; set; }
        public string? TOTAL_AMT { get; set; }
        public string? FUEL_EFF_KM_L { get; set; }
        public string? LUBE_PER_L_MNT { get; set; }
    }
    public class FED_HEADER_DriverUsageVehicle_REPORT
    {
        public string? DEPT_NAME {get;set;}
		public string? VEHICLE_TYPE_NAME {get;set;}
		public string? USAGE_DATE_PERIOD {get;set;}
		public string? MONTH {get;set;}
		public string? YEAR { get; set; }
        public List<FED_DETAIL_DriverUsageVehicle_REPORT> Detail { get; set; } = new List<FED_DETAIL_DriverUsageVehicle_REPORT>();

    }
    public class FED_DETAIL_DriverUsageVehicle_REPORT
    {
        public string? DRIVER_NAME {get;set;}
        public string? USAGE_DATE_PERIOD {get;set;}
        public string? LICENSE_PLATE {get;set;}
        public string? VEHICLE_TYPE_NAME {get;set;}
        public string? TOTAL_TASKS {get;set;}
        public string? TOTAL_DIST_KM {get;set;}
        public string? TOTAL_TASKS_SUM {get;set;}
        public string? TOTAL_DIST_SUM { get; set; }
    }
    public class FED_HEADER_MachineUse_REPORT
    {
        public string? USAGE_DATE { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? REFUEL_PER_HOURS { get; set; }
        public string? CONTROLLER_SIGN_1 { get; set; }
        public string? CONTROLLER_SIGN_2 { get; set; }
        public string? CONTROLLER_POSITION { get; set; }
        public string? DRIVER_SIGN_1 { get; set; }
        public string? DRIVER_SIGN_2 { get; set; }
        public string? DRIVER_POSITION { get; set; }
        public List<FED_DETAIL_MachineUse_REPORT> Detail { get; set; } = new List<FED_DETAIL_MachineUse_REPORT>();
    }
    public class FED_DETAIL_MachineUse_REPORT
    {
        public string? START_LOCATION { get; set; }
        public string? START_TIME { get; set; }
        public string? START_MILEAGE { get; set; }
        public string? START_FUEL { get; set; }
        public string? END_LOCATION { get; set; }
        public string? END_TIME { get; set; }
        public string? END_MILEAGE { get; set; }
        public string? END_FUEL { get; set; }
        public string? HOURS { get; set; }
        public string? MINUTES { get; set; }
        public string? SUM_MILEAGE { get; set; }
        public string? SUM_FUEL { get; set; }
        public string? REQUEST_NAME { get; set; }
        public string? TOTAL_HOURS { get; set; }
        public string? TOTAL_MINUTES { get; set; }
        public string? TOTAL_MILEAGE { get; set; }
        public string? TOTAL_FUEL { get; set; }

    }

}
