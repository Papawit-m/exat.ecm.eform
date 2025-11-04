# ตัวอย่าง Request Body สำหรับทดสอบ API

## 📋 คำแนะนำการใช้งาน

คัดลอก JSON ด้านล่างไปใส่ใน Swagger UI หรือ Postman เพื่อทดสอบ endpoints:
- POST `/api/books/create/approved`
- POST `/api/books/create/non-compliant`
- POST `/api/books/create/under-construction`

---

## ✅ ตัวอย่าง 1: Request Body แบบเต็ม (Full)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "นายทดสอบ ระบบ",
    "book_subject": "ทดสอบการสร้างเอกสารผ่าน API",
    "book_to": "สผว.",
    "book_originaldocumentdetail": "เอกสารต้นฉบับจาก แผนกบริหารงานกลาง",
    "book_searchterm": "ทดสอบ,API,เอกสาร",
    "book_remark": "หมายเหตุ: นี่คือการทดสอบระบบ",
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
      "file_name": "attachment1.pdf",
      "file_extension": "pdf",
      "file_path": "/uploads/attachment1.pdf",
      "file_url": "http://example.com/attachment1.pdf",
      "file_remark": "ไฟล์แนบประกอบการพิจารณา",
      "alfresco_parentid": null,
      "alfresco_foldername": "2025/10",
      "alfresco_nodetype": "cm:content",
      "alfresco_noderef": null,
      "alfresco_nodeid": null
    }
  ],
  "bookFile": [
    {
      "file_content": null,
      "file_name": "main_document.docx",
      "file_extension": "docx",
      "file_path": "/uploads/main_document.docx",
      "file_url": "http://example.com/main_document.docx",
      "file_remark": "เอกสารหลัก",
      "alfresco_parentid": null,
      "alfresco_foldername": "2025/10",
      "alfresco_nodetype": "cm:content",
      "alfresco_noderef": null,
      "alfresco_nodeid": null
    }
  ],
  "bookHistory": [
    {
      "history_id": null,
      "action": "สร้างเอกสาร",
      "action_by": "EXAT\\ECMUSR07",
      "action_date": "2025-10-30T10:30:00Z",
      "remark": "สร้างเอกสารครั้งแรก"
    }
  ],
  "bookReferences": [
    {
      "reference_bookid": "REF-2025-001",
      "reference_bookcode": "AG0101/2025/001",
      "reference_bookdate": "2025-10-01T00:00:00Z",
      "reference_subject": "เอกสารอ้างอิงประกอบการพิจารณา",
      "referencetype_id": 1,
      "referencetype_name": "อ้างอิง"
    }
  ],
  "bookReferenceAttach": [
    {
      "reference_bookid": "REF-2025-001",
      "file_content": null,
      "file_name": "ref_document.pdf",
      "file_extension": "pdf",
      "file_path": "/uploads/ref_document.pdf",
      "file_url": "http://example.com/ref_document.pdf",
      "file_remark": "ไฟล์แนบของเอกสารอ้างอิง",
      "alfresco_parentid": null,
      "alfresco_foldername": "references/2025",
      "alfresco_nodetype": "cm:content",
      "alfresco_noderef": null,
      "alfresco_nodeid": null
    }
  ]
}
```

---

## ⚡ ตัวอย่าง 2: Request Body แบบย่อ (Minimal - Required Fields Only)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "นายทดสอบ ระบบ",
    "book_subject": "ทดสอบการสร้างเอกสารผ่าน API",
    "book_to": "สผว.",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_org_code": "AG0101",
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
    "create_page": 1,
    "is_circular": 0
  }
}
```

---

