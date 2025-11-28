# API /api/books/create/approved/simple - รองรับ bookAttach

## 📋 สรุปการอัพเดท

เพิ่มพารามิเตอร์ `bookAttach` ให้กับ `/api/books/create/approved/simple` เพื่อรองรับไฟล์แนบเพิ่มเติม (Attachments) นอกเหนือจาก `bookFile` (ไฟล์เอกสารหลัก)

### ✨ คุณสมบัติใหม่
- ✅ รองรับ `bookAttach` (ไฟล์แนบเพิ่มเติม)
- ✅ สามารถส่งได้มากกว่า 1 ไฟล์
- ✅ ส่งแค่ 3 ฟิลด์: `file_content`, `file_name`, `file_extension`
- ✅ Defaults จะถูก apply อัตโนมัติจาก `book-defaults.json`
- ✅ Response มี `attach_count` แสดงจำนวนไฟล์แนบ

---

## 🎯 Request Body Format

### พารามิเตอร์ที่รองรับ

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "บันทึกเพิ่มเติมประกอบการพิจารณา",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "parent_bookid": "ABC123DEF456",
  "parent_orgid": "ORG001",
  "parent_positionname": "ผู้อำนวยการ",
  "bookFile": [
    {
      "file_content": "base64content...",
      "file_name": "main_document.pdf",
      "file_extension": "pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "base64content...",
      "file_name": "attachment1.pdf",
      "file_extension": "pdf"
    },
    {
      "file_content": "base64content...",
      "file_name": "attachment2.png",
      "file_extension": "png"
    }
  ]
}
```

---

## 📝 ตัวอย่าง Request Body

### 1️⃣ กรณีมี bookFile และ bookAttach

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "บันทึกเพิ่มเติมประกอบการพิจารณา",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "parent_bookid": "ABC123DEF456",
  "parent_orgid": "ORG001",
  "parent_positionname": "ผู้อำนวยการ",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MKMyAwIG9iago8PC9UeXBlL1BhZ2UvUGFyZW50IDIgMCBSL01lZGlhQm94WzAgMCA2MTIgNzkyXS9Db250ZW50cyA0IDAgUj4+CmVuZG9iago0IDAgb2JqCjw8L0xlbmd0aCA0Mz4+CnN0cmVhbQpCVAovRjEgMTIgVGYKNTAgNzAwIFRkCihCb29rRmlsZSBUZXN0IERvY3VtZW50KSBUagpFVAplbmRzdHJlYW0KZW5kb2JqCjEgMCBvYmoKPDwvVHlwZS9DYXRhbG9nL1BhZ2VzIDIgMCBSPj4KZW5kb2JqCjIgMCBvYmoKPDwvVHlwZS9QYWdlcy9LaWRzWzMgMCBSXS9Db3VudCAxPj4KZW5kb2JqCjUgMCBvYmoKPDwvVHlwZS9Gb250L1N1YnR5cGUvVHlwZTEvQmFzZUZvbnQvSGVsdmV0aWNhPj4KZW5kb2JqCnhyZWYKMCA2CjAwMDAwMDAwMDAgNjU1MzUgZiAKMDAwMDAwMDE1NyAwMDAwMCBuIAowMDAwMDAwMjA2IDAwMDAwIG4gCjAwMDAwMDAwMTUgMDAwMDAgbiAKMDAwMDAwMDA5MyAwMDAwMCBuIAowMDAwMDAwMjY1IDAwMDAwIG4gCnRyYWlsZXIKPDwvU2l6ZSA2L1Jvb3QgMSAwIFI+PgpzdGFydHhyZWYKMzUyCiUlRU9G",
      "file_name": "book_document.pdf",
      "file_extension": "pdf"
    }
  ],
  "bookAttach": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MKMyAwIG9iago8PC9UeXBlL1BhZ2UvUGFyZW50IDIgMCBSL01lZGlhQm94WzAgMCA2MTIgNzkyXS9Db250ZW50cyA0IDAgUj4+CmVuZG9iago0IDAgb2JqCjw8L0xlbmd0aCA0NT4+CnN0cmVhbQpCVAovRjEgMTIgVGYKNTAgNzAwIFRkCihBdHRhY2htZW50IFRlc3QgRG9jdW1lbnQpIFRqCkVUCmVuZHN0cmVhbQplbmRvYmoKMSAwIG9iago8PC9UeXBlL0NhdGFsb2cvUGFnZXMgMiAwIFI+PgplbmRvYmoKMiAwIG9iago8PC9UeXBlL1BhZ2VzL0tpZHNbMyAwIFJdL0NvdW50IDE+PgplbmRvYmoKNSAwIG9iago8PC9UeXBlL0ZvbnQvU3VidHlwZS9UeXBlMS9CYXNlRm9udC9IZWx2ZXRpY2E+PgplbmRvYmoKeHJlZgowIDYKMDAwMDAwMDAwMCA2NTUzNSBmIAowMDAwMDAwMTU5IDAwMDAwIG4gCjAwMDAwMDAyMDggMDAwMDAgbiAKMDAwMDAwMDAxNSAwMDAwMCBuIAowMDAwMDAwMDkzIDAwMDAwIG4gCjAwMDAwMDAyNjcgMDAwMDAgbiAKdHJhaWxlcgo8PC9TaXplIDYvUm9vdCAxIDAgUj4+CnN0YXJ0eHJlZgozNTQKJSVFT0Y=",
      "file_name": "attachment_info.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

### 2️⃣ กรณีมีหลาย bookAttach (2 ไฟล์)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบหลาย bookAttach",
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
      "file_name": "attachment1.pdf",
      "file_extension": "pdf"
    },
    {
      "file_content": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==",
      "file_name": "attachment2.png",
      "file_extension": "png"
    }
  ]
}
```

