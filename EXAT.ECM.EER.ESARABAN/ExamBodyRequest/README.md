# Example Request Body Files

โฟลเดอร์นี้เก็บไฟล์ตัวอย่าง Request Body สำหรับ API Books - Create (Full Format)

---

## 📁 ไฟล์ในโฟลเดอร์

---

## 📦 Books - Create (K2 Compatible) - Simple Format

### 1. books-create-k2-approved-simple-example.json
**คำอธิบาย**: K2 API - สร้างเอกสาร (อนุมัติ/เข้าสู่หลักเกณ์) - แบบง่าย

**Endpoint**: `POST /api/books/create/approved/simple`

**เนื้อหา**:
- ✅ `user_ad`: ระบุ user_ad เป็น `EXAT\TESTUSER01`
- ✅ `book`: ข้อมูล Book (34 fields)
- ❌ `bookFile`: ไม่มี (Simple format)
- ❌ `bookAttach`: ไม่มี (Simple format)
- ❌ `bookHistory`: ไม่มี (Simple format)

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-create-k2-approved-simple-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" -Method Post -Body $body -ContentType "application/json"
Write-Host "Book Code: $($response.data.book_code)"
```

---

### 2. books-create-k2-non-compliant-simple-example.json
**คำอธิบาย**: K2 API - สร้างเอกสาร (ไม่เข้าหลักเกณ์) - แบบง่าย

**Endpoint**: `POST /api/books/create/non-compliant/simple`

**เนื้อหา**:
- ✅ `user_ad`: ระบุ user_ad เป็น `EXAT\TESTUSER02`
- ✅ `book`: ข้อมูล Book สำหรับกรณีไม่เข้าหลักเกณ์
- ❌ ไม่มี arrays (Simple format)

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-create-k2-non-compliant-simple-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/non-compliant/simple" -Method Post -Body $body -ContentType "application/json"
Write-Host "Book Code: $($response.data.book_code)"
```

---

### 3. books-create-k2-under-construction-simple-example.json
**คำอธิบาย**: K2 API - สร้างเอกสาร (ระหว่างก่อสร้าง) - แบบง่าย

**Endpoint**: `POST /api/books/create/under-construction/simple`

**เนื้อหา**:
- ✅ `user_ad`: ระบุ user_ad เป็น `EXAT\TESTUSER03`
- ✅ `book`: ข้อมูล Book สำหรับโครงการระหว่างก่อสร้าง
- ✅ `law_id`, `law_code`: มีข้อมูลกฎหมาย/ระเบียบที่เกี่ยวข้อง
- ❌ ไม่มี arrays (Simple format)

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-create-k2-under-construction-simple-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/under-construction/simple" -Method Post -Body $body -ContentType "application/json"
Write-Host "Book Code: $($response.data.book_code)"
```

---

### 4. books-create-k2-without-user_ad-example.json
**คำอธิบาย**: K2 API - Request แบบไม่ระบุ user_ad (ใช้ default value)

**Endpoint**: ใช้ได้กับทุก Simple endpoint

**เนื้อหา**:
- ❌ `user_ad`: ไม่ระบุ (ระบบจะใช้ default `EXAT\ECMUSR07`)
- ✅ `book`: ข้อมูล Book fields
- ❌ ไม่มี arrays (Simple format)

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-create-k2-without-user_ad-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" -Method Post -Body $body -ContentType "application/json"
Write-Host "Book Code: $($response.data.book_code)"
```

**ผลลัพธ์**:
- ระบบจะ apply default `user_ad = "EXAT\ECMUSR07"` อัตโนมัติ
- เอกสารถูกสร้างสำเร็จโดยไม่ต้องระบุ user_ad

---

## 📦 Books - Workflow (Combined)

### 5. books-workflow-approved-example.json
**คำอธิบาย**: Workflow Combined API - อนุมัติ (Create → Generate-Code → Transfer)

