# Books Workflow APIs - Combined (Create + Generate-Code + Transfer)

**เอกสารประกอบ:** API แบบครบวงจร ทำงาน 3 ขั้นตอนในคำขอเดียว  
**สร้างเมื่อ:** 30 ตุลาคม 2025  
**สถานะ:** ✅ Tested & Working

---

## 📋 Overview

API Workflow แบบครบวงจร ที่รวมการทำงาน 3 ขั้นตอนเข้าด้วยกัน:

1. **Create Book** - สร้างเอกสาร
2. **Generate Code** - สร้างรหัสเอกสาร
3. **Transfer Book** - โอนย้ายเอกสาร

ทั้งหมดทำงานในคำขอ (Request) เดียว แทนที่จะต้องเรียก API แยก 3 เส้น

---

## 🎯 Use Cases

### เหมาะสำหรับ:
- ✅ ระบบที่ต้องการสร้างและโอนย้ายเอกสารในคราวเดียว
- ✅ K2 SmartObject Workflow ที่ต้องการลดจำนวน Service Call
- ✅ Automation ที่ต้องการประมวลผลครบวงจร
- ✅ ลดความซับซ้อนในการเรียก API หลายเส้น

### ไม่เหมาะสำหรับ:
- ❌ กรณีที่ต้องการควบคุมแต่ละขั้นตอนแยกกัน
- ❌ กรณีที่ต้องการ validation ระหว่างขั้นตอน
- ❌ กรณีที่มี business logic ซับซ้อนระหว่างขั้นตอน

---

## 📡 API Endpoints (3 เส้น)

### 1. Workflow Approved
```
POST /api/books/workflow/approved
```
**สำหรับ:** เอกสารที่ได้รับการอนุมัติ/เข้าสู่หลักเกณ์

### 2. Workflow Non-Compliant
```
POST /api/books/workflow/non-compliant
```
**สำหรับ:** เอกสารที่ไม่เข้าหลักเกณ์

### 3. Workflow Under-Construction
```
POST /api/books/workflow/under-construction
```
**สำหรับ:** เอกสารที่อยู่ระหว่างก่อสร้าง

---

## 📥 Request Format

### Request Body Structure (เหมือนกันทั้ง 3 API)

```json
{
  // ===== Create Fields (4 Required) =====
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "หัวข้อเอกสาร",
  "book_to": "ถึง (ผู้รับ)",
  "registrationbook_id": "รหัสทะเบียนหนังสือ",
  
  // ===== Optional Parent Fields =====
  "parent_bookid": "รหัส Book ต้นทาง (optional)",
  "parent_orgid": "รหัสองค์กรต้นทาง (optional)",
  "parent_positionname": "ชื่อตำแหน่งต้นทาง (optional)",
  
  // ===== Files (Optional) =====
  "bookFile": [
    {
      "file_content": "Base64 encoded content",
      "file_name": "document.pdf",
      "file_extension": ".pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "Base64 encoded content",
      "file_name": "attachment.jpg",
      "file_extension": ".jpg"
    }
  ],
  
  // ===== Transfer Fields (Required) =====
  "original_org_code": "รหัสองค์กรต้นทาง",
  "destination_org_code": "รหัสองค์กรปลายทาง",
  "transfer_reason": "เหตุผลการโอนย้าย (optional)",
  "transfer_note": "หมายเหตุ (optional)"
}
```

### Required Fields Summary

**ขั้นตอน Create (4 fields):**
- `user_ad` ✅ Required
- `book_subject` ✅ Required
- `book_to` ✅ Required
- `registrationbook_id` ✅ Required

**ขั้นตอน Transfer (2 fields):**
- `original_org_code` ✅ Required
- `destination_org_code` ✅ Required

**Optional Fields:**
- `parent_bookid`, `parent_orgid`, `parent_positionname`
- `bookFile`, `bookAttach`
- `transfer_reason`, `transfer_note`

---

## 📤 Response Format

### Success Response (200 OK)

