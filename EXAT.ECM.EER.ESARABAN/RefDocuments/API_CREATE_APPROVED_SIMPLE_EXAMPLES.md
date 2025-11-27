# API Request Body Examples
## Endpoint: POST /api/books/create/approved/simple

**Base URL:** `http://localhost:5152/api/books/create/approved/simple`

---

## 📋 ตัวอย่าง Request Body

### 1. กรณีพื้นฐาน - ไม่มีไฟล์แนบ (Minimal)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ขออนุมัติจัดซื้อครุภัณฑ์คอมพิวเตอร์",
  "book_to": "ผอ.",
  "registrationbook_id": "RB001"
}
```

**คำอธิบาย:**
- ส่งเฉพาะ 4 ฟิลด์ที่จำเป็น (Required)
- ไม่มีไฟล์แนบ
- เหมาะสำหรับเอกสารทั่วไป

---

### 2. กรณีมีไฟล์แนบ 1 ไฟล์ (แค่ 3 ฟิลด์)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "รายงานการประชุมประจำเดือน",
  "book_to": "ผอ.",
  "registrationbook_id": "RB002",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MKMyAwIG9iago8PC9UeXBlIC9QYWdlCi9QYXJlbnQgMSAwIFIKL01lZGlhQm94IFswIDAgNjEyIDc5Ml0KL0NvbnRlbnRzIDQgMCBSCj4+CmVuZG9iago0IDAgb2JqCjw8L0xlbmd0aCA0NT4+CnN0cmVhbQpCVAovRjEgMjQgVGYKMTAwIDcwMCBUZAooSGVsbG8gV29ybGQhKSBUagpFVAplbmRzdHJlYW0KZW5kb2JqCg==",
      "file_name": "meeting_report.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

**คำอธิบาย:**
- ส่งไฟล์แนบ 1 ไฟล์
- ส่งเฉพาะ 3 ฟิลด์: `file_content`, `file_name`, `file_extension`
- Defaults จะถูก apply อัตโนมัติ:
  - `file_path`: "/documents/books"
  - `alfresco_foldername`: "Books"
  - `alfresco_nodetype`: "cm:content"

---

### 3. กรณีมีหลายไฟล์แนบ (Multiple Files)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "เอกสารประกอบการประชุม",
  "book_to": "ผอ.",
  "registrationbook_id": "RB003",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQK...",
      "file_name": "agenda.pdf",
      "file_extension": "pdf"
    },
    {
      "file_content": "UEsDBBQABgAI...",
      "file_name": "presentation.pptx",
      "file_extension": "pptx"
    },
    {
      "file_content": "iVBORw0KGgoA...",
      "file_name": "chart.png",
      "file_extension": "png"
    }
  ]
}
```

**คำอธิบาย:**
- ส่งไฟล์แนบได้หลายไฟล์ (array)
- แต่ละไฟล์ส่งแค่ 3 ฟิลด์
- Defaults จะถูก apply ให้ทุกไฟล์

---

### 4. กรณีมี Parent Book (เอกสารลูก)

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
      "file_name": "additional_info.pdf",
      "file_extension": "pdf"
    }
  ]
}
```

**คำอธิบาย:**
- มีการระบุ parent book (เอกสารแม่)
- `parent_bookid`: รหัสเอกสารแม่
- `parent_orgid`: รหัสหน่วยงานต้นทาง
- `parent_positionname`: ชื่อตำแหน่งผู้ส่ง

---

### 5. กรณี Override Defaults (Custom Values)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "เอกสารสำคัญพิเศษ",
  "book_to": "ผอ.",
  "registrationbook_id": "RB005",
  "bookFile": [
    {
      "file_content": "Q29udGVudA==",
      "file_name": "confidential.pdf",
      "file_extension": "pdf",
      "file_path": "/confidential/documents",
      "file_remark": "เอกสารลับ",
      "alfresco_parentid": "workspace://SpacesStore/abc123",
      "alfresco_foldername": "ConfidentialDocs",
      "alfresco_nodetype": "cm:content"
    }
  ]
}
```

**คำอธิบาย:**
- Override defaults ด้วยค่าที่กำหนดเอง
- ส่งฟิลด์เพิ่มเติมตามต้องการ
- ระบบจะใช้ค่าที่ส่งมา (ไม่ apply defaults)

---

## 📝 PowerShell Test Examples

### Test 1: Basic Request (No Files)
```powershell
$body = @'
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบเอกสาร",
  "book_to": "ผอ.",
  "registrationbook_id": "RB001"
}
'@

Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" `
  -Method POST `
  -Body $body `
  -ContentType "application/json; charset=utf-8"
```

### Test 2: With Single File
```powershell
$body = @'
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "เอกสารพร้อมไฟล์",
  "book_to": "ผอ.",
  "registrationbook_id": "RB002",
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQK",
      "file_name": "document.pdf",
      "file_extension": "pdf"
    }
  ]
}
'@

Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" `
  -Method POST `
  -Body $body `
  -ContentType "application/json; charset=utf-8"