**Endpoint**: `POST /api/books/workflow/approved`

**เนื้อหา**:
- ✅ `user_ad`: ระบุ user_ad เป็น `EXAT\ECMUSR07`
- ✅ `book`: ข้อมูล Book (34 fields) - กรณีอนุมัติ
- ✅ `transfer`: ข้อมูลการโอนย้าย (final_org_code, transfer_remark, etc.)
- 🔄 **Workflow**: ทำงาน 3 ขั้นตอนอัตโนมัติ
  1. สร้างเอกสาร (Create)
  2. สร้างรหัสเอกสาร (Generate-Code)
  3. โอนย้ายเอกสาร (Transfer)

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-workflow-approved-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/workflow/approved" -Method Post -Body $body -ContentType "application/json"
Write-Host "Book Code: $($response.data.book_code)"
Write-Host "Transferred to: $($response.data.final_org_nameth)"
```

---

### 6. books-workflow-non-compliant-example.json
**คำอธิบาย**: Workflow Combined API - ไม่เข้าหลักเกณ์ (Create → Generate-Code → Transfer)

**Endpoint**: `POST /api/books/workflow/non-compliant`

**เนื้อหา**:
- ✅ `user_ad`: ระบุ user_ad เป็น `EXAT\ECMUSR07`
- ✅ `book`: ข้อมูล Book - กรณีไม่เข้าหลักเกณ์
- ✅ `transfer`: ข้อมูลการโอนย้ายไปหน่วยงานที่เกี่ยวข้อง

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-workflow-non-compliant-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/workflow/non-compliant" -Method Post -Body $body -ContentType "application/json"
Write-Host "Book Code: $($response.data.book_code)"
Write-Host "Transferred to: $($response.data.final_org_nameth)"
```

---

### 7. books-workflow-under-construction-example.json
**คำอธิบาย**: Workflow Combined API - ระหว่างก่อสร้าง (Create → Generate-Code → Transfer)

**Endpoint**: `POST /api/books/workflow/under-construction`

**เนื้อหา**:
- ✅ `user_ad`: ระบุ user_ad เป็น `EXAT\ECMUSR07`
- ✅ `book`: ข้อมูล Book - โครงการระหว่างก่อสร้าง
- ✅ `law_id`, `law_code`: มีข้อมูลกฎหมาย/ระเบียบที่เกี่ยวข้อง
- ✅ `transfer`: ข้อมูลการโอนย้ายโครงการ

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-workflow-under-construction-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/workflow/under-construction" -Method Post -Body $body -ContentType "application/json"
Write-Host "Book Code: $($response.data.book_code)"
Write-Host "Transferred to: $($response.data.final_org_nameth)"
```

---

### 8. books-workflow-without-user_ad-example.json
**คำอธิบาย**: Workflow Combined API - ไม่ระบุ user_ad (ใช้ default value)

**Endpoint**: ใช้ได้กับทุก Workflow endpoint

**เนื้อหา**:
- ❌ `user_ad`: ไม่ระบุ (ระบบจะใช้ default `EXAT\ECMUSR07`)
- ✅ `book`: ข้อมูล Book
- ✅ `transfer`: ข้อมูลการโอนย้าย

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-workflow-without-user_ad-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/workflow/approved" -Method Post -Body $body -ContentType "application/json"
Write-Host "Book Code: $($response.data.book_code)"
```

**ผลลัพธ์**:
- ระบบจะ apply default `user_ad = "EXAT\ECMUSR07"` อัตโนมัติ
- Workflow ทำงานครบ 3 ขั้นตอนโดยไม่ต้องระบุ user_ad

---

## 📦 Books - Create (Full Format)

### 9. books-create-full-format-example.json
**คำอธิบาย**: ตัวอย่าง Full Request Body แบบครบถ้วน

