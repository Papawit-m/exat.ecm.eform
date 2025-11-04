# API /api/books/create/under-construction/simple - โครงการอยู่ระหว่างก่อสร้าง

## 📋 สรุปการอัพเดท

สร้าง endpoint `/api/books/create/under-construction/simple` สำหรับการสร้างเอกสารแบบง่าย (Simplified) กรณีโครงการอยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา โดยรองรับ K2 SmartObject และ K2 REST Service integration

### ✨ คุณสมบัติ
- ✅ รองรับพารามิเตอร์แบบง่าย (4 ฟิลด์ Required)
- ✅ รองรับ `bookFile` (ไฟล์เอกสารหลัก - Optional, array)
- ✅ รองรับ `bookAttach` (ไฟล์แนบเพิ่มเติม - Optional, array)
- ✅ ส่งแค่ 3 ฟิลด์ต่อไฟล์: `file_content`, `file_name`, `file_extension`
- ✅ Defaults ถูก apply อัตโนมัติจาก `book-defaults.json`
- ✅ K2 SmartObject Compatible
- ✅ Book Code Format: `UNC-YYYYMMDD-XXXX` (Under Construction)

---

## 🎯 API Endpoint

**Method:** `POST`  
**URL:** `/api/books/create/under-construction/simple`  
**Content-Type:** `application/json; charset=utf-8`

---

## 📝 Request Body Format

### พารามิเตอร์ที่รองรับ

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบโครงการอยู่ระหว่างก่อสร้าง",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "parent_bookid": "PROJECT001",
  "parent_orgid": "ORG001",
  "parent_positionname": "หัวหน้าโครงการ",
  "bookFile": [
    {
      "file_content": "base64content...",
      "file_name": "construction_plan.pdf",
      "file_extension": "pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "base64content...",
      "file_name": "blueprint.pdf",
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
| `parent_bookid` | string | Parent Book ID (เช่น รหัสโครงการ) |
| `parent_orgid` | string | Parent Organization ID (เช่น รหัสหน่วยงาน) |
| `parent_positionname` | string | Parent Position Name (เช่น หัวหน้าโครงการ, ผู้จัดการโครงการ) |
| `bookFile` | array | ไฟล์เอกสารหลัก เช่น แผนก่อสร้าง, ตารางเวลา (สามารถส่งได้มากกว่า 1 ไฟล์) |
| `bookAttach` | array | ไฟล์แนบเพิ่มเติม เช่น แบบแปลน, รูปถ่าย, เอกสารอนุมัติ (สามารถส่งได้มากกว่า 1 ไฟล์) |

### BookFile / BookAttach Fields (ส่งแค่ 3 ฟิลด์)

| ฟิลด์ | ประเภท | คำอธิบาย | Required |
|------|--------|----------|----------|
| `file_content` | string | ไฟล์ในรูปแบบ Base64 | ✅ Yes |
| `file_name` | string | ชื่อไฟล์ (รวม extension) | ✅ Yes |
| `file_extension` | string | นามสกุลไฟล์ (e.g., pdf, png, jpg, dwg) | ✅ Yes |

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
  "book_subject": "ทดสอบโครงการอยู่ระหว่างก่อสร้าง",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "parent_bookid": "PROJECT001",
  "parent_orgid": "ORG001",
  "parent_positionname": "หัวหน้าโครงการ",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "construction_plan.pdf",
      "file_extension": "pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "blueprint.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

### 2️⃣ กรณีมีหลายไฟล์ (2 bookFile + 3 bookAttach)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "หลายไฟล์โครงการก่อสร้าง",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "main_plan.pdf",
      "file_extension": "pdf"
    },
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "schedule.pdf",
      "file_extension": "pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "blueprint1.pdf",
      "file_extension": "pdf"
    },
    {
      "file_content": "iVBORw0KGgoAAAANSUhEU...",
      "file_name": "photo.png",
      "file_extension": "png"
    },
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "approval_doc.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

