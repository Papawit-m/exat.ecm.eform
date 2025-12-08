namespace EXAT.ECM.EService.API.Model.Configuration
{
    public class LogEntry
    {
        public string? LogLevel { get; set; }
        public string? Endpoint { get; set; }
        public string? HttpMethod { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestParameters { get; set; }
        public string? Username { get; set; }
        public string? CustomerId { get; set; }
        public string? Email { get; set; }
        public int? StatusCode { get; set; }
        public string? SuccessFlag { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }
        public double? ExecutionTime { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public DateTime? ResponseTimestamp { get; set; }
        public string? RequestJson { get; set; }
        public string? ResponseJson { get; set; }
    }
}