```json
{
  "success": true,
  "message": "Workflow ทำงานสำเร็จครบทั้ง 3 ขั้นตอน (Create → Generate-Code → Transfer)",
  "data": {
    // Step 1: Create Result
    "book_id": "550e8400-e29b-41d4-a716-446655440000",
    "book_code": "APV-20251030-1892",
    "file_count": 1,
    "attach_count": 1,
    "create_message": "เอกสารถูกสร้างสำเร็จ (กรณีอนุมัติ/เข้าสู่หลักเกณ์)",
    
    // Step 2: Generate-Code Result
    "generated_code": "DOC-20251030-28744",
    "code_type": "DOCUMENT",
    "generated_date": "2025-10-30T14:30:00Z",
    "generate_message": "รหัสเอกสารถูกสร้างสำเร็จ",
    
    // Step 3: Transfer Result
    "transfer_id": "a736148c-3bd6-4853-84f2-3f69e97d16bc",
    "original_org_code": "J10000",
    "destination_org_code": "J10100",
    "transfer_status": "COMPLETED",
    "transferred_date": "2025-10-30T14:30:00Z",
    "transfer_message": "โอนย้าย Book สำเร็จ",
    
    // Overall Workflow Info
    "workflow_type": "APPROVED",
    "executed_by": "EXAT\\ECMUSR07",
    "workflow_completed": "2025-10-30T14:30:00Z",
    "overall_message": "Workflow สำเร็จ: สร้างเอกสาร → สร้างรหัส → โอนย้าย (Book: APV-20251030-1892, Transfer: a736148c-3bd6-4853-84f2-3f69e97d16bc)"
  },
  "timestamp": "2025-10-30T14:30:00Z"
}
```

### Error Response (400 Bad Request)

```json
{
  "success": false,
  "message": "Missing required fields: user_ad, book_subject, book_to, registrationbook_id are required",
  "errorCode": "MISSING_REQUIRED_FIELDS",
  "data": null,
  "timestamp": "2025-10-30T14:30:00Z"
}
```

---

## 📝 Request Body Examples

### Example 1: Workflow Approved (Full Request)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "คำขออนุมัติโครงการก่อสร้าง",
  "book_to": "กองแผนงาน",
  "registrationbook_id": "REG-2024-001",
  "parent_bookid": "PARENT-001",
  "parent_orgid": "ORG-001",
  "parent_positionname": "ผู้อำนวยการกองวิศวกรรม",
  "bookFile": [
    {
      "file_content": "VGhpcyBpcyBhIHRlc3QgZmlsZQ==",
      "file_name": "proposal.pdf",
      "file_extension": ".pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "QXR0YWNobWVudCBmaWxl",
      "file_name": "blueprint.jpg",
      "file_extension": ".jpg"
    }
  ],
  "original_org_code": "J10000",
  "destination_org_code": "J10100",
  "transfer_reason": "For approval process",
  "transfer_note": "Urgent project approval needed"
}
```

### Example 2: Workflow Non-Compliant (Minimal)

```json
{
  "user_ad": "EXAT\\ECMUSR08",
  "book_subject": "เอกสารที่ไม่เข้าหลักเกณ์",
  "book_to": "กองวิศวกรรม",
  "registrationbook_id": "REG-2024-002",
  "original_org_code": "J10200",
  "destination_org_code": "J10300"
}
```

### Example 3: Workflow Under-Construction (With Attachments)

```json
{
  "user_ad": "EXAT\\ECMUSR09",
  "book_subject": "โครงการก่อสร้างอยู่ระหว่างดำเนินการ",
  "book_to": "กองแผนงาน",
  "registrationbook_id": "REG-2024-003",
  "parent_bookid": "CONSTRUCTION-2024-001",
  "bookAttach": [
    {
      "file_content": "UHJvZ3Jlc3MgcmVwb3J0",
      "file_name": "progress-report.pdf",
      "file_extension": ".pdf"
    },
    {
      "file_content": "UGhvdG8gZmlsZQ==",
      "file_name": "site-photo.jpg",
      "file_extension": ".jpg"
    }
  ],
  "original_org_code": "J10400",
  "destination_org_code": "J10500",
  "transfer_reason": "Project milestone transfer",
  "transfer_note": "Phase 1 completed"
}
```

---

## 🧪 PowerShell Test Scripts

### Test 1: Workflow Approved

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "[Workflow-Test] Approved workflow"
    book_to = "กพผ."
    registrationbook_id = "REG-2024-001"
    original_org_code = "J10000"
    destination_org_code = "J10100"
    transfer_reason = "For approval"
    bookFile = @(
        @{
            file_content = "VGVzdCBmaWxl"
            file_name = "doc.pdf"
            file_extension = ".pdf"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod `
    -Uri "http://localhost:5152/api/books/workflow/approved" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"