### 3️⃣ กรณีไม่มี bookAttach (Optional)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบไม่มี bookAttach",
  "book_to": "กพผ.",
  "registrationbook_id": "RB004",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MK",
      "file_name": "only_main.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

**หมายเหตุ:** `bookAttach` เป็น optional parameter หากไม่มีก็จะ return `null` และ `attach_count = 0`

---

## 📤 Response Format

### กรณีสำเร็จ (Status 200)

```json
{
  "success": true,
  "message": "เอกสารถูกสร้างสำเร็จ",
  "data": {
    "status": "S",
    "statusCode": "200",
    "message": "Success: generate book.",
    "book_id": "86C0634925224248ADA15560BFC1F23F",
    "book_code": "APV-20251030-8201",
    "book_subject": "บันทึกเพิ่มเติมประกอบการพิจารณา",
    "book_to": "กพผ.",
    "registrationbook_id": "RB004",
    "parent_bookid": "ABC123DEF456",
    "parent_orgid": "ORG001",
    "parent_positionname": "ผู้อำนวยการ",
    "booktype_id": 93,
    "bookFile": [
      {
        "file_content": "JVBERi0xLjQK...",
        "file_name": "book_document.pdf",
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
        "file_content": "JVBERi0xLjQK...",
        "file_name": "attachment_info.pdf",
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
    "created_date": "2025-10-30T21:38:07.1285373+07:00"
  },
  "error": null,
  "errorCode": null,
  "timestamp": "2025-10-30T14:38:07.128919Z"
}
```

---

## 🧪 PowerShell Test Script

### Test 1: มี bookFile และ bookAttach

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "บันทึกเพิ่มเติมประกอบการพิจารณา"
    book_to = "กพผ."
    registrationbook_id = "RB004"
    parent_bookid = "ABC123DEF456"
    parent_orgid = "ORG001"
    parent_positionname = "ผู้อำนวยการ"
    bookFile = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK..."
            file_name = "book_document.pdf"
            file_extension = "pdf"
        }
    )
    bookAttach = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK..."
            file_name = "attachment_info.pdf"
            file_extension = "pdf"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"

Write-Host "Book Code: $($response.data.book_code)"
Write-Host "File Count: $($response.data.file_count)"
Write-Host "Attach Count: $($response.data.attach_count)"
```

### Test 2: หลาย bookAttach (2 ไฟล์)

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "ทดสอบหลาย bookAttach"
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
            file_name = "attachment1.pdf"
            file_extension = "pdf"
        },
        @{
            file_content = "iVBORw0KGgoAAAANSUhEU..."
            file_name = "attachment2.png"
            file_extension = "png"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"
```

