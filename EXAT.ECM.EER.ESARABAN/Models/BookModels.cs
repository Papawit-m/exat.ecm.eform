using System.Text.Json.Serialization;

namespace EXAT.ECM.EER.ESARABAN.Models
{

    // ============================================================================
    // eSaraban API Models - ตาม api_create.txt specification
    // ============================================================================

    /// <summary>
    /// Main Request Model สำหรับการสร้างเอกสาร (ตาม eSaraban API)
    /// </summary>
    public class ESarabanCreateBookRequest
    {
        /// <summary>
        /// Active Directory username (e.g., EXAT\ECMUSR07)
        /// </summary>
        public string user_ad { get; set; } = string.Empty;

        /// <summary>
        /// ข้อมูลหลักของเอกสาร
        /// </summary>
        public BookData book { get; set; } = new BookData();

        /// <summary>
        /// ไฟล์แนบเอกสาร
        /// </summary>
        public List<BookAttachment>? bookAttach { get; set; } = new List<BookAttachment>();

        /// <summary>
        /// ไฟล์เอกสารหลัก
        /// </summary>
        public List<BookFile>? bookFile { get; set; } = new List<BookFile>();

        /// <summary>
        /// ประวัติการดำเนินการ
        /// </summary>
        public List<BookHistory>? bookHistory { get; set; } = new List<BookHistory>();

        /// <summary>
        /// เอกสารอ้างอิง
        /// </summary>
        public List<BookReference>? bookReferences { get; set; } = new List<BookReference>();

        /// <summary>
        /// ไฟล์แนบของเอกสารอ้างอิง
        /// </summary>
        public List<BookReferenceAttachment>? bookReferenceAttach { get; set; } = new List<BookReferenceAttachment>();
    }

    /// <summary>
    /// ข้อมูลหลักของเอกสาร Book
    /// </summary>
    public class BookData
    {
        public string book_owner { get; set; } = string.Empty;
        public string book_subject { get; set; } = string.Empty;
        public string book_to { get; set; } = string.Empty;
        public string? book_originaldocumentdetail { get; set; }
        public string? book_searchterm { get; set; }
        public string? book_remark { get; set; }

        // Registration Book Information
        public string registrationbook_id { get; set; } = string.Empty;
        public string? registrationbook_nameth { get; set; }
        public string? registrationbook_nameen { get; set; }
        public string? registrationbook_ogr_id { get; set; }
        public string? registrationbook_org_code { get; set; }
        public string? registrationbook_org_nameth { get; set; }
        public string? registrationbook_org_nameen { get; set; }
        public string? registrationbook_org_shtname { get; set; }

        // Book Type and Settings
        public int booktype_id { get; set; }
        public int sendtype_id { get; set; }
        public int format_id { get; set; }
        public int subformat_id { get; set; }
        public int speed_id { get; set; }
        public int secret_id { get; set; }
        public int optiondate_id { get; set; }
        public int optionlanguage_id { get; set; }
        public int optionno_id { get; set; }
        public int status_id { get; set; }

        // Additional Information
        public string? request_org_code { get; set; }
        public int create_page { get; set; }
        public string? parent_bookid { get; set; }
        public string? parent_orgid { get; set; }
        public string? parent_orgcode { get; set; }

        /// <summary>
        /// รหัสกฎหมาย (Reserved for future use - ตาม eSaraban API spec)
        /// </summary>
        public string? law_id { get; set; }

        /// <summary>
        /// รหัสอ้างอิงกฎหมาย (Reserved for future use - ตาม eSaraban API spec)
        /// </summary>
        public string? law_code { get; set; }

        public int is_circular { get; set; }

        /// <summary>
        /// รหัสตำแหน่งผู้บังคับบัญชา (Reserved for future use - ตาม eSaraban API spec)
        /// </summary>
        public string? parent_positioncode { get; set; }

        public string? parent_positionname { get; set; }
    }

    /// <summary>
    /// ไฟล์แนบเอกสาร
    /// </summary>
    public class BookAttachment
    {
        public string? file_content { get; set; }
        public string? file_name { get; set; }
        public string? file_extension { get; set; }
        public string? file_path { get; set; }
        public string? file_url { get; set; }
        public string? file_remark { get; set; }
        public string? alfresco_parentid { get; set; }
        public string? alfresco_foldername { get; set; }
        public string? alfresco_nodetype { get; set; }
        public string? alfresco_noderef { get; set; }
        public string? alfresco_nodeid { get; set; }
    }

    /// <summary>
    /// ไฟล์เอกสารหลัก
    /// </summary>
    public class BookFile
    {
        public string? file_content { get; set; }
        public string? file_name { get; set; }
        public string? file_extension { get; set; }
        public string? file_path { get; set; }
        public string? file_url { get; set; }
        public string? file_remark { get; set; }
        public string? alfresco_parentid { get; set; }
        public string? alfresco_foldername { get; set; }
        public string? alfresco_nodetype { get; set; }
        public string? alfresco_noderef { get; set; }
        public string? alfresco_nodeid { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("originaL_NODEID")]
        public string? originaL_NODEID { get; set; }  // Original node ID (exact case from JSON spec)
    }

    /// <summary>
    /// ประวัติการดำเนินการ
    /// </summary>
    public class BookHistory
    {
        public string? tranfer_id { get; set; }  // Transfer ID (from JSON spec)
        public string? history_id { get; set; }
        public string? action { get; set; }
        public string? action_by { get; set; }
        public DateTime? action_date { get; set; }
        public string? remark { get; set; }
    }

