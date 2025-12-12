using EXAT.ECM.EService.API.Converters;
using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Responses
{
    public class CustomerSearchItem
    {
        [JsonPropertyName("TxnStatus")]
        public string? TxnStatus { get; set; }

        [JsonPropertyName("AccountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("AccountType")]
        public string? AccountType { get; set; }

        [JsonPropertyName("CustomerType")]
        public string? CustomerType { get; set; }

        [JsonPropertyName("HighwayNo")]
        public string? HighwayNo { get; set; }

        [JsonPropertyName("Title")]
        public string? Title { get; set; }

        [JsonPropertyName("TitleEng")]
        public string? TitleEng { get; set; }

        [JsonPropertyName("Family_Name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("Family_NameEng")]
        public string? FamilyNameEng { get; set; }

        [JsonPropertyName("Given_Name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("Given_NameEng")]
        public string? GivenNameEng { get; set; }

        [JsonPropertyName("Gender")]
        public string? Gender { get; set; }

        [JsonPropertyName("BirthDate")]
        public string? BirthDate { get; set; }

        [JsonPropertyName("ICPassportType")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? ICPassportType { get; set; }

        [JsonPropertyName("ICPassport")]
        public string? ICPassport { get; set; }

        [JsonPropertyName("Address1")]
        public string? Address1 { get; set; }

        [JsonPropertyName("Address2")]
        public string? Address2 { get; set; }

        [JsonPropertyName("Address3")]
        public string? Address3 { get; set; }

        [JsonPropertyName("Province")]
        public string? Province { get; set; }

        [JsonPropertyName("City")]
        public string? City { get; set; }

        [JsonPropertyName("PlateNo")]
        public string? PlateNo { get; set; }

        [JsonPropertyName("PlateProvince")]
        public string? PlateProvince { get; set; }

        [JsonPropertyName("PostalCode")]
        public string? PostalCode { get; set; }

        [JsonPropertyName("TelMobile")]
        public string? TelMobile { get; set; }

        [JsonPropertyName("TelHome")]
        public string? TelHome { get; set; }

        [JsonPropertyName("TelOffice")]
        public string? TelOffice { get; set; }

        [JsonPropertyName("Email")]
        public string? Email { get; set; }

        [JsonPropertyName("Remark")]
        public string? Remark { get; set; }

        [JsonPropertyName("CustomerAccountStatus")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? CustomerAccountStatus { get; set; }

        [JsonPropertyName("PAN_NUM")]
        public string? PanNum { get; set; }

        [JsonPropertyName("SmartcardID")]
        public string? SmartcardID { get; set; }

        [JsonPropertyName("AC_Balance")]
        public string? ACBalance { get; set; }

        [JsonPropertyName("TagAction")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? TagAction { get; set; }

        [JsonPropertyName("TagStatus")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? TagStatus { get; set; }

        [JsonPropertyName("TaxID")]
        public string? TaxID { get; set; }

        [JsonPropertyName("BranchID")]
        public string? BranchID { get; set; }

        [JsonPropertyName("CustomerID")]
        public string? CustomerID { get; set; }

        [JsonPropertyName("CarModel")]
        public string? CarModel { get; set; }

        [JsonPropertyName("CarColor")]
        public string? CarColor { get; set; }

        [JsonPropertyName("AC_Balance2")]
        public string? ACBalance2 { get; set; }

        [JsonPropertyName("CardName")]
        public string? CardName { get; set; }

        [JsonPropertyName("TagActionText")]
        public string? TagActionText { get; set; }

        [JsonPropertyName("TagStatusText")]
        public string? TagStatusText { get; set; }

        [JsonPropertyName("AC_BalanceShow")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? ACBalanceShow { get; set; }
    }

    public class CustomerSearchResponse
    {
        [JsonPropertyName("status_code")]
        public string? StatusCode { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("result_code")]
        public string? ResultCode { get; set; }

        [JsonPropertyName("result")]
        public string? Result { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public List<CustomerSearchItem>? Data { get; set; }
    }
}
