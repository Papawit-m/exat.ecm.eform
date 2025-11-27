# API Create Implementation - eSaraban Specification

## 📋 สรุปการเปลี่ยนแปลง

เอกสารนี้สรุปการปรับปรุง Request Body ของ 3 endpoints ให้สอดคล้องกับ eSaraban External API Specification (`api_create.txt`)

### วันที่อัปเดต
- **วันที่:** 30 ตุลาคม 2025
- **ผู้อัปเดต:** GitHub Copilot
- **เหตุผล:** ปรับ Request Body ให้ตรงกับ Postman Collection Specification

---

## 🔄 Endpoints ที่ได้รับการอัปเดต

### 1. POST `/api/books/create/approved`
**สถานะ:** ✅ อัปเดตเรียบร้อย

**การเปลี่ยนแปลง:**
- เปลี่ยนจาก `[FromQuery] user_ad` และ `[FromBody] CreateBookApprovedRequest` 
- เป็น `[FromBody] ESarabanCreateBookRequest` (รวม user_ad ใน body)

### 2. POST `/api/books/create/non-compliant`
**สถานะ:** ✅ อัปเดตเรียบร้อย

**การเปลี่ยนแปลง:**
- เปลี่ยนจาก `[FromQuery] user_ad` และ `[FromBody] CreateBookNonCompliantRequest`
- เป็น `[FromBody] ESarabanCreateBookRequest` (รวม user_ad ใน body)

### 3. POST `/api/books/create/under-construction`
**สถานะ:** ✅ อัปเดตเรียบร้อย

**การเปลี่ยนแปลง:**
- เปลี่ยนจาก `[FromQuery] user_ad` และ `[FromBody] CreateBookUnderConstructionRequest`
- เป็น `[FromBody] ESarabanCreateBookRequest` (รวม user_ad ใน body)

---

## 📦 Model Classes ใหม่

### ESarabanCreateBookRequest
Model หลักสำหรับการสร้างเอกสารตาม eSaraban API Specification

```csharp
public class ESarabanCreateBookRequest
{
    public string user_ad { get; set; }              // Active Directory Username
    public BookData book { get; set; }               // ข้อมูลหลักของเอกสาร
    public List<BookAttachment>? bookAttach { get; set; }        // ไฟล์แนบ
    public List<BookFile>? bookFile { get; set; }                // ไฟล์เอกสารหลัก
    public List<BookHistory>? bookHistory { get; set; }          // ประวัติการดำเนินการ
    public List<BookReference>? bookReferences { get; set; }     // เอกสารอ้างอิง
    public List<BookReferenceAttachment>? bookReferenceAttach { get; set; }  // ไฟล์แนบอ้างอิง
}
```

### BookData
ข้อมูลหลักของเอกสาร (34 fields)

**หมวดข้อมูลหลัก:**
- `book_owner` - เจ้าของเอกสาร
- `book_subject` - เรื่อง/หัวข้อเอกสาร
- `book_to` - ถึง
- `book_originaldocumentdetail` - รายละเอียดเอกสารต้นฉบับ
- `book_searchterm` - คำค้นหา
- `book_remark` - หมายเหตุ

**ข้อมูล Registration Book:**
- `registrationbook_id` - รหัสทะเบียน (GUID)
- `registrationbook_nameth` - ชื่อทะเบียน (ไทย)
- `registrationbook_nameen` - ชื่อทะเบียน (อังกฤษ)
- `registrationbook_ogr_id` - รหัสองค์กร
- `registrationbook_org_code` - รหัสหน่วยงาน
- `registrationbook_org_nameth` - ชื่อหน่วยงาน (ไทย)
- `registrationbook_org_nameen` - ชื่อหน่วยงาน (อังกฤษ)
- `registrationbook_org_shtname` - ชื่อย่อหน่วยงาน

**ประเภทและการตั้งค่า:**
- `booktype_id` - ประเภทเอกสาร
- `sendtype_id` - ประเภทการส่ง
- `format_id` - รูปแบบเอกสาร
- `subformat_id` - รูปแบบย่อย
- `speed_id` - ความเร่งด่วน
- `secret_id` - ชั้นความลับ
- `optiondate_id` - ตัวเลือกวันที่
- `optionlanguage_id` - ตัวเลือกภาษา
- `optionno_id` - ตัวเลือกเลขที่
- `status_id` - สถานะ