    /// <summary>
    /// เอกสารอ้างอิง
    /// </summary>
    public class BookReference
    {
        public int? referencetype_id { get; set; }
        public string? referencetype_name { get; set; }
        public string? reference_bookid { get; set; }
        public string? reference_bookcode { get; set; }
        public DateTime? reference_bookdate { get; set; }
        public string? reference_subject { get; set; }
        public string? is_active { get; set; }  // Active status flag
    }

    /// <summary>
    /// ไฟล์แนบของเอกสารอ้างอิง
    /// </summary>
    public class BookReferenceAttachment
    {
        public string? reference_bookid { get; set; }
        public string? file_content { get; set; }
        public string? file_name { get; set; }
        public string? file_extension { get; set; }
        public string? file_path { get; set; }
        public string? file_url { get; set; }
        public string? file_remark { get; set; }
        public string? alfresco_parentid { get; set; }
        public string? alfresco_foldername { get; set; }
        public string? alfresco_nodetype { get; set; }
        public string? alfresco_noderef { get; set; }
        public string? alfresco_nodeid { get; set; }
    }

    // ============================================================================
    // Simplified Request Models (สำหรับ K2 SmartObject Integration)
    // ============================================================================

    /// <summary>
    /// Simplified Request Model สำหรับการสร้างเอกสาร - กรณีอนุมัติ/เข้าสู่หลักเกณ์
    /// ออกแบบสำหรับ K2 REST Service integration และ K2 SmartObject
    /// </summary>
    public class CreateBookApprovedSimpleRequest
    {
        /// <summary>
        /// Active Directory username (e.g., EXAT\ECMUSR07)
        /// </summary>
        public string user_ad { get; set; } = string.Empty;

        /// <summary>
        /// เรื่อง/หัวข้อเอกสาร
        /// </summary>
        public string book_subject { get; set; } = string.Empty;

        /// <summary>
        /// ถึง (ผู้รับ)
        /// </summary>
        public string book_to { get; set; } = string.Empty;

        /// <summary>
        /// Registration Book ID
        /// </summary>
        public string registrationbook_id { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อสมุดทะเบียน (ภาษาไทย) - optional
        /// </summary>
        public string? registrationbook_nameth { get; set; }

        /// <summary>
        /// ชื่อสมุดทะเบียน (ภาษาอังกฤษ) - optional
        /// </summary>
        public string? registrationbook_nameen { get; set; }

        /// <summary>
        /// Registration Book Organization ID - optional
        /// </summary>
        public string? registrationbook_ogr_id { get; set; }

        /// <summary>
        /// รหัสองค์กรของสมุดทะเบียน - optional
        /// </summary>
        public string? registrationbook_org_code { get; set; }

        /// <summary>
        /// ชื่อองค์กรของสมุดทะเบียน (ภาษาไทย) - optional
        /// </summary>
        public string? registrationbook_org_nameth { get; set; }

        /// <summary>
        /// ชื่อองค์กรของสมุดทะเบียน (ภาษาอังกฤษ) - optional
        /// </summary>
        public string? registrationbook_org_nameen { get; set; }

        /// <summary>
        /// ชื่อย่อองค์กรของสมุดทะเบียน - optional
        /// </summary>
        public string? registrationbook_org_shtname { get; set; }

        /// <summary>
        /// Parent Book ID (optional)
        /// </summary>
        public string? parent_bookid { get; set; }

        /// <summary>
        /// Parent Organization ID (optional)
        /// </summary>
        public string? parent_orgid { get; set; }

        /// <summary>
        /// รหัสองค์กรหลัก (Parent Organization Code) - optional
        /// </summary>
        public string? parent_orgcode { get; set; }

        /// <summary>
        /// รหัสตำแหน่งผู้บังคับบัญชา - optional
        /// </summary>
        public string? parent_positioncode { get; set; }

        /// <summary>
        /// ชื่อตำแหน่งผู้บังคับบัญชา - optional
        /// </summary>
        public string? parent_positionname { get; set; }

        /// <summary>
        /// ไฟล์เอกสารหลัก (สามารถส่งได้มากกว่า 1 ไฟล์)
        /// </summary>
        public List<BookFile>? bookFile { get; set; }

        /// <summary>
        /// ไฟล์แนบเพิ่มเติม (สามารถส่งได้มากกว่า 1 ไฟล์)
        /// </summary>
        public List<BookAttachment>? bookAttach { get; set; }
    }

    /// <summary>
    /// Simplified Request Model สำหรับการสร้างเอกสาร - กรณีไม่เข้าหลักเกณ์
    /// ออกแบบสำหรับ K2 REST Service integration และ K2 SmartObject
    /// </summary>
    public class CreateBookNonCompliantSimpleRequest
    {
        /// <summary>
        /// Active Directory username (e.g., EXAT\ECMUSR07)
        /// </summary>
        public string user_ad { get; set; } = string.Empty;

        /// <summary>
        /// เรื่อง/หัวข้อเอกสาร
        /// </summary>
        public string book_subject { get; set; } = string.Empty;

        /// <summary>
        /// ถึง (ผู้รับ)
        /// </summary>
        public string book_to { get; set; } = string.Empty;