## 🧪 ตัวอย่าง 3: Request Body สำหรับ Approved (กรณีอนุมัติ)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "นายอนุมัติ ผ่าน",
    "book_subject": "ทดสอบเอกสารกรณีอนุมัติเข้าสู่หลักเกณ์",
    "book_to": "ผู้อำนวยการฝ่ายวิศวกรรม",
    "book_originaldocumentdetail": "หนังสืออนุมัติจากคณะกรรมการ",
    "book_searchterm": "อนุมัติ,หลักเกณ์,วิศวกรรม",
    "book_remark": "ได้รับการอนุมัติแล้ว เข้าสู่หลักเกณ์",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_nameth": "สมุดทะเบียนส่ง",
    "registrationbook_org_code": "AG0101",
    "registrationbook_org_nameth": "แผนกบริหารงานกลาง",
    "registrationbook_org_shtname": "บร.",
    "booktype_id": 93,
    "sendtype_id": 1,
    "format_id": 2,
    "subformat_id": 0,
    "speed_id": 2,
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
      "file_name": "approval_certificate.pdf",
      "file_extension": "pdf",
      "file_remark": "หนังสือรับรองการอนุมัติ"
    }
  ]
}
```

---

## ❌ ตัวอย่าง 4: Request Body สำหรับ Non-Compliant (กรณีไม่เข้าหลักเกณ์)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "นายไม่ผ่าน เกณฑ์",
    "book_subject": "ทดสอบเอกสารกรณีไม่เข้าหลักเกณ์",
    "book_to": "ผู้อำนวยการฝ่ายวิศวกรรม",
    "book_originaldocumentdetail": "เอกสารขาดหลักฐานประกอบ",
    "book_searchterm": "ไม่อนุมัติ,ขาดเกณฑ์",
    "book_remark": "ไม่เข้าหลักเกณ์ ต้องทบทวน",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_nameth": "สมุดทะเบียนส่ง",
    "registrationbook_org_code": "AG0101",
    "registrationbook_org_nameth": "แผนกบริหารงานกลาง",
    "registrationbook_org_shtname": "บร.",
    "booktype_id": 93,
    "sendtype_id": 1,
    "format_id": 2,
    "subformat_id": 0,
    "speed_id": 3,
    "secret_id": 1,
    "optiondate_id": 1,
    "optionlanguage_id": 1,
    "optionno_id": 1,
    "status_id": 2,
    "request_org_code": "AG0101",
    "create_page": 1,
    "is_circular": 0
  },
  "bookAttach": [
    {
      "file_name": "incomplete_documents.pdf",
      "file_extension": "pdf",
      "file_remark": "เอกสารที่ไม่สมบูรณ์"
    }
  ],
  "bookHistory": [
    {
      "action": "ส่งกลับแก้ไข",
      "action_by": "EXAT\\REVIEWER01",
      "action_date": "2025-10-29T15:00:00Z",
      "remark": "ขาดเอกสารประกอบการพิจารณา"
    }
  ]
}
```

---

## 🏗️ ตัวอย่าง 5: Request Body สำหรับ Under Construction (กรณีอยู่ระหว่างก่อสร้าง)

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "นายโครงการ ก่อสร้าง",
    "book_subject": "ขอหนังสือรับรองจากที่ปรึกษาโครงการ",
    "book_to": "บริษัทที่ปรึกษา ABC จำกัด",
    "book_originaldocumentdetail": "โครงการอยู่ระหว่างก่อสร้าง ความคืบหน้า 45%",
    "book_searchterm": "ก่อสร้าง,ที่ปรึกษา,โครงการ",
    "book_remark": "ขอหนังสือรับรองสำหรับขั้นตอนที่ 3",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_nameth": "สมุดทะเบียนส่ง",
    "registrationbook_org_code": "AG0101",
    "registrationbook_org_nameth": "แผนกบริหารงานกลาง",
    "registrationbook_org_shtname": "บร.",
    "booktype_id": 93,
    "sendtype_id": 1,
    "format_id": 2,
    "subformat_id": 0,
    "speed_id": 2,
    "secret_id": 1,
    "optiondate_id": 1,
    "optionlanguage_id": 1,
    "optionno_id": 1,
    "status_id": 3,
    "request_org_code": "AG0101",
    "create_page": 1,
    "is_circular": 0,
    "parent_bookid": "PROJ-2024-001"
  },
  "bookAttach": [
    {
      "file_name": "construction_progress_report.pdf",
      "file_extension": "pdf",
      "file_remark": "รายงานความคืบหน้าโครงการ"
    },
    {
      "file_name": "request_letter_draft.docx",
      "file_extension": "docx",
      "file_remark": "ร่างหนังสือขอรับรอง"
    }
  ],
  "bookReferences": [
    {
      "reference_bookid": "PROJ-2024-001",
      "reference_bookcode": "AG0101/2024/PROJ-001",
      "reference_bookdate": "2024-01-15T00:00:00Z",
      "reference_subject": "โครงการก่อสร้างทางด่วน",
      "referencetype_id": 2,
      "referencetype_name": "โครงการอ้างอิง"
    }
  ]
}
```

---

## 🔍 ตัวอย่าง 6: Request Body พร้อม Base64 File Content

```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book": {
    "book_owner": "นายทดสอบ ไฟล์",
    "book_subject": "ทดสอบการ upload ไฟล์พร้อม Base64",
    "book_to": "สผว.",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "registrationbook_org_code": "AG0101",
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
    "create_page": 1,
    "is_circular": 0
  },
  "bookFile": [
    {
      "file_content": "JVBERi0xLjQKJeLjz9MKMSAwIG9iago8PC9UeXBlL0NhdGFsb2cvUGFnZXMgMiAwIFI+PgplbmRvYmoKMiAwIG9iago8PC9UeXBlL1BhZ2VzL0tpZHNbMyAwIFJdL0NvdW50IDE+PgplbmRvYmoKMyAwIG9iago8PC9UeXBlL1BhZ2UvTWVkaWFCb3hbMCAwIDYxMiA3OTJdL1BhcmVudCAyIDAgUi9SZXNvdXJjZXMgNCAwIFI+PgplbmRvYmoKNCAwIG9iago8PC9Gb250PDwvRjEgNSAwIFI+Pj4+CmVuZG9iago1IDAgb2JqCjw8L1R5cGUvRm9udC9TdWJ0eXBlL1R5cGUxL0Jhc2VGb250L0hlbHZldGljYT4+CmVuZG9iagp4cmVmCjAgNgowMDAwMDAwMDAwIDY1NTM1IGYgCjAwMDAwMDAwMTUgMDAwMDAgbiAKMDAwMDAwMDA2NCAwMDAwMCBuIAowMDAwMDAwMTIzIDAwMDAwIG4gCjAwMDAwMDAyMTQgMDAwMDAgbiAKMDAwMDAwMDI1NiAwMDAwMCBuIAp0cmFpbGVyCjw8L1NpemUgNi9Sb290IDEgMCBSPj4Kc3RhcnR4cmVmCjM1NAolJUVPRgo=",
      "file_name": "sample.pdf",
      "file_extension": "pdf",
      "file_path": null,
      "file_url": null,
      "file_remark": "ไฟล์ PDF ตัวอย่าง (Base64 encoded)"
    }
  ]
}
```

---

## 📋 การทดสอบด้วย cURL

### ทดสอบ Approved Endpoint
```bash
curl -X POST "http://localhost:5152/api/books/create/approved" \
  -H "Content-Type: application/json" \
  -d '{
    "user_ad": "EXAT\\ECMUSR07",
    "book": {
      "book_owner": "นายทดสอบ ระบบ",
      "book_subject": "ทดสอบการสร้างเอกสาร",
      "book_to": "สผว.",
      "registrationbook_id": "E1786792382247A49DD27072718DB187",
      "registrationbook_org_code": "AG0101",
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
      "create_page": 1,
      "is_circular": 0
    }
  }'
