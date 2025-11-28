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

        public string? AccessToken { get; set; }

        public string? ExpiredIn { get; set; }

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
        public LoginData? LoginData { get; set; }
        public Member? MemberByEmail { get; set; }
        public Member? MemberByCustomerId { get; set; }
    }

    // EXAT Login Response Data
    public class LoginData
    {
        [JsonPropertyName("member_id")]
        public string? MemberId { get; set; }

        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("user_type")]
        public string? UserType { get; set; }

        [JsonPropertyName("account_type")]
        public string? AccountType { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("phone_no")]
        public string? PhoneNo { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("created_by")]
        public string? CreatedBy { get; set; }

        [JsonPropertyName("active")]
        public string? Active { get; set; }

        [JsonPropertyName("is_consent_latest")]
        public bool IsConsentLatest { get; set; }

        [JsonPropertyName("contact_address")]
        public AddressInfo? ContactAddress { get; set; }

        [JsonPropertyName("tax_address")]
        public AddressInfo? TaxAddress { get; set; }

    }

    public class Member
    {
        public string? CustomerId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? MembershipType { get; set; }
        public string? Status { get; set; }
        public string? contact_address_house_no { get; set; }
        public string? contact_address_village_no { get; set; }
        public string? contact_address_village_name { get; set; }
        public string? contact_address_lane { get; set; }
        public string? contact_address_road { get; set; }
        public string? contact_address_sub_district { get; set; }
        public string? contact_address_district { get; set; }
        public string? contact_address_province { get; set; }
        public string? contact_address_postal_code { get; set; }
        public string? tax_address_house_no { get; set; }
        public string? tax_address_village_no { get; set; }
        public string? tax_address_village_name { get; set; }
        public string? tax_address_lane { get; set; }
        public string? tax_address_road { get; set; }
        public string? tax_address_sub_district { get; set; }
        public string? tax_address_district { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
        public string? AccessToken { get; internal set; }
        public string? ExpiredIn { get; internal set; }
    }

    public class AddressInfo
    {
        [JsonPropertyName("house_no")]
        public string? HouseNo { get; set; }

        [JsonPropertyName("village_no")]
        public string? VillageNo { get; set; }

        [JsonPropertyName("village_name")]
        public string? VillageName { get; set; }

        [JsonPropertyName("lane")]
        public string? Lane { get; set; }

        [JsonPropertyName("road")]
        public string? Road { get; set; }

        [JsonPropertyName("sub_district")]
        public string? SubDistrict { get; set; }

        [JsonPropertyName("district")]
        public string? District { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("postal_code")]
        public string? PostalCode { get; set; }
    }

}
