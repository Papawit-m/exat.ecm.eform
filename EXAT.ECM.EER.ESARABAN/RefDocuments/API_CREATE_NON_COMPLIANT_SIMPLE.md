# API /api/books/create/non-compliant/simple - เอกสารไม่เข้าหลักเกณ์

## 📋 สรุปการอัพเดท

สร้าง endpoint `/api/books/create/non-compliant/simple` สำหรับการสร้างเอกสารแบบง่าย (Simplified) กรณีไม่เข้าหลักเกณ์ โดยรองรับ K2 SmartObject และ K2 REST Service integration

### ✨ คุณสมบัติ
- ✅ รองรับพารามิเตอร์แบบง่าย (4 ฟิลด์ Required)
- ✅ รองรับ `bookFile` (ไฟล์เอกสารหลัก - Optional, array)
- ✅ รองรับ `bookAttach` (ไฟล์แนบเพิ่มเติม - Optional, array)
- ✅ ส่งแค่ 3 ฟิลด์ต่อไฟล์: `file_content`, `file_name`, `file_extension`
- ✅ Defaults ถูก apply อัตโนมัติจาก `book-defaults.json`
- ✅ K2 SmartObject Compatible
- ✅ Book Code Format: `NCL-YYYYMMDD-XXXX`

---

## 🎯 API Endpoint

**Method:** `POST`  
**URL:** `/api/books/create/non-compliant/simple`  
**Content-Type:** `application/json; charset=utf-8`

---

## 📝 Request Body Format

