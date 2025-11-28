using EXAT.ECM.Business.Models.PRS;

namespace EXAT.ECM.Business.Models.FED
{
    public class PoliceFuelExceed
    {
        public string? MONTH { get; set; }
        public string? YEAR { get; set; }

        public List<DETAIL_PoliceFuelExceed> Detail { get; set; } = new List<DETAIL_PoliceFuelExceed>();
    }
    public class DETAIL_PoliceFuelExceed
    {
        public string? NO { get; set; }
        public string? LICENSE_PLATE { get; set; }
        public string? FLEET_CARD_NO { get; set; }
        public string? QUOTA_VOLUME { get; set; }
        public string? FUEL_USED { get; set; }
        public string? EXCESS_USED { get; set; }
        public string? VEHICLE_TYPE_NAME { get; set; }
        public string? AMOUNT_PAID { get; set; }
        public string? TOTAL_AMOUNT_PAID { get; set; }
    }
}