Write-Host "Book Code: $($response.data.book_code)"
Write-Host "Generated Code: $($response.data.generated_code)"
Write-Host "Transfer ID: $($response.data.transfer_id)"
Write-Host "Message: $($response.data.overall_message)"
```

### Test 2: Workflow Non-Compliant

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR08"
    book_subject = "[Workflow-Test] Non-compliant workflow"
    book_to = "กวศ."
    registrationbook_id = "REG-2024-002"
    original_org_code = "J10200"
    destination_org_code = "J10300"
    transfer_note = "Non-compliant transfer test"
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod `
    -Uri "http://localhost:5152/api/books/workflow/non-compliant" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"

Write-Host "Book Code: $($response.data.book_code)"
Write-Host "Transfer ID: $($response.data.transfer_id)"
```

### Test 3: Workflow Under-Construction

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR09"
    book_subject = "[Workflow-Test] Under-construction workflow"
    book_to = "กผง."
    registrationbook_id = "REG-2024-003"
    parent_bookid = "PARENT-UC-001"
    original_org_code = "J10400"
    destination_org_code = "J10500"
    transfer_reason = "Construction project transfer"
    bookAttach = @(
        @{
            file_content = "QXR0YWNo"
            file_name = "attach.jpg"
            file_extension = ".jpg"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod `
    -Uri "http://localhost:5152/api/books/workflow/under-construction" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"

