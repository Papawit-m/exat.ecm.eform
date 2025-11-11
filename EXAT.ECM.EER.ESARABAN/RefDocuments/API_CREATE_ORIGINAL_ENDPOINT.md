# Endpoint: POST /api/books/create/original

## 📋 ภาพรวม

Endpoint นี้เป็นการสร้างเอกสารตามรูปแบบมาตรฐานของ eSaraban API ตามที่กำหนดใน Postman Collection (`/api/books/create`)

### ข้อมูลพื้นฐาน
- **Method:** POST
- **Path:** `/api/books/create/original`
- **Content-Type:** `application/json`
- **Tag:** Books - Create
- **Based On:** `/api/books/create` จาก Postman Collection

---

## 📥 Request Body Structure

### Model: `ESarabanCreateBookRequest`

```json
{
  "user_ad": "string",
  "book": { BookData },
  "bookAttach": [ BookAttachment ],
  "bookFile": [ BookFile ],
  "bookHistory": [ BookHistory ],
  "bookReferences": [ BookReference ],
  "bookReferenceAttach": [ BookReferenceAttachment ]
}
```

### Required Fields

**Level 1 (Root):**
- ✅ `user_ad` - Active Directory username (e.g., "EXAT\\ECMUSR07")
- ✅ `book` - BookData object

**Level 2 (BookData - Critical Fields):**
- ✅ `book_subject` - หัวข้อเอกสาร
- ✅ `registrationbook_id` - รหัสทะเบียนเอกสาร (GUID)

**Optional But Recommended:**
- `book_owner` - เจ้าของเอกสาร
- `book_to` - ถึง
- `booktype_id` - ประเภทเอกสาร
- `sendtype_id` - ประเภทการส่ง
- `format_id` - รูปแบบ
- `speed_id` - ความเร่งด่วน
- `secret_id` - ชั้นความลับ

---

## 📤 Response Format

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "เอกสารถูกสร้างสำเร็จ",
  "data": {
    "status": "success",
    "statusCode": "200",
    "bookId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "book_code": "BK-20251030-5678",
    "book_subject": "ทดสอบการสร้างเอกสาร",
    "book_owner": "นายทดสอบ ระบบ",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_org_code": "AG0101",
    "booktype_id": 93,
    "sendtype_id": 1,
    "format_id": 2,
    "speed_id": 1,
    "secret_id": 1,
    "message": "เอกสารถูกสร้างสำเร็จ (/api/books/create - Original)",
    "created_by": "EXAT\\ECMUSR07",
    "created_date": "2025-10-30T11:00:00.1234567Z",
    "attachments_count": 1,
    "files_count": 1,
    "history_count": 0,
    "references_count": 0
  }
}
```

### Error Responses

#### 400 Bad Request - Missing user_ad
```json
{
  "success": false,
  "message": "user_ad is required",
  "errorCode": "USER_AD_REQUIRED",
  "data": null
}
```

#### 400 Bad Request - Missing book data
```json
{
  "success": false,
  "message": "book data is required",
  "errorCode": "BOOK_DATA_REQUIRED",
  "data": null
}
```

#### 400 Bad Request - Missing book_subject
```json
{
  "success": false,
  "message": "book_subject is required",
  "errorCode": "BOOK_SUBJECT_REQUIRED",
  "data": null
}
```

#### 400 Bad Request - Missing registrationbook_id
```json
{
  "success": false,
  "message": "registrationbook_id is required",
  "errorCode": "REGISTRATIONBOOK_ID_REQUIRED",
  "data": null
}
```

#### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Internal server error",
  "errorCode": "INTERNAL_ERROR",
  "data": null
}
```

---

## 🧪 ตัวอย่างการใช้งาน

### Example 1: Request Body ตามไฟล์ api_create.txt

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "aliquip labore reprehenderit ea in",
    "book_subject": "non dolore",
    "book_to": "สผว.",
    "book_originaldocumentdetail": "officia magna aliquip ex",
    "book_searchterm": "et deserunt anim",
    "book_remark": "elit deserunt ad officia sint",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_nameth": "สมุดทะเบียนส่ง",
    "registrationbook_nameen": "ipsum",
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

### Example 2: Request Body แบบย่อ (Minimal)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_subject": "ทดสอบการสร้างเอกสาร",
    "registrationbook_id": "E1786792382247A49DD27072718DB187"
  }
}
```

### Example 3: Request Body แบบเต็ม (พร้อมไฟล์แนบ)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "นายทดสอบ ระบบ",
    "book_subject": "เอกสารทดสอบระบบ API",
    "book_to": "ผู้อำนวยการฝ่ายวิศวกรรม",
    "book_originaldocumentdetail": "เอกสารต้นฉบับจากแผนกบริหาร",
    "book_searchterm": "ทดสอบ,API,ระบบ",
    "book_remark": "เอกสารสำหรับการทดสอบระบบ",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_nameth": "สมุดทะเบียนส่ง",
    "registrationbook_org_code": "AG0101",
    "registrationbook_org_nameth": "แผนกบริหารงานกลาง",
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
    "is_circular": 0
  },
  "bookAttach": [
    {
      "file_name": "document_attachment.pdf",
      "file_extension": "pdf",
      "file_remark": "ไฟล์แนบเอกสาร",
      "alfresco_foldername": "2025/10"
    }
  ],
  "bookFile": [
    {
      "file_name": "main_document.docx",
      "file_extension": "docx",
      "file_remark": "เอกสารหลัก",
      "alfresco_foldername": "2025/10"
    }
  ]
}
```

