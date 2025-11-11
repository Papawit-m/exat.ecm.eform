namespace EXAT.ECM.FED.API.Models
{
    public class Header_Report
    {
        public string? ORG_CODE { get; set; }
        public string? VEHICLE_ID { get; set; }
        public string? MONTH { get; set; }
        public string? YEAR { get; set; }
        public DateTime? DATE_FROM { get; set; }
        public DateTime? DATE_TO { get; set; }
        public string? HEADER_ID { get; set; }
        public string? CATEGORY_ID { get; set; }
        public DateTime? REQUEST_DATE { get; set; }
    }
    public class ParameterHEADER_REPORT
    {
        public string? p_ORG_CODE { get; set; }
        public string? p_VEHICLE_ID { get; set; }
        public string? p_MONTH_NO { get; set; }
        public string? p_YEAR { get; set; }
        public DateTime? p_REQUEST_DOCDATE_FROM { get; set; }
        public DateTime? p_REQUEST_DOCDATE_TO { get; set; }
        public string? p_HEADER_ID { get; set; }
        public string? p_CATEGORY_ID { get; set; }
    }
    public class FuelFleetCard
    {
        public string? REPORT_MONTH { get; set; }
        public string? PERIOD_DATE { get; set; }
        public string? VEHICLE_LICENSE_NO { get; set; }
        public string? DEP_NAME { get; set; }
        public string? TEXT_TAX_INV_PENDING { get; set; }
        //public string? CREATOT_SIGN_1 { get; set; }
        //public string? CREATOT_SIGN_2 { get; set; }
        //public string? CREATOT_DATE { get; set; }
        //public string? APPROVAL_SIGN_1 { get; set; }
        //public string? APPROVAL_SIGN_2 { get; set; }
        //public string? APPROVAL_DATE { get; set; }
        //public string? SUPPERVISOR_SIGN_1 { get; set; }
        //public string? SUPPERVISOR_SIGN_2 { get; set; }
        //public string? SUPPERVISOR_DATE { get; set; }

        public List<DETAIL_FuelFleetCard> Detail { get; set; } = new List<DETAIL_FuelFleetCard>();
        public List<DETAIL2_FuelFleetCard> Detail2 { get; set; } = new List<DETAIL2_FuelFleetCard>();
    }
    public class DETAIL_FuelFleetCard
    {
        //public string? ROW_NO { get; set; }
        //public string? CARD_NO { get; set; }
        //public string? TRANSACTION_DATE { get; set; }
        //public string? STATION_NAME { get; set; }
        //public string? BRANCH { get; set; }
        //public string? FUEL_TYPE { get; set; }
        //public string? TAX_INV { get; set; }
        //public string? AMT_EXCL_VAT { get; set; }
        //public string? VAT_AMT { get; set; }
        //public string? TOTAL_AMT { get; set; }
        //public string? VOLUME_LITERS { get; set; }
        //public string? PRICE_PER_LITER { get; set; }
        //public string? ODOMETER_BEFORE { get; set; }
        //public string? ODOMETER_AFTER { get; set; }
        //public string? DISTANCE_KM { get; set; }
        //public string? TAX_INVOICE_FLAG { get; set; }
        //public string? REMARKS { get; set; }
        //public string? TOTAL_AMT_EXCL_VAT { get; set; }
        //public string? TOTAL_VAT_AMT { get; set; }
        //public string? GRAND_TOTAL_AMT { get; set; }
        //public string? TOTAL_VOLUME_LITERS { get; set; }
        //public string? TOTAL_PRICE_PER_LITER { get; set; }
        //public string? TOTAL_TAX_INV { get; set; }
        //public string? HAS_INV { get; set; }
        //public string? NO_INV { get; set; }
        //public string? APP_FUEL_LTR { get; set; }
        //public string? APP_FUEL_AMT { get; set; }
        public string? ROW_NO { get; set; }
        public string? CARD_NO { get; set; }
        public string? TRANSACTION_DATE { get; set; }
        public string? STATION_NAME { get; set; }
        public string? BRANCH { get; set; }
        public string? FUEL_TYPE { get; set; }
        public string? TAX_INV { get; set; }
        public string? AMT_EXCL_VAT { get; set; }
        public string? VAT_AMT { get; set; }
        public string? TOTAL_AMT { get; set; }
        public string? VOLUME_LITERS { get; set; }
        public string? PRICE_PER_LITER { get; set; }
        public string? ODOMETER_BEFORE { get; set; }
        public string? ODOMETER_AFTER { get; set; }
        public string? DISTANCE_KM { get; set; }
        public string? TAX_INVOICE_FLAG { get; set; }
        public string? REMARKS { get; set; }
        public string? HIGHLIGHT_COLOR { get; set; }
        public string? TOTAL_AMT_EXCL_VAT { get; set; }
        public string? TOTAL_VAT_AMT { get; set; }
        public string? GRAND_TOTAL_AMT { get; set; }
        public string? TOTAL_VOLUME_LITERS { get; set; }
        public string? TOTAL_PRICE_PER_LITER { get; set; }
        public string? TOTAL_TAX_INV { get; set; }
        public string? HAS_INV { get; set; }
        public string? NO_INV { get; set; }
        public string? APP_FUEL_LTR { get; set; }
        public string? APP_FUEL_AMT { get; set; }
    }

    public class DETAIL2_FuelFleetCard
    {
        //public string? ROW_NO { get; set; }
        //public string? CARD_NO { get; set; }
        //public string? TRANSACTION_DATE { get; set; }
        //public string? STATION_NAME { get; set; }
        //public string? BRANCH { get; set; }
        //public string? FUEL_TYPE { get; set; }
        //public string? TAX_INV { get; set; }
        //public string? AMT_EXCL_VAT { get; set; }
        //public string? VAT_AMT { get; set; }
        //public string? TOTAL_AMT { get; set; }
        //public string? VOLUME_LITERS { get; set; }
        //public string? PRICE_PER_LITER { get; set; }
        //public string? ODOMETER_BEFORE { get; set; }
        //public string? ODOMETER_AFTER { get; set; }
        //public string? DISTANCE_KM { get; set; }
        //public string? TAX_INVOICE_FLAG { get; set; }
        //public string? REMARKS { get; set; }
        public string? ROW_NO { get; set; }
        public string? CARD_NO { get; set; }
        public string? TRANSACTION_DATE { get; set; }
        public string? STATION_NAME { get; set; }
        public string? BRANCH { get; set; }
        public string? FUEL_TYPE { get; set; }
        public string? TAX_INV { get; set; }
        public string? AMT_EXCL_VAT { get; set; }
        public string? VAT_AMT { get; set; }
        public string? TOTAL_AMT { get; set; }
        public string? VOLUME_LITERS { get; set; }
        public string? PRICE_PER_LITER { get; set; }
        public string? ODOMETER_BEFORE { get; set; }
        public string? ODOMETER_AFTER { get; set; }
        public string? DISTANCE_KM { get; set; }
        public string? TAX_INVOICE_FLAG { get; set; }
        public string? REMARKS { get; set; }
    }
}