        /// <summary>
        /// Registration Book ID
        /// </summary>
        public string registrationbook_id { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อสมุดทะเบียน (ภาษาไทย) - optional
        /// </summary>
        public string? registrationbook_nameth { get; set; }

        /// <summary>
        /// ชื่อสมุดทะเบียน (ภาษาอังกฤษ) - optional
        /// </summary>
        public string? registrationbook_nameen { get; set; }

        /// <summary>
        /// Registration Book Organization ID - optional
        /// </summary>
        public string? registrationbook_ogr_id { get; set; }

        /// <summary>
        /// รหัสองค์กรของสมุดทะเบียน - optional
        /// </summary>
        public string? registrationbook_org_code { get; set; }

        /// <summary>
        /// ชื่อองค์กรของสมุดทะเบียน (ภาษาไทย) - optional
        /// </summary>
        public string? registrationbook_org_nameth { get; set; }

        /// <summary>
        /// ชื่อองค์กรของสมุดทะเบียน (ภาษาอังกฤษ) - optional
        /// </summary>
        public string? registrationbook_org_nameen { get; set; }

        /// <summary>
        /// ชื่อย่อองค์กรของสมุดทะเบียน - optional
        /// </summary>
        public string? registrationbook_org_shtname { get; set; }

        /// <summary>
        /// Parent Book ID (optional)
        /// </summary>
        public string? parent_bookid { get; set; }

        /// <summary>
        /// Parent Organization ID (optional)
        /// </summary>
        public string? parent_orgid { get; set; }

        /// <summary>
        /// รหัสองค์กรหลัก (Parent Organization Code) - optional
        /// </summary>
        public string? parent_orgcode { get; set; }

        /// <summary>
        /// รหัสตำแหน่งผู้บังคับบัญชา - optional
        /// </summary>
        public string? parent_positioncode { get; set; }

        /// <summary>
        /// ชื่อตำแหน่งผู้บังคับบัญชา - optional
        /// </summary>
        public string? parent_positionname { get; set; }

        /// <summary>
        /// ไฟล์เอกสารหลัก (สามารถส่งได้มากกว่า 1 ไฟล์)
        /// </summary>
        public List<BookFile>? bookFile { get; set; }

        /// <summary>
        /// ไฟล์แนบเพิ่มเติม (สามารถส่งได้มากกว่า 1 ไฟล์)
        /// </summary>
        public List<BookAttachment>? bookAttach { get; set; }
    }

    /// <summary>
    /// Simplified Request Model สำหรับการสร้างเอกสาร - กรณีอยู่ระหว่างก่อสร้าง
    /// ออกแบบสำหรับ K2 REST Service integration และ K2 SmartObject
    /// </summary>
    public class CreateBookUnderConstructionSimpleRequest
    {
        /// <summary>
        /// Active Directory username (e.g., EXAT\ECMUSR07)
        /// </summary>
        public string user_ad { get; set; } = string.Empty;

        /// <summary>
        /// เรื่อง/หัวข้อเอกสาร
        /// </summary>
        public string book_subject { get; set; } = string.Empty;

        /// <summary>
        /// ถึง (ผู้รับ)
        /// </summary>
        public string book_to { get; set; } = string.Empty;

        /// <summary>
        /// Registration Book ID
        /// </summary>
        public string registrationbook_id { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อสมุดทะเบียน (ภาษาไทย) - optional
        /// </summary>
        public string? registrationbook_nameth { get; set; }

        /// <summary>
        /// ชื่อสมุดทะเบียน (ภาษาอังกฤษ) - optional
        /// </summary>
        public string? registrationbook_nameen { get; set; }

        /// <summary>
        /// Registration Book Organization ID - optional
        /// </summary>
        public string? registrationbook_ogr_id { get; set; }

        /// <summary>
        /// รหัสองค์กรของสมุดทะเบียน - optional
        /// </summary>
        public string? registrationbook_org_code { get; set; }

        /// <summary>
        /// ชื่อองค์กรของสมุดทะเบียน (ภาษาไทย) - optional
        /// </summary>
        public string? registrationbook_org_nameth { get; set; }

        /// <summary>
        /// ชื่อองค์กรของสมุดทะเบียน (ภาษาอังกฤษ) - optional
        /// </summary>
        public string? registrationbook_org_nameen { get; set; }

        /// <summary>
        /// ชื่อย่อองค์กรของสมุดทะเบียน - optional
        /// </summary>
        public string? registrationbook_org_shtname { get; set; }

        /// <summary>
        /// Parent Book ID (optional)
        /// </summary>
        public string? parent_bookid { get; set; }

        /// <summary>
        /// Parent Organization ID (optional)
        /// </summary>
        public string? parent_orgid { get; set; }

        /// <summary>
        /// รหัสองค์กรหลัก (Parent Organization Code) - optional
        /// </summary>
        public string? parent_orgcode { get; set; }

        /// <summary>
        /// รหัสตำแหน่งผู้บังคับบัญชา - optional
        /// </summary>
        public string? parent_positioncode { get; set; }

        /// <summary>
        /// ชื่อตำแหน่งผู้บังคับบัญชา - optional
        /// </summary>
        public string? parent_positionname { get; set; }

        /// <summary>
        /// ไฟล์เอกสารหลัก (สามารถส่งได้มากกว่า 1 ไฟล์)
        /// </summary>
        public List<BookFile>? bookFile { get; set; }

        /// <summary>
        /// ไฟล์แนบเพิ่มเติม (สามารถส่งได้มากกว่า 1 ไฟล์)
        /// </summary>
        public List<BookAttachment>? bookAttach { get; set; }
    }

    // ============================================================================
    // Legacy Base Book Models (เก็บไว้เพื่อความเข้ากันได้)
    // ============================================================================

    /// <summary>
    /// Base class สำหรับ Book Request
    /// </summary>
    public class BaseBookRequest
    {
        /// <summary>
        /// ชื่อเอกสาร
        /// </summary>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// รหัสประเภทเอกสาร (GUID)
        /// </summary>
        public string BookTypeId { get; set; } = string.Empty;