---

## 🔧 การทดสอบ

### วิธีที่ 1: Swagger UI

1. เปิด Swagger UI: `http://localhost:5152`
2. ค้นหา **Books - Create** section
3. เลือก `POST /api/books/create/original`
4. คลิก **Try it out**
5. Copy JSON จาก `api_create.txt` หรือตัวอย่างด้านบน
6. วาง JSON ใน Request body
7. คลิก **Execute**
8. ตรวจสอบ Response

### วิธีที่ 2: cURL

```bash
curl -X POST "http://localhost:5152/api/books/create/original" \
  -H "Content-Type: application/json" \
  -d @api_create.txt
```

หรือ

```bash
curl -X POST "http://localhost:5152/api/books/create/original" \
  -H "Content-Type: application/json" \
  -d '{
    "user_ad": "EXAT\\ECMUSR07",
    "book": {
      "book_subject": "ทดสอบ",
      "registrationbook_id": "E1786792382247A49DD27072718DB187"
    }
  }'
```

### วิธีที่ 3: PowerShell

```powershell
$body = Get-Content -Path "C:\Users\wimut\Desktop\api_create.txt" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/original" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body

$response | ConvertTo-Json -Depth 10
```

### วิธีที่ 4: Postman

1. Method: POST
2. URL: `http://localhost:5152/api/books/create/original`
3. Headers: `Content-Type: application/json`
4. Body: เลือก `raw` และ `JSON`
5. Copy JSON จาก `api_create.txt`
6. คลิก **Send**

---

## 📊 Validation Rules

### Level 1: Root Level Validation
| Field | Required | Type | Validation |
|-------|----------|------|------------|
| `user_ad` | ✅ Yes | string | ไม่เป็น null หรือ empty, format: "DOMAIN\\USERNAME" |
| `book` | ✅ Yes | object | ต้องเป็น BookData object |
| `bookAttach` | ❌ No | array | สามารถเป็น null หรือ empty array |
| `bookFile` | ❌ No | array | สามารถเป็น null หรือ empty array |
| `bookHistory` | ❌ No | array | สามารถเป็น null หรือ empty array |
| `bookReferences` | ❌ No | array | สามารถเป็น null หรือ empty array |
| `bookReferenceAttach` | ❌ No | array | สามารถเป็น null หรือ empty array |

### Level 2: BookData Validation
| Field | Required | Type | Validation |
|-------|----------|------|------------|
| `book_subject` | ✅ Yes | string | ไม่เป็น null หรือ empty |
| `registrationbook_id` | ✅ Yes | string | ไม่เป็น null หรือ empty, GUID format |
| `book_owner` | ❌ No | string | - |
| `book_to` | ❌ No | string | - |
| `booktype_id` | ❌ No | int | - |
| `sendtype_id` | ❌ No | int | - |
| `format_id` | ❌ No | int | - |
| `speed_id` | ❌ No | int | - |
| `secret_id` | ❌ No | int | - |

---

## 🔄 เปรียบเทียบกับ Endpoints อื่น

| Endpoint | Use Case | Key Difference |
|----------|----------|----------------|
| `/api/books/create/original` | **General Purpose** - สร้างเอกสารทั่วไป | รองรับทุก scenario, ไม่มี logic เฉพาะเจาะจง |
| `/api/books/create/approved` | กรณีอนุมัติ/เข้าสู่หลักเกณ์ | มี logic เฉพาะสำหรับเอกสารที่ได้รับอนุมัติ |
| `/api/books/create/non-compliant` | กรณีไม่เข้าหลักเกณ์ | มี logic เฉพาะสำหรับเอกสารที่ไม่ผ่านเกณฑ์ |
| `/api/books/create/under-construction` | กรณีอยู่ระหว่างก่อสร้าง | มี logic เฉพาะสำหรับโครงการก่อสร้าง |

**คำแนะนำ:**
- ใช้ `/api/books/create/original` สำหรับการสร้างเอกสารทั่วไป
- ใช้ endpoints เฉพาะเจาะจง (approved, non-compliant, under-construction) เมื่อต้องการ logic พิเศษ

---

## 🚀 Response Fields Explained