**ข้อมูลเพิ่มเติม:**
- `request_org_code` - รหัสหน่วยงานที่ร้องขอ
- `create_page` - หน้าที่สร้าง
- `parent_bookid` - รหัสเอกสารต้นทาง
- `parent_orgid` - รหัสองค์กรต้นทาง
- `parent_orgcode` - รหัสหน่วยงานต้นทาง
- `law_id` - รหัสกฎหมาย
- `law_code` - รหัสอ้างอิงกฎหมาย
- `is_circular` - เป็นหนังสือเวียนหรือไม่ (0/1)
- `parent_positioncode` - รหัสตำแหน่งต้นทาง
- `parent_positionname` - ชื่อตำแหน่งต้นทาง

### BookAttachment & BookFile
ไฟล์แนบและไฟล์เอกสารหลัก (11 fields แต่ละตัว)

```csharp
public class BookAttachment
{
    public string? file_content { get; set; }          // เนื้อหาไฟล์ (Base64)
    public string? file_name { get; set; }             // ชื่อไฟล์
    public string? file_extension { get; set; }        // นามสกุลไฟล์
    public string? file_path { get; set; }             // Path ไฟล์
    public string? file_url { get; set; }              // URL ไฟล์
    public string? file_remark { get; set; }           // หมายเหตุไฟล์
    public string? alfresco_parentid { get; set; }     // Alfresco Parent ID
    public string? alfresco_foldername { get; set; }   // Alfresco Folder Name
    public string? alfresco_nodetype { get; set; }     // Alfresco Node Type
    public string? alfresco_noderef { get; set; }      // Alfresco Node Reference
    public string? alfresco_nodeid { get; set; }       // Alfresco Node ID
}
```

### BookHistory
ประวัติการดำเนินการ

```csharp
public class BookHistory
{
    public string? history_id { get; set; }
    public string? action { get; set; }
    public string? action_by { get; set; }
    public DateTime? action_date { get; set; }
    public string? remark { get; set; }
}
```

### BookReference
เอกสารอ้างอิง

```csharp
public class BookReference
{
    public string? reference_bookid { get; set; }
    public string? reference_bookcode { get; set; }
    public DateTime? reference_bookdate { get; set; }
    public string? reference_subject { get; set; }
    public int? referencetype_id { get; set; }
    public string? referencetype_name { get; set; }
}
```

### BookReferenceAttachment
ไฟล์แนบของเอกสารอ้างอิง (12 fields)

```csharp
public class BookReferenceAttachment
{
    public string? reference_bookid { get; set; }      // รหัสเอกสารอ้างอิง
    // + ฟิลด์เดียวกับ BookAttachment (11 fields)
}
```

---

## 📝 Request Body Format ใหม่