**เนื้อหา**:
- ✅ `user_ad`: ระบุ user_ad เป็น `EXAT\TESTUSER01`
- ✅ `book`: ข้อมูล Book ครบทั้ง 34 fields
- ✅ `bookFile`: 1 ไฟล์
- ✅ `bookAttach`: 2 ไฟล์แนบ
- ✅ `bookHistory`: 1 ประวัติ
- ✅ `bookReferences`: 2 เอกสารอ้างอิง
- ✅ `bookReferenceAttach`: 1 ไฟล์แนบเอกสารอ้างอิง

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-create-full-format-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/original" -Method Post -Body $body -ContentType "application/json"
```

---

### 10. books-create-without-user_ad-example.json
**คำอธิบาย**: ตัวอย่าง Request Body แบบไม่ระบุ user_ad (ใช้ default value)

**เนื้อหา**:
- ❌ `user_ad`: ไม่ระบุ (ระบบจะใช้ default `EXAT\ECMUSR07`)
- ✅ `book`: ข้อมูล Book fields
- ✅ `bookFile`: 1 ไฟล์
- ✅ `bookAttach`: 1 ไฟล์แนบ
- ✅ `bookHistory`: 1 ประวัติ

**การใช้งาน**:
```powershell
$body = Get-Content "ExamBodyRequest\books-create-without-user_ad-example.json" -Raw
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/original" -Method Post -Body $body -ContentType "application/json"
```

**ผลลัพธ์**:
- ระบบจะ apply default `user_ad = "EXAT\ECMUSR07"` อัตโนมัติ
- เอกสารถูกสร้างสำเร็จโดยไม่ต้องระบุ user_ad

---

## 🎯 API Endpoints ที่รองรับ

### K2 Compatible Endpoints (Simple Format)
1. **POST** `/api/books/create/approved/simple` - สร้างเอกสาร (อนุมัติ) - แบบง่าย
2. **POST** `/api/books/create/non-compliant/simple` - สร้างเอกสาร (ไม่เข้าหลักเกณ์) - แบบง่าย
3. **POST** `/api/books/create/under-construction/simple` - สร้างเอกสาร (ระหว่างก่อสร้าง) - แบบง่าย

### Workflow Combined Endpoints (3 Steps: Create → Generate-Code → Transfer)
1. **POST** `/api/books/workflow/approved` - Workflow อนุมัติ (3 ขั้นตอนรวม)
2. **POST** `/api/books/workflow/non-compliant` - Workflow ไม่เข้าหลักเกณ์ (3 ขั้นตอนรวม)
3. **POST** `/api/books/workflow/under-construction` - Workflow ระหว่างก่อสร้าง (3 ขั้นตอนรวม)

### Full Format Endpoints
1. **POST** `/api/books/create/original` - สร้างเอกสารทั่วไป
2. **POST** `/api/books/create/approved` - สร้างเอกสาร (อนุมัติ/เข้าสู่หลักเกณ์)
3. **POST** `/api/books/create/non-compliant` - สร้างเอกสาร (ไม่เข้าหลักเกณ์)
4. **POST** `/api/books/create/under-construction` - สร้างเอกสาร (อยู่ระหว่างก่อสร้าง)

---

## 📋 โครงสร้าง Request Body

### Root Level - Create APIs
```json
{
  "user_ad": "string (optional)",
  "book": { ... },
  "bookFile": [ ... ],
  "bookAttach": [ ... ],
  "bookHistory": [ ... ],
  "bookReferences": [ ... ],
  "bookReferenceAttach": [ ... ]
}
```

### Root Level - Workflow APIs (Combined)
```json
{
  "user_ad": "string (optional)",
  "book": { ... },
  "transfer": {
    "final_org_code": "string (required)",
    "final_org_nameth": "string",
    "final_org_nameen": "string",
    "transfer_remark": "string",
    "transfer_by": "string"
  }
}
```

### Book Object (Required)
```json
{
  "book_owner": "string",
  "book_subject": "string (required)",
  "book_to": "string",
  "book_originaldocumentdetail": "string",
  "book_searchterm": "string",
  "book_remark": "string",
  "registrationbook_id": "string (required)",
  "registrationbook_nameth": "string",
  "registrationbook_nameen": "string",
  "registrationbook_ogr_id": "string",
  "registrationbook_org_code": "string",
  "registrationbook_org_nameth": "string",
  "registrationbook_org_nameen": "string",
  "registrationbook_org_shtname": "string",
  "booktype_id": "integer",
  "sendtype_id": "integer",
  "format_id": "integer",
  "subformat_id": "integer",
  "speed_id": "integer",
  "secret_id": "integer",
  "optiondate_id": "integer",
  "optionlanguage_id": "integer",
  "optionno_id": "integer",
  "status_id": "integer",
  "request_org_code": "string",
  "create_page": "integer",
  "parent_bookid": "string",
  "parent_orgid": "string",
  "parent_orgcode": "string",
  "law_id": "string",
  "law_code": "string",
  "is_circular": "integer",
  "parent_positioncode": "string",
  "parent_positionname": "string"
}
```

### BookFile Array (Optional)
```json
[
  {
    "file_name": "string",
    "file_extension": "string",
    "file_content": "string (base64)",
    "file_path": "string",
    "file_url": "string",
    "file_remark": "string",
    "alfresco_foldername": "string",
    "alfresco_nodetype": "string"
  }
]
```

### BookAttach Array (Optional)
```json
[
  {
    "file_name": "string",
    "file_extension": "string",
    "file_content": "string (base64)",
    "file_path": "string",
    "file_remark": "string",
    "alfresco_foldername": "string",
    "alfresco_nodetype": "string"
  }
]
```

### BookHistory Array (Optional)
```json
[
  {
    "action": "string",
    "action_by": "string",
    "remark": "string"
  }
]
```

### BookReferences Array (Optional)
```json
[
  {
    "referencetype_id": "integer",
    "referencetype_name": "string",
    "reference_bookid": "string",
    "reference_bookcode": "string",
    "reference_bookdate": "string (ISO 8601)",
    "reference_subject": "string",
    "is_active": "string"
  }
]
```

### BookReferenceAttach Array (Optional)
```json
[
  {
    "reference_bookid": "string",
    "file_name": "string",
    "file_extension": "string",
    "file_content": "string (base64)",
    "file_remark": "string"
  }
]
```

---

## 💡 Tips การใช้งาน

### 1. user_ad Field
- **Optional**: ไม่จำเป็นต้องระบุ
- **Default Value**: `EXAT\ECMUSR07` (จาก `book-defaults.json`)
- **Custom Value**: สามารถระบุค่าที่ต้องการได้ เช่น `EXAT\TESTUSER01`

### 2. File Content
- ต้องเป็น **Base64 Encoded String**
- ตัวอย่างในไฟล์เป็น Base64 ของ PDF ขนาดเล็ก (placeholder)
- ใช้งานจริงต้องแปลงไฟล์จริงเป็น Base64

### 3. Date Format
- ใช้ **ISO 8601 Format**: `YYYY-MM-DDTHH:mm:ss.sssZ`
- ตัวอย่าง: `2025-10-15T10:45:47.922Z`

### 4. Required Fields
- ✅ `book.book_subject` - หัวข้อเอกสาร
- ✅ `book.registrationbook_id` - ID สมุดทะเบียน
- ⚠️ `user_ad` - ไม่ required (มี default value)

---

## 🧪 การทดสอบ

### ทดสอบ Workflow Combined API
```powershell
# Test 1: Workflow Approved (with user_ad)
$body1 = Get-Content "ExamBodyRequest\books-workflow-approved-example.json" -Raw
$response1 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/workflow/approved" `
    -Method Post -Body $body1 -ContentType "application/json"
