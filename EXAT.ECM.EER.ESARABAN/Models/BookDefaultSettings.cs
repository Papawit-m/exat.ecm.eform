namespace EXAT.ECM.EER.ESARABAN.Models
{

    /// <summary>
    /// Configuration สำหรับค่า default ของ Book Creation endpoints
    /// </summary>
    public class BookDefaultSettings
    {
        /// <summary>
        /// ค่า default สำหรับ user_ad (Active Directory username)
        /// </summary>
        public string? UserAd { get; set; }

        /// <summary>
        /// ค่า default สำหรับ BookData
        /// </summary>
        public BookDataDefaults BookData { get; set; } = new();

        /// <summary>
        /// ค่า default สำหรับ BookFile
        /// </summary>
        public BookFileDefaults BookFile { get; set; } = new();

        /// <summary>
        /// ค่า default สำหรับ BookAttachment
        /// </summary>
        public BookAttachmentDefaults BookAttachment { get; set; } = new();

        /// <summary>
        /// ค่า default สำหรับ BookHistory
        /// </summary>
        public BookHistoryDefaults BookHistory { get; set; } = new();

        /// <summary>
        /// ค่า default สำหรับ Transfer (โอนย้าย)
        /// </summary>
        public TransferDefaults Transfer { get; set; } = new();

        /// <summary>
        /// ค่า default สำหรับแต่ละ endpoint
        /// </summary>
        public EndpointDefaults Endpoints { get; set; } = new();
    }

    /// <summary>
    /// ค่า default สำหรับ BookData
    /// </summary>
    public class BookDataDefaults
    {
        // Book Information
        public string? BookOwner { get; set; }
        public string? BookSubject { get; set; }
        public string? BookTo { get; set; }
        public string? BookOriginalDocumentDetail { get; set; }
        public string? BookSearchTerm { get; set; }
        public string? BookRemark { get; set; }

        // Registration Book Information
        public string? RegistrationBookId { get; set; }
        public string? RegistrationBookNameTh { get; set; }
        public string? RegistrationBookNameEn { get; set; }
        public string? RegistrationBookOgrId { get; set; }
        public string? RegistrationBookOrgId { get; set; }
        public string? RegistrationBookOrgCode { get; set; }
        public string? RegistrationBookOrgNameTh { get; set; }
        public string? RegistrationBookOrgNameEn { get; set; }
        public string? RegistrationBookOrgShtName { get; set; }

        // Book Type and Format IDs
        public int? BookTypeId { get; set; }
        public int? SendTypeId { get; set; }
        public int? FormatId { get; set; }
        public int? SubFormatId { get; set; }
        public int? SpeedId { get; set; }
        public int? SecretId { get; set; }
        public int? OptionDateId { get; set; }
        public int? OptionLanguageId { get; set; }
        public int? OptionNoId { get; set; }
        public int? StatusId { get; set; }

        // Additional Information
        public string? RequestOrgCode { get; set; }
        public int? CreatePage { get; set; }
        public int? IsCircular { get; set; }

        /// <summary>
        /// รหัสกฎหมาย (Reserved for future use - ตาม eSaraban API spec)
        /// </summary>
        public string? LawId { get; set; }

        /// <summary>
        /// รหัสอ้างอิงกฎหมาย (Reserved for future use - ตาม eSaraban API spec)
        /// </summary>
        public string? LawCode { get; set; }

        /// <summary>
        /// รหัสตำแหน่งผู้บังคับบัญชา (Reserved for future use - ตาม eSaraban API spec)
        /// </summary>
        public string? ParentPositionCode { get; set; }
    }

    /// <summary>
    /// ค่า default สำหรับ BookFile
    /// </summary>
    public class BookFileDefaults
    {
        public string? FileExtension { get; set; }
        public string? FilePath { get; set; }
        public string? FileUrl { get; set; }
        public string? AlfrescoParentId { get; set; }
        public string? AlfrescoFolderName { get; set; }
        public string? AlfrescoNodeType { get; set; }
        public bool? SupportMultipleFiles { get; set; }
        public int? MaxFilesCount { get; set; }
    }

    /// <summary>
    /// ค่า default สำหรับ BookAttachment
    /// </summary>
    public class BookAttachmentDefaults
    {
        public string? FileExtension { get; set; }
        public string? FilePath { get; set; }
        public string? FileUrl { get; set; }
        public string? AlfrescoParentId { get; set; }
        public string? AlfrescoFolderName { get; set; }
        public string? AlfrescoNodeType { get; set; }
        public bool? SupportMultipleFiles { get; set; }
        public int? MaxFilesCount { get; set; }
    }

    /// <summary>
    /// ค่า default สำหรับ BookHistory
    /// </summary>
    public class BookHistoryDefaults
    {
        public string? Action { get; set; }
        public string? ActionBy { get; set; }
        public string? Remark { get; set; }
    }

    /// <summary>
    /// ค่า default สำหรับ Transfer (โอนย้าย)
    /// </summary>
    public class TransferDefaults
    {
        /// <summary>
        /// รหัสองค์กรต้นทาง (default)
        /// </summary>
        public string? DefaultOriginalOrgCode { get; set; }

        /// <summary>
        /// รหัสองค์กรปลายทาง (default)
        /// </summary>
        public string? DefaultDestinationOrgCode { get; set; }
    }

    /// <summary>
    /// ค่า default สำหรับแต่ละ endpoint
    /// </summary>
    public class EndpointDefaults
    {
        /// <summary>
        /// ค่า default สำหรับ /api/books/create/original
        /// </summary>
        public EndpointConfig Original { get; set; } = new();

        /// <summary>
        /// ค่า default สำหรับ /api/books/create/approved
        /// </summary>
        public EndpointConfig Approved { get; set; } = new();

        /// <summary>
        /// ค่า default สำหรับ /api/books/create/non-compliant
        /// </summary>
        public EndpointConfig NonCompliant { get; set; } = new();

        /// <summary>
        /// ค่า default สำหรับ /api/books/create/under-construction
        /// </summary>
        public EndpointConfig UnderConstruction { get; set; } = new();
    }

    /// <summary>
    /// Configuration สำหรับแต่ละ endpoint
    /// </summary>
    public class EndpointConfig
    {
        /// <summary>
        /// Prefix สำหรับ Book Code (เช่น BK-, APV-, NCL-, UNC-)
        /// </summary>
        public string? BookCodePrefix { get; set; }

        /// <summary>
        /// Status ID เฉพาะของ endpoint
        /// </summary>
        public int? StatusId { get; set; }

        /// <summary>
        /// Action ที่บันทึกใน History
        /// </summary>
        public string? HistoryAction { get; set; }

        /// <summary>
        /// คำอธิบายเพิ่มเติมสำหรับ endpoint
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// ค่า default เพิ่มเติมสำหรับ BookData ที่เฉพาะเจาะจงกับ endpoint
        /// </summary>
        public Dictionary<string, object>? CustomDefaults { get; set; }
    }
}