| Field | Type | Description |
|-------|------|-------------|
| `bookId` | string | GUID ของเอกสารที่สร้างขึ้น |
| `book_code` | string | รหัสเอกสาร (Format: BK-YYYYMMDD-XXXX) |
| `book_subject` | string | หัวข้อเอกสารจาก request |
| `book_owner` | string | เจ้าของเอกสารจาก request |
| `registrationbook_id` | string | รหัสทะเบียนเอกสาร |
| `registrationbook_org_code` | string | รหัสหน่วยงาน |
| `booktype_id` | int | รหัสประเภทเอกสาร |
| `sendtype_id` | int | รหัสประเภทการส่ง |
| `format_id` | int | รหัสรูปแบบ |
| `speed_id` | int | รหัสความเร่งด่วน |
| `secret_id` | int | รหัสชั้นความลับ |
| `created_by` | string | ผู้สร้างเอกสาร (user_ad) |
| `created_date` | datetime | วันที่สร้างเอกสาร (ISO 8601) |
| `attachments_count` | int | จำนวนไฟล์แนบ (bookAttach) |
| `files_count` | int | จำนวนไฟล์เอกสาร (bookFile) |
| `history_count` | int | จำนวน history records |
| `references_count` | int | จำนวนเอกสารอ้างอิง |

---

## 🛠️ TODO: Database Integration

```csharp
// TODO Items ที่ต้องทำเมื่อเชื่อมต่อ Database:

1. เชื่อมต่อกับ Oracle Database เพื่อสร้างเอกสาร
   - Insert ข้อมูลลงตาราง BOOKS
   - Generate book_code จาก sequence

2. ตรวจสอบสิทธิ์ของผู้ใช้
   - Validate user_ad กับ Active Directory
   - ตรวจสอบว่าผู้ใช้มีสิทธิ์สร้างเอกสารในหน่วยงานนั้นหรือไม่

3. Validate ข้อมูล Master Data
   - ตรวจสอบ registrationbook_id ว่ามีอยู่จริง
   - ตรวจสอบ booktype_id, format_id, speed_id, secret_id
   - ตรวจสอบ org_code ว่ามีอยู่ในระบบ

4. บันทึกข้อมูลลง S_API_ESARABAN_LOG
   - Log request body
   - Log response
   - Log timestamp และ user_ad

5. บันทึก Related Data
   - Insert bookAttach records
   - Insert bookFile records
   - Insert bookHistory records
   - Insert bookReferences records
   - Insert bookReferenceAttach records

6. เชื่อมต่อ Alfresco สำหรับจัดเก็บไฟล์
   - Upload file_content (Base64) to Alfresco
   - บันทึก alfresco_nodeid, alfresco_noderef
   - จัดการ folder structure
```

---

## 📈 Performance Considerations

### Request Size Limits
- **Maximum Request Body Size:** 30 MB (configurable)
- **Maximum File Size:** 10 MB per file
- **Maximum Files:** 50 files per request

### Timeout Settings
- **Request Timeout:** 60 seconds
- **Database Query Timeout:** 30 seconds
- **File Upload Timeout:** 120 seconds

### Best Practices
1. ใช้ Base64 encoding สำหรับ file_content
2. แยก file upload ออกเป็น endpoint แยกถ้าไฟล์ขนาดใหญ่
3. Implement pagination สำหรับ bulk operations
4. ใช้ async/await สำหรับ database operations

---

## 🔐 Security Considerations

### Authentication
- ตรวจสอบ user_ad กับ Active Directory
- Implement JWT token authentication

### Authorization
- ตรวจสอบสิทธิ์การสร้างเอกสารในหน่วยงาน
- Implement role-based access control (RBAC)

### Input Validation
- Sanitize input strings
- Validate GUID format
- Prevent SQL injection
- Validate file types และขนาด

### Audit Logging
- บันทึกทุก request/response
- เก็บ IP address และ timestamp
- Track การเปลี่ยนแปลงข้อมูล

---

## 📚 Related Files

- **Controller:** `Controllers/BooksController.cs` (Line ~302-417)
- **Models:** `Models/BookModels.cs`
- **Request Sample:** `C:\Users\wimut\Desktop\api_create.txt`
- **Test Examples:** `RefDocuments/API_CREATE_TEST_EXAMPLES.md`
- **Implementation Guide:** `RefDocuments/API_CREATE_IMPLEMENTATION.md`

---

## 📞 Support

หากมีคำถามหรือพบปัญหา กรุณาติดต่อ:
- **Development Team:** EXAT ECM-EER Development
- **Repository:** https://github.com/iNix4S/EXAT.ECM.EER.ESARABAN
- **Documentation:** `RefDocuments/` folder

---

**Created:** October 30, 2025  
**Last Updated:** October 30, 2025  
**Version:** 1.0.0  
**Status:** ✅ Active  
**Endpoint:** POST `/api/books/create/original`