### พารามิเตอร์ที่รองรับ

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบเอกสารไม่เข้าหลักเกณ์",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "parent_bookid": "PARENT001",
  "parent_orgid": "ORG001",
  "parent_positionname": "ผู้อำนวยการ",
  "bookFile": [
    {
      "file_content": "base64content...",
      "file_name": "non_compliant_doc.pdf",
      "file_extension": "pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "base64content...",
      "file_name": "attachment1.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

---

## 📊 Field Descriptions

### ฟิลด์ Required (จำเป็น)

| ฟิลด์ | ประเภท | คำอธิบาย |
|------|--------|----------|
| `user_ad` | string | Active Directory username (e.g., EXAT\\ECMUSR07) |
| `book_subject` | string | เรื่อง/หัวข้อเอกสาร |
| `book_to` | string | ถึง (ผู้รับ) |
| `registrationbook_id` | string | Registration Book ID |

### ฟิลด์ Optional (ไม่จำเป็น)

| ฟิลด์ | ประเภท | คำอธิบาย |
|------|--------|----------|
| `parent_bookid` | string | Parent Book ID (optional) |
| `parent_orgid` | string | Parent Organization ID (optional) |
| `parent_positionname` | string | Parent Position Name (optional) |
| `bookFile` | array | ไฟล์เอกสารหลัก (สามารถส่งได้มากกว่า 1 ไฟล์) |
| `bookAttach` | array | ไฟล์แนบเพิ่มเติม (สามารถส่งได้มากกว่า 1 ไฟล์) |

### BookFile / BookAttach Fields (ส่งแค่ 3 ฟิลด์)

| ฟิลด์ | ประเภท | คำอธิบาย | Required |
|------|--------|----------|----------|
| `file_content` | string | ไฟล์ในรูปแบบ Base64 | ✅ Yes |
| `file_name` | string | ชื่อไฟล์ (รวม extension) | ✅ Yes |
| `file_extension` | string | นามสกุลไฟล์ (e.g., pdf, png, jpg) | ✅ Yes |

**ฟิลด์อื่น ๆ จะถูก apply defaults อัตโนมัติจาก `book-defaults.json`:**
- `file_path` → "/documents/books"
- `file_url` → ""
- `alfresco_parentid` → ""
- `alfresco_foldername` → "Books"
- `alfresco_nodetype` → "cm:content"

---

## 📝 ตัวอย่าง Request Body

### 1️⃣ กรณีมี bookFile และ bookAttach

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบเอกสารไม่เข้าหลักเกณ์",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "parent_bookid": "PARENT001",
  "parent_orgid": "ORG001",
  "parent_positionname": "ผู้อำนวยการ",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "non_compliant_doc.pdf",
      "file_extension": "pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "attachment1.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

### 2️⃣ กรณีมีหลาย bookAttach (3 ไฟล์)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบหลาย bookAttach (Non-Compliant)",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "main_doc.pdf",
      "file_extension": "pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "attach1.pdf",
      "file_extension": "pdf"
    },
    {
      "file_content": "iVBORw0KGgoAAAANSUhEU...",
      "file_name": "attach2.png",
      "file_extension": "png"
    },
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "attach3.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

### 3️⃣ กรณีไม่มีไฟล์เลย (Required fields only)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบไม่มีไฟล์เลย",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004"
}
```

---

## 📤 Response Format

### กรณีสำเร็จ (Status 200)

```json
{
  "success": true,
  "message": "เอกสารถูกสร้างสำเร็จ (กรณีไม่เข้าหลักเกณ์)",
  "data": {
    "status": "S",
    "statusCode": "200",
    "message": "Success: generate book.",
    "book_id": "C38381DDB1AB48C59AD551B996106239",
    "book_code": "NCL-20251030-9097",
    "book_subject": "ทดสอบเอกสารไม่เข้าหลักเกณ์",
    "book_to": "กพผ.",
    "registrationbook_id": "RB004",
    "parent_bookid": "PARENT001",
    "parent_orgid": "ORG001",
    "parent_positionname": "ผู้อำนวยการ",
    "booktype_id": 93,
    "bookFile": [
      {
        "file_content": "JVBERi0xLjQKJeLjz9MK",
        "file_name": "non_compliant_doc.pdf",
        "file_extension": "pdf",
        "file_path": "/documents/books",
        "file_url": "",
        "file_remark": null,
        "alfresco_parentid": "",
        "alfresco_foldername": "Books",
        "alfresco_nodetype": "cm:content",
        "alfresco_noderef": null,
        "alfresco_nodeid": null,
        "originaL_NODEID": null
      }
    ],
    "file_count": 1,
    "bookAttach": [
      {
        "file_content": "JVBERi0xLjQKJeLjz9MK",
        "file_name": "attachment1.pdf",
        "file_extension": "pdf",
        "file_path": "/documents/books",
        "file_url": "",
        "file_remark": null,
        "alfresco_parentid": "",
        "alfresco_foldername": "Books",
        "alfresco_nodetype": "cm:content",
        "alfresco_noderef": null,
        "alfresco_nodeid": null
      }
    ],
    "attach_count": 1,
    "created_by": "EXAT\\ECMUSR07",
    "created_date": "2025-10-30T21:45:59.7190994+07:00"
  },
  "error": null,
  "errorCode": null,
  "timestamp": "2025-10-30T14:45:59.7194514Z"
}
```

---

## 🧪 PowerShell Test Script

### Test 1: มี bookFile และ bookAttach

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "ทดสอบเอกสารไม่เข้าหลักเกณ์"
    book_to = "กพผ."
    registrationbook_id = "RB004"
    parent_bookid = "PARENT001"
    parent_orgid = "ORG001"
    parent_positionname = "ผู้อำนวยการ"
    bookFile = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "non_compliant_doc.pdf"
            file_extension = "pdf"
        }
    )
    bookAttach = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "attachment1.pdf"
            file_extension = "pdf"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/non-compliant/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"

Write-Host "Book Code: $($response.data.book_code)"
Write-Host "File Count: $($response.data.file_count)"
Write-Host "Attach Count: $($response.data.attach_count)"
```

### Test 2: หลาย bookAttach (3 ไฟล์)

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "ทดสอบหลาย bookAttach (Non-Compliant)"
    book_to = "กพผ."
    registrationbook_id = "RB004"
    bookFile = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "main_doc.pdf"
            file_extension = "pdf"
        }
    )
    bookAttach = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "attach1.pdf"
            file_extension = "pdf"
        },
        @{
            file_content = "iVBORw0KGgoAAAANSUhEU..."
            file_name = "attach2.png"
            file_extension = "png"
        },
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "attach3.pdf"
            file_extension = "pdf"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/non-compliant/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"
```

### Test 3: ไม่มีไฟล์เลย

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "ทดสอบไม่มีไฟล์เลย"
    book_to = "กพผ."
    registrationbook_id = "RB004"
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/non-compliant/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"
```

---

## 🧾 Test Results Summary

