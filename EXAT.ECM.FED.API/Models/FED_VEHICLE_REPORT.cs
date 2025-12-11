using EXAT.ECM.FED.API.Models;

namespace EXAT.ECM.FED.API.Models
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
        //public FED_TOTAL_MONTHLYVEHIUSE_REPORT TOTAL { get; set; } = new FED_TOTAL_MONTHLYVEHIUSE_REPORT();
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
    public class FED_TOTAL_MONTHLYVEHIUSE_REPORT
    {
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
        public string? NO { get; set; }
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
        public string? TOTAL_HOURS {get;set;}
        public string? TOTAL_MINUTES {get;set;}
        public string? TOTAL_MILEAGE {get;set;}
        public string? TOTAL_FUEL { get; set; }
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
        //public string? TOTAL_HOURS { get; set; }
        //public string? TOTAL_MINUTES { get; set; }
        //public string? TOTAL_MILEAGE { get; set; }
        //public string? TOTAL_FUEL { get; set; }

    }
    public class FED_Header_FUEL_FLEET_CARD_BANK
    {
        public string? TOTAL_EXCL_VAT_AMT { get; set; }
        public string? TOTAL_VAT_AMT { get; set; }
        public string? GRAND_TOTAL_AMT { get; set; }
        public string? TOTAL_VOLUME_LITERS { get; set; }
        public string? TOTAL_PRICE_PER_LITER { get; set; }
        public FED_DATA_FUEL_FLEET_CARD_BANK header_data { get; set; } = new FED_DATA_FUEL_FLEET_CARD_BANK();
        public List<FED_DETAIL_FUEL_FLEET_CARD_BANK> Detail { get; set; } = new List<FED_DETAIL_FUEL_FLEET_CARD_BANK>();
    }
    public class FED_DATA_FUEL_FLEET_CARD_BANK
    {
        public string? DEP_NAME {get;set;}
        public string? LICENSE_PLATE {get;set;}
        public string? DATE_FROM {get;set;}
        public string? DATE_TO { get; set; }
    }
    public class FED_DETAIL_FUEL_FLEET_CARD_BANK
    {
        public string? ROW_NO { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? ORG_NAME { get; set; }
        public string? TRANSACTION_DATE { get; set; }
        public string? CARD_NO { get; set; }
        public string? MERCHANT { get; set; }
        public string? BRANCH { get; set; }
        public string? FUEL_TYPE { get; set; }
        public string? INVOICE_NO { get; set; }
        public string? EXCL_VAT_AMT { get; set; }
        public string? VAT_AMT { get; set; }
        public string? TOTAL_AMT { get; set; }
        public string? VOLUME_LITERS { get; set; }
        public string? PRICE_PER_LITER { get; set; }
        public string? START_MILEAGE { get; set; }
        public string? END_MILEAGE { get; set; }
        public string? DISTANCE_KM { get; set; }
        public string? TOTAL_EXCL_VAT_AMT { get; set; }
        public string? TOTAL_VAT_AMT { get; set; }
        public string? GRAND_TOTAL_AMT { get; set; }
        public string? TOTAL_VOLUME_LITERS { get; set; }
        public string? TOTAL_PRICE_PER_LITER { get; set; }
    }
    public class FED_HEADER_VEHICLEHANDOVER
    {
        public string? HEADER_ID { get; set; }
        public string? REQUEST_ID { get; set; }
        public string? USAGE_ID { get; set; }
        public string? REQUEST_DATE { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? VEHICLE_NAME_TH { get; set; }
        public string? VEHICLE_MODEL { get; set; }
        public string? VEHICLE_TYPE_NAME_TH { get; set; }
        public string? VEHICLE_COLOR { get; set; }
        public string? FLAG_PUBLIC_VEH_HTML { get; set; }
        public string? FLAG_PASSENGER_VEH_HTML { get; set; }
        public string? FLAG_COMPANY_VEH_HTML { get; set; }
        public string? REQUEST_SEC_CODE { get; set; }
        public string? REQUEST_DIV_NAME { get; set; }
        public string? FLAG_VEH_MANUAL_ANT_HTML { get; set; }
        public string? VEHICLE_MANUAL_ANTIRUST { get; set; }
        public string? FLAG_ACK_HANDLE_QTY_HTML { get; set; }
        public string? ACK_WITH_HANDLE_QTY { get; set; }
        public string? FLAG_SPARE_TIRE_INCH_HTML { get; set; }
        public string? SPARE_TIRE_SIZE_INCH { get; set; }
        public string? FLAG_RUBBER_MAT_HTML { get; set; }
        public string? RUBBER_MAT_AVAILABLE { get; set; }
        public string? FLAG_WHEEL_COVER_HTML { get; set; }
        public string? WHEEL_COVER_AVAILABLE { get; set; }
        public string? FLAG_CLUTCH_LOCK_HTML { get; set; }
        public string? CLUTCH_LOCK_BRAND { get; set; }
        public string? FLAG_RADIO_BRAND_HTML { get; set; }
        public string? RADIO_BRAND { get; set; }
        public string? FLAG_AIR_COND_BRAND_HTML { get; set; }
        public string? AIR_CONDITIONER_BRAND { get; set; }
        public string? OTHER_REMARK { get; set; }
        public string? TOOL_SPARK_PLUG_WRENCH_QTY { get; set; }
        public string? TOOL_WHEEL_WRENCH { get; set; }
        public string? TOOL_ADJUSTABLE_WRENCH { get; set; }
        public string? TOOL_RING_WRENCH { get; set; }
        public string? TOOL_OPEN_END_WRENCH { get; set; }
        public string? TOOL_PLIERS { get; set; }
        public string? TOOL_LOCKING_PLIERS { get; set; }
        public string? TOOL_SCREWDRIVER_TYPE { get; set; }
        public string? TOOL_OTHER { get; set; }
        public string? DELIVER_DATE { get; set; }
        public string? DELIVER_TIME { get; set; }
        public string? LICENSE_PLATE_2 { get; set; }
        public string? DRIVER_SIGN { get; set; }
        public string? DRIVER_NAME { get; set; }
        public string? POSITION_NAME { get; set; }
        public string? ACCEPT_DATE { get; set; }
        public string? ACCEPT_TIME { get; set; }
        public string? CONTROL_SIGN { get; set; }
        public string? CONTROL_NAME { get; set; }
        public string? CONTROL_POSI { get; set; }
        public string? ATTENTION_SIGN { get; set; }
        public string? ATTENTION_NAME { get; set; }
        public string? ATTENTION_POSITION { get; set; }
    }
    public class FED_HEADER_VEHICLEREPAIRREQUEST
    {
        public string? VEHICLE_LICENSE_PLATE { get; set; }
        public string? USAGE_TYPE_1_HTML { get; set; }
        public string? USAGE_TYPE_2_HTML { get; set; }
        public string? USAGE_TYPE_3_HTML { get; set; }
        public string? USAGE_TYPE_4_HTML { get; set; }
        public string? VEHICLE_NAME_TH { get; set; }
        public string? VEHICLE_TYPE_NAME_TH { get; set; }
        public string? FISCAL_YEAR { get; set; }
        public string? START_MILEAGE { get; set; }
        public string? END_MILEAGE { get; set; }
    }
    public class FED_HEADER_INSPT_EQUIPMNT
    {
        public string? HEADER_ID { get; set; }
        public string? REQUEST_ID { get; set; }
        public string? USAGE_ID { get; set; }
        public string? REQUEST_DATE { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? VEHICLE_NAME_TH { get; set; }
        public string? VEHICLE_MODEL { get; set; }
        public string? VEHICLE_TYPE_NAME_TH { get; set; }
        public string? VEHICLE_COLOR { get; set; }
        public string? USAGE_TYPE_NAME_TH { get; set; }
        public string? FLAG_PUBLIC_VEH_HTML { get; set; }
        public string? FLAG_PASSENGER_VEH_HTML { get; set; }
        public string? FLAG_COMPANY_VEH_HTML { get; set; }
        public string? REQUEST_SEC_NAME { get; set; }
        public string? REQUEST_DIV_NAME { get; set; }
        public string? FLAG_FULL_FUEL_HTML { get; set; }
        public string? FLAG_75_FUEL_HTML { get; set; }
        public string? FLAG_50_FUEL_HTML { get; set; }
        public string? FLAG_25_FUEL_HTML { get; set; }
        public string? FLAG_EMPTY_FUEL_HTML { get; set; }
        public string? FLAG_WATER_STS1_HTML { get; set; }
        public string? FLAG_WATER_STS2_HTML { get; set; }
        public string? FLAG_DIST_WATER_STS1_HTML { get; set; }
        public string? FLAG_DIST_WATER_STS2_HTML { get; set; }
        public string? FLAG_LUB_OIL_STS1_HTML { get; set; }
        public string? FLAG_LUB_OIL_STS2_HTML { get; set; }
        public string? FLAG_BRAKE_OIL_STS1_HTML { get; set; }
        public string? FLAG_BRAKE_OIL_STS2_HTML { get; set; }
        public string? FLAG_CLUTCH_OIL_STS1_HTML { get; set; }
        public string? FLAG_CLUTCH_OIL_STS2_HTML { get; set; }
        public string? OTHER_REMARK { get; set; }
        public string? FLAG_BODY_STS1_HTML { get; set; }
        public string? FLAG_BODY_STS2_HTML { get; set; }
        public string? FLAG_LIGHT_STS1_HTML { get; set; }
        public string? FLAG_LIGHT_STS2_HTML { get; set; }
        public string? FLAG_WIPER_STS1_HTML { get; set; }
        public string? FLAG_WIPER_STS2_HTML { get; set; }
        public string? FLAG_WARN_PANEL1_HTML { get; set; }
        public string? FLAG_WARN_PANEL2_HTML { get; set; }
        public string? FLAG_TIRE_STS1_HTML { get; set; }
        public string? FLAG_TIRE_STS2_HTML { get; set; }
        public string? FLAG_JACK_W_HANDLE_HTML { get; set; }
        public string? FLAG_SPARE_TIRE_RIM_HTML { get; set; }
        public string? FLAG_TOOL_SPK_WR_HTML { get; set; }
        public string? FLAG_TOOL_WHEEL_WR_HTML { get; set; }
        public string? FLAG_TOOL_ADJ_WR_HTML { get; set; }
        public string? FLAG_TOOL_RING_WR_HTML { get; set; }
        public string? FLAG_TOOL_OPEN_WR_HTML { get; set; }
        public string? FLAG_TOOL_PLIERS_HTML { get; set; }
        public string? FLAG_TOOL_LOCK_PLI_HTML { get; set; }
        public string? FLAG_TOOL_SCREWDRV_HTML { get; set; }
        public string? TOOL_OTHER_REMARK { get; set; }
        public string? VEH_CONTROLLER { get; set; }
        public string? DRIVER_SIGN_1 { get; set; }
        public string? DRIVER_SIGN_2 { get; set; }
        public string? DRIVER_SIGN_DATE { get; set; }
        public string? VEH_CONTROLLER_SIGN_1 { get; set; }
        public string? VEH_CONTROLLER_SIGN_2 { get; set; }
        public string? VEH_CONTROLLER_POSITION { get; set; }
        public string? VEH_CONTROLLER_SIGN_DATE_3 { get; set; }
    }
}