        /// <summary>
        /// รหัสทะเบียนเอกสาร (GUID)
        /// </summary>
        public string RegistrationBookId { get; set; } = string.Empty;

        /// <summary>
        /// ปีของเอกสาร
        /// </summary>
        public int BookYear { get; set; }

        /// <summary>
        /// รหัสหน่วยงาน (e.g., J10100)
        /// </summary>
        public string OrgCode { get; set; } = string.Empty;
    }

    // ============================================================================
    // 1. กรณี อนุมัติ/เข้าสู่หลักเกณ์
    // ============================================================================

    /// <summary>
    /// Request Model สำหรับการสร้างเอกสาร - กรณีอนุมัติ/เข้าสู่หลักเกณ์
    /// </summary>
    public class CreateBookApprovedRequest : BaseBookRequest
    {
        /// <summary>
        /// เลขที่หนังสืออนุมัติ
        /// </summary>
        public string ApprovalDocumentNo { get; set; } = string.Empty;

        /// <summary>
        /// วันที่ได้รับการอนุมัติ
        /// </summary>
        public DateTime ApprovalDate { get; set; }

        /// <summary>
        /// เลขที่หนังสือรับรอง
        /// </summary>
        public string CertificateNo { get; set; } = string.Empty;

        /// <summary>
        /// หน่วยงานที่ออกหนังสือรับรอง
        /// </summary>
        public string? CertificateIssuer { get; set; }

        /// <summary>
        /// วันที่ออกหนังสือรับรอง
        /// </summary>
        public DateTime? CertificateIssueDate { get; set; }

        /// <summary>
        /// หมายเหตุเพิ่มเติม
        /// </summary>
        public string? Remarks { get; set; }

        /// <summary>
        /// ไฟล์แนบเอกสารประกอบ (Array of file paths or IDs)
        /// </summary>
        public List<string>? AttachedDocuments { get; set; }
    }

    /// <summary>
    /// Response Model สำหรับการสร้างเอกสาร - กรณีอนุมัติ/เข้าสู่หลักเกณ์
    /// </summary>
    public class CreateBookApprovedResponse
    {
        /// <summary>
        /// Book ID (GUID)
        /// </summary>
        public string BookId { get; set; } = string.Empty;

