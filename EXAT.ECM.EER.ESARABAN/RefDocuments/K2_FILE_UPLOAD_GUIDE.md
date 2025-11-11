# K2 SmartObject - File Upload Guide
## คู่มือการส่งไฟล์ (`bookFile` และ `bookAttach`) ผ่าน K2 SmartObject

---

## 📋 สรุปสั้น ๆ

| ประเด็น | คำตอบ |
|---------|------|
| **ส่งยังไง?** | ส่งเป็น **JSON String** ผ่าน Property `bookFile` และ `bookAttach` |
| **Format?** | JSON Array String: `'[{...},{...}]'` |
| **Required Fields?** | `file_name`, `file_content` (Base64), `file_extension` |
| **ไม่มีไฟล์?** | ส่ง `null` หรือ `[]` (empty array) |
| **Endpoints ที่รองรับ?** | **10 endpoints**: Create Simple (3) + Create Full (4) + Workflow (3) |

---

## 🎯 Endpoints ที่รองรับ `bookFile` และ `bookAttach`

### ✅ GROUP 1: Books - Create (K2 Compatible - Simple Format) - 3 Endpoints
1. `/api/books/create/approved/simple`
2. `/api/books/create/non-compliant/simple`
3. `/api/books/create/under-construction/simple`

**Use Case**: สร้างเอกสารแบบง่าย (ส่งเฉพาะฟิลด์ที่จำเป็น)

### ✅ GROUP 2: Books - Create (Full Format) - 4 Endpoints
1. `/api/books/create/original`
2. `/api/books/create/approved`
3. `/api/books/create/non-compliant`
4. `/api/books/create/under-construction`

**Use Case**: สร้างเอกสารแบบเต็ม (ส่ง Request Body ตาม eSaraban API Spec - มี nested objects)

### ✅ GROUP 4: Books - Workflow (Combined) - 3 Endpoints
1. `/api/books/workflow/approved`
2. `/api/books/workflow/non-compliant`
3. `/api/books/workflow/under-construction`

**Use Case**: ดำเนินการ 3 ขั้นตอนพร้อมกัน (Create + Generate Code + Transfer)

**หมายเหตุ:**
- GROUP 3 (Operations) และ GROUP 5 (Query) **ไม่รองรับ** `bookFile`/`bookAttach` เพราะเป็น API สำหรับดำเนินการกับเอกสารที่มีอยู่แล้ว หรือดึงข้อมูล

---

## 📐 โครงสร้างข้อมูล

### `bookFile` (ไฟล์เอกสารหลัก)
```json
[
  {
    "file_name": "document.pdf",           // ✅ Required - ชื่อไฟล์
    "file_content": "JVBERi0xLjQKJ...",   // ✅ Required - Base64 encoded content
    "file_extension": "pdf",               // ✅ Required - นามสกุลไฟล์
    "file_remark": "เอกสารหลัก",          // Optional - หมายเหตุ
    "file_path": "/path/to/file",          // Optional - Path
    "file_url": "http://example.com/file", // Optional - URL
    "alfresco_parentid": "123",            // Optional - Alfresco Parent ID
    "alfresco_foldername": "folder1",      // Optional - Alfresco Folder Name
    "alfresco_nodetype": "cm:content",     // Optional - Alfresco Node Type
    "alfresco_noderef": "workspace://...", // Optional - Alfresco Node Reference
    "alfresco_nodeid": "abc123",           // Optional - Alfresco Node ID
    "originaL_NODEID": "original123"       // Optional - Original Node ID (case-sensitive)
  }
]
```

### `bookAttach` (ไฟล์แนบ)
```json
[
  {
    "file_name": "attachment.jpg",         // ✅ Required - ชื่อไฟล์
    "file_content": "iVBORw0KGgoAAA...",  // ✅ Required - Base64 encoded content
    "file_extension": "jpg",               // ✅ Required - นามสกุลไฟล์
    "file_remark": "ไฟล์แนบรูปภาพ",       // Optional - หมายเหตุ
    "file_path": "/path/to/file",          // Optional
    "file_url": "http://example.com/file", // Optional
    "alfresco_parentid": "123",            // Optional
    "alfresco_foldername": "folder1",      // Optional
    "alfresco_nodetype": "cm:content",     // Optional
    "alfresco_noderef": "workspace://...", // Optional
    "alfresco_nodeid": "abc123"            // Optional
  }
]
```