### ตัวอย่าง Request Body

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "ชื่อเจ้าของเอกสาร",
    "book_subject": "หัวข้อเอกสาร",
    "book_to": "สผว.",
    "book_originaldocumentdetail": "รายละเอียดเอกสารต้นฉบับ",
    "book_searchterm": "คำค้นหา",
    "book_remark": "หมายเหตุ",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_nameth": "สมุดทะเบียนส่ง",
    "registrationbook_nameen": "Sending Registration Book",
    "registrationbook_ogr_id": "AB5C943827A4445286C3A0BC8D10CF82",
    "registrationbook_org_code": "AG0101",
    "registrationbook_org_nameth": "แผนกบริหารงานกลาง",
    "registrationbook_org_nameen": "Central Administration Section",
    "registrationbook_org_shtname": "บร.",
    "booktype_id": 93,
    "sendtype_id": 1,
    "format_id": 2,
    "subformat_id": 0,
    "speed_id": 1,
    "secret_id": 1,
    "optiondate_id": 1,
    "optionlanguage_id": 1,
    "optionno_id": 1,
    "status_id": 1,
    "request_org_code": "AG0101",
    "create_page": 1,
    "parent_bookid": "",
    "parent_orgid": "",
    "parent_orgcode": "",
    "law_id": "",
    "law_code": "",
    "is_circular": 0,
    "parent_positioncode": "",
    "parent_positionname": ""
  },
  "bookAttach": [
    {
      "file_content": null,
      "file_name": null,
      "file_extension": null,
      "file_path": null,
      "file_url": null,
      "file_remark": null,
      "alfresco_parentid": null,
      "alfresco_foldername": null,
      "alfresco_nodetype": null,
      "alfresco_noderef": null,
      "alfresco_nodeid": null
    }
  ],
  "bookFile": [
    {
      "file_content": null,
      "file_name": null,
      "file_extension": null,
      "file_path": null,
      "file_url": null,
      "file_remark": null,
      "alfresco_parentid": null,
      "alfresco_foldername": null,
      "alfresco_nodetype": null,
      "alfresco_noderef": null,
      "alfresco_nodeid": null
    }
  ],
  "bookHistory": [
    {
      "history_id": null,
      "action": null
    }
  ],
  "bookReferences": [
    {
      "reference_bookid": null,
      "reference_bookcode": null,
      "reference_bookdate": null,
      "reference_subject": null,
      "referencetype_id": null,
      "referencetype_name": null
    }
  ],
  "bookReferenceAttach": [
    {
      "reference_bookid": null,
      "file_content": null,
      "file_name": null,
      "file_extension": null,
      "file_path": null,
      "file_url": null,
      "file_remark": null,
      "alfresco_parentid": null,
      "alfresco_foldername": null,
      "alfresco_nodetype": null,
      "alfresco_noderef": null,
      "alfresco_nodeid": null
    }
  ]
}
```

---

## 🎯 Response Format

### Success Response
```json
{
  "success": true,
  "message": "เอกสารถูกสร้างสำเร็จ",
  "data": {
    "status": "success",
    "statusCode": "200",
    "bookId": "GUID-ที่สร้างขึ้น",
    "book_code": "APV-20251030-1234",
    "book_subject": "หัวข้อเอกสาร",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "booktype_id": 93,
    "message": "เอกสารถูกสร้างสำเร็จ (กรณีอนุมัติ/เข้าสู่หลักเกณ์)",
    "created_by": "EXAT\\ECMUSR07",
    "created_date": "2025-10-30T10:30:00Z"
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "user_ad is required",
  "errorCode": "USER_AD_REQUIRED",
  "data": null
}
```

---

## ⚙️ Validation Rules

### Required Fields

**Level 1 (Root):**
- ✅ `user_ad` - ต้องไม่เป็น null หรือ empty string
- ✅ `book` - ต้องมีข้อมูล BookData object

**Level 2 (BookData):**
- ✅ `book_owner` - ต้องไม่เป็น empty
- ✅ `book_subject` - ต้องไม่เป็น empty
- ✅ `registrationbook_id` - ต้องเป็น GUID format
- ✅ `booktype_id` - ต้องเป็น integer > 0
- ✅ `sendtype_id` - ต้องเป็น integer > 0
- ✅ `format_id` - ต้องเป็น integer > 0
- ✅ `speed_id` - ต้องเป็น integer > 0
- ✅ `secret_id` - ต้องเป็น integer > 0

**Optional Fields:**
- `bookAttach`, `bookFile`, `bookHistory`, `bookReferences`, `bookReferenceAttach` - สามารถเป็น null หรือ empty array ได้

---

## 🔧 Technical Notes

### 1. Backward Compatibility
- **Legacy Models** (BaseBookRequest, CreateBookApprovedRequest, etc.) ยังคงถูกเก็บไว้ใน `BookModels.cs`
- เพื่อความเข้ากันได้กับโค้ดเก่าที่อาจอ้างอิงถึง

### 2. Database Integration (TODO)
```csharp
// TODO: เชื่อมต่อกับ Oracle Database เพื่อสร้างเอกสาร
// TODO: ตรวจสอบสิทธิ์ของผู้ใช้
// TODO: บันทึกข้อมูลลง S_API_ESARABAN_LOG
// TODO: บันทึก bookAttach, bookFile, bookHistory, bookReferences, bookReferenceAttach
```

### 3. File Upload Considerations
- `file_content` คาดว่าเป็น Base64 encoded string
- ต้องมีการ validate ขนาดไฟล์และ file type
- Alfresco integration สำหรับจัดเก็บไฟล์

### 4. Performance Considerations
- Request body อาจมีขนาดใหญ่ (มีไฟล์แนบหลายไฟล์)
- ควรพิจารณา:
  - File size limits
  - Request timeout settings
  - Multipart form-data สำหรับการ upload ไฟล์ขนาดใหญ่

---

## 📊 Field Count Summary

| Model | Field Count | Purpose |
|-------|-------------|---------|
| `ESarabanCreateBookRequest` | 7 | Root level - รวมทุก components |
| `BookData` | 34 | ข้อมูลหลักของเอกสาร |
| `BookAttachment` | 11 | ไฟล์แนบเอกสาร |
| `BookFile` | 11 | ไฟล์เอกสารหลัก |
| `BookHistory` | 5 | ประวัติการดำเนินการ |
| `BookReference` | 6 | เอกสารอ้างอิง |
| `BookReferenceAttachment` | 12 | ไฟล์แนบอ้างอิง (reference_bookid + 11 file fields) |

**Total Unique Fields:** ~86 fields (รวม nested objects และ arrays)

---

## 🧪 Testing

### Test with Swagger UI
1. เปิด Swagger UI: `http://localhost:5152`
2. Navigate to **Books - Create** section
3. เลือก endpoint ที่ต้องการทดสอบ:
   - POST `/api/books/create/approved`
   - POST `/api/books/create/non-compliant`
   - POST `/api/books/create/under-construction`
4. คลิก **Try it out**
5. Copy request body จาก `api_create.txt` หรือใช้ตัวอย่างด้านบน
6. คลิก **Execute**

### Test with Postman
1. Import Postman Collection: `postman-collections/eSaraban External Service API Verson UAT Copy.postman_collection.json`
2. ตั้งค่า environment variable: `{{baseUrl}}` = `http://localhost:5152`
3. เลือก request: `/api/books/create`
4. ส่ง request และตรวจสอบ response

### Test with cURL
```bash
curl -X POST "http://localhost:5152/api/books/create/approved" \
  -H "Content-Type: application/json" \
  -d @api_create.txt
```

---

## 📚 Related Documentation

- `api_create.txt` - Original eSaraban API Specification
- `RefDocuments/PROJECT_SUMMARY.md` - Project Overview
- `RefDocuments/ORACLE_INTEGRATION_GUIDE.md` - Database Integration Guide
- `Models/BookModels.cs` - Model Definitions
- `Controllers/BooksController.cs` - API Implementation

---

## ✅ Build Status

```
✅ Build successful with 7 warning(s)
⚠️  Warnings: CS1998 - Async methods lack 'await' operators (ปกติสำหรับ mock implementation)
```

### Build Output
```
Restore complete (0.4s)
K2RestApi succeeded (6.6s) → bin\Debug\net8.0\K2RestApi.dll
Build succeeded with 7 warning(s) in 7.8s
```

---

## 🚀 Next Steps

1. **Database Integration**
   - เชื่อมต่อ Oracle Database
   - สร้าง stored procedures สำหรับ book creation
   - บันทึกข้อมูล bookAttach, bookFile, bookHistory, bookReferences

2. **Authentication & Authorization**
   - Validate `user_ad` กับ Active Directory
   - ตรวจสอบสิทธิ์การสร้างเอกสาร

3. **Logging**
   - บันทึกทุก request ลง `S_API_ESARABAN_LOG`
   - เก็บ request body, response, และ error messages

4. **File Upload**
   - Implement Alfresco integration
   - Validate file types และขนาด
   - จัดการ Base64 encoding/decoding

5. **Validation Enhancement**
   - เพิ่ม field-level validation
   - Validate GUID format
   - Validate date formats
   - Validate organization codes

6. **Unit Testing**
   - สร้าง unit tests สำหรับ validation logic
   - Test error scenarios
   - Test database integration

---

## 📞 Support

หากมีคำถามหรือพบปัญหา กรุณาติดต่อ:
- **Development Team:** EXAT ECM-EER Development
- **Repository:** https://github.com/iNix4S/EXAT.ECM.EER.ESARABAN

---

**Last Updated:** October 30, 2025  
**Version:** 1.0.0  
**Status:** ✅ Completed
