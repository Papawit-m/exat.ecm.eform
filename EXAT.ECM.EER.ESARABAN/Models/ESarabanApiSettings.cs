namespace EXAT.ECM.EER.ESARABAN.Models
{

    /// <summary>
    /// eSaraban API Settings Configuration
    /// </summary>
    public class ESarabanApiSettings
    {
        /// <summary>
        /// Base URL for eSaraban External API
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int Timeout { get; set; } = 30;

        /// <summary>
        /// Environment name (UAT, PROD, etc.)
        /// </summary>
        public string Environment { get; set; } = "UAT";

        /// <summary>
        /// API Endpoints Configuration
        /// </summary>
        public ESarabanEndpoints Endpoints { get; set; } = new();
    }

    /// <summary>
    /// eSaraban API Endpoints
    /// </summary>
    public class ESarabanEndpoints
    {
        /// <summary>
        /// สร้าง Book ใหม่
        /// </summary>
        public string BooksCreate { get; set; } = "/api/books/create";

        /// <summary>
        /// สร้างรหัสเอกสาร
        /// </summary>
        public string BooksGenerateCode { get; set; } = "/api/books/generate-code";

        /// <summary>
        /// โอนย้าย Book
        /// </summary>
        public string BooksTransfer { get; set; } = "/api/books/transfer";

        /// <summary>
        /// ดึงข้อมูลองค์กรปลายทาง (base endpoint)
        /// </summary>
        public string BooksFinalOrgs { get; set; } = "/api/books/final-orgs";

        /// <summary>
        /// ดึงข้อมูลองค์กรปลายทาง (พร้อม Alert)
        /// </summary>
        public string BooksFinalOrgsByAction { get; set; } = "/api/books/final-orgs/by-action";

        /// <summary>
        /// ดึงข้อมูลองค์กรปลายทาง (ไม่มี Alert)
        /// </summary>
        public string BooksFinalOrgsByActionNoAlert { get; set; } = "/api/books/final-orgs/by-action/no-alert";
    }
}
