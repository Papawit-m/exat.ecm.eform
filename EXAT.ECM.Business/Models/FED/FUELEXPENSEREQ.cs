namespace EXAT.ECM.Business.Models.FED
{
    public class FUELEXPENSEREQ
    {
        public string? DOC_NO { get; set; }
        public string? DOC_DATE { get; set; }
        public string? DEAR { get; set; }
        public string? VEHICLE_TYPE_NAME { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? MAX_VOLUME { get; set; }
        public string? UNIT_PRICE { get; set; }
        public string? REQUESTOR_NAME_1 { get; set; }
        public string? REQUESTOR_NAME_2 { get; set; }
        public string? VEHICLE_CONTROLLER_1 { get; set; }
        public string? VEHICLE_CONTROLLER_2 { get; set; }
        public string? APPROVER_NAME_1 { get; set; }
        public string? APPROVER_NAME_2 { get; set; }

        public List<DETAIL_FUELEXPENSEREQ> Detail { get; set; } = new List<DETAIL_FUELEXPENSEREQ>();
    }

    public class DETAIL_FUELEXPENSEREQ
    {
        public string? FUEL_TYPE_NAME { get; set; }
        public string? MAX_VOLUME { get; set; }
        public string? LAST_REQUEST_VOLUME { get; set; }
        public string? START_DATE { get; set; }
        public string? LAST_MILEAGE { get; set; }
        public string? CURRENT_MILEAGE { get; set; }
        public string? NOTES { get; set; }
    }
}
