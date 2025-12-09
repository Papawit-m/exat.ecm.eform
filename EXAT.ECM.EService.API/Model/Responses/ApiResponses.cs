using EXAT.ECM.EService.API.Converters;
using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Responses
{
    // EXAT API Standard Response Wrapper
    public class ExatApiResponse<T>
    {
        [JsonPropertyName("status_code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

    // Access Token Data
    public class AccessTokenData
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expired_in")]
        public string? ExpiredIn { get; set; }
    }

    // Legacy Response Model (for our API response)
    public class AccessTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? ExpiredIn { get; set; }
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? AccessToken { get; set; }
        public string? ExpiredIn { get; set; }
        public LoginData? LoginData { get; set; }
        public Member? MemberByEmail { get; set; }
        public Member? MemberByCustomerId { get; set; }
    }

    // EXAT Login Response Data
    public class LoginData
    {
        [JsonPropertyName("member_id")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? MemberId { get; set; }

        [JsonPropertyName("customer_id")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? CustomerId { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("user_type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? UserType { get; set; }

        [JsonPropertyName("account_type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? AccountType { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("created_by")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? CreatedBy { get; set; }

        [JsonPropertyName("active")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? Active { get; set; }

        [JsonPropertyName("is_consent_latest")]
        public bool IsConsentLatest { get; set; }
    }

    // Address Data (for contact_address and tax_address)
    public class Address
    {
        [JsonPropertyName("house_no")]
        public string? HouseNo { get; set; }

        [JsonPropertyName("village_no")]
        public string? VillageNo { get; set; }

        [JsonPropertyName("village_name")]
        public string? VillageName { get; set; }

        [JsonPropertyName("road")]
        public string? Road { get; set; }

        [JsonPropertyName("lane")]
        public string? Lane { get; set; }

        [JsonPropertyName("sub_district")]
        public string? SubDistrict { get; set; }

        [JsonPropertyName("district")]
        public string? District { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }
    }

    // Member Data (complete with all fields from EXAT API)
    public class Member
    {
        [JsonPropertyName("member_id")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? MemberId { get; set; }

        [JsonPropertyName("customer_id")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? CustomerId { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("user_type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? UserType { get; set; }

        [JsonPropertyName("account_type")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? AccountType { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("phone_no")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? PhoneNo { get; set; }

        [JsonPropertyName("line_id")]
        public string? LineId { get; set; }

        [JsonPropertyName("date_of_birth")]
        public string? DateOfBirth { get; set; }

        [JsonPropertyName("is_consent_latest")]
        public bool IsConsentLatest { get; set; }

        [JsonPropertyName("contact_address")]
        public Address? ContactAddress { get; set; }

        [JsonPropertyName("tax_address")]
        public Address? TaxAddress { get; set; }

        [JsonPropertyName("active")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? Active { get; set; }

        [JsonPropertyName("is_cs_member")]
        public bool IsCsMember { get; set; }

        [JsonPropertyName("cs_member_id")]
        public string? CsMemberId { get; set; }

        [JsonPropertyName("last_login")]
        public string? LastLogin { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public string? DataMemberId { get; set; }
        public string? DataCustomerId { get; set; }
        public string? DataEmail { get; set; }
        public string? DataUserType { get; set; }
        public string? DataAccountType { get; set; }
        public string? DataTitle { get; set; }
        public string? DataFirstName { get; set; }
        public string? DataLastName { get; set; }
        public string? DataPhoneNo { get; set; }
        public string? DataLineId { get; set; }
        public string? DataDateOfBirth { get; set; }
        public bool    DataIsConsentLatest { get; set; }
        public string? DataContactAddressHouseNo { get; set; }
        public string? DataContactAddressVillageNo { get; set; }
        public string? DataContactAddressVillageName { get; set; }
        public string? DataContactAddressRoad { get; set; }
        public string? DataContactAddressLane { get; set; }
        public string? DataContactAddressSubDistrict { get; set; }
        public string? DataContactAddressDistrict { get; set; }
        public string? DataContactAddressProvince { get; set; }
        public string? DataContactAddressPostalCode { get; set; }
        public string? DataTaxAddressHouseNo { get; set; }
        public string? DataTaxAddressVillageNo { get; set; }
        public string? DataTaxAddressVillageName { get; set; }
        public string? DataTaxAddressRoad { get; set; }
        public string? DataTaxAddressLane { get; set; }
        public string? DataTaxAddressSubDistrict { get; set; }
        public string? DataTaxAddressDistrict { get; set; }
        public string? DataTaxAddressProvince { get; set; }
        public string? DataTaxAddressPostalCode { get; set; }
        public string? DataActive { get; set; }
        public bool DataIsCsMember { get; set; }
        public string? DataCsMemberId { get; set; }
        public string? DataLastLogin { get; set; }
        public string? ErrorCode { get; set; }
    }

}