### Test 3: ไม่มี bookAttach

```powershell
$body = @{
    user_ad = "EXAT\ECMUSR07"
    book_subject = "ทดสอบไม่มี bookAttach"
    book_to = "กพผ."
    registrationbook_id = "RB004"
    bookFile = @(
        @{
            file_content = "JVBERi0xLjQKJeLjz9MK"
            file_name = "only_main.pdf"
            file_extension = "pdf"
        }
    )
} | ConvertTo-Json -Depth 5

$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" `
    -Method POST `
    -Body $body `
    -ContentType "application/json; charset=utf-8"
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
| `bookAttach` | array | **ไฟล์แนบเพิ่มเติม (สามารถส่งได้มากกว่า 1 ไฟล์)** ⭐ NEW |

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

## 🧾 Test Results Summary

| Test Case | bookFile Count | bookAttach Count | Result | Book Code |
|-----------|----------------|------------------|--------|-----------|
| Test 1: มี bookFile + bookAttach | 1 | 1 | ✅ PASSED | APV-20251030-8201 |
| Test 2: หลาย bookAttach (2 files) | 1 | 2 | ✅ PASSED | APV-20251030-8733 |
| Test 3: ไม่มี bookAttach | 1 | 0 | ✅ PASSED | APV-20251030-4131 |

**สรุป:** ทดสอบทั้งหมด 3 กรณี ✅ PASSED 3/3 (100%)

---

## 🔍 ความแตกต่างระหว่าง bookFile และ bookAttach

### bookFile (ไฟล์เอกสารหลัก)
- เป็นไฟล์เอกสารหลักของ Book
- ใช้สำหรับเก็บเอกสารที่เป็นเนื้อหาหลัก
- ส่งผ่าน array `bookFile`

### bookAttach (ไฟล์แนบเพิ่มเติม)
- เป็นไฟล์แนบเพิ่มเติมประกอบการพิจารณา
- ใช้สำหรับเก็บเอกสารอ้างอิง, ภาพประกอบ, หรือข้อมูลเพิ่มเติม
- ส่งผ่าน array `bookAttach` ⭐ NEW

**ทั้ง 2 ประเภทมี structure เหมือนกัน** และใช้ defaults เดียวกันจาก `book-defaults.json`

---

## 📝 หมายเหตุ

1. **bookAttach เป็น optional parameter**
   - หากไม่ส่งมา จะ return `bookAttach: null` และ `attach_count: 0`

2. **รองรับหลายไฟล์**
   - `bookFile` สามารถส่งได้มากกว่า 1 ไฟล์
   - `bookAttach` สามารถส่งได้มากกว่า 1 ไฟล์

3. **ส่งแค่ 3 ฟิลด์**
   - `file_content` (Base64)
   - `file_name`
   - `file_extension`
   - ฟิลด์อื่น ๆ จะถูก apply defaults อัตโนมัติ

4. **Defaults Configuration**
   - อ่านจากไฟล์ `DefaultSettings/book-defaults.json`
   - สามารถแก้ไข default values ได้ที่ไฟล์นี้

5. **API Logging**
   - ทุก request จะถูกบันทึกใน `S_API_ESARABAN_LOG` table
   - รวมถึง bookFile และ bookAttach counts

---

## 🔗 Related Documentation

- [API_CREATE_APPROVED_SIMPLE_EXAMPLES.md](./API_CREATE_APPROVED_SIMPLE_EXAMPLES.md) - ตัวอย่าง Request Body (bookFile only)
- [BOOK_DEFAULT_CONFIG_GUIDE.md](./BOOK_DEFAULT_CONFIG_GUIDE.md) - คู่มือ Configuration
- [API_CREATE_IMPLEMENTATION.md](./API_CREATE_IMPLEMENTATION.md) - Implementation Details
- [DefaultSettings/book-defaults.json](../DefaultSettings/book-defaults.json) - Default Values Config

---

**Last Updated:** 2025-10-30  
**Version:** 1.0.0  
**Status:** ✅ Implemented & Tested