---

## 💡 วิธีการใช้งานใน K2

### **Simple Format Endpoints (GROUP 1 & GROUP 4)**

ส่ง `bookFile` และ `bookAttach` โดยตรง (ไม่ต้อง wrap ใน nested objects)

### **วิธีที่ 1: ส่งไฟล์เดียว (แนะนำ)**

```javascript
// K2 Assign Variable Activity - สำหรับ Simple Format
varBookFileJSON = '[{"file_name":"report.pdf","file_content":"' + varBase64Content + '","file_extension":"pdf"}]'
varBookAttachJSON = '[]'  // ไม่มีไฟล์แนบ
```

### **วิธีที่ 2: ส่งหลายไฟล์**

```javascript
// K2 Assign Variable Activity
varBookFileJSON = '[' +
  '{"file_name":"doc1.pdf","file_content":"' + varFile1Content + '","file_extension":"pdf"},' +
  '{"file_name":"doc2.pdf","file_content":"' + varFile2Content + '","file_extension":"pdf"}' +
']'

varBookAttachJSON = '[' +
  '{"file_name":"img1.jpg","file_content":"' + varImg1Content + '","file_extension":"jpg"},' +
  '{"file_name":"img2.png","file_content":"' + varImg2Content + '","file_extension":"png"}' +
']'
```

### **วิธีที่ 3: ไม่มีไฟล์**

```javascript
// K2 Assign Variable Activity
varBookFileJSON = null        // หรือ "[]"
varBookAttachJSON = null      // หรือ "[]"
```

---

### **Full Format Endpoints (GROUP 2)**

ส่ง Request Body แบบเต็ม (มี nested object `book`) พร้อมกับ `bookFile`, `bookAttach`, และ optional arrays อื่น ๆ

### **วิธีที่ 4: Full Format - สร้าง Request Body แบบเต็ม**

```javascript
// ใน K2 Assign Variable Activity

// 1. สร้าง book object (nested)
varBookJSON = '{' +
  '"book_subject":"' + varBookSubject + '",' +
  '"book_to":"' + varBookTo + '",' +
  '"registrationbook_id":"' + varRegBookId + '",' +
  '"booktype_id":93,' +
  '"sendtype_id":1,' +
  '"format_id":1,' +
  '"subformat_id":1,' +
  '"speed_id":1,' +
  '"secret_id":1,' +
  '"optiondate_id":1,' +
  '"optionlanguage_id":1,' +
  '"optionno_id":1,' +
  '"status_id":1,' +
  '"create_page":1,' +
  '"is_circular":0,' +
  '"parent_bookid":"' + varParentBookId + '",' +
  '"parent_orgid":"' + varParentOrgId + '",' +
  '"parent_positionname":"' + varParentPosition + '"' +
'}'

// 2. สร้าง bookFile array (เหมือน Simple Format)
varBookFileJSON = '[{"file_name":"doc.pdf","file_content":"' + varBase64Content + '","file_extension":"pdf"}]'

// 3. สร้าง bookAttach array
varBookAttachJSON = '[{"file_name":"attachment.jpg","file_content":"' + varAttachBase64 + '","file_extension":"jpg"}]'

// 4. Optional: bookHistory, bookReferences, bookReferenceAttach
varBookHistoryJSON = '[]'          // ถ้าไม่มีให้ส่ง empty array
varBookReferencesJSON = '[]'       // ถ้าไม่มีให้ส่ง empty array
varBookRefAttachJSON = '[]'        // ถ้าไม่มีให้ส่ง empty array
```