| Test Case | bookFile Count | bookAttach Count | Result | Book Code |
|-----------|----------------|------------------|--------|-----------|
| Test 1: มี bookFile + bookAttach | 1 | 1 | ✅ PASSED | NCL-20251030-9097 |
| Test 2: หลาย bookAttach (3 files) | 1 | 3 | ✅ PASSED | NCL-20251030-8176 |
| Test 3: ไม่มีไฟล์เลย | 0 | 0 | ✅ PASSED | NCL-20251030-8942 |

**สรุป:** ทดสอบทั้งหมด 3 กรณี ✅ PASSED 3/3 (100%)

---

## 🔍 ความแตกต่างกับ /api/books/create/approved/simple

| Feature | Approved Simple | Non-Compliant Simple |
|---------|----------------|---------------------|
| Book Code Prefix | `APV-` | `NCL-` |
| Use Case | เอกสารที่อนุมัติ/เข้าสู่หลักเกณ์ | เอกสารที่ไม่เข้าหลักเกณ์ |
| Required Fields | 4 fields | 4 fields (เหมือนกัน) |
| Optional Fields | bookFile, bookAttach, parent_* | bookFile, bookAttach, parent_* (เหมือนกัน) |
| Defaults Source | `book-defaults.json` → `Endpoints.Approved` | `book-defaults.json` → `Endpoints.NonCompliant` |
| Response Message | "เอกสารถูกสร้างสำเร็จ" | "เอกสารถูกสร้างสำเร็จ (กรณีไม่เข้าหลักเกณ์)" |

---

## 📝 หมายเหตุ

1. **Book Code Format**
   - Prefix: `NCL-` (Non-Compliant)
   - Format: `NCL-YYYYMMDD-XXXX`
   - Example: `NCL-20251030-9097`

2. **bookFile และ bookAttach เป็น optional**
   - หากไม่ส่งมา จะ return `null` และ count = 0
   - สามารถส่ง bookFile อย่างเดียว, bookAttach อย่างเดียว, หรือทั้งคู่

3. **รองรับหลายไฟล์**
   - `bookFile` สามารถส่งได้มากกว่า 1 ไฟล์
   - `bookAttach` สามารถส่งได้มากกว่า 1 ไฟล์

4. **ส่งแค่ 3 ฟิลด์ต่อไฟล์**
   - `file_content` (Base64)
   - `file_name`
   - `file_extension`
   - ฟิลด์อื่น ๆ จะถูก apply defaults อัตโนมัติ

5. **Defaults Configuration**
   - อ่านจากไฟล์ `DefaultSettings/book-defaults.json`
   - Section: `Endpoints.NonCompliant`
   - สามารถแก้ไข default values ได้ที่ไฟล์นี้

6. **API Logging**
   - ทุก request จะถูกบันทึกใน `S_API_ESARABAN_LOG` table
   - รวมถึง bookFile และ bookAttach counts

7. **K2 SmartObject Compatible**
   - รองรับการเรียกใช้งานผ่าน K2 REST Service
   - รูปแบบ Request/Response เป็นไปตามมาตรฐาน K2

---

## 🔗 Related Documentation

- [API_CREATE_APPROVED_SIMPLE_WITH_BOOKATTACH.md](./API_CREATE_APPROVED_SIMPLE_WITH_BOOKATTACH.md) - Approved Simple API
- [BOOK_DEFAULT_CONFIG_GUIDE.md](./BOOK_DEFAULT_CONFIG_GUIDE.md) - คู่มือ Configuration
- [API_CREATE_IMPLEMENTATION.md](./API_CREATE_IMPLEMENTATION.md) - Implementation Details
- [DefaultSettings/book-defaults.json](../DefaultSettings/book-defaults.json) - Default Values Config

---

## 🔧 Configuration Example

**DefaultSettings/book-defaults.json** - Non-Compliant Section:

```json
{
  "Endpoints": {
    "NonCompliant": {
      "BookCodePrefix": "NCL-",
      "StatusId": 1,
      "HistoryAction": "สร้างเอกสาร (ไม่เข้าหลักเกณ์)",
      "CustomDefaults": {}
    }
  }
}
```

---

**Last Updated:** 2025-10-30  
**Version:** 1.0.0  
**Status:** ✅ Implemented & Tested