Write-Host "✅ Workflow Approved - Book Code: $($response1.data.book_code)" -ForegroundColor Green
Write-Host "   Transferred to: $($response1.data.final_org_nameth)" -ForegroundColor Cyan

# Test 2: Workflow Non-Compliant
$body2 = Get-Content "ExamBodyRequest\books-workflow-non-compliant-example.json" -Raw
$response2 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/workflow/non-compliant" `
    -Method Post -Body $body2 -ContentType "application/json"
Write-Host "✅ Workflow Non-Compliant - Book Code: $($response2.data.book_code)" -ForegroundColor Green
Write-Host "   Transferred to: $($response2.data.final_org_nameth)" -ForegroundColor Cyan

# Test 3: Workflow Under Construction
$body3 = Get-Content "ExamBodyRequest\books-workflow-under-construction-example.json" -Raw
$response3 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/workflow/under-construction" `
    -Method Post -Body $body3 -ContentType "application/json"
Write-Host "✅ Workflow Under Construction - Book Code: $($response3.data.book_code)" -ForegroundColor Green
Write-Host "   Transferred to: $($response3.data.final_org_nameth)" -ForegroundColor Cyan

# Test 4: Workflow Without user_ad (use default)
$body4 = Get-Content "ExamBodyRequest\books-workflow-without-user_ad-example.json" -Raw
$response4 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/workflow/approved" `
    -Method Post -Body $body4 -ContentType "application/json"