        /// <summary>
        /// Book Code (รหัสเอกสาร)
        /// </summary>
        public string BookCode { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อเอกสาร
        /// </summary>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// รหัสประเภทเอกสาร
        /// </summary>
        public string BookTypeId { get; set; } = string.Empty;

        /// <summary>
        /// รหัสทะเบียนเอกสาร
        /// </summary>
        public string RegistrationBookId { get; set; } = string.Empty;

        /// <summary>
        /// ปีของเอกสาร
        /// </summary>
        public int BookYear { get; set; }

        /// <summary>
        /// รหัสหน่วยงาน
        /// </summary>
        public string OrgCode { get; set; } = string.Empty;

        /// <summary>
        /// สถานะการอนุมัติ (APPROVED)
        /// </summary>
        public string ApprovalStatus { get; set; } = string.Empty;

        /// <summary>
        /// เลขที่หนังสืออนุมัติ
        /// </summary>
        public string ApprovalDocumentNo { get; set; } = string.Empty;

        /// <summary>
        /// วันที่ได้รับการอนุมัติ
        /// </summary>
        public DateTime ApprovalDate { get; set; }

        /// <summary>
        /// เลขที่หนังสือรับรอง
        /// </summary>
        public string CertificateNo { get; set; } = string.Empty;

        /// <summary>
        /// ผู้สร้าง
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// วันที่สร้าง
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// ข้อความตอบกลับ
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    // ============================================================================
    // 2. กรณี แบบไม่เข้าหลักเกณ์
    // ============================================================================

    /// <summary>
    /// Request Model สำหรับการสร้างเอกสาร - กรณีไม่เข้าหลักเกณ์
    /// </summary>
    public class CreateBookNonCompliantRequest : BaseBookRequest
    {
        /// <summary>
        /// เหตุผลที่ไม่เข้าหลักเกณ์
        /// </summary>
        public string NonCompliantReason { get; set; } = string.Empty;

        /// <summary>
        /// รายละเอียดเพิ่มเติมเกี่ยวกับการไม่เข้าหลักเกณ์
        /// </summary>
        public string NonCompliantDetails { get; set; } = string.Empty;

        /// <summary>
        /// ต้องการให้มีการพิจารณาทบทวน
        /// </summary>
        public bool RequiresReview { get; set; } = false;

        /// <summary>
        /// รหัสหน่วยงานที่จะทำการทบทวน
        /// </summary>
        public string? ReviewerOrgCode { get; set; }

        /// <summary>
        /// ชื่อผู้ทบทวน
        /// </summary>
        public string? ReviewerName { get; set; }

        /// <summary>
        /// เอกสารอ้างอิง
        /// </summary>
        public string? ReferenceDocuments { get; set; }

        /// <summary>
        /// หมายเหตุ
        /// </summary>
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Response Model สำหรับการสร้างเอกสาร - กรณีไม่เข้าหลักเกณ์
    /// </summary>
    public class CreateBookNonCompliantResponse
    {
        /// <summary>
        /// Book ID (GUID)
        /// </summary>
        public string BookId { get; set; } = string.Empty;

        /// <summary>
        /// Book Code (รหัสเอกสาร)
        /// </summary>
        public string BookCode { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อเอกสาร
        /// </summary>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// รหัสประเภทเอกสาร
        /// </summary>
        public string BookTypeId { get; set; } = string.Empty;

        /// <summary>
        /// รหัสทะเบียนเอกสาร
        /// </summary>
        public string RegistrationBookId { get; set; } = string.Empty;

        /// <summary>
        /// ปีของเอกสาร
        /// </summary>
        public int BookYear { get; set; }

        /// <summary>
        /// รหัสหน่วยงาน
        /// </summary>
        public string OrgCode { get; set; } = string.Empty;

        /// <summary>
        /// สถานะการเข้าหลักเกณ์ (NON_COMPLIANT)
        /// </summary>
        public string ComplianceStatus { get; set; } = string.Empty;

        /// <summary>
        /// เหตุผลที่ไม่เข้าหลักเกณ์
        /// </summary>
        public string NonCompliantReason { get; set; } = string.Empty;

        /// <summary>
        /// รายละเอียดเพิ่มเติม
        /// </summary>
        public string NonCompliantDetails { get; set; } = string.Empty;

        /// <summary>
        /// ต้องการให้มีการพิจารณาทบทวน
        /// </summary>
        public bool RequiresReview { get; set; }

        /// <summary>
        /// รหัสหน่วยงานที่จะทำการทบทวน
        /// </summary>
        public string? ReviewerOrgCode { get; set; }

        /// <summary>
        /// ผู้สร้าง
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// วันที่สร้าง
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// ข้อความตอบกลับ
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    // ============================================================================
    // 3. กรณี อยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา
    // ============================================================================

    /// <summary>
    /// Request Model สำหรับการสร้างเอกสาร - กรณีอยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา
    /// </summary>
    public class CreateBookUnderConstructionRequest : BaseBookRequest
    {
        /// <summary>
        /// ชื่อโครงการก่อสร้าง
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// รหัสโครงการ
        /// </summary>
        public string ProjectCode { get; set; } = string.Empty;

        /// <summary>
        /// วันที่เริ่มก่อสร้าง
        /// </summary>
        public DateTime ConstructionStartDate { get; set; }

        /// <summary>
        /// วันที่คาดว่าจะแล้วเสร็จ
        /// </summary>
        public DateTime ExpectedCompletionDate { get; set; }

        /// <summary>
        /// ความคืบหน้าการก่อสร้าง (%)
        /// </summary>
        public decimal ConstructionProgress { get; set; }

        /// <summary>
        /// ชื่อบริษัทที่ปรึกษา
        /// </summary>
        public string ConsultantName { get; set; } = string.Empty;

        /// <summary>
        /// รหัสหน่วยงานที่ปรึกษา
        /// </summary>
        public string ConsultantOrgCode { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อผู้ติดต่อที่ปรึกษา
        /// </summary>
        public string? ConsultantContactPerson { get; set; }

        /// <summary>
        /// เบอร์โทรติดต่อที่ปรึกษา
        /// </summary>
        public string? ConsultantContactPhone { get; set; }

        /// <summary>
        /// อีเมลที่ปรึกษา
        /// </summary>
        public string? ConsultantEmail { get; set; }

        /// <summary>
        /// หัวข้อหนังสือที่ขอจากที่ปรึกษา
        /// </summary>
        public string RequestLetterSubject { get; set; } = string.Empty;

        /// <summary>
        /// รายละเอียดหนังสือที่ขอจากที่ปรึกษา
        /// </summary>
        public string RequestLetterDetails { get; set; } = string.Empty;

        /// <summary>
        /// เอกสารที่ต้องการจากที่ปรึกษา
        /// </summary>
        public List<string> RequiredDocuments { get; set; } = new List<string>();

        /// <summary>
        /// วันที่ต้องการเอกสาร
        /// </summary>
        public DateTime? RequiredByDate { get; set; }

        /// <summary>
        /// หมายเหตุเพิ่มเติม
        /// </summary>
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// Response Model สำหรับการสร้างเอกสาร - กรณีอยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา
    /// </summary>
    public class CreateBookUnderConstructionResponse
    {
        /// <summary>
        /// Book ID (GUID)
        /// </summary>
        public string BookId { get; set; } = string.Empty;

        /// <summary>
        /// Book Code (รหัสเอกสาร)
        /// </summary>
        public string BookCode { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อเอกสาร
        /// </summary>
        public string BookTitle { get; set; } = string.Empty;

        /// <summary>
        /// รหัสประเภทเอกสาร
        /// </summary>
        public string BookTypeId { get; set; } = string.Empty;

        /// <summary>
        /// รหัสทะเบียนเอกสาร
        /// </summary>
        public string RegistrationBookId { get; set; } = string.Empty;

        /// <summary>
        /// ปีของเอกสาร
        /// </summary>
        public int BookYear { get; set; }

        /// <summary>
        /// รหัสหน่วยงาน
        /// </summary>
        public string OrgCode { get; set; } = string.Empty;

        /// <summary>
        /// สถานะการก่อสร้าง (UNDER_CONSTRUCTION)
        /// </summary>
        public string ConstructionStatus { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อโครงการก่อสร้าง
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// รหัสโครงการ
        /// </summary>
        public string ProjectCode { get; set; } = string.Empty;

        /// <summary>
        /// วันที่เริ่มก่อสร้าง
        /// </summary>
        public DateTime ConstructionStartDate { get; set; }

        /// <summary>
        /// วันที่คาดว่าจะแล้วเสร็จ
        /// </summary>
        public DateTime ExpectedCompletionDate { get; set; }

        /// <summary>
        /// ความคืบหน้าการก่อสร้าง (%)
        /// </summary>
        public decimal ConstructionProgress { get; set; }

        /// <summary>
        /// ชื่อบริษัทที่ปรึกษา
        /// </summary>
        public string ConsultantName { get; set; } = string.Empty;

        /// <summary>
        /// รหัสหน่วยงานที่ปรึกษา
        /// </summary>
        public string ConsultantOrgCode { get; set; } = string.Empty;

        /// <summary>
        /// หัวข้อหนังสือที่ขอจากที่ปรึกษา
        /// </summary>
        public string RequestLetterSubject { get; set; } = string.Empty;

        /// <summary>
        /// รายละเอียดหนังสือที่ขอจากที่ปรึกษา
        /// </summary>
        public string RequestLetterDetails { get; set; } = string.Empty;

        /// <summary>
        /// เอกสารที่ต้องการจากที่ปรึกษา
        /// </summary>
        public List<string> RequiredDocuments { get; set; } = new List<string>();

        /// <summary>
        /// ผู้สร้าง
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// วันที่สร้าง
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// ข้อความตอบกลับ
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    // ============================================================================
    // Create Book Response Models (K2 Compatible)
    // ============================================================================

    /// <summary>
    /// Response Model สำหรับ Create Book Simple (K2 Compatible)
    /// </summary>
    public class CreateBookSimpleResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "S";

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = "200";

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("book_id")]
        public string BookId { get; set; } = string.Empty;

        [JsonPropertyName("book_subject")]
        public string BookSubject { get; set; } = string.Empty;

        [JsonPropertyName("book_to")]
        public string BookTo { get; set; } = string.Empty;

        [JsonPropertyName("registrationbook_id")]
        public string RegistrationBookId { get; set; } = string.Empty;

        [JsonPropertyName("parent_bookid")]
        public string? ParentBookId { get; set; }

        [JsonPropertyName("parent_orgid")]
        public string? ParentOrgId { get; set; }

        [JsonPropertyName("parent_positionname")]
        public string? ParentPositionName { get; set; }

        [JsonPropertyName("booktype_id")]
        public int BookTypeId { get; set; }

        [JsonPropertyName("bookFile")]
        public List<BookFile>? BookFile { get; set; }

        [JsonPropertyName("file_count")]
        public int FileCount { get; set; }

        [JsonPropertyName("bookAttach")]
        public List<BookAttachment>? BookAttach { get; set; }

        [JsonPropertyName("attach_count")]
        public int AttachCount { get; set; }

        [JsonPropertyName("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [JsonPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Response Model สำหรับ Create Book Full Format (K2 Compatible)
    /// </summary>
    public class ESarabanCreateBookResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "S";

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = "200";

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("book_id")]
        public string BookId { get; set; } = string.Empty;

        [JsonPropertyName("book_subject")]
        public string BookSubject { get; set; } = string.Empty;

        [JsonPropertyName("book_to")]
        public string BookTo { get; set; } = string.Empty;

        [JsonPropertyName("registrationbook_id")]
        public string RegistrationBookId { get; set; } = string.Empty;

        [JsonPropertyName("parent_bookid")]
        public string? ParentBookId { get; set; }

        [JsonPropertyName("parent_orgid")]
        public string? ParentOrgId { get; set; }

        [JsonPropertyName("parent_positionname")]
        public string? ParentPositionName { get; set; }

        [JsonPropertyName("booktype_id")]
        public int BookTypeId { get; set; }

        [JsonPropertyName("bookFile")]
        public List<BookFile>? BookFile { get; set; }

        [JsonPropertyName("file_count")]
        public int FileCount { get; set; }

        [JsonPropertyName("bookAttach")]
        public List<BookAttachment>? BookAttach { get; set; }

        [JsonPropertyName("attach_count")]
        public int AttachCount { get; set; }

        [JsonPropertyName("bookHistory")]
        public List<BookHistory>? BookHistory { get; set; }

        [JsonPropertyName("history_count")]
        public int HistoryCount { get; set; }

        [JsonPropertyName("bookReferences")]
        public List<BookReference>? BookReferences { get; set; }

        [JsonPropertyName("reference_count")]
        public int ReferenceCount { get; set; }

        [JsonPropertyName("bookReferenceAttach")]
        public List<BookReferenceAttachment>? BookReferenceAttach { get; set; }

        [JsonPropertyName("reference_attach_count")]
        public int ReferenceAttachCount { get; set; }

        [JsonPropertyName("created_by")]
        public string CreatedBy { get; set; } = string.Empty;

        [JsonPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }
    }

    // ============================================================================
    // Generate Code Models
    // ============================================================================

    /// <summary>
    /// Response Model สำหรับ Generate Code (K2 Compatible)
    /// </summary>
    public class GenerateCodeResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "S";

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = "200";

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("book_id")]
        public string BookId { get; set; } = string.Empty;

        [JsonPropertyName("book_code")]
        public string BookCode { get; set; } = string.Empty;

        [JsonPropertyName("to_date")]
        public string ToDate { get; set; } = string.Empty;

        [JsonPropertyName("generated_code")]
        public string GeneratedCode { get; set; } = string.Empty;

        [JsonPropertyName("code_type")]
        public string CodeType { get; set; } = string.Empty;

        [JsonPropertyName("generated_by")]
        public string GeneratedBy { get; set; } = string.Empty;

        [JsonPropertyName("generated_date")]
        public DateTime GeneratedDate { get; set; }
    }

    // ============================================================================
    // Transfer Book Models
    // ============================================================================

    /// <summary>
    /// Request Model สำหรับการโอนย้าย Book
    /// </summary>
    public class TransferBookRequest
    {
        /// <summary>
        /// เหตุผลในการโอนย้าย
        /// </summary>
        public string TransferReason { get; set; } = string.Empty;

        /// <summary>
        /// หมายเหตุเพิ่มเติม
        /// </summary>
        public string? TransferNote { get; set; }

        /// <summary>
        /// ผู้สร้างคำขอโอนย้าย
        /// </summary>
        public string? CreateBy { get; set; }
    }

    /// <summary>
    /// Response Model สำหรับการโอนย้าย Book (K2 Compatible)
    /// </summary>
    public class TransferBookResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "S";

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = "200";

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("book_id")]
        public string BookId { get; set; } = string.Empty;

        [JsonPropertyName("transfer_id")]
        public string TransferId { get; set; } = string.Empty;

        [JsonPropertyName("original_org_code")]
        public string OriginalOrgCode { get; set; } = string.Empty;

        [JsonPropertyName("destination_org_code")]
        public string DestinationOrgCode { get; set; } = string.Empty;

        [JsonPropertyName("transfer_reason")]
        public string TransferReason { get; set; } = string.Empty;

        [JsonPropertyName("transfer_note")]
        public string? TransferNote { get; set; }

        [JsonPropertyName("transfer_status")]
        public string TransferStatus { get; set; } = string.Empty;

        [JsonPropertyName("transferred_by")]
        public string TransferredBy { get; set; } = string.Empty;

        [JsonPropertyName("transferred_date")]
        public DateTime TransferredDate { get; set; }
    }

    // ============================================================================
    // Final Organizations Models
    // ============================================================================

    /// <summary>
    /// ข้อมูลองค์กร
    /// </summary>
    /// <summary>
    /// Organization/Book Transfer Information (ตาม Postman Collection)
    /// ใช้สำหรับ Final Organizations API Response
    /// </summary>
    public class OrganizationInfo
    {
        /// <summary>
        /// ลำดับที่
        /// </summary>
        [JsonPropertyName("running_no")]
        public int RunningNo { get; set; }

        /// <summary>
        /// ชื่อองค์กรผู้ส่ง
        /// </summary>
        [JsonPropertyName("send_org_nameth")]
        public string SendOrgNameTh { get; set; } = string.Empty;

        /// <summary>
        /// วันที่ส่ง (format: DD-MMM-YY)
        /// </summary>
        [JsonPropertyName("send_date")]
        public string? SendDate { get; set; }

        /// <summary>
        /// รหัสการรับ
        /// </summary>
        [JsonPropertyName("receive_code")]
        public string? ReceiveCode { get; set; }

        /// <summary>
        /// วันที่รับ (format: DD-MMM-YY)
        /// </summary>
        [JsonPropertyName("receive_date")]
        public string? ReceiveDate { get; set; }

        /// <summary>
        /// ชื่อองค์กรผู้รับ (รวมรหัสองค์กร)
        /// </summary>
        [JsonPropertyName("receive_org_nameth")]
        public string ReceiveOrgNameTh { get; set; } = string.Empty;

        /// <summary>
        /// สถานะ (ภาษาไทย)
        /// </summary>
        [JsonPropertyName("status_nameth")]
        public string StatusNameTh { get; set; } = string.Empty;

        /// <summary>
        /// หมายเหตุการรับ
        /// </summary>
        [JsonPropertyName("receive_comment")]
        public string? ReceiveComment { get; set; }

        /// <summary>
        /// Book ID
        /// </summary>
        [JsonPropertyName("book_id")]
        public string BookId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response Model สำหรับ Final Organizations (ตาม Postman Collection)
    /// </summary>
    public class FinalOrgsResponse
    {
        /// <summary>
        /// สถานะการตอบกลับ (S = Success)
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = "S";

        /// <summary>
        /// รหัสสถานะ HTTP
        /// </summary>
        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = "200";

        /// <summary>
        /// รายการ Books/องค์กรปลายทาง
        /// </summary>
        [JsonPropertyName("books")]
        public List<OrganizationInfo> Books { get; set; } = new List<OrganizationInfo>();
    }

    // ============================================================================
    // Combined Workflow Models - สำหรับ API ที่รวม Create + Generate-Code + Transfer
    // ============================================================================

    /// <summary>
    /// Request Model สำหรับ Create-Generate-Transfer Workflow (Approved)
    /// </summary>
    public class CreateGenerateTransferApprovedRequest
    {
        // Create fields (4 required)
        public string user_ad { get; set; } = string.Empty;
        public string book_subject { get; set; } = string.Empty;
        public string book_to { get; set; } = string.Empty;
        public string registrationbook_id { get; set; } = string.Empty;

        // Registration Book details (optional)
        public string? registrationbook_nameth { get; set; }
        public string? registrationbook_nameen { get; set; }
        public string? registrationbook_ogr_id { get; set; }
        public string? registrationbook_org_code { get; set; }
        public string? registrationbook_org_nameth { get; set; }
        public string? registrationbook_org_nameen { get; set; }
        public string? registrationbook_org_shtname { get; set; }

        // Optional parent fields
        public string? parent_bookid { get; set; }
        public string? parent_orgid { get; set; }
        public string? parent_orgcode { get; set; }
        public string? parent_positioncode { get; set; }
        public string? parent_positionname { get; set; }

        // Files
        public List<BookFile>? bookFile { get; set; }
        public List<BookAttachment>? bookAttach { get; set; }

        // Transfer fields (required for transfer step)
        public string original_org_code { get; set; } = string.Empty;
        public string destination_org_code { get; set; } = string.Empty;
        public string? transfer_reason { get; set; }
        public string? transfer_note { get; set; }
    }

    /// <summary>
    /// Request Model สำหรับ Create-Generate-Transfer Workflow (Non-Compliant)
    /// </summary>
    public class CreateGenerateTransferNonCompliantRequest
    {
        // Create fields (4 required)
        public string user_ad { get; set; } = string.Empty;
        public string book_subject { get; set; } = string.Empty;
        public string book_to { get; set; } = string.Empty;
        public string registrationbook_id { get; set; } = string.Empty;

        // Registration Book details (optional)
        public string? registrationbook_nameth { get; set; }
        public string? registrationbook_nameen { get; set; }
        public string? registrationbook_ogr_id { get; set; }
        public string? registrationbook_org_code { get; set; }
        public string? registrationbook_org_nameth { get; set; }
        public string? registrationbook_org_nameen { get; set; }
        public string? registrationbook_org_shtname { get; set; }

        // Optional parent fields
        public string? parent_bookid { get; set; }
        public string? parent_orgid { get; set; }
        public string? parent_orgcode { get; set; }
        public string? parent_positioncode { get; set; }
        public string? parent_positionname { get; set; }

        // Files
        public List<BookFile>? bookFile { get; set; }
        public List<BookAttachment>? bookAttach { get; set; }

        // Transfer fields (required for transfer step)
        public string original_org_code { get; set; } = string.Empty;
        public string destination_org_code { get; set; } = string.Empty;
        public string? transfer_reason { get; set; }
        public string? transfer_note { get; set; }
    }

    /// <summary>
    /// Request Model สำหรับ Create-Generate-Transfer Workflow (Under-Construction)
    /// </summary>
    public class CreateGenerateTransferUnderConstructionRequest
    {
        // Create fields (4 required)
        public string user_ad { get; set; } = string.Empty;
        public string book_subject { get; set; } = string.Empty;
        public string book_to { get; set; } = string.Empty;
        public string registrationbook_id { get; set; } = string.Empty;

        // Registration Book details (optional)
        public string? registrationbook_nameth { get; set; }
        public string? registrationbook_nameen { get; set; }
        public string? registrationbook_ogr_id { get; set; }
        public string? registrationbook_org_code { get; set; }
        public string? registrationbook_org_nameth { get; set; }
        public string? registrationbook_org_nameen { get; set; }
        public string? registrationbook_org_shtname { get; set; }

        // Optional parent fields
        public string? parent_bookid { get; set; }
        public string? parent_orgid { get; set; }
        public string? parent_orgcode { get; set; }
        public string? parent_positioncode { get; set; }
        public string? parent_positionname { get; set; }

        // Files
        public List<BookFile>? bookFile { get; set; }
        public List<BookAttachment>? bookAttach { get; set; }

        // Transfer fields (required for transfer step)
        public string original_org_code { get; set; } = string.Empty;
        public string destination_org_code { get; set; } = string.Empty;
        public string? transfer_reason { get; set; }
        public string? transfer_note { get; set; }
    }

    /// <summary>
    /// Response Model สำหรับ Create-Generate-Transfer Workflow (K2 Compatible)
    /// </summary>
    public class CreateGenerateTransferResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "S";

        [JsonPropertyName("statusCode")]
        public string StatusCode { get; set; } = "200";

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        // Step 1: Create result
        [JsonPropertyName("book_id")]
        public string BookId { get; set; } = string.Empty;

        [JsonPropertyName("file_count")]
        public int FileCount { get; set; }

        [JsonPropertyName("attach_count")]
        public int AttachCount { get; set; }

        [JsonPropertyName("create_message")]
        public string CreateMessage { get; set; } = string.Empty;

        // Step 2: Generate-Code result
        [JsonPropertyName("generated_code")]
        public string GeneratedCode { get; set; } = string.Empty;

        [JsonPropertyName("to_date")]
        public string ToDate { get; set; } = string.Empty;

        [JsonPropertyName("code_type")]
        public string CodeType { get; set; } = string.Empty;

        [JsonPropertyName("generated_date")]
        public DateTime GeneratedDate { get; set; }

        [JsonPropertyName("generate_message")]
        public string GenerateMessage { get; set; } = string.Empty;

        // Step 3: Transfer result
        [JsonPropertyName("transfer_id")]
        public string TransferId { get; set; } = string.Empty;

        [JsonPropertyName("original_org_code")]
        public string OriginalOrgCode { get; set; } = string.Empty;

        [JsonPropertyName("destination_org_code")]
        public string DestinationOrgCode { get; set; } = string.Empty;

        [JsonPropertyName("transfer_status")]
        public string TransferStatus { get; set; } = string.Empty;

        [JsonPropertyName("transferred_date")]
        public DateTime TransferredDate { get; set; }

        [JsonPropertyName("transfer_message")]
        public string TransferMessage { get; set; } = string.Empty;

        // Overall workflow info
        [JsonPropertyName("workflow_type")]
        public string WorkflowType { get; set; } = string.Empty;

        [JsonPropertyName("executed_by")]
        public string ExecutedBy { get; set; } = string.Empty;

        [JsonPropertyName("workflow_completed")]
        public DateTime WorkflowCompleted { get; set; }

        [JsonPropertyName("overall_message")]
        public string OverallMessage { get; set; } = string.Empty;
    }

}