```

### ทดสอบ Non-Compliant Endpoint
```bash
curl -X POST "http://localhost:5152/api/books/create/non-compliant" \
  -H "Content-Type: application/json" \
  -d @request_non_compliant.json
```

### ทดสอบ Under Construction Endpoint
```bash
curl -X POST "http://localhost:5152/api/books/create/under-construction" \
  -H "Content-Type: application/json" \
  -d @request_under_construction.json
```

---

## ✅ Expected Response

### Success Response (200 OK)
```json
{
  "success": true,
  "message": "เอกสารถูกสร้างสำเร็จ",
  "data": {
    "status": "success",
    "statusCode": "200",
    "bookId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "book_code": "APV-20251030-5678",
    "book_subject": "ทดสอบการสร้างเอกสารผ่าน API",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "booktype_id": 93,
    "message": "เอกสารถูกสร้างสำเร็จ (กรณีอนุมัติ/เข้าสู่หลักเกณ์)",
    "created_by": "EXAT\\ECMUSR07",
    "created_date": "2025-10-30T10:45:23.1234567Z"
  }
}
```

### Error Response - Missing user_ad (400 Bad Request)
```json
{
  "success": false,
  "message": "user_ad is required",
  "errorCode": "USER_AD_REQUIRED",
  "data": null
}
```

### Error Response - Missing book data (400 Bad Request)
```json
{
  "success": false,
  "message": "book data is required",
  "errorCode": "BOOK_DATA_REQUIRED",
  "data": null
}
```

---

## 📝 Notes

1. **user_ad Format:** ต้องเป็น `DOMAIN\\USERNAME` format (e.g., `EXAT\\ECMUSR07`)
2. **GUID Format:** `registrationbook_id` ต้องเป็น GUID format (32 hexadecimal digits)
3. **Date Format:** ใช้ ISO 8601 format (`YYYY-MM-DDTHH:mm:ssZ`)
4. **File Content:** Base64 encoded string สำหรับ `file_content` field
5. **Arrays:** `bookAttach`, `bookFile`, `bookHistory`, `bookReferences`, `bookReferenceAttach` สามารถเป็น empty array `[]` หรือ `null` ได้

---

**Last Updated:** October 30, 2025  
**File Location:** `RefDocuments/API_CREATE_TEST_EXAMPLES.md`