Write-Host "✅ Workflow Default user_ad - Book Code: $($response4.data.book_code)" -ForegroundColor Green
Write-Host "   Transferred to: $($response4.data.final_org_nameth)" -ForegroundColor Cyan
```

### ทดสอบ K2 Compatible API (Simple Format)
```powershell
# Test 1: K2 Approved (with user_ad)
$body1 = Get-Content "ExamBodyRequest\books-create-k2-approved-simple-example.json" -Raw
$response1 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" `
    -Method Post -Body $body1 -ContentType "application/json"
Write-Host "✅ Approved - Book Code: $($response1.data.book_code)" -ForegroundColor Green

# Test 2: K2 Non-Compliant (with user_ad)
$body2 = Get-Content "ExamBodyRequest\books-create-k2-non-compliant-simple-example.json" -Raw
$response2 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/non-compliant/simple" `
    -Method Post -Body $body2 -ContentType "application/json"
Write-Host "✅ Non-Compliant - Book Code: $($response2.data.book_code)" -ForegroundColor Green

# Test 3: K2 Under Construction (with user_ad)
$body3 = Get-Content "ExamBodyRequest\books-create-k2-under-construction-simple-example.json" -Raw
$response3 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/under-construction/simple" `
    -Method Post -Body $body3 -ContentType "application/json"
Write-Host "✅ Under Construction - Book Code: $($response3.data.book_code)" -ForegroundColor Green

# Test 4: K2 Without user_ad (use default EXAT\ECMUSR07)
$body4 = Get-Content "ExamBodyRequest\books-create-k2-without-user_ad-example.json" -Raw
$response4 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" `
    -Method Post -Body $body4 -ContentType "application/json"
Write-Host "✅ Default user_ad - Book Code: $($response4.data.book_code)" -ForegroundColor Green
```

### ทดสอบ Full Format API
```powershell
# Test 1: Full Request with user_ad
$body1 = Get-Content "ExamBodyRequest\books-create-full-format-example.json" -Raw
$response1 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/original" `
    -Method Post -Body $body1 -ContentType "application/json"
Write-Host "Book ID: $($response1.data.bookId)"
Write-Host "Book Code: $($response1.data.book_code)"

# Test 2: Request without user_ad (use default)
$body2 = Get-Content "ExamBodyRequest\books-create-without-user_ad-example.json" -Raw
$response2 = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/original" `
    -Method Post -Body $body2 -ContentType "application/json"