### 3️⃣ กรณีไม่มีไฟล์เลย (Required fields only)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ไม่มีไฟล์ - โครงการก่อสร้าง",
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
  "message": "เอกสารถูกสร้างสำเร็จ (กรณีอยู่ระหว่างก่อสร้าง)",
  "data": {
    "status": "S",
    "statusCode": "200",
    "message": "Success: generate book.",
    "book_id": "3277F8C9D8E54A73A0C3385A385E3EBA",
    "book_code": "UNC-20251030-2122",
    "book_subject": "ทดสอบโครงการอยู่ระหว่างก่อสร้าง",
    "book_to": "กพผ.",
    "registrationbook_id": "RB004",
    "parent_bookid": "PROJECT001",
    "parent_orgid": "ORG001",
    "parent_positionname": "หัวหน้าโครงการ",
    "booktype_id": 93,
    "bookFile": [
      {
        "file_content": "JVBERi0xLjQKJeLjz9MK",
        "file_name": "construction_plan.pdf",
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
        "file_name": "blueprint.pdf",
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
    "created_date": "2025-10-30T21:51:33.3880321+07:00"
  },
  "error": null,
  "errorCode": null,
  "timestamp": "2025-10-30T14:51:33.3883959Z"
}
```

---

## 🧪 PowerShell Test Script

### Test 1: มี bookFile และ bookAttach

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "ทดสอบโครงการอยู่ระหว่างก่อสร้าง"
    book_to = "กพผ."
    registrationbook_id = "RB004"
    parent_bookid = "PROJECT001"
    parent_orgid = "ORG001"
    parent_positionname = "หัวหน้าโครงการ"
    bookFile = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "construction_plan.pdf"
            file_extension = "pdf"
        }
    )
    bookAttach = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "blueprint.pdf"
            file_extension = "pdf"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/under-construction/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"

Write-Host "Book Code: $($response.data.book_code)"
Write-Host "File Count: $($response.data.file_count)"
Write-Host "Attach Count: $($response.data.attach_count)"
```

### Test 2: หลายไฟล์ (2 bookFile + 3 bookAttach)

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "หลายไฟล์โครงการก่อสร้าง"
    book_to = "กพผ."
    registrationbook_id = "RB004"
    bookFile = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "main_plan.pdf"
            file_extension = "pdf"
        },
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "schedule.pdf"
            file_extension = "pdf"
        }
    )
    bookAttach = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "blueprint1.pdf"
            file_extension = "pdf"
        },
        @{
            file_content = "iVBORw0KGgoAAAANSUhEU..."
            file_name = "photo.png"
            file_extension = "png"
        },
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "approval_doc.pdf"
            file_extension = "pdf"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/under-construction/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"
```

### Test 3: ไม่มีไฟล์

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "ไม่มีไฟล์ - โครงการก่อสร้าง"
    book_to = "กพผ."
    registrationbook_id = "RB004"
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/under-construction/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"
```

---

## 🧾 Test Results Summary

| Test Case | bookFile Count | bookAttach Count | Result | Book Code |
|-----------|----------------|------------------|--------|-----------|
| Test 1: มี bookFile + bookAttach | 1 | 1 | ✅ PASSED | UNC-20251030-2122 |
| Test 2: หลายไฟล์ (2 + 3) | 2 | 3 | ✅ PASSED | UNC-20251030-7150 |
| Test 3: ไม่มีไฟล์เลย | 0 | 0 | ✅ PASSED | UNC-20251030-5122 |

**สรุป:** ทดสอบทั้งหมด 3 กรณี ✅ PASSED 3/3 (100%)

---

## 🔍 Use Cases (กรณีการใช้งาน)

API นี้เหมาะสำหรับสถานการณ์ต่อไปนี้:

### 1. โครงการก่อสร้างใหม่
- ขอหนังสือรับรองจากที่ปรึกษาโครงการ
- แนบแผนการก่อสร้าง (bookFile)
- แนบแบบแปลน, รูปถ่าย (bookAttach)

### 2. โครงการก่อสร้างระหว่างดำเนินการ
- รายงานความคืบหน้า
- แนบภาพถ่ายงาน
- แนบเอกสารอนุมัติเพิ่มเติม

### 3. ขอหนังสือจากที่ปรึกษา
- ขอคำปรึกษาด้านวิศวกรรม
- แนบรายละเอียดปัญหา
- แนบเอกสารประกอบการพิจารณา

---

## 🔍 ความแตกต่างกับ API อื่น ๆ