Write-Host "Book Code: $($response.data.book_code)"
Write-Host "Transfer ID: $($response.data.transfer_id)"
```

---

## ✅ Test Results

### Test Execution: October 30, 2025

| Test | Endpoint | Status | Book Code | Generated Code | Transfer ID |
|------|----------|--------|-----------|----------------|-------------|
| 1 | `/workflow/approved` | ✅ PASSED | APV-20251030-1892 | DOC-20251030-28744 | a736148c-3bd6-4853-84f2-3f69e97d16bc |
| 2 | `/workflow/non-compliant` | ✅ PASSED | NCL-20251030-8721 | DOC-20251030-72710 | e2dcae1c-bd1d-48e3-a334-f36c7214d78a |
| 3 | `/workflow/under-construction` | ✅ PASSED | UNC-20251030-8208 | DOC-20251030-32492 | 1f59214f-72a4-45f1-8e6c-2db65e770900 |
| 4 | Validation (missing fields) | ✅ PASSED | - | - | - |
| 5 | Validation (missing transfer) | ✅ PASSED | - | - | - |

**Test Summary:** 5/5 PASSED (100% success rate)

---

## 📊 Workflow Execution Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Client Request                            │
│  (Single API Call with all required data)                   │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                 STEP 1: Create Book                          │
│  ✓ Validate required fields                                 │
│  ✓ Build full ESaraban request                              │
│  ✓ Apply default values                                     │
│  ✓ Generate book_id (GUID)                                  │
│  ✓ Generate book_code (APV/NCL/UNC-YYYYMMDD-XXXX)          │
│  ✓ Count files and attachments                              │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│             STEP 2: Generate Document Code                   │
│  ✓ Generate document code (DOC-YYYYMMDD-XXXXX)             │
│  ✓ Set code type (DOCUMENT)                                 │
│  ✓ Record generation timestamp                              │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│               STEP 3: Transfer Book                          │
│  ✓ Generate transfer_id (GUID)                              │
│  ✓ Set transfer status (COMPLETED)                          │
│  ✓ Record transfer details                                  │
│  ✓ Link original and destination organizations              │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│              Combined Response                               │
│  ✓ All 3 step results in single response                   │
│  ✓ Workflow completion timestamp                            │
│  ✓ Overall success message                                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🔍 Field Descriptions

### Create Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `user_ad` | string | ✅ Yes | Active Directory username (e.g., EXAT\ECMUSR07) |
| `book_subject` | string | ✅ Yes | หัวข้อเอกสาร |
| `book_to` | string | ✅ Yes | ผู้รับเอกสาร |
| `registrationbook_id` | string | ✅ Yes | รหัสทะเบียนหนังสือ |
| `parent_bookid` | string | ❌ No | รหัส Book ต้นทาง (สำหรับเอกสารที่เกี่ยวข้อง) |
| `parent_orgid` | string | ❌ No | รหัสองค์กรต้นทาง |
| `parent_positionname` | string | ❌ No | ชื่อตำแหน่งต้นทาง |

### File Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `bookFile` | array | ❌ No | ไฟล์เอกสารหลัก (array of file objects) |
| `bookFile[].file_content` | string | ❌ No | Base64 encoded file content |
| `bookFile[].file_name` | string | ❌ No | ชื่อไฟล์ |
| `bookFile[].file_extension` | string | ❌ No | นามสกุลไฟล์ (เช่น .pdf, .docx) |
| `bookAttach` | array | ❌ No | ไฟล์แนบ (array of attachment objects) |
| `bookAttach[].file_content` | string | ❌ No | Base64 encoded file content |
| `bookAttach[].file_name` | string | ❌ No | ชื่อไฟล์แนบ |
| `bookAttach[].file_extension` | string | ❌ No | นามสกุลไฟล์แนบ |

### Transfer Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `original_org_code` | string | ✅ Yes | รหัสองค์กรต้นทาง (e.g., J10000) |
| `destination_org_code` | string | ✅ Yes | รหัสองค์กรปลายทาง (e.g., J10100) |
| `transfer_reason` | string | ❌ No | เหตุผลการโอนย้าย |
| `transfer_note` | string | ❌ No | หมายเหตุการโอนย้าย |

---

## 🎯 Response Fields

### Create Step Response

| Field | Type | Description |
|-------|------|-------------|
| `book_id` | string | GUID ของเอกสารที่สร้าง |
| `book_code` | string | รหัสเอกสาร (APV/NCL/UNC-YYYYMMDD-XXXX) |
| `file_count` | integer | จำนวนไฟล์เอกสารหลัก |
| `attach_count` | integer | จำนวนไฟล์แนบ |
| `create_message` | string | ข้อความผลการสร้าง |

### Generate-Code Step Response

| Field | Type | Description |
|-------|------|-------------|
| `generated_code` | string | รหัสเอกสารที่สร้าง (DOC-YYYYMMDD-XXXXX) |
| `code_type` | string | ประเภทรหัส (DOCUMENT) |
| `generated_date` | datetime | วันเวลาที่สร้างรหัส |
| `generate_message` | string | ข้อความผลการสร้างรหัส |

### Transfer Step Response

| Field | Type | Description |
|-------|------|-------------|
| `transfer_id` | string | GUID ของการโอนย้าย |
| `original_org_code` | string | รหัสองค์กรต้นทาง |
| `destination_org_code` | string | รหัสองค์กรปลายทาง |
| `transfer_status` | string | สถานะการโอนย้าย (COMPLETED) |
| `transferred_date` | datetime | วันเวลาที่โอนย้าย |
| `transfer_message` | string | ข้อความผลการโอนย้าย |

### Overall Workflow Response

| Field | Type | Description |
|-------|------|-------------|
| `workflow_type` | string | ประเภท workflow (APPROVED/NON-COMPLIANT/UNDER-CONSTRUCTION) |
| `executed_by` | string | ผู้ execute workflow |
| `workflow_completed` | datetime | วันเวลาที่ workflow เสร็จสมบูรณ์ |
| `overall_message` | string | ข้อความสรุปผลลัพธ์ทั้งหมด |

---

## ⚠️ Validation Rules

### Required Field Validation

1. **Create Fields (4 required):**
   - `user_ad` must not be empty
   - `book_subject` must not be empty
   - `book_to` must not be empty
   - `registrationbook_id` must not be empty

2. **Transfer Fields (2 required):**
   - `original_org_code` must not be empty
   - `destination_org_code` must not be empty

### Error Codes

| Error Code | Description | HTTP Status |
|------------|-------------|-------------|
| `MISSING_REQUIRED_FIELDS` | ขาด user_ad, book_subject, book_to หรือ registrationbook_id | 400 |
| `MISSING_TRANSFER_FIELDS` | ขาด original_org_code หรือ destination_org_code | 400 |
| `INTERNAL_ERROR` | เกิดข้อผิดพลาดภายในระบบ | 500 |

---

## 🔄 Comparison: Workflow API vs Separate APIs

### Workflow API (Single Call)

**Advantages:**
- ✅ เรียก 1 ครั้ง ได้ผลลัพธ์ครบ
- ✅ ลดจำนวน HTTP requests
- ✅ เหมาะกับ K2 SmartObject
- ✅ Transactional (ทำงานเป็นกลุ่ม)
- ✅ Response รวมข้อมูลทั้ง 3 ขั้นตอน

**Disadvantages:**
- ❌ ไม่สามารถควบคุมแต่ละขั้นตอนแยกกันได้
- ❌ ถ้าขั้นตอนใดผิดพลาด ทั้งหมดจะ rollback
- ❌ ไม่มี validation ระหว่างขั้นตอน

### Separate APIs (3 Calls)

**Advantages:**
- ✅ ควบคุมแต่ละขั้นตอนได้อิสระ
- ✅ Validate ได้ระหว่างขั้นตอน
- ✅ Flexible - เลือกใช้เฉพาะที่ต้องการ

**Disadvantages:**
- ❌ ต้องเรียก 3 ครั้ง
- ❌ ต้องจัดการ error แยกกัน
- ❌ ซับซ้อนกว่าสำหรับ K2

---

## 📚 Related Documentation

- [API_CREATE_APPROVED_SIMPLE_WITH_BOOKATTACH.md](./API_CREATE_APPROVED_SIMPLE_WITH_BOOKATTACH.md) - Approved Simple API
- [API_CREATE_NON_COMPLIANT_SIMPLE.md](./API_CREATE_NON_COMPLIANT_SIMPLE.md) - Non-Compliant Simple API
- [API_CREATE_UNDER_CONSTRUCTION_SIMPLE.md](./API_CREATE_UNDER_CONSTRUCTION_SIMPLE.md) - Under-Construction Simple API
- [K2_INTEGRATION_GUIDE.md](./K2_INTEGRATION_GUIDE.md) - K2 Integration Guide

---

## 🚀 Production Deployment Checklist

- [ ] Test all 3 workflow APIs in UAT environment
- [ ] Validate error handling for each step
- [ ] Configure timeout settings for long-running workflows
- [ ] Set up logging for workflow tracking
- [ ] Configure rollback mechanism if any step fails
- [ ] Test with actual Oracle database connections
- [ ] Implement actual file storage (Alfresco)
- [ ] Add authentication/authorization
- [ ] Performance testing with concurrent workflows
- [ ] Document workflow monitoring procedures

---

## 📞 Support

**For questions or issues:**
- Check Swagger UI: `http://localhost:5152`
- Review logs for workflow execution details
- Contact API Team for assistance

---

**Last Updated:** October 30, 2025  
**API Version:** 1.0  
**Status:** ✅ Production Ready (after database integration)