**การเรียกใช้ SmartObject (Full Format)**:
```xml
<Execute>
  <SmartObject>ESarabanBook_CreateApproved</SmartObject>
  <Method>Create</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book" Value="{ProcessData.varBookJSON}" />
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
    <Property Name="bookHistory" Value="{ProcessData.varBookHistoryJSON}" />
    <Property Name="bookReferences" Value="{ProcessData.varBookReferencesJSON}" />
    <Property Name="bookReferenceAttach" Value="{ProcessData.varBookRefAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
  </OutputProperties>
</Execute>
```

---

## 🔧 K2 Workflow ตัวอย่างแบบสมบูรณ์

### **Scenario: อ่านไฟล์จาก SharePoint แล้วสร้าง Book**

```xml
<!-- STEP 1: Read File from SharePoint -->
<Execute>
  <SmartObject>SharePointFile</SmartObject>
  <Method>ReadFile</Method>
  <InputProperties>
    <Property Name="SiteURL" Value="{ProcessData.varSharePointSite}" />
    <Property Name="LibraryName" Value="{ProcessData.varLibraryName}" />
    <Property Name="FileName" Value="{ProcessData.varFileName}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="FileContent" Store="ProcessData.varFileBytes" />
    <Property Name="FileName" Store="ProcessData.varFileName" />
    <Property Name="FileExtension" Store="ProcessData.varFileExt" />
  </OutputProperties>
</Execute>

<!-- STEP 2: Convert to Base64 -->
<Assign>
  <Property Name="varBase64Content" 
            Value="System.Convert.ToBase64String(varFileBytes)" />
</Assign>

<!-- STEP 3: Build JSON String for bookFile -->
<Assign>
  <Property Name="varBookFileJSON" 
            Value="'[{\"file_name\":\"' + varFileName + 
                   '\",\"file_content\":\"' + varBase64Content + 
                   '\",\"file_extension\":\"' + varFileExt + 
                   '\",\"file_remark\":\"อัพโหลดจาก SharePoint\"}]'" />
</Assign>

<!-- STEP 4: Set empty bookAttach -->
<Assign>
  <Property Name="varBookAttachJSON" Value="'[]'" />
</Assign>

<!-- STEP 5: Call eSaraban API to Create Book -->
<Execute>
  <SmartObject>ESarabanBook_CreateApproved_Simple</SmartObject>
  <Method>Create</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_subject" Value="{ProcessData.varBookSubject}" />
    <Property Name="book_to" Value="{ProcessData.varBookTo}" />
    <Property Name="registrationbook_id" Value="{ProcessData.varRegistrationBookId}" />
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
  </OutputProperties>
</Execute>

<!-- STEP 6: Check Result -->
<Decision>
  <Condition>{ProcessData.varStatus} == "success"</Condition>
  <TruePath>
    <!-- Success: Send Email Notification -->
    <Email>
      <To>{ProcessData.varUserEmail}</To>
      <Subject>เอกสารสร้างสำเร็จ</Subject>
      <Body>
        Book ID: {ProcessData.varBookId}
        Book Code: {ProcessData.varBookCode}
        Message: {ProcessData.varMessage}
      </Body>
    </Email>
  </TruePath>
  <FalsePath>
    <!-- Failure: Send Error Email -->
    <Email>
      <To>{ProcessData.varAdminEmail}</To>
      <Subject>เกิดข้อผิดพลาด - สร้างเอกสารไม่สำเร็จ</Subject>
      <Body>
        Error: {ProcessData.varMessage}
      </Body>
    </Email>
  </FalsePath>
</Decision>
```

---

## 🔍 ตัวอย่าง JSON String ที่ถูกต้อง

### ✅ **CORRECT - Valid JSON**

```javascript
// ไฟล์เดียว
'[{"file_name":"doc.pdf","file_content":"JVBERi0xLjQK","file_extension":"pdf"}]'

// หลายไฟล์
'[{"file_name":"doc1.pdf","file_content":"JVBERi0xLjQK","file_extension":"pdf"},{"file_name":"doc2.pdf","file_content":"UEsDBBQABg","file_extension":"pdf"}]'

// ไม่มีไฟล์
'[]'
null
```