```

### Test 3: With Multiple Files
```powershell
$body = @'
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "เอกสารหลายไฟล์",
  "book_to": "ผอ.",
  "registrationbook_id": "RB003",
  "bookFile": [
    {
      "file_content": "UEsDBBQA",
      "file_name": "file1.pdf",
      "file_extension": "pdf"
    },
    {
      "file_content": "iVBORw0K",
      "file_name": "file2.png",
      "file_extension": "png"
    }
  ]
}
'@

Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/approved/simple" `
  -Method POST `
  -Body $body `
  -ContentType "application/json; charset=utf-8"
```

---

## 📊 Response Format

```json
{
  "success": true,
  "message": "เอกสารถูกสร้างสำเร็จ",
  "data": {
    "status": "S",
    "statusCode": "200",
    "message": "Success: generate book.",
    "book_id": "1F2C8E2DAE894461893F2CA09D3A15C1",
    "book_code": "APV-20251030-8156",
    "book_subject": "รายงานการประชุมประจำเดือน",
    "book_to": "ผอ.",
    "registrationbook_id": "RB002",
    "parent_bookid": "",
    "parent_orgid": "",
    "parent_positionname": "",
    "booktype_id": 93,
    "bookFile": [
      {
        "file_content": "JVBERi0xLjQK...",
        "file_name": "meeting_report.pdf",
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
    "created_by": "EXAT\\ECMUSR07",
    "created_date": "2025-10-30T21:07:38+07:00"
  },
  "error": null,
  "errorCode": null,
  "timestamp": "2025-10-30T14:07:38.1234567Z"
}
```

---

## 🔑 Field Descriptions

### Required Fields (จำเป็นต้องส่ง)
| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `user_ad` | string | Active Directory username | "EXAT\\ECMUSR07" |
| `book_subject` | string | เรื่อง/หัวข้อเอกสาร | "ขออนุมัติจัดซื้อ" |
| `book_to` | string | ถึง (ผู้รับ) | "ผอ.", "กพผ." |
| `registrationbook_id` | string | รหัส Registration Book | "RB001" |

### Optional Fields (ส่งหรือไม่ส่งก็ได้)
| Field | Type | Description | Example |
|-------|------|-------------|---------|
| `parent_bookid` | string | รหัสเอกสารแม่ | "ABC123DEF456" |
| `parent_orgid` | string | รหัสหน่วยงานต้นทาง | "ORG001" |
| `parent_positionname` | string | ชื่อตำแหน่งผู้ส่ง | "ผู้อำนวยการ" |
| `bookFile` | array | ไฟล์แนบ (BookFile[]) | [...] |

### BookFile Fields (ส่งแค่ 3 ฟิลด์แรกก็ได้)
| Field | Type | Required | Description | Default |
|-------|------|----------|-------------|---------|
| `file_content` | string | ✅ | Base64 encoded content | - |
| `file_name` | string | ✅ | ชื่อไฟล์ | - |
| `file_extension` | string | ✅ | นามสกุลไฟล์ | - |
| `file_path` | string | ⚪ | Path ไฟล์ | "/documents/books" |
| `file_url` | string | ⚪ | URL ไฟล์ | "" |
| `file_remark` | string | ⚪ | หมายเหตุ | null |
| `alfresco_parentid` | string | ⚪ | Alfresco parent ID | "" |
| `alfresco_foldername` | string | ⚪ | Alfresco folder | "Books" |
| `alfresco_nodetype` | string | ⚪ | Alfresco node type | "cm:content" |
| `alfresco_noderef` | string | ⚪ | Alfresco node reference | null |
| `alfresco_nodeid` | string | ⚪ | Alfresco node ID | null |
| `originaL_NODEID` | string | ⚪ | Original node ID | null |

---

## 📌 Notes

1. **file_content**: ต้องเป็น Base64 encoded string
2. **user_ad**: Format ต้องเป็น `DOMAIN\\USERNAME` (ใช้ double backslash `\\`)
3. **bookFile**: เป็น array รองรับหลายไฟล์
4. **Defaults**: ถ้าไม่ส่งฟิลด์ optional จะถูก apply จาก `DefaultSettings/book-defaults.json`
5. **Book Code Format**: `APV-YYYYMMDD-XXXX` (APV = Approved)
6. **API Logging**: ทุก request จะถูก log ไปที่ `S_API_ESARABAN_LOG` อัตโนมัติ

---

## 🔗 Related Documentation

- `RefDocuments/K2_SMARTOBJECT_INTEGRATION_GUIDE.md` - K2 integration guide
- `RefDocuments/SWAGGER_API_DOCUMENTATION.md` - Swagger documentation
- `DefaultSettings/book-defaults.json` - Default configuration
- `RefDocuments/API_CREATE_IMPLEMENTATION.md` - Implementation details

---

**Last Updated:** October 30, 2025  
**API Version:** v1  
**Status:** ✅ Production Ready