| Feature | Approved Simple | Non-Compliant Simple | Under-Construction Simple |
|---------|----------------|---------------------|--------------------------|
| Book Code Prefix | `APV-` | `NCL-` | `UNC-` |
| Use Case | เอกสารที่อนุมัติ | เอกสารไม่เข้าหลักเกณ์ | โครงการก่อสร้าง |
| Required Fields | 4 fields | 4 fields | 4 fields |
| Optional Fields | ✅ Same | ✅ Same | ✅ Same |
| bookFile Support | ✅ Yes | ✅ Yes | ✅ Yes |
| bookAttach Support | ✅ Yes | ✅ Yes | ✅ Yes |
| Defaults Source | `Endpoints.Approved` | `Endpoints.NonCompliant` | `Endpoints.UnderConstruction` |

---

## 📝 หมายเหตุ

1. **Book Code Format**
   - Prefix: `UNC-` (Under Construction)
   - Format: `UNC-YYYYMMDD-XXXX`
   - Example: `UNC-20251030-2122`

2. **bookFile และ bookAttach เป็น optional**
   - หากไม่ส่งมา จะ return `null` และ count = 0
   - สามารถส่ง bookFile อย่างเดียว, bookAttach อย่างเดียว, หรือทั้งคู่

3. **รองรับหลายไฟล์**
   - `bookFile` สามารถส่งได้มากกว่า 1 ไฟล์ (เช่น แผนหลัก + ตารางเวลา)
   - `bookAttach` สามารถส่งได้มากกว่า 1 ไฟล์ (เช่น แบบแปลน + รูปถ่าย + เอกสารอนุมัติ)

4. **ส่งแค่ 3 ฟิลด์ต่อไฟล์**
   - `file_content` (Base64)
   - `file_name`
   - `file_extension`
   - ฟิลด์อื่น ๆ จะถูก apply defaults อัตโนมัติ

5. **Defaults Configuration**
   - อ่านจากไฟล์ `DefaultSettings/book-defaults.json`
   - Section: `Endpoints.UnderConstruction`
   - สามารถแก้ไข default values ได้ที่ไฟล์นี้

6. **API Logging**
   - ทุก request จะถูกบันทึกใน `S_API_ESARABAN_LOG` table
   - รวมถึง bookFile และ bookAttach counts

7. **K2 SmartObject Compatible**
   - รองรับการเรียกใช้งานผ่าน K2 REST Service
   - รูปแบบ Request/Response เป็นไปตามมาตรฐาน K2

8. **Parent Fields สำหรับโครงการ**
   - `parent_bookid` = รหัสโครงการ
   - `parent_orgid` = รหัสหน่วยงานรับผิดชอบ
   - `parent_positionname` = หัวหน้าโครงการ/ผู้จัดการโครงการ

---

## 🔗 Related Documentation

- [API_CREATE_APPROVED_SIMPLE_WITH_BOOKATTACH.md](./API_CREATE_APPROVED_SIMPLE_WITH_BOOKATTACH.md) - Approved Simple API
- [API_CREATE_NON_COMPLIANT_SIMPLE.md](./API_CREATE_NON_COMPLIANT_SIMPLE.md) - Non-Compliant Simple API
- [BOOK_DEFAULT_CONFIG_GUIDE.md](./BOOK_DEFAULT_CONFIG_GUIDE.md) - คู่มือ Configuration
- [API_CREATE_IMPLEMENTATION.md](./API_CREATE_IMPLEMENTATION.md) - Implementation Details
- [DefaultSettings/book-defaults.json](../DefaultSettings/book-defaults.json) - Default Values Config

---

## 🔧 Configuration Example

**DefaultSettings/book-defaults.json** - Under-Construction Section:

```json
{
  "Endpoints": {
    "UnderConstruction": {
      "BookCodePrefix": "UNC-",
      "StatusId": 1,
      "HistoryAction": "สร้างเอกสาร (อยู่ระหว่างก่อสร้าง)",
      "CustomDefaults": {}
    }
  }
}
```

---

## 📚 API Summary

เส้น API นี้ออกแบบมาสำหรับ:
- ✅ โครงการก่อสร้างที่อยู่ระหว่างดำเนินการ
- ✅ การขอหนังสือจากที่ปรึกษาโครงการ
- ✅ รองรับเอกสารและไฟล์แนบหลายไฟล์
- ✅ ใช้งานง่าย - ส่งเฉพาะฟิลด์ที่จำเป็น
- ✅ K2 SmartObject Compatible

---

**Last Updated:** 2025-10-30  
**Version:** 1.0.0  
**Status:** ✅ Implemented & Tested