### ❌ **WRONG - Invalid JSON**

```javascript
// ใช้ single quote แทน double quote
"[{'file_name':'doc.pdf','file_content':'JVBERi0xLjQK','file_extension':'pdf'}]"

// ขาด comma คั่นระหว่างไฟล์
'[{"file_name":"doc1.pdf","file_content":"JVBERi0xLjQK","file_extension":"pdf"}{"file_name":"doc2.pdf","file_content":"UEsDBBQABg","file_extension":"pdf"}]'

// ขาด required fields
'[{"file_name":"doc.pdf"}]'  // ขาด file_content และ file_extension
```

---

## ⚠️ ข้อควรระวัง (IMPORTANT NOTES)

| หัวข้อ | รายละเอียด |
|--------|-----------|
| **Base64 Encoding** | ไฟล์ต้อง encode เป็น Base64 string ก่อนส่ง (ใช้ `System.Convert.ToBase64String()`) |
| **JSON Format** | ต้องเป็น valid JSON array string (ใช้ double quotes `"` ไม่ใช่ single quotes `'`) |
| **Required Fields** | `file_name`, `file_content`, `file_extension` เป็นฟิลด์บังคับ (ขาดไม่ได้) |
| **Empty Arrays** | ถ้าไม่มีไฟล์ ให้ส่ง `[]` หรือ `null` (ไม่ส่งก็ได้ แต่แนะนำส่ง `[]`) |
| **Multiple Files** | ใช้ comma (`,`) คั่นระหว่างไฟล์ใน JSON array |
| **File Size** | ระวังขนาดไฟล์ที่ใหญ่เกินไป (แนะนำไม่เกิน 10MB ต่อไฟล์) |
| **Special Characters** | ต้อง escape special characters ใน JSON string (`"` → `\"`, `\` → `\\`, `\n` → `\\n`) |
| **Property Naming** | ใช้ snake_case (`file_name` ไม่ใช่ `fileName`) |
| **Case Sensitive** | Property `originaL_NODEID` เป็น case-sensitive (L ตัวพิมพ์เล็ก) |

---

## 📊 การตรวจสอบผลลัพธ์

เมื่อส่ง `bookFile` และ `bookAttach` สำเร็จ API จะ return:

```json
{
  "status": "success",
  "statusCode": 200,
  "message": "Book created successfully",
  "book_id": "550e8400-e29b-41d4-a716-446655440000",
  "book_code": "EXT-AP-20251102-001",
  "file_count": 2,      // ✅ จำนวน bookFile ที่ส่งไป
  "attach_count": 3     // ✅ จำนวน bookAttach ที่ส่งไป
}
```

ใน K2 SmartObject สามารถเช็คได้จาก:
- `ProcessData.varFileCount` = จำนวน bookFile
- `ProcessData.varAttachCount` = จำนวน bookAttach

---

## 🔗 เอกสารอ้างอิง

- **K2 SmartObject Integration Guide**: `RefDocuments/K2_SMARTOBJECT_INTEGRATION_GUIDE.md`
- **API Implementation Details**: `RefDocuments/API_CREATE_IMPLEMENTATION.md`
- **Request Body Examples**: `RefDocuments/API_CREATE_TEST_EXAMPLES.md`
- **Model Definitions**: `Models/BookModels.cs`
  - `BookFile` class (line 120-132)
  - `BookAttachment` class (line 103-113)
  - `CreateBookApprovedSimpleRequest` class (line 190-240)

---

## 📞 Support

หากมีปัญหาหรือข้อสงสัย:
1. ตรวจสอบ JSON format ด้วย JSON validator
2. เช็ค Base64 encoding ว่าถูกต้อง
3. ดู API Response message เพื่อหาสาเหตุข้อผิดพลาด
4. ตรวจสอบ K2 SmartObject configuration ว่าตรงกับ documentation

---

**Document Version**: 1.0  
**Last Updated**: November 2, 2025  
**Status**: ✅ Production Ready