Write-Host "Book ID: $($response2.data.bookId)"
Write-Host "Book Code: $($response2.data.book_code)"
```

### ทดสอบด้วย cURL

**Workflow Combined API**:
```bash
# Workflow Approved
curl -X POST http://localhost:5152/api/books/workflow/approved \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-workflow-approved-example.json

# Workflow Non-Compliant
curl -X POST http://localhost:5152/api/books/workflow/non-compliant \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-workflow-non-compliant-example.json

# Workflow Under Construction
curl -X POST http://localhost:5152/api/books/workflow/under-construction \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-workflow-under-construction-example.json

# Workflow Without user_ad (default)
curl -X POST http://localhost:5152/api/books/workflow/approved \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-workflow-without-user_ad-example.json
```

**K2 Compatible API**:
```bash
# K2 Approved
curl -X POST http://localhost:5152/api/books/create/approved/simple \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-create-k2-approved-simple-example.json

# K2 Non-Compliant
curl -X POST http://localhost:5152/api/books/create/non-compliant/simple \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-create-k2-non-compliant-simple-example.json

# K2 Under Construction
curl -X POST http://localhost:5152/api/books/create/under-construction/simple \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-create-k2-under-construction-simple-example.json

# K2 Without user_ad (default)
curl -X POST http://localhost:5152/api/books/create/approved/simple \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-create-k2-without-user_ad-example.json
```

**Full Format API**:
```bash
# Test 1: Full Request
curl -X POST http://localhost:5152/api/books/create/original \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-create-full-format-example.json

# Test 2: Without user_ad
curl -X POST http://localhost:5152/api/books/create/original \
  -H "Content-Type: application/json" \
  -d @ExamBodyRequest/books-create-without-user_ad-example.json
```

---

## 📚 เอกสารอ้างอิง

### Related Documentation
- `RefDocuments/API_CREATE_IMPLEMENTATION.md` - คู่มือการใช้งาน Books API
- `RefDocuments/API_CREATE_ORIGINAL_ENDPOINT.md` - รายละเอียด /create/original endpoint
- `chat-log/2025-10-31-user_ad-default-implementation.md` - การทำงานของ user_ad default

### Configuration Files
- `DefaultSettings/book-defaults.json` - ค่า default ทั้งหมด
- `Models/BookDefaultSettings.cs` - Model definition
- `Controllers/BooksController.cs` - Business logic

---

## ⚙️ Configuration

### Default Values Location
ค่า default สำหรับ `user_ad` อยู่ในไฟล์:
```
DefaultSettings/book-defaults.json
```

### Current Default Value
```json
{
  "BookDefaultSettings": {
    "UserAd": "EXAT\\ECMUSR07"
  }
}
```

### To Change Default Value
1. แก้ไขไฟล์ `DefaultSettings/book-defaults.json`
2. เปลี่ยนค่า `UserAd` เป็นค่าที่ต้องการ
3. บันทึกไฟล์
4. Restart API server (ไม่ต้อง recompile)

---

## 🔍 Troubleshooting

### ปัญหา: Request ไม่ผ่าน
- ตรวจสอบ JSON syntax ว่าถูกต้อง
- ตรวจสอบ required fields (`book_subject`, `registrationbook_id`)
- ตรวจสอบ Content-Type header เป็น `application/json`

### ปัญหา: Default user_ad ไม่ทำงาน
- ตรวจสอบ `book-defaults.json` มี `UserAd` property
- ตรวจสอบค่า escape character ใช้ `\\` สำหรับ backslash
- Restart API server หลังแก้ไข configuration

### ปัญหา: File content error
- ตรวจสอบ Base64 encoding ถูกต้อง
- ตรวจสอบขนาดไฟล์ไม่เกิน limit
- ตรวจสอบ file extension ตรงกับ content

---

**Last Updated**: October 31, 2025  
**API Version**: 1.0  
**Status**: Production Ready ✅
