using Microsoft.Extensions.Hosting;
using System.Reflection.PortableExecutable;
using System.Text.Json.Serialization;

namespace EXAT.ECM.EService.API.Model.Responses
{
    public class TagUsageProfile
    {
        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        [JsonPropertyName("customer_name")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("cust_id")]
        public string? CustId { get; set; }

        [JsonPropertyName("cust_acct_id")]
        public string? CustAcctId { get; set; }

        [JsonPropertyName("pan_num")]
        public string? PanNum { get; set; }

        [JsonPropertyName("smartcard_id")]
        public string? SmartcardId { get; set; }

        [JsonPropertyName("balance")]
        public string? Balance { get; set; }

        [JsonPropertyName("cust_acct_status")]
        public string? CustAcctStatus { get; set; }

        [JsonPropertyName("register_date")]
        public string? RegisterDate { get; set; }

        [JsonPropertyName("license_plate")]
        public string? LicensePlate { get; set; }

        [JsonPropertyName("car_detail")]
        public string? CarDetail { get; set; }

        [JsonPropertyName("address1")]
        public string? Address1 { get; set; }

        [JsonPropertyName("address2")]
        public string? Address2 { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("txn_sort")]
        public string? TxnSort { get; set; }

        [JsonPropertyName("txn_during")]
        public string? TxnDuring { get; set; }

        [JsonPropertyName("card_name")]
        public string? CardName { get; set; }
    }

    public class TagUsageData
    {
        [JsonPropertyName("report_name")]
        public string? ReportName { get; set; }

        [JsonPropertyName("profile")]
        public TagUsageProfile? Profile { get; set; }

        // ปล่อยแบบ dynamic ไปก่อน เผื่อ structure ซับซ้อน
        [JsonPropertyName("tag_usage")]
        public object? TagUsage { get; set; }
    }

    public class TagUsageResponse
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

        [JsonPropertyName("message_ext")]
        public string? MessageExt { get; set; }

        [JsonPropertyName("data")]
        public TagUsageData? Data { get; set; }
    }
}
