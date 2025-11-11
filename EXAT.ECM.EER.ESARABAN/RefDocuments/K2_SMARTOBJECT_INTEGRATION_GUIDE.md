# K2 SmartObject Integration Guide
## คู่มือขั้นตอนการนำ API ไปใช้งานกับ K2 SmartObject

**Version:** 2.0 (Updated)  
**Date:** November 1, 2025  
**API Version:** .NET 8 Web API  
**K2 Version:** K2 Five / K2 Cloud

---

## 📋 สารบัญ

1. [ภาพรวม](#1-ภาพรวม)
2. [ข้อกำหนดเบื้องต้น](#2-ข้อกำหนดเบื้องต้น)
3. [ขั้นตอนที่ 1: เตรียม API Server](#3-ขั้นตอนที่-1-เตรียม-api-server)
4. [ขั้นตอนที่ 2: สร้าง K2 Service Instance](#4-ขั้นตอนที่-2-สร้าง-k2-service-instance)
5. [ขั้นตอนที่ 3: Generate SmartObjects](#5-ขั้นตอนที่-3-generate-smartobjects)
6. [ขั้นตอนที่ 4: กำหนด Properties และ Methods](#6-ขั้นตอนที่-4-กำหนด-properties-และ-methods)
7. [ขั้นตอนที่ 5: ทดสอบ SmartObject](#7-ขั้นตอนที่-5-ทดสอบ-smartobject)
8. [ขั้นตอนที่ 6: สร้าง K2 SmartForms](#8-ขั้นตอนที่-6-สร้าง-k2-smartforms)
9. [ขั้นตอนที่ 7: สร้าง K2 Workflow](#9-ขั้นตอนที่-7-สร้าง-k2-workflow)
10. [แนะนำการใช้งาน API แต่ละ Endpoint](#10-แนะนำการใช้งาน-api-แต่ละ-endpoint)
11. [Troubleshooting](#11-troubleshooting)

---

## 1. ภาพรวม

API นี้ถูกออกแบบให้ **100% K2 Compatible** โดยเฉพาะ มีการ return response แบบ **Direct Format** (ไม่มี wrapper) ทำให้ K2 SmartObject สามารถ map properties ได้โดยตรง

### ✅ K2 Compatibility Features
- ✅ **Direct Response Format**: ไม่มี `ApiResponse<T>` wrapper
- ✅ **Snake_case JSON Naming**: `book_code`, `file_count` (ยกเว้น Query endpoints)
- ✅ **Top-level Status Fields**: `status`, `statusCode`, `message`
- ✅ **Flat Structure**: Workflow endpoints ไม่มี nested objects
- ✅ **OpenAPI 2.0 (Swagger JSON)**: K2 อ่าน Swagger ได้โดยตรง
- ✅ **Tested 14/14 Endpoints**: ทุก endpoint ผ่านการทดสอบแล้ว

### 📊 API Endpoints Summary

| Category | Endpoints | K2 Compatible | Response Fields | Status |
|----------|-----------|---------------|-----------------|--------|
| **Create (K2 Simple)** | 3 | ✅ Yes | 17 fields | ✅ Tested |
| **Create (Full Format)** | 4 | ✅ Yes | 22+ fields | ✅ Tested |
| **Operations** | 2 | ✅ Yes | 9-13 fields | ✅ Tested |
| **Workflow (Combined)** | 3 | ✅ Yes | 22 fields (flat) | ✅ Tested |
| **Query** | 2 | ✅ Yes | Array of 9 fields | ✅ Tested |
| **Database (Admin)** | 4 | ⚠️ Admin Only | N/A | Not for K2 |
| **Schema (Admin)** | 3 | ⚠️ Admin Only | N/A | Not for K2 |

**Total K2-Ready Endpoints**: **14 endpoints**

---

## 2. ข้อกำหนดเบื้องต้น

### Software Requirements
- ✅ **K2 Five** (Version 5.x) หรือ **K2 Cloud**
- ✅ **K2 Designer** (for SmartObjects)
- ✅ **K2 Management Console** (for Service Instance)
- ✅ **API Server** (.NET 8 Runtime)
- ✅ **Network Access**: K2 Server → API Server

### User Permissions
- ✅ K2 Designer User (สำหรับสร้าง SmartObjects)
- ✅ K2 Workflow Designer (สำหรับสร้าง Workflows)
- ✅ K2 SmartForms Designer (สำหรับสร้าง Forms)

### API Server Information
- **Base URL (DEV)**: `http://localhost:5152/api`
- **Base URL (UAT)**: `http://api-uat.exat.co.th/esrb-external-api/api`
- **Base URL (PROD)**: `http://api.exat.co.th/esrb-external-api/api`
- **Swagger JSON**: `{BaseURL}/../swagger/v1/swagger.json`
- **OpenAPI Version**: 2.0 (K2 Compatible)
- **CORS**: Enabled (ต้อง allow K2 Server IP)

---

## 3. ขั้นตอนที่ 1: เตรียม API Server

### 3.1 Deploy API to Server

```powershell
# Build API
dotnet build K2RestApi.csproj --configuration Release

# Publish to folder
dotnet publish K2RestApi.csproj -c Release -o ./publish

# Deploy to IIS/Server
# Copy ./publish/* to production server
```

### 3.2 Configure CORS for K2 Server

แก้ไข `appsettings.json` หรือ `appsettings.Production.json`:

```json
{
  "AllowedOrigins": [
    "http://k2-server",
    "https://k2-server",
    "http://k2-server.exat.co.th",
    "https://k2-server.exat.co.th"
  ],
  "ESarabanApi": {
    "BaseUrl": "http://api.exat.co.th/esrb-external-api",
    "Timeout": 30
  }
}
```

### 3.3 ทดสอบ API จาก K2 Server

```powershell
# จาก K2 Server - Test API Connection
Invoke-RestMethod -Uri "http://api-server:5152/swagger/v1/swagger.json" `
  -Method Get | ConvertTo-Json -Depth 10

# Test Sample Endpoint
$body = @{
  user_ad = "EXAT\K2USER"
  book_subject = "ทดสอบจาก K2 Server"
  book_to = "สผว."
  registrationbook_id = "REG001"
  booktype_id = 93
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://api-server:5152/api/books/create/approved/simple" `
  -Method Post -Body $body -ContentType "application/json; charset=utf-8"
```

### 3.4 เปิด Firewall (ถ้าจำเป็น)

```powershell
# Allow K2 Server IP to access API
New-NetFirewallRule -DisplayName "K2 API Access" `
  -Direction Inbound -LocalPort 5152 -Protocol TCP -Action Allow `
  -RemoteAddress [K2-Server-IP]
```

---

## 4. ขั้นตอนที่ 2: สร้าง K2 Service Instance

### 4.1 เปิด K2 Management Console

1. เปิดเว็บเบราว์เซอร์
2. ไปที่ `http://k2-server/Management`
3. Login ด้วย K2 Admin Account

### 4.2 สร้าง Service Instance ใหม่

1. ไปที่ **Integration** → **Service Instances**
2. คลิก **New Service Instance**
3. เลือก **REST** Service Descriptor
4. กรอกข้อมูล:

```
Service Name: eSaraban Books API
Display Name: eSaraban Books API (K2)
Description: K2 Compatible REST API for eSaraban Books Management
Base URL: http://api-server:5152
```

### 4.3 กำหนด Service Configuration

**Configuration Settings**:

| Setting | Value | Note |
|---------|-------|------|
| **Service Type** | REST | REST Service |
| **OpenAPI Version** | 2.0 | K2 Compatible |
| **Base URL** | `http://api-server:5152` | API Server Address |
| **Swagger URL** | `{BaseURL}/swagger/v1/swagger.json` | Auto-discover |
| **Authentication** | Anonymous | หรือ Basic/OAuth |
| **Timeout** | 60 seconds | เพิ่มเป็น 120 สำหรับ Workflow |

### 4.4 Authentication Configuration

**Option 1: Anonymous (ไม่ต้อง login)**
```json
{
  "AuthenticationType": "None"
}
```

**Option 2: Basic Authentication**
```json
{
  "AuthenticationType": "Basic",
  "Username": "api_user",
  "Password": "api_password"
}
```

**Option 3: OAuth 2.0** (แนะนำสำหรับ Production)
```json
{
  "AuthenticationType": "OAuth2.0",
  "TokenUrl": "http://auth-server/token",
  "ClientId": "k2-client",
  "ClientSecret": "secret",
  "Scope": "api.read api.write"
}
```

### 4.5 Import Swagger Definition

1. คลิก **Import from Swagger/OpenAPI**
2. ใส่ URL: `http://api-server:5152/swagger/v1/swagger.json`
3. คลิก **Load Definition**
4. K2 จะอ่าน OpenAPI 2.0 และแสดง Endpoints ทั้งหมด
5. ตรวจสอบว่า K2 อ่าน definition ได้ถูกต้อง

---

## 5. ขั้นตอนที่ 3: Generate SmartObjects

### 5.1 เลือก Endpoints สำหรับ SmartObject

**แนะนำเริ่มจาก endpoints เหล่านี้**:

#### ✅ Group 1: Books - Create (K2 Compatible) - 3 Endpoints
```
✓ POST /api/books/create/approved/simple
✓ POST /api/books/create/non-compliant/simple
✓ POST /api/books/create/under-construction/simple
```
**Use Case**: สร้างเอกสารแบบ Simple (17 fields)

#### ✅ Group 2: Books - Operations - 2 Endpoints
```
✓ GET /api/books/generate-code
✓ POST /api/books/transfer
```
**Use Case**: Generate เลขที่เอกสาร และโอนย้าย Book

#### ✅ Group 3: Books - Workflow (Combined) - 3 Endpoints
```
✓ POST /api/books/workflow/approved
✓ POST /api/books/workflow/non-compliant
✓ POST /api/books/workflow/under-construction
```
**Use Case**: ดำเนินการ 3 ขั้นตอนพร้อมกัน (Create + Generate + Transfer)

#### ✅ Group 4: Books - Query - 2 Endpoints
```
✓ GET /api/books/final-orgs/by-action
✓ GET /api/books/final-orgs/by-action/no-alert
```
**Use Case**: ดึงข้อมูลองค์กรปลายทาง

### 5.2 Generate SmartObjects

1. เลือก Endpoints ที่ต้องการ (แนะนำเลือกทั้ง 10 endpoints)
2. คลิก **Generate SmartObjects**
3. K2 จะสร้าง SmartObject ให้อัตโนมัติจาก Swagger Definition
4. ตั้งชื่อ SmartObjects:
   - `ESarabanBook_CreateApproved_Simple`
   - `ESarabanBook_CreateNonCompliant_Simple`
   - `ESarabanBook_CreateUnderConstruction_Simple`
   - `ESarabanBook_GenerateCode`
   - `ESarabanBook_Transfer`
   - `ESarabanBook_WorkflowApproved`
   - `ESarabanBook_WorkflowNonCompliant`
   - `ESarabanBook_WorkflowUnderConstruction`
   - `ESarabanBook_GetFinalOrgs`
   - `ESarabanBook_GetFinalOrgs_NoAlert`
5. คลิก **Finish**

### 5.3 ตรวจสอบ SmartObjects ที่สร้าง

1. เปิด **K2 Designer**
2. ไปที่ **SmartObjects**
3. ตรวจสอบว่า SmartObjects ถูกสร้างครบ 10 objects
4. ตรวจสอบ Properties แต่ละ SmartObject

---

## 6. ขั้นตอนที่ 4: กำหนด Properties และ Methods

### 6.1 ตัวอย่าง SmartObject Properties

#### SmartObject: **ESarabanBook_CreateApproved_Simple**

**Method**: Create (POST)

**Input Properties** (17 fields รับได้ทั้งหมด):

| Property | Type | Required | Description | K2 Property Name |
|----------|------|----------|-------------|------------------|
| `book_subject` | String | ✅ Yes | เรื่อง | BookSubject |
| `book_to` | String | ✅ Yes | ถึง | BookTo |
| `booktype_id` | Number | ✅ Yes | รหัสประเภท (93) | BookTypeId |
| `parent_bookid` | String | ⚪ No | Parent Book | ParentBookId |
| `parent_orgid` | String | ⚪ No | Parent Org | ParentOrgId |
| `parent_positionname` | String | ⚪ No | Position | ParentPosition |

**Return Properties** (17 fields ทั้งหมด):

| Property | Type | K2 Access | Example Value |
|----------|------|-----------|---------------|
| `status` | String | `response.status` | "S" |
| `statusCode` | String | `response.statusCode` | "200" |
| `message` | String | `response.message` | "Success: generate book." |
| `book_id` | String | `response.book_id` | "658919D9..." |
| `book_code` | String | `response.book_code` | "APV-20251101-6984" |
| `book_subject` | String | `response.book_subject` | "ทดสอบ..." |
| `book_to` | String | `response.book_to` | "สผว." |
| `booktype_id` | Number | `response.booktype_id` | 93 |
| `registrationbook_id` | String | `response.registrationbook_id` | "REG001" |
| `file_count` | Number | `response.file_count` | 2 |
| `attach_count` | Number | `response.attach_count` | 3 |
| `created_by` | String | `response.created_by` | "EXAT\\ECMUSR07" |
| `created_date` | DateTime | `response.created_date` | ISO 8601 |

### 6.2 ตรวจสอบ Property Types

**ตรวจสอบให้แน่ใจว่า K2 map types ถูกต้อง**:

| K2 Property Type | API JSON Type | Note |
|------------------|---------------|------|
| Text | String | ใช้สำหรับ text, GUID |
| Number | Integer/Number | ใช้สำหรับตัวเลข |
| Date Time | String (ISO 8601) | K2 convert อัตโนมัติ |
| Boolean | Boolean | true/false |
| Memo | Long Text | ใช้สำหรับ description |

### 6.3 กำหนด Method Signatures

K2 SmartObject จะมี Methods ดังนี้ (auto-generated จาก Swagger):

#### Method 1: **Create** (สำหรับ POST endpoints)
```
Input: book_subject, book_to, booktype_id, parent_bookid, parent_orgid, parent_positionname
Output: All 17 return fields
```

#### Method 2: **Load** (สำหรับ GET endpoints)
```
Input: user_ad, book_id (query parameters)
Output: Generated response fields
```

#### Method 3: **Execute** (สำหรับ Workflow endpoints)
```
Input: Full request body
Output: All workflow fields (22 fields flat structure)
```

---

## 7. ขั้นตอนที่ 5: ทดสอบ SmartObject

### 7.1 ทดสอบใน K2 Designer

1. เปิด SmartObject: **ESarabanBook_CreateApproved_Simple**
2. คลิกขวาที่ Method **Create**
3. เลือก **Execute Method**
4. กรอกข้อมูล Test:

```json
{
  "book_subject": "ทดสอบจาก K2 SmartObject",
  "book_to": "สผว.",
  "booktype_id": 93,
  "registrationbook_id": "REG-K2-001"
}
```

5. คลิก **Execute**
6. ตรวจสอบ Response:

```
✅ status: "S"
✅ statusCode: "200"
✅ book_code: "APV-20251101-xxxx"
✅ book_id: "guid-here"
✅ message: "Success: generate book."
```

### 7.2 ทดสอบ Property Access ใน SmartForm

**สร้าง Test Form**:
1. สร้าง Form ใหม่: `Test_CreateBook_Form`
2. เพิ่ม Controls:
   - Text Box: `BookSubject` (Input)
   - Text Box: `BookTo` (Input)
   - Button: `CreateBook` (Action)
   - Data Label: `ResultBookCode` (Output)
   - Data Label: `ResultBookId` (Output)
   - Data Label: `ResultStatus` (Output)

3. Configure Data Binding:
```
Data Source: ESarabanBook_CreateApproved_Simple
Method: Create

Input Mapping:
  BookSubject → book_subject
  BookTo → book_to
  (booktype_id = 93 - Fixed value)

Output Mapping:
  ResultBookCode ← book_code
  ResultBookId ← book_id
  ResultStatus ← message
```

4. Button Rules:
```
WHEN: User clicks "CreateBook"
THEN:
  Execute SmartObject Method: Create
  IF status = "S" THEN
    Show: Result section
    Display: "Book created: {book_code}"
  ELSE
    Display Error: {message}
```

### 7.3 ทดสอบ Workflow Endpoint (Flat Structure)

**⚠️ IMPORTANT: Workflow endpoints ใช้ Flat Structure**

```javascript
// ❌ WRONG (Nested objects - เก่า)
var bookCode = SmartObject.create_response.book_code;
var generatedCode = SmartObject.generate_response.generated_code;

// ✅ CORRECT (Flat structure - ใหม่)
var bookCode = SmartObject.book_code;
var generatedCode = SmartObject.generated_code;
var transferId = SmartObject.transfer_id;
var workflowType = SmartObject.workflow_type;
var workflowCompleted = SmartObject.workflow_completed;
```

### 7.4 ทดสอบ Query Endpoint (Array Response)

```javascript
// Access organizations array
var orgs = SmartObject_GetFinalOrgs.books;
var orgCount = orgs.length;

// Loop through array
for (var i = 0; i < orgCount; i++) {
    var org = orgs[i];
    var runningNo = org.running_no;
    var sendOrg = org.send_org_nameth; // ⚠️ No underscore before "th"
    var receiveOrg = org.receive_org_nameth;
    var status = org.status_nameth;
}
```

---

## 8. ขั้นตอนที่ 6: สร้าง K2 SmartForms

### 8.1 Form สำหรับ Create Book (Approved)

**Form Name**: `CreateBook_Approved_Form`

**Form Layout**:

```
┌──────────────────────────────────────────────────────┐
│            สร้างเอกสารอนุมัติ/เข้าสู่หลักเกณ์         │
├──────────────────────────────────────────────────────┤
│                                                      │
│  เรื่อง: [______________________________________]   │
│                                                      │
│  ถึง:    [______________________________________]   │
│                                                      │
│  รหัสทะเบียน: [_____________________________]      │
│                                                      │
│  ☐ Advanced Options                                  │
│    Parent Book ID:   [_________________________]    │
│    Parent Org ID:    [_________________________]    │
│    Parent Position:  [_________________________]    │
│                                                      │
│  📎 แนบไฟล์ (Optional):                              │
│    [File Upload Control - bookFile]                 │
│                                                      │
│  📎 เอกสารอ้างอิง (Optional):                        │
│    [File Upload Control - bookAttach]               │
│                                                      │
│                   [สร้างเอกสาร]  [ยกเลิก]           │
│                                                      │
│  ─────────────────────────────────────────────────  │
│  📊 ผลลัพธ์ (แสดงหลังสร้างเสร็จ):                    │
│                                                      │
│  ✅ สถานะ: [สำเร็จ]                                  │
│  📄 เลขที่: [APV-20251101-6984]                      │
│  🆔 Book ID: [658919D9...]                           │
│  📁 ไฟล์: [2]  📎 แนบ: [3]                           │
│  👤 สร้างโดย: [EXAT\ECMUSR07]                        │
│  📅 วันที่: [01/11/2025 23:19:20]                    │
│                                                      │
└──────────────────────────────────────────────────────┘
```

### 8.2 Configure SmartObject Data Source

**Data Source Configuration**:
```
SmartObject: ESarabanBook_CreateApproved_Simple
Method: Create

Property Mappings:
  Input (Form → SmartObject):
    TextBox_BookSubject.Value → book_subject
    TextBox_BookTo.Value → book_to
    TextBox_RegBookId.Value → registrationbook_id
    TextBox_ParentBookId.Value → parent_bookid (if shown)
    TextBox_ParentOrgId.Value → parent_orgid (if shown)
    TextBox_ParentPosition.Value → parent_positionname (if shown)
    [Fixed Value] 93 → booktype_id

  Output (SmartObject → Form):
    book_code → DataLabel_BookCode.Text
    book_id → DataLabel_BookId.Text
    message → DataLabel_Status.Text
    file_count → DataLabel_FileCount.Text
    attach_count → DataLabel_AttachCount.Text
    created_by → DataLabel_CreatedBy.Text
    created_date → DataLabel_CreatedDate.Text
```

### 8.3 Form Rules

**Rule 1: Validation**
```
WHEN: Form is initializing
THEN:
  book_subject is Required
  book_to is Required
  registrationbook_id is Required
  booktype_id must be 93 (fixed)
```

**Rule 2: Create Button Click**
```
WHEN: User clicks "สร้างเอกสาร" button
THEN:
  1. Validate all required fields
  2. Execute SmartObject: ESarabanBook_CreateApproved_Simple.Create
  3. IF SmartObject.status = "S" THEN
       Show: Result Section
       Hide: Input Section
       Display: "เอกสารถูกสร้างเรียบร้อย: {book_code}"
     ELSE
       Display Error: SmartObject.message
       Keep: Input Section visible
```

**Rule 3: Cancel Button**
```
WHEN: User clicks "ยกเลิก"
THEN:
  Close Form (without saving)
```

### 8.4 Form สำหรับ Workflow (Combined - 3 Steps)

**Form Name**: `Workflow_Approved_Complete_Form`

**Form Layout**:

```
┌──────────────────────────────────────────────────────┐
│          Workflow: สร้าง + Generate + โอนย้าย         │
├──────────────────────────────────────────────────────┤
│                                                      │
│  เรื่อง: [______________________________________]   │
│  ถึง:    [______________________________________]   │
│                                                      │
│  โอนจาก: [J10100 กองวิศวกรรม ▼]                     │
│  โอนไป:  [J10200 กองแผนงาน ▼]                       │
│                                                      │
│  เหตุผล: [______________________________________]    │
│                                                      │
│                [ดำเนินการ Workflow]  [ยกเลิก]       │
│                                                      │
│  ─────────────────────────────────────────────────  │
│  📊 ผลการดำเนินงาน Workflow:                         │
│                                                      │
│  ✅ Workflow Type: [approved_workflow]               │
│  ✅ Completed: [true]                                │
│  👤 Executed By: [EXAT\ECMUSR07]                     │
│                                                      │
│  📝 STEP 1: สร้างเอกสาร                              │
│     Book Code: [APV-20251101-3693]                   │
│     Book ID: [B8C64A45...]                           │
│     Message: [Success: generate book.]               │
│                                                      │
│  🔢 STEP 2: Generate Code                            │
│     Generated Code: [DOC-20251101-92998]             │
│     Code Type: [DOCUMENT]                            │
│     Message: [Code generated successfully]           │
│                                                      │
│  🔄 STEP 3: Transfer                                 │
│     Transfer ID: [c870f8b7-98e6-44bf...]             │
│     Status: [COMPLETED]                              │
│     From → To: [J10100 → J10200]                     │
│     Message: [Book transferred successfully]         │
│                                                      │
│  📄 Overall: [All 3 steps completed successfully]    │
│                                                      │
└──────────────────────────────────────────────────────┘
```

**SmartObject Data Source**:
```
SmartObject: ESarabanBook_WorkflowApproved
Method: ExecuteWorkflow

Input Mapping:
  TextBox_BookSubject → book_subject
  TextBox_BookTo → book_to
  DropDown_OriginalOrg → original_org_code
  DropDown_DestOrg → destination_org_code
  TextArea_Reason → transfer_reason

Output Mapping (Flat Structure - 22 fields):
  workflow_type → DataLabel_WorkflowType
  workflow_completed → DataLabel_Completed
  executed_by → DataLabel_ExecutedBy
  book_code → DataLabel_BookCode
  book_id → DataLabel_BookId
  create_message → DataLabel_CreateMsg
  generated_code → DataLabel_GeneratedCode
  code_type → DataLabel_CodeType
  generate_message → DataLabel_GenerateMsg
  transfer_id → DataLabel_TransferId
  transfer_status → DataLabel_TransferStatus
  transfer_message → DataLabel_TransferMsg
  overall_message → DataLabel_OverallMsg
```

---

## 9. ขั้นตอนที่ 7: สร้าง K2 Workflow

### 9.1 Simple Workflow: Create and Notify

**Workflow Name**: `CreateBook_Notify_Workflow`

**Workflow Diagram**:

```
[Start]
   ↓
[Create Book]
(SmartObject: CreateApproved_Simple)
   ↓
[Decision: Success?]
   ├─ Yes → [Send Success Email]
   │           ↓
   │        [End Success]
   │
   └─ No → [Send Error Email]
               ↓
            [End Error]
```

**Step Details**:

**Step 1: Create Book**
```
Activity Type: SmartObject Activity
SmartObject: ESarabanBook_CreateApproved_Simple
Method: Create

Input Data:
  book_subject = Process Data: BookSubject
  book_to = Process Data: BookTo
  booktype_id = 93 (fixed)
  registrationbook_id = Process Data: RegBookId

Output Data:
  Process Data Field: CreatedBookCode = book_code
  Process Data Field: CreatedBookId = book_id
  Process Data Field: CreationStatus = status
  Process Data Field: CreationMessage = message
```

**Step 2: Decision**
```
Condition: CreationStatus = "S"
  IF True → Go to: Send Success Email
  IF False → Go to: Send Error Email
```

**Step 3: Send Success Email**
```
Activity Type: Email Activity
To: Process Data: RequesterEmail
Subject: "เอกสารถูกสร้างเรียบร้อย"
Body:
  "เรียน คุณ {RequesterName},
  
   เอกสารของท่านถูกสร้างเรียบร้อยแล้ว
   
   เลขที่เอกสาร: {CreatedBookCode}
   Book ID: {CreatedBookId}
   เรื่อง: {BookSubject}
   
   ขอบคุณครับ"
```

### 9.2 Advanced Workflow: Complete Book Processing

**Workflow Name**: `CompleteBook_Processing_Workflow`

**Workflow Diagram**:

```
[Start]
   ↓
[User Task: Enter Book Info]
   ↓
[Execute Workflow Approved]
(SmartObject: WorkflowApproved)
   ↓
[Decision: All Steps OK?]
   ├─ Yes → [Query Final Orgs]
   │        (SmartObject: GetFinalOrgs)
   │           ↓
   │        [Loop: Send to Each Org]
   │           ↓
   │        [End Success]
   │
   └─ No → [Log Error]
               ↓
            [Notify Admin]
               ↓
            [End Error]
```

**Process Data Fields**:

| Field Name | Type | Description |
|------------|------|-------------|
| BookSubject | String | เรื่อง |
| BookTo | String | ถึง |
| OriginalOrg | String | องค์กรต้นทาง |
| DestOrg | String | องค์กรปลายทาง |
| WorkflowCompleted | Boolean | Workflow สำเร็จ |
| BookCode | String | เลขที่เอกสาร |
| BookId | String | Book ID (GUID) |
| GeneratedCode | String | เลขเอกสารที่ Generate |
| TransferId | String | Transfer ID |
| FinalOrgs | Array | องค์กรปลายทาง |

**Workflow Activity Configuration**:

**Activity 1: Execute Workflow**
```
SmartObject: ESarabanBook_WorkflowApproved
Method: ExecuteWorkflow

Input:
  book_subject = BookSubject
  book_to = BookTo
  booktype_id = 93
  original_org_code = OriginalOrg
  destination_org_code = DestOrg

Output (Flat Structure - access directly):
  BookCode = book_code
  BookId = book_id
  GeneratedCode = generated_code
  TransferId = transfer_id
  WorkflowCompleted = workflow_completed
  OverallMessage = overall_message
```

**Activity 2: Query Final Orgs**
```
SmartObject: ESarabanBook_GetFinalOrgs
Method: Load

Input:
  user_ad = "EXAT\\WORKFLOW"
  book_id = BookId (from previous step)

Output:
  FinalOrgs = books (array)
```

**Activity 3: Loop Through Organizations**
```
For Each: org in FinalOrgs
  Send Email:
    To: org.receive_org_nameth (email lookup)
    Subject: "เอกสารใหม่: {BookCode}"
    Body:
      "มีเอกสารใหม่ส่งถึงหน่วยงานของท่าน
       
       เลขที่: {BookCode}
       จาก: {org.send_org_nameth}
       สถานะ: {org.status_nameth}
       วันที่: {org.send_date}"
Next
```

---

## 10. แนะนำการใช้งาน API แต่ละ Endpoint

### 10.1 Books - Create (K2 Compatible) - 3 Endpoints

#### Endpoint: `POST /api/books/create/approved/simple`

**Use Case**: สร้างเอกสารอนุมัติ/เข้าสู่หลักเกณ์ (Simple - 17 fields)

**K2 SmartObject**: `ESarabanBook_CreateApproved_Simple`

**K2 Workflow Variable Mapping**:
```javascript
// After SmartObject execution
var status = SmartObject.status; // "S" or "E"
var bookCode = SmartObject.book_code; // "APV-20251101-6984"
var bookId = SmartObject.book_id; // GUID
var message = SmartObject.message;

if (status === "S") {
    // Success
    ProcessDataField_BookCode = bookCode;
    ProcessDataField_BookId = bookId;
    // Continue workflow...
} else {
    // Error handling
    LogError(message);
    SendEmailToAdmin("Book creation failed: " + message);
}
```

**K2 SmartForm Property Binding**:
```
Form Controls → SmartObject Properties:
  - TextBox_Subject → book_subject (Input)
  - TextBox_To → book_to (Input)
  - DataLabel_BookCode ← book_code (Output)
  - DataLabel_Status ← message (Output)
```

---

### 10.2 Books - Operations - 2 Endpoints

#### Endpoint 1: `GET /api/books/generate-code`

**Use Case**: Generate เลขที่เอกสาร

**K2 SmartObject**: `ESarabanBook_GenerateCode`

**K2 Workflow Usage**:
```javascript
// Step 1: Create book first
SmartObject_CreateBook.Execute();
var bookId = SmartObject_CreateBook.book_id;

// Step 2: Generate code
SmartObject_GenerateCode.user_ad = "EXAT\\ECMUSR07";
SmartObject_GenerateCode.book_id = bookId;
SmartObject_GenerateCode.Execute();

// Step 3: Get generated code
var generatedCode = SmartObject_GenerateCode.generated_code; // "DOC-20251101-95178"
var codeType = SmartObject_GenerateCode.code_type; // "DOCUMENT"

ProcessDataField_DocumentCode = generatedCode;
```

#### Endpoint 2: `POST /api/books/transfer`

**Use Case**: โอนย้าย Book ระหว่างองค์กร

**K2 SmartForm Example**:
```
Form: Transfer Book

Controls:
  - Data Label: Current Book (book_code) - Display only
  - Drop Down: Destination Org - Required
  - Text Area: Transfer Reason - Required
  - Text Area: Transfer Note - Optional
  - Button: Transfer Book

When Transfer Button Clicked:
  Execute SmartObject: ESarabanBook_Transfer
  Input:
    user_ad = K2.System.UserName
    book_id = Form.BookId
    original_org_code = "J10100"
    destination_org_code = DropDown_DestOrg.Value
    transfer_reason = TextArea_Reason.Value
    transfer_note = TextArea_Note.Value
    
  IF SmartObject.status = "S" THEN
    Display: "Transfer successful: {transfer_id}"
    Display: "Status: {transfer_status}"
    Close Form
  ELSE
    Display Error: {message}
```

---

### 10.3 Books - Workflow (Combined) - 3 Endpoints

**⚠️ CRITICAL: Workflow endpoints use FLAT STRUCTURE (22 fields)**

#### Endpoint: `POST /api/books/workflow/approved`

**Use Case**: ดำเนินการครบ 3 ขั้นตอน (Create + Generate + Transfer)

**K2 SmartObject**: `ESarabanBook_WorkflowApproved`

**K2 Property Access (Flat Structure)**:
```javascript
// ✅ CORRECT - Direct access (Flat structure)
var workflowType = SmartObject.workflow_type; // "approved_workflow"
var executedBy = SmartObject.executed_by; // "EXAT\\ECMUSR07"
var workflowCompleted = SmartObject.workflow_completed; // true
var overallMessage = SmartObject.overall_message; // "All 3 steps completed..."

// Create step fields (direct access)
var bookCode = SmartObject.book_code; // "APV-20251101-3693"
var bookId = SmartObject.book_id;
var createMessage = SmartObject.create_message;

// Generate step fields (direct access)
var generatedCode = SmartObject.generated_code; // "DOC-20251101-92998"
var codeType = SmartObject.code_type; // "DOCUMENT"
var generatedDate = SmartObject.generated_date;
var generateMessage = SmartObject.generate_message;

// Transfer step fields (direct access)
var transferId = SmartObject.transfer_id; // "c870f8b7-98e6..."
var transferStatus = SmartObject.transfer_status; // "COMPLETED"
var originalOrg = SmartObject.original_org_code;
var destOrg = SmartObject.destination_org_code;
var transferredDate = SmartObject.transferred_date;
var transferMessage = SmartObject.transfer_message;

// ❌ WRONG - Nested access (OLD format - doesn't work)
// var bookCode = SmartObject.create_response.book_code; // ERROR!
// var generatedCode = SmartObject.generate_response.generated_code; // ERROR!
```

**K2 SmartForm - Display All Results**:
```
Result Panel (All 22 fields displayed):

📊 Workflow Summary:
   Workflow Type: [workflow_type]
   Executed By: [executed_by]
   Completed: [workflow_completed]
   Overall Message: [overall_message]

📝 Step 1 - Create Book:
   Book Code: [book_code]
   Book ID: [book_id]
   Message: [create_message]

🔢 Step 2 - Generate Code:
   Generated Code: [generated_code]
   Code Type: [code_type]
   Generated Date: [generated_date]
   Message: [generate_message]

🔄 Step 3 - Transfer:
   Transfer ID: [transfer_id]
   Status: [transfer_status]
   From: [original_org_code]
   To: [destination_org_code]
   Date: [transferred_date]
   Message: [transfer_message]
```

---

### 10.4 Books - Query - 2 Endpoints

#### Endpoint 1: `GET /api/books/final-orgs/by-action` (with Alert)

**Use Case**: ดึงข้อมูลองค์กรปลายทาง + ส่ง Alert

**K2 SmartObject**: `ESarabanBook_GetFinalOrgs`

**K2 Workflow - Array Access**:
```javascript
// Execute SmartObject
SmartObject_GetFinalOrgs.user_ad = "EXAT\\WORKFLOW";
SmartObject_GetFinalOrgs.book_id = ProcessData_BookId;
SmartObject_GetFinalOrgs.Execute();

// Access array response
var organizations = SmartObject_GetFinalOrgs.books; // Array
var orgCount = organizations.length; // 3

// Loop through organizations
for (var i = 0; i < orgCount; i++) {
    var org = organizations[i];
    
    // ⚠️ Property names: send_org_nameth, receive_org_nameth, status_nameth
    // (no underscore before "th" - different from other endpoints)
    
    var runningNo = org.running_no; // 1, 2, 3...
    var bookId = org.book_id; // GUID
    var sendOrgName = org.send_org_nameth; // "กองกรรมสิทธิ์ที่ดิน"
    var sendDate = org.send_date; // "01-NOV-25"
    var receiveOrgName = org.receive_org_nameth; // "J10000 ฝ่ายกรรมสิทธิ์ที่ดิน"
    var receiveCode = org.receive_code; // null if pending
    var receiveDate = org.receive_date; // null if not received
    var statusName = org.status_nameth; // "รอดำเนินการรับหนังสือ"
    var comment = org.receive_comment; // null if none
    
    // Send notification to organization
    SendEmailToOrg(receiveOrgName, sendOrgName, statusName);
}
```

**K2 SmartForm - Display Organizations List**:
```
Form: View Final Organizations

List View Control:
  Data Source: ESarabanBook_GetFinalOrgs.books
  
  Columns:
    [#] Running No: {running_no}
    [From] Send Org: {send_org_nameth}
    [To] Receive Org: {receive_org_nameth}
    [Status] Status: {status_nameth}
    [Date] Send Date: {send_date}
    [Date] Receive Date: {receive_date}
    [Code] Receive Code: {receive_code}
    [Note] Comment: {receive_comment}

When Form Loads:
  Execute SmartObject: GetFinalOrgs
  Display: "Alert sent to {orgCount} organizations"
  Bind: organizations array to List View
```

#### Endpoint 2: `GET /api/books/final-orgs/by-action/no-alert` (Silent Query)

**Use Case**: ดึงข้อมูลองค์กรปลายทาง แบบ Silent (ไม่ส่ง Alert)

**K2 Workflow - Preview Before Action**:
```
Workflow: Document Routing with Confirmation

Step 1: Get Organizations (No Alert)
  SmartObject: ESarabanBook_GetFinalOrgs_NoAlert
  Input: user_ad, book_id
  Output: organizations list (no alert sent)

Step 2: Display to User
  Activity: User Task
  Form: Shows organization list for confirmation
  Question: "Send document to these {orgCount} organizations?"
  Buttons: [Confirm] [Cancel]

Step 3: Decision
  IF User clicks "Confirm" THEN
    → Execute: Transfer Book SmartObject
    → Execute: Get Final Orgs (WITH Alert) - Send notifications
    → Send Email: Confirmation to user
  ELSE
    → Cancel workflow
    → Log: "User cancelled routing"
```

---

## 11. Troubleshooting

### Issue 1: SmartObject ไม่สามารถ map properties ได้

**Symptom**: Error "Property '{property_name}' not found"

**Causes**:
- K2 ไม่ได้อ่าน Swagger definition ถูกต้อง
- API return format ไม่ตรงกับ K2 คาดหวัง
- Property name ไม่ตรงกัน (case-sensitive)

**Solutions**:
1. ✅ ตรวจสอบ Swagger JSON:
```powershell
Invoke-RestMethod -Uri "http://api-server:5152/swagger/v1/swagger.json" | ConvertTo-Json -Depth 10
```

2. ✅ ตรวจสอบว่า API return Direct Response (ไม่มี wrapper):
```json
// ✅ CORRECT (Direct format - K2 compatible)
{
  "status": "S",
  "statusCode": "200",
  "book_code": "APV-20251101-6984",
  "book_id": "658919D9..."
}

// ❌ WRONG (Wrapper format - K2 ไม่ map ได้)
{
  "data": {
    "status": "S",
    "book_code": "APV-20251101-6984"
  }
}
```

3. ✅ Refresh SmartObject Definition:
```
K2 Management Console
→ Service Instances
→ eSaraban Books API
→ Refresh Service
→ Re-generate SmartObjects
```

---

### Issue 2: CORS Error

**Symptom**: API return 403 Forbidden หรือ "No 'Access-Control-Allow-Origin' header"

**Solution**:

1. แก้ไข `appsettings.json`:
```json
{
  "AllowedOrigins": [
    "http://k2-server",
    "https://k2-server",
    "http://k2-server.exat.co.th",
    "https://k2-server.exat.co.th"
  ]
}
```

2. ตรวจสอบ `Program.cs`:
```csharp
// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Use CORS
app.UseCors();
```

3. Restart API Service

---

### Issue 3: Workflow Endpoints - Nested Object Error

**Symptom**: Error accessing `create_response.book_code`

**Cause**: Workflow endpoints ใช้ Flat Structure แล้ว (ไม่มี nested objects)

**Solution**:
```javascript
// ❌ WRONG (Old format - nested)
var bookCode = SmartObject.create_response.book_code; // ERROR!

// ✅ CORRECT (New format - flat)
var bookCode = SmartObject.book_code; // Direct access
```

**All Workflow Fields (Direct Access)**:
```
- workflow_type
- executed_by
- workflow_completed
- overall_message
- book_code, book_id, file_count, attach_count, create_message
- generated_code, code_type, generated_date, generate_message
- transfer_id, original_org_code, destination_org_code, 
  transfer_status, transferred_date, transfer_message
```

---

### Issue 4: Array Properties (Query Endpoints)

**Symptom**: K2 แสดง array ว่าง หรือ access ไม่ได้

**Solution**:

1. ตรวจสอบว่า API return array (ไม่ใช่ null):
```json
{
  "status": "S",
  "statusCode": "200",
  "books": [
    {
      "running_no": 1,
      "send_org_nameth": "กองกรรมสิทธิ์ที่ดิน",
      "receive_org_nameth": "J10000 ฝ่ายกรรมสิทธิ์ที่ดิน"
    }
  ]
}
```

2. ใน K2 Workflow ใช้ loop:
```javascript
var orgs = SmartObject.books;
if (orgs && orgs.length > 0) {
    for (var i = 0; i < orgs.length; i++) {
        var org = orgs[i];
        // Access properties: org.send_org_nameth, org.receive_org_nameth
    }
}
```

3. ใน SmartForm ใช้ List View หรือ Repeater control

---

### Issue 5: Property Naming - Query Endpoints

**Symptom**: Properties ไม่แสดงค่า (send_org_name_th, receive_org_name_th)

**Cause**: Query endpoints ใช้ property naming ต่างจาก endpoints อื่น

**Solution**:

✅ **ชื่อ Properties ที่ถูกต้อง** (ไม่มี underscore ก่อน "th"):
```javascript
// ✅ CORRECT
var sendOrg = org.send_org_nameth; // No underscore before "th"
var receiveOrg = org.receive_org_nameth;
var status = org.status_nameth;

// ❌ WRONG
var sendOrg = org.send_org_name_th; // With underscore - doesn't exist!
```

**All Query Endpoint Properties**:
```
running_no
book_id
send_org_nameth (⚠️ no underscore)
send_date
receive_org_nameth (⚠️ no underscore)
receive_code
receive_date
status_nameth (⚠️ no underscore)
receive_comment
```

---

### Issue 6: Authentication Failed

**Symptom**: API return 401 Unauthorized

**Solutions**:

1. **Anonymous (No Auth)**:
```
K2 Service Instance → Authentication → None
```

2. **Basic Authentication**:
```
K2 Service Instance → Authentication → Basic
Username: api_user
Password: ********
```

3. **OAuth 2.0**:
```
K2 Service Instance → Authentication → OAuth 2.0
Token URL: http://auth-server/token
Client ID: k2-client
Client Secret: ********
Scope: api.read api.write
```

---

### Issue 7: Timeout Error

**Symptom**: Request timeout (especially Workflow endpoints)

**Solutions**:

1. เพิ่ม Timeout ใน K2 SmartObject:
```
K2 Designer → SmartObject Properties
→ Timeout: 120 seconds (for Workflow endpoints)
→ Timeout: 60 seconds (for simple endpoints)
```

2. เพิ่ม Timeout ใน API:
```csharp
// Configure HTTP Client timeout
builder.Services.AddHttpClient("ESarabanApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
});
```

---

### Issue 8: DateTime Format ไม่ตรง

**Symptom**: K2 แสดง date/time ผิดรูปแบบ

**Solutions**:

1. API return ISO 8601 format (ถูกต้องแล้ว):
```
"created_date": "2025-11-01T23:19:20.7192442+07:00"
```

2. ใน K2 SmartForm ใช้ Date Picker control:
```
Control Type: Date Picker
Format: Thai Short Date (dd/MM/yyyy)
Display: 01/11/2025
```

3. ใน K2 Workflow format date:
```javascript
var createdDate = SmartObject.created_date;
var k2Date = new Date(createdDate);
var thaiDate = k2Date.toLocaleDateString('th-TH', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
});
// Result: "1 พฤศจิกายน 2568"
```

---

## สรุป

### ✅ Checklist การ Deploy K2 SmartObjects

**Phase 1: API Server Preparation**
- [ ] 1.1 API deployed และ accessible จาก K2 Server
- [ ] 1.2 CORS configured สำหรับ K2 Server IP/Domain
- [ ] 1.3 Swagger JSON accessible: `/swagger/v1/swagger.json`
- [ ] 1.4 Firewall rules configured
- [ ] 1.5 Test API endpoints จาก K2 Server

**Phase 2: K2 Service Instance**
- [ ] 2.1 K2 Service Instance created
- [ ] 2.2 Base URL configured
- [ ] 2.3 Authentication configured (if required)
- [ ] 2.4 Swagger definition imported successfully
- [ ] 2.5 Test connection from K2 to API

**Phase 3: SmartObjects**
- [ ] 3.1 SmartObjects generated จาก Swagger (10 objects)
- [ ] 3.2 SmartObject properties mapped correctly
- [ ] 3.3 Property types verified (String, Number, DateTime)
- [ ] 3.4 Methods configured (Create, Load, Execute)
- [ ] 3.5 Test SmartObject execution ใน K2 Designer

**Phase 4: SmartForms**
- [ ] 4.1 SmartForms created (Create, Transfer, Workflow)
- [ ] 4.2 Form controls configured and bound to SmartObjects
- [ ] 4.3 Form rules implemented (Validation, Actions)
- [ ] 4.4 Test forms with real data
- [ ] 4.5 Forms published and accessible

**Phase 5: Workflows**
- [ ] 5.1 Workflows created (Simple, Advanced)
- [ ] 5.2 SmartObject activities configured
- [ ] 5.3 Process data fields defined
- [ ] 5.4 Decision rules implemented
- [ ] 5.5 Error handling configured
- [ ] 5.6 Email notifications configured
- [ ] 5.7 Test workflows end-to-end

**Phase 6: Testing & Validation**
- [ ] 6.1 Test all 10 SmartObject methods
- [ ] 6.2 Test SmartForms (Create, Transfer, Query)
- [ ] 6.3 Test Workflows (Create + Notify, Complete Processing)
- [ ] 6.4 Test array properties (Query endpoints)
- [ ] 6.5 Test workflow endpoints (Flat structure)
- [ ] 6.6 Load testing (if required)

**Phase 7: Documentation & Training**
- [ ] 7.1 User documentation created
- [ ] 7.2 Admin documentation created
- [ ] 7.3 Training materials prepared
- [ ] 7.4 Training sessions conducted
- [ ] 7.5 FAQs documented

**Phase 8: Production Deployment**
- [ ] 8.1 UAT testing completed
- [ ] 8.2 Production environment configured
- [ ] 8.3 SmartObjects deployed to production
- [ ] 8.4 Workflows deployed to production
- [ ] 8.5 Monitoring configured
- [ ] 8.6 Backup procedures established
- [ ] 8.7 Go-live checklist completed

---

### 📚 เอกสารอ้างอิง

| Document | Location | Description |
|----------|----------|-------------|
| **API Documentation** | `RefDocuments/README.md` | API overview และ endpoints |
| **K2 Test Results** | `RefDocuments/K2_COMPATIBILITY_TEST_RESULTS.md` | ผลการทดสอบ 14 endpoints |
| **K2 Update Guide** | `RefDocuments/K2_COMPATIBILITY_UPDATE_GUIDE.md` | การ update Models และ Controllers |
| **K2 Summary** | `RefDocuments/K2_COMPATIBILITY_SUMMARY.md` | สรุปการเปลี่ยนแปลง |
| **Swagger UI** | `http://api-server:5152` | API documentation (interactive) |
| **Postman Collection** | `postman-collections/` | ตัวอย่าง API calls |
| **PowerShell Tests** | `PsUnitTest/` | Scripts สำหรับทดสอบ |

---

### 📞 Support

**Technical Issues**:
- Email: [API Team Email]
- K2 Support: [K2 Team Email]
- Documentation: `RefDocuments/` folder

**Training Requests**:
- Contact: [Training Team]
- Schedule: [Training Calendar]

---

**Document Version**: 2.0 (Updated November 1, 2025)  
**Last Updated**: November 1, 2025  
**Author**: K2 Integration Team  
**Status**: ✅ Production Ready - All 14 Endpoints Tested
│    Parent Org ID:    [________________]    │
│    Parent Position:  [________________]    │
│                                             │
│  [ สร้างเอกสาร ]                           │
│                                             │
└─────────────────────────────────────────────┘
```

### **K2 SmartObject Method Configuration**

**Method**: `CreateBookApprovedSimple` (Execute)

**Input Mappings**:
```javascript
// From Form Controls
user_ad = CurrentUser.FQN  // EXAT\ECMUSR07
book_subject = txtSubject.Value
book_to = txtTo.Value
registrationbook_id = ddRegistrationBook.SelectedValue
parent_bookid = txtParentBookId.Value
parent_orgid = txtParentOrgId.Value
parent_positionname = txtParentPosition.Value
```

**Output Mappings**:
```javascript
// To Form Data Labels
lblBookId.Text = book_id
lblBookCode.Text = book_code
lblStatus.Text = status
lblMessage.Text = message
lblCreatedDate.Text = created_date
```

---

## 🔄 K2 Workflow Example

### **Workflow Steps**

```
1. Start
   ↓
2. Initialize Variables
   - varUserAD = CurrentUser.FQN
   - varBookSubject = (from Form)
   - varBookTo = (from Form)
   - varRegistrationBookId = (from Form)
   ↓
3. Execute SmartObject Method
   - SmartObject: eSarabanBooksAPI.CreateBookApprovedSimple
   - Input: varUserAD, varBookSubject, varBookTo, varRegistrationBookId
   - Output: varBookId, varBookCode, varStatus
   ↓
4. Check Status
   If varStatus = "S"
      ↓
   5a. Success Actions
      - Send Notification
      - Update Database
      - Log Success
   Else
      ↓
   5b. Error Actions
      - Send Error Notification
      - Log Error
   ↓
6. End
```

### **SmartObject Execute Configuration - ครบทุก Endpoint**

#### **GROUP 1: Books - Create (K2 Compatible) - 3 Endpoints**

**1.1 Create Book Approved Simple**
```xml
<Execute>
  <SmartObject>ESarabanBook_CreateApproved_Simple</SmartObject>
  <Method>Create</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_subject" Value="{ProcessData.varBookSubject}" />
    <Property Name="book_to" Value="{ProcessData.varBookTo}" />
    <Property Name="registrationbook_id" Value="{ProcessData.varRegistrationBookId}" />
    <Property Name="parent_bookid" Value="{ProcessData.varParentBookId}" />
    <Property Name="parent_orgid" Value="{ProcessData.varParentOrgId}" />
    <Property Name="parent_positionname" Value="{ProcessData.varParentPosition}" />
    
    <!-- 📎 ไฟล์เอกสารหลัก (bookFile) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    
    <!-- 📎 ไฟล์แนบ (bookAttach) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="book_subject" Store="ProcessData.varBookSubject" />
    <Property Name="book_to" Store="ProcessData.varBookTo" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="registrationbook_id" Store="ProcessData.varRegBookId" />
    <Property Name="parent_bookid" Store="ProcessData.varParentBookId" />
    <Property Name="parent_orgid" Store="ProcessData.varParentOrgId" />
    <Property Name="parent_positionname" Store="ProcessData.varParentPosition" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="created_by" Store="ProcessData.varCreatedBy" />
    <Property Name="created_date" Store="ProcessData.varCreatedDate" />
  </OutputProperties>
</Execute>
```

**💡 วิธีส่ง `bookFile` และ `bookAttach` ใน K2:**

**Option 1: ส่งเป็น JSON String (แนะนำ)**
```javascript
// ใน K2 Data Event (Assign Variables)
varBookFileJSON = '[{"file_name":"document.pdf","file_content":"base64string...","file_extension":"pdf"}]'
varBookAttachJSON = '[{"file_name":"attachment.jpg","file_content":"base64string...","file_extension":"jpg"}]'
```

**Option 2: ส่งค่าว่างถ้าไม่มีไฟล์**
```javascript
varBookFileJSON = null     // หรือ "[]" (empty array)
varBookAttachJSON = null   // หรือ "[]" (empty array)
```

**⚠️ IMPORTANT:** ถ้าไม่ส่ง `bookFile` และ `bookAttach` ให้ใส่ค่า `null` หรือ `[]` (empty array)

**1.2 Create Book Non-Compliant Simple**
```xml
<Execute>
  <SmartObject>ESarabanBook_CreateNonCompliant_Simple</SmartObject>
  <Method>Create</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_subject" Value="{ProcessData.varBookSubject}" />
    <Property Name="book_to" Value="{ProcessData.varBookTo}" />
    <Property Name="registrationbook_id" Value="{ProcessData.varRegistrationBookId}" />
    <Property Name="parent_bookid" Value="{ProcessData.varParentBookId}" />
    <Property Name="parent_orgid" Value="{ProcessData.varParentOrgId}" />
    <Property Name="parent_positionname" Value="{ProcessData.varParentPosition}" />
    
    <!-- 📎 ไฟล์เอกสารหลัก (bookFile) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    
    <!-- 📎 ไฟล์แนบ (bookAttach) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="book_subject" Store="ProcessData.varBookSubject" />
    <Property Name="book_to" Store="ProcessData.varBookTo" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="registrationbook_id" Store="ProcessData.varRegBookId" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="created_by" Store="ProcessData.varCreatedBy" />
    <Property Name="created_date" Store="ProcessData.varCreatedDate" />
  </OutputProperties>
</Execute>
```

**1.3 Create Book Under-Construction Simple**
```xml
<Execute>
  <SmartObject>ESarabanBook_CreateUnderConstruction_Simple</SmartObject>
  <Method>Create</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_subject" Value="{ProcessData.varBookSubject}" />
    <Property Name="book_to" Value="{ProcessData.varBookTo}" />
    <Property Name="registrationbook_id" Value="{ProcessData.varRegistrationBookId}" />
    <Property Name="parent_bookid" Value="{ProcessData.varParentBookId}" />
    <Property Name="parent_orgid" Value="{ProcessData.varParentOrgId}" />
    <Property Name="parent_positionname" Value="{ProcessData.varParentPosition}" />
    
    <!-- 📎 ไฟล์เอกสารหลัก (bookFile) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    
    <!-- 📎 ไฟล์แนบ (bookAttach) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="book_subject" Store="ProcessData.varBookSubject" />
    <Property Name="book_to" Store="ProcessData.varBookTo" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="registrationbook_id" Store="ProcessData.varRegBookId" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="created_by" Store="ProcessData.varCreatedBy" />
    <Property Name="created_date" Store="ProcessData.varCreatedDate" />
  </OutputProperties>
</Execute>
```

---

#### **GROUP 2: Books - Create (Full Format) - 4 Endpoints**

**⚠️ NOTE:** Full Format endpoints รับ Request Body แบบเต็ม (ตาม eSaraban API Spec) มี nested objects: `book`, `bookFile`, `bookAttach`, `bookHistory`, `bookReferences`, `bookReferenceAttach`

**2.1 Create Book - Original**
```xml
<Execute>
  <SmartObject>ESarabanBook_CreateOriginal</SmartObject>
  <Method>Create</Method>
  <InputProperties>
    <!-- Request Body จะเป็น JSON String ของ ESarabanCreateBookRequest -->
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book" Value="{ProcessData.varBookJSON}" />
    
    <!-- 📎 ไฟล์เอกสารหลัก (bookFile) - JSON Array String -->
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    
    <!-- 📎 ไฟล์แนบ (bookAttach) - JSON Array String -->
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
    
    <!-- Optional: ประวัติ, เอกสารอ้างอิง -->
    <Property Name="bookHistory" Value="{ProcessData.varBookHistoryJSON}" />
    <Property Name="bookReferences" Value="{ProcessData.varBookReferencesJSON}" />
    <Property Name="bookReferenceAttach" Value="{ProcessData.varBookRefAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="book_subject" Store="ProcessData.varBookSubject" />
    <Property Name="book_to" Store="ProcessData.varBookTo" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="registrationbook_id" Store="ProcessData.varRegBookId" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="history_count" Store="ProcessData.varHistoryCount" />
    <Property Name="reference_count" Store="ProcessData.varReferenceCount" />
    <Property Name="reference_attach_count" Store="ProcessData.varRefAttachCount" />
    <Property Name="created_by" Store="ProcessData.varCreatedBy" />
    <Property Name="created_date" Store="ProcessData.varCreatedDate" />
  </OutputProperties>
</Execute>
```

**2.2 Create Book - Approved (Full)**
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
    <!-- Same 17 fields as Original -->
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="book_subject" Store="ProcessData.varBookSubject" />
    <Property Name="book_to" Store="ProcessData.varBookTo" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="registrationbook_id" Store="ProcessData.varRegBookId" />
    <Property Name="parent_bookid" Store="ProcessData.varParentBookId" />
    <Property Name="parent_orgid" Store="ProcessData.varParentOrgId" />
    <Property Name="parent_positionname" Store="ProcessData.varParentPosition" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="history_count" Store="ProcessData.varHistoryCount" />
    <Property Name="reference_count" Store="ProcessData.varReferenceCount" />
    <Property Name="reference_attach_count" Store="ProcessData.varRefAttachCount" />
    <Property Name="created_by" Store="ProcessData.varCreatedBy" />
    <Property Name="created_date" Store="ProcessData.varCreatedDate" />
  </OutputProperties>
</Execute>
```

**2.3 Create Book - Non-Compliant (Full)**
```xml
<Execute>
  <SmartObject>ESarabanBook_CreateNonCompliant</SmartObject>
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
    <!-- Same fields as Approved (without parent fields) -->
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="book_subject" Store="ProcessData.varBookSubject" />
    <Property Name="book_to" Store="ProcessData.varBookTo" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="registrationbook_id" Store="ProcessData.varRegBookId" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="history_count" Store="ProcessData.varHistoryCount" />
    <Property Name="reference_count" Store="ProcessData.varReferenceCount" />
    <Property Name="reference_attach_count" Store="ProcessData.varRefAttachCount" />
    <Property Name="created_by" Store="ProcessData.varCreatedBy" />
    <Property Name="created_date" Store="ProcessData.varCreatedDate" />
  </OutputProperties>
</Execute>
```

**2.4 Create Book - Under-Construction (Full)**
```xml
<Execute>
  <SmartObject>ESarabanBook_CreateUnderConstruction</SmartObject>
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
    <!-- Same fields as Non-Compliant -->
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="book_subject" Store="ProcessData.varBookSubject" />
    <Property Name="book_to" Store="ProcessData.varBookTo" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="registrationbook_id" Store="ProcessData.varRegBookId" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="history_count" Store="ProcessData.varHistoryCount" />
    <Property Name="reference_count" Store="ProcessData.varReferenceCount" />
    <Property Name="reference_attach_count" Store="ProcessData.varRefAttachCount" />
    <Property Name="created_by" Store="ProcessData.varCreatedBy" />
    <Property Name="created_date" Store="ProcessData.varCreatedDate" />
  </OutputProperties>
</Execute>
```

**💡 วิธีใช้ Full Format Endpoints ใน K2:**

**ตัวอย่าง: สร้าง `book` JSON String**
```javascript
// ใน K2 Assign Variable Activity
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
  '"is_circular":0' +
'}'

// ใช้ bookFile และ bookAttach เหมือน Simple Format
varBookFileJSON = '[{"file_name":"doc.pdf","file_content":"' + varBase64 + '","file_extension":"pdf"}]'
varBookAttachJSON = '[]'

// Optional: History, References (ส่ง empty array ถ้าไม่มี)
varBookHistoryJSON = '[]'
varBookReferencesJSON = '[]'
varBookRefAttachJSON = '[]'
```

---

#### **GROUP 3: Books - Operations - 2 Endpoints**

**⚠️ NOTE:** Operations endpoints **ไม่รองรับ** `bookFile`/`bookAttach` เพราะใช้กับเอกสารที่มีอยู่แล้ว

**3.1 Generate Book Code**
```xml
<Execute>
  <SmartObject>ESarabanBook_GenerateCode</SmartObject>
  <Method>Load</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_id" Value="{ProcessData.varBookId}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="generated_code" Store="ProcessData.varGeneratedCode" />
    <Property Name="code_type" Store="ProcessData.varCodeType" />
    <Property Name="generated_date" Store="ProcessData.varGeneratedDate" />
    <Property Name="generated_by" Store="ProcessData.varGeneratedBy" />
  </OutputProperties>
</Execute>
```

**3.2 Transfer Book**
```xml
<Execute>
  <SmartObject>ESarabanBook_Transfer</SmartObject>
  <Method>Execute</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_id" Value="{ProcessData.varBookId}" />
    <Property Name="original_org_code" Value="{ProcessData.varOriginalOrgCode}" />
    <Property Name="destination_org_code" Value="{ProcessData.varDestOrgCode}" />
    <Property Name="transfer_reason" Value="{ProcessData.varTransferReason}" />
    <Property Name="transfer_note" Value="{ProcessData.varTransferNote}" />
    <Property Name="transfer_date" Value="{ProcessData.varTransferDate}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="transfer_id" Store="ProcessData.varTransferId" />
    <Property Name="original_org_code" Store="ProcessData.varOriginalOrgCode" />
    <Property Name="destination_org_code" Store="ProcessData.varDestOrgCode" />
    <Property Name="transfer_status" Store="ProcessData.varTransferStatus" />
    <Property Name="transfer_reason" Store="ProcessData.varTransferReason" />
    <Property Name="transfer_note" Store="ProcessData.varTransferNote" />
    <Property Name="transferred_date" Store="ProcessData.varTransferredDate" />
    <Property Name="book_locked" Store="ProcessData.varBookLocked" />
  </OutputProperties>
</Execute>
```

---

#### **GROUP 4: Books - Workflow (Combined) - 3 Endpoints**

**⚠️ IMPORTANT: Workflow endpoints ใช้ FLAT STRUCTURE (22 fields at top level)**

**4.1 Workflow Approved (3 Steps Combined)**
```xml
<Execute>
  <SmartObject>ESarabanBook_WorkflowApproved</SmartObject>
  <Method>ExecuteWorkflow</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_subject" Value="{ProcessData.varBookSubject}" />
    <Property Name="book_to" Value="{ProcessData.varBookTo}" />
    <Property Name="registrationbook_id" Value="{ProcessData.varRegistrationBookId}" />
    <Property Name="original_org_code" Value="{ProcessData.varOriginalOrgCode}" />
    <Property Name="destination_org_code" Value="{ProcessData.varDestOrgCode}" />
    <Property Name="transfer_reason" Value="{ProcessData.varTransferReason}" />
    <Property Name="parent_bookid" Value="{ProcessData.varParentBookId}" />
    <Property Name="parent_orgid" Value="{ProcessData.varParentOrgId}" />
    <Property Name="parent_positionname" Value="{ProcessData.varParentPosition}" />
    
    <!-- 📎 ไฟล์เอกสารหลัก (bookFile) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    
    <!-- 📎 ไฟล์แนบ (bookAttach) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <!-- Workflow Status (4 fields) -->
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="workflow_type" Store="ProcessData.varWorkflowType" />
    <Property Name="workflow_completed" Store="ProcessData.varWorkflowCompleted" />
    <Property Name="executed_by" Store="ProcessData.varExecutedBy" />
    <Property Name="overall_message" Store="ProcessData.varOverallMessage" />
    
    <!-- Step 1: Create Book (6 fields - flat structure) -->
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="create_message" Store="ProcessData.varCreateMessage" />
    
    <!-- Step 2: Generate Code (4 fields - flat structure) -->
    <Property Name="generated_code" Store="ProcessData.varGeneratedCode" />
    <Property Name="code_type" Store="ProcessData.varCodeType" />
    <Property Name="generated_date" Store="ProcessData.varGeneratedDate" />
    <Property Name="generate_message" Store="ProcessData.varGenerateMessage" />
    
    <!-- Step 3: Transfer (6 fields - flat structure) -->
    <Property Name="transfer_id" Store="ProcessData.varTransferId" />
    <Property Name="original_org_code" Store="ProcessData.varOriginalOrgCode" />
    <Property Name="destination_org_code" Store="ProcessData.varDestOrgCode" />
    <Property Name="transfer_status" Store="ProcessData.varTransferStatus" />
    <Property Name="transferred_date" Store="ProcessData.varTransferredDate" />
    <Property Name="transfer_message" Store="ProcessData.varTransferMessage" />
  </OutputProperties>
</Execute>
```

**4.2 Workflow Non-Compliant (3 Steps Combined)**
```xml
<Execute>
  <SmartObject>ESarabanBook_WorkflowNonCompliant</SmartObject>
  <Method>ExecuteWorkflow</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_subject" Value="{ProcessData.varBookSubject}" />
    <Property Name="book_to" Value="{ProcessData.varBookTo}" />
    <Property Name="registrationbook_id" Value="{ProcessData.varRegistrationBookId}" />
    <Property Name="original_org_code" Value="{ProcessData.varOriginalOrgCode}" />
    <Property Name="destination_org_code" Value="{ProcessData.varDestOrgCode}" />
    <Property Name="transfer_reason" Value="{ProcessData.varTransferReason}" />
    
    <!-- 📎 ไฟล์เอกสารหลัก (bookFile) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    
    <!-- 📎 ไฟล์แนบ (bookAttach) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <!-- Same 22 fields as Workflow Approved (flat structure) -->
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="workflow_type" Store="ProcessData.varWorkflowType" />
    <Property Name="workflow_completed" Store="ProcessData.varWorkflowCompleted" />
    <Property Name="executed_by" Store="ProcessData.varExecutedBy" />
    <Property Name="overall_message" Store="ProcessData.varOverallMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="create_message" Store="ProcessData.varCreateMessage" />
    <Property Name="generated_code" Store="ProcessData.varGeneratedCode" />
    <Property Name="code_type" Store="ProcessData.varCodeType" />
    <Property Name="generated_date" Store="ProcessData.varGeneratedDate" />
    <Property Name="generate_message" Store="ProcessData.varGenerateMessage" />
    <Property Name="transfer_id" Store="ProcessData.varTransferId" />
    <Property Name="original_org_code" Store="ProcessData.varOriginalOrgCode" />
    <Property Name="destination_org_code" Store="ProcessData.varDestOrgCode" />
    <Property Name="transfer_status" Store="ProcessData.varTransferStatus" />
    <Property Name="transferred_date" Store="ProcessData.varTransferredDate" />
    <Property Name="transfer_message" Store="ProcessData.varTransferMessage" />
  </OutputProperties>
</Execute>
```

**4.3 Workflow Under-Construction (3 Steps Combined)**
```xml
<Execute>
  <SmartObject>ESarabanBook_WorkflowUnderConstruction</SmartObject>
  <Method>ExecuteWorkflow</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_subject" Value="{ProcessData.varBookSubject}" />
    <Property Name="book_to" Value="{ProcessData.varBookTo}" />
    <Property Name="registrationbook_id" Value="{ProcessData.varRegistrationBookId}" />
    <Property Name="original_org_code" Value="{ProcessData.varOriginalOrgCode}" />
    <Property Name="destination_org_code" Value="{ProcessData.varDestOrgCode}" />
    <Property Name="transfer_reason" Value="{ProcessData.varTransferReason}" />
    
    <!-- 📎 ไฟล์เอกสารหลัก (bookFile) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    
    <!-- 📎 ไฟล์แนบ (bookAttach) - ส่งเป็น JSON String หรือทำ Loop ใน K2 -->
    <Property Name="bookAttach" Value="{ProcessData.varBookAttachJSON}" />
  </InputProperties>
  <OutputProperties>
    <!-- Same 22 fields as Workflow Approved (flat structure) -->
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="workflow_type" Store="ProcessData.varWorkflowType" />
    <Property Name="workflow_completed" Store="ProcessData.varWorkflowCompleted" />
    <Property Name="executed_by" Store="ProcessData.varExecutedBy" />
    <Property Name="overall_message" Store="ProcessData.varOverallMessage" />
    <Property Name="book_id" Store="ProcessData.varBookId" />
    <Property Name="book_code" Store="ProcessData.varBookCode" />
    <Property Name="booktype_id" Store="ProcessData.varBookTypeId" />
    <Property Name="file_count" Store="ProcessData.varFileCount" />
    <Property Name="attach_count" Store="ProcessData.varAttachCount" />
    <Property Name="create_message" Store="ProcessData.varCreateMessage" />
    <Property Name="generated_code" Store="ProcessData.varGeneratedCode" />
    <Property Name="code_type" Store="ProcessData.varCodeType" />
    <Property Name="generated_date" Store="ProcessData.varGeneratedDate" />
    <Property Name="generate_message" Store="ProcessData.varGenerateMessage" />
    <Property Name="transfer_id" Store="ProcessData.varTransferId" />
    <Property Name="original_org_code" Store="ProcessData.varOriginalOrgCode" />
    <Property Name="destination_org_code" Store="ProcessData.varDestOrgCode" />
    <Property Name="transfer_status" Store="ProcessData.varTransferStatus" />
    <Property Name="transferred_date" Store="ProcessData.varTransferredDate" />
    <Property Name="transfer_message" Store="ProcessData.varTransferMessage" />
  </OutputProperties>
</Execute>
```

---

#### **GROUP 5: Books - Query - 2 Endpoints**

**⚠️ NOTE:** Query endpoints **ไม่รองรับ** `bookFile`/`bookAttach` เพราะเป็นการดึงข้อมูล (Read-only)

**5.1 Get Final Organizations (with Alert)**
```xml
<Execute>
  <SmartObject>ESarabanBook_GetFinalOrgs</SmartObject>
  <Method>Load</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_id" Value="{ProcessData.varBookId}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="books" Store="ProcessData.varOrganizations" />
    <!-- books is an ARRAY with these fields per item:
         - running_no (Number)
         - book_id (String)
         - send_org_nameth (String) ⚠️ NO underscore before "th"
         - send_date (String)
         - receive_org_nameth (String) ⚠️ NO underscore before "th"
         - receive_code (String)
         - receive_date (String)
         - status_nameth (String) ⚠️ NO underscore before "th"
         - receive_comment (String)
    -->
  </OutputProperties>
</Execute>
```

**5.2 Get Final Organizations (No Alert - Silent)**
```xml
<Execute>
  <SmartObject>ESarabanBook_GetFinalOrgs_NoAlert</SmartObject>
  <Method>Load</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_id" Value="{ProcessData.varBookId}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="status" Store="ProcessData.varStatus" />
    <Property Name="statusCode" Store="ProcessData.varStatusCode" />
    <Property Name="message" Store="ProcessData.varMessage" />
    <Property Name="books" Store="ProcessData.varOrganizations" />
    <!-- books is an ARRAY with same 9 fields as above -->
  </OutputProperties>
</Execute>
```

---

### **📎 วิธีส่ง `bookFile` และ `bookAttach` ใน K2 SmartObject**

#### **🎯 สรุป: มี 3 วิธี**

**วิธีที่ 1: ส่งเป็น JSON String (แนะนำ - ง่ายที่สุด)**
```javascript
// ใน K2 Data Event (Assign Variables)
// Format: JSON Array String
varBookFileJSON = '[{"file_name":"document.pdf","file_content":"JVBERi0xLjQKJ...","file_extension":"pdf","file_remark":"เอกสารหลัก"}]'

varBookAttachJSON = '[{"file_name":"attachment.jpg","file_content":"iVBORw0KGgoAAA...","file_extension":"jpg","file_remark":"ไฟล์แนบ"}]'
```

**วิธีที่ 2: ส่งค่าว่าง (ถ้าไม่มีไฟล์)**
```javascript
varBookFileJSON = null        // หรือ "[]"
varBookAttachJSON = null      // หรือ "[]"
```

**วิธีที่ 3: ใช้ K2 Loop สำหรับหลายไฟล์**
```xml
<!-- ใช้ For Each Loop ใน K2 Workflow -->
<ForEach Collection="{ProcessData.varFilesList}">
  <!-- Build JSON Array ทีละไฟล์ -->
  <Assign>
    <Property Name="varBookFileJSON" Value="Concat(varBookFileJSON, FileItem)" />
  </Assign>
</ForEach>
```

---

#### **📋 โครงสร้างของ `bookFile` และ `bookAttach`**

```json
// ตัวอย่าง bookFile (ไฟล์เอกสารหลัก)
[
  {
    "file_name": "document.pdf",           // ชื่อไฟล์ (required)
    "file_content": "JVBERi0xLjQKJ...",   // Base64 encoded content (required)
    "file_extension": "pdf",               // นามสกุลไฟล์ (required)
    "file_remark": "เอกสารหลัก",          // หมายเหตุ (optional)
    "file_path": "/path/to/file",          // Path (optional)
    "file_url": "http://...",              // URL (optional)
    "alfresco_parentid": "123",            // Alfresco ID (optional)
    "alfresco_foldername": "folder1",      // Alfresco folder (optional)
    "alfresco_nodetype": "cm:content",     // Node type (optional)
    "alfresco_noderef": "workspace://...", // Node ref (optional)
    "alfresco_nodeid": "abc123",           // Node ID (optional)
    "originaL_NODEID": "original123"       // Original Node ID (optional)
  }
]

// ตัวอย่าง bookAttach (ไฟล์แนบ)
[
  {
    "file_name": "attachment.jpg",         // ชื่อไฟล์ (required)
    "file_content": "iVBORw0KGgoAAA...",  // Base64 encoded content (required)
    "file_extension": "jpg",               // นามสกุลไฟล์ (required)
    "file_remark": "ไฟล์แนบรูปภาพ"        // หมายเหตุ (optional)
  },
  {
    "file_name": "data.xlsx",
    "file_content": "UEsDBBQABgAI...",
    "file_extension": "xlsx",
    "file_remark": "ไฟล์ข้อมูล Excel"
  }
]
```

---

#### **💡 ตัวอย่างการใช้งานใน K2**

**ตัวอย่าง 1: ส่งไฟล์เดียว**
```javascript
// ใน K2 Assign Variable Activity
varBookFileJSON = '[{"file_name":"report.pdf","file_content":"' + varBase64Content + '","file_extension":"pdf"}]'
varBookAttachJSON = '[]'  // ไม่มีไฟล์แนบ
```

**ตัวอย่าง 2: ส่งหลายไฟล์**
```javascript
// ใน K2 Assign Variable Activity
varBookFileJSON = '[' +
  '{"file_name":"doc1.pdf","file_content":"' + varFile1Content + '","file_extension":"pdf"},' +
  '{"file_name":"doc2.pdf","file_content":"' + varFile2Content + '","file_extension":"pdf"}' +
']'

varBookAttachJSON = '[' +
  '{"file_name":"img1.jpg","file_content":"' + varImg1Content + '","file_extension":"jpg"},' +
  '{"file_name":"img2.png","file_content":"' + varImg2Content + '","file_extension":"png"}' +
']'
```

**ตัวอย่าง 3: อ่านไฟล์จาก SmartObject และแปลงเป็น Base64**
```xml
<!-- Step 1: Read File from SharePoint/File System SmartObject -->
<Execute>
  <SmartObject>SharePointFile</SmartObject>
  <Method>ReadFile</Method>
  <InputProperties>
    <Property Name="FilePath" Value="{ProcessData.varFilePath}" />
  </InputProperties>
  <OutputProperties>
    <Property Name="FileContent" Store="ProcessData.varFileContentBytes" />
    <Property Name="FileName" Store="ProcessData.varFileName" />
  </OutputProperties>
</Execute>

<!-- Step 2: Convert to Base64 using K2 Built-in Function -->
<Assign>
  <Property Name="varBase64Content" Value="Convert.ToBase64String(varFileContentBytes)" />
</Assign>

<!-- Step 3: Build JSON String -->
<Assign>
  <Property Name="varBookFileJSON" 
            Value="'[{\"file_name\":\"' + varFileName + '\",\"file_content\":\"' + varBase64Content + '\",\"file_extension\":\"pdf\"}]'" />
</Assign>

<!-- Step 4: Call eSaraban SmartObject -->
<Execute>
  <SmartObject>ESarabanBook_CreateApproved_Simple</SmartObject>
  <Method>Create</Method>
  <InputProperties>
    <Property Name="user_ad" Value="{ProcessData.varUserAD}" />
    <Property Name="book_subject" Value="{ProcessData.varBookSubject}" />
    <Property Name="book_to" Value="{ProcessData.varBookTo}" />
    <Property Name="registrationbook_id" Value="{ProcessData.varRegistrationBookId}" />
    <Property Name="bookFile" Value="{ProcessData.varBookFileJSON}" />
    <Property Name="bookAttach" Value="[]" />
  </InputProperties>
</Execute>
```

---

#### **⚠️ IMPORTANT NOTES**

1. **Base64 Encoding**: ไฟล์ต้อง encode เป็น Base64 string ก่อนส่ง
2. **JSON Format**: ต้องเป็น valid JSON array string (ใช้ double quotes `"` ไม่ใช่ single quotes `'`)
3. **Required Fields**: `file_name`, `file_content`, `file_extension` เป็นฟิลด์บังคับ
4. **Empty Arrays**: ถ้าไม่มีไฟล์ ให้ส่ง `[]` หรือ `null` (ไม่ส่งก็ได้)
5. **Multiple Files**: ใช้ comma (`,`) คั่นระหว่างไฟล์ใน JSON array
6. **File Size**: ระวังขนาดไฟล์ที่ใหญ่เกินไป (แนะนำไม่เกิน 10MB ต่อไฟล์)
7. **Special Characters**: ต้อง escape special characters ใน JSON string (`"` → `\"`, `\` → `\\`)

---

### **K2 Workflow - Complete Example with All SmartObjects**

```
Workflow: Complete Book Processing (All 14 Endpoints)

Process Data Fields:
├─ varUserAD (String)
├─ varBookSubject (String)
├─ varBookTo (String)
├─ varRegistrationBookId (String)
├─ varOriginalOrgCode (String)
├─ varDestOrgCode (String)
├─ varTransferReason (String)
├─ varBookFileJSON (String) ← 📎 JSON Array String
├─ varBookAttachJSON (String) ← 📎 JSON Array String
├─ varBookId (String)
├─ varBookCode (String)
├─ varStatus (String)
├─ varStatusCode (String)
├─ varMessage (String)
├─ varWorkflowCompleted (Boolean)
└─ varOrganizations (Array)

Workflow Steps:
1. Start
   ↓
2. Initialize Variables
   varUserAD = CurrentUser.FQN
   ↓
3. Execute Workflow Approved (SmartObject Group 3)
   [ESarabanBook_WorkflowApproved]
   → Creates Book + Generates Code + Transfers (all in one call)
   → Output: 22 fields (flat structure)
   ↓
4. Check Workflow Status
   IF varWorkflowCompleted = true THEN
      ↓
   5a. Query Final Organizations (SmartObject Group 4)
       [ESarabanBook_GetFinalOrgs]
       → Returns array of organizations
       → Output: varOrganizations (9 fields per item)
       ↓
   5b. Loop Through Organizations
       FOR EACH org IN varOrganizations
         - Send Email to org.receive_org_nameth
         - Log activity
       NEXT
       ↓
   5c. Success Notification
       Send Email: "Workflow completed successfully"
       ↓
   6. End Success
   
   ELSE (Workflow Failed)
      ↓
   7. Error Handling
       Log Error: varMessage
       Send Email: "Workflow failed"
       ↓
   8. End Error
```

---

## 🧪 Testing in K2

### **1. Test SmartObject Method**

1. เปิด K2 Designer / K2 Management
2. ไปที่ **SmartObjects** → `eSarabanBooksAPI`
3. คลิกขวา → **Execute**
4. กรอกข้อมูล Input Properties:
   ```
   user_ad: EXAT\ECMUSR07
   book_subject: ทดสอบสร้างเอกสาร
   book_to: ผู้อำนวยการใหญ่
   registrationbook_id: E1786792382247A49DD27072718DB187
   ```
5. คลิก **Execute**
6. ตรวจสอบ Output Properties

### **2. Expected Results**

```json
{
  "status": "S",
  "statusCode": "200",
  "message": "Success: generate book.",
  "book_id": "DF4E19B272DE4FD78880B4CE65CECD75",
  "book_code": "APV-20251030-5810",
  "booktype_id": 93
}
```

---

## 📊 Response Handling

### **Success Response** (status = "S")

```javascript
// In K2 Workflow
if (varStatus == "S") {
    // Success actions
    varBookId = SmartObject.Output.book_id;
    varBookCode = SmartObject.Output.book_code;
    
    // Send notification
    SendEmail(
        To: varUserEmail,
        Subject: "เอกสารถูกสร้างสำเร็จ",
        Body: "Book Code: " + varBookCode
    );
}
```

### **Error Response** (status != "S")

```javascript
// In K2 Workflow
if (varStatus != "S") {
    // Error handling
    varErrorMessage = SmartObject.Output.message;
    
    // Log error
    LogError("CreateBook Failed: " + varErrorMessage);
    
    // Send error notification
    SendEmail(
        To: varAdminEmail,
        Subject: "ERROR: สร้างเอกสารไม่สำเร็จ",
        Body: varErrorMessage
    );
}
```

---

## 🔐 Security Considerations

### **1. Authentication** (Future Implementation)
- Active Directory integration
- API Key validation
- OAuth 2.0

### **2. Authorization**
- Role-based access control
- User permissions validation
- Organization-level access

### **3. Data Protection**
- HTTPS/TLS encryption
- Input validation
- SQL injection prevention
- XSS protection

---

## 📈 Performance Optimization

### **Best Practices**

1. **Caching**
   - Cache registration book list
   - Cache organization data
   - Cache user information

2. **Batch Operations**
   - Use bulk create for multiple books
   - Implement queue for heavy loads

3. **Connection Pooling**
   - Configure K2 connection pool
   - Optimize database connections

4. **Monitoring**
   - Monitor API response times
   - Track success/failure rates
   - Log all operations

---

## 🐛 Troubleshooting

### **Common Issues**

#### **1. Connection Error**
```
Error: Unable to connect to service
```
**Solution**:
- ตรวจสอบ Base URL
- ตรวจสอบ network connectivity
- ตรวจสอบ firewall settings

#### **2. Authentication Error**
```
Error: 401 Unauthorized
```
**Solution**:
- ตรวจสอบ credentials
- ตรวจสอบ API Key
- ตรวจสอบ token expiration

#### **3. Validation Error**
```
Error: 400 Bad Request - "book_subject is required"
```
**Solution**:
- ตรวจสอบ required fields
- ตรวจสอบ data format
- ตรวจสอบ character encoding (UTF-8)

#### **4. Thai Language Display Issue**
```
Issue: ภาษาไทยแสดงเป็น ???
```
**Solution**:
- ใช้ UTF-8 encoding
- ตั้งค่า Content-Type: application/json; charset=utf-8
- ตรวจสอบ K2 Form encoding settings

---

## 📞 Support

**Development Team**:
- **API Team**: api-support@exat.co.th
- **K2 Team**: k2-support@exat.co.th

**Documentation**:
- API Documentation: `/swagger`
- Project Repository: GitHub - EXAT.ECM.EER.ESARABAN

---

## 📝 Change Log

| Date | Version | Changes |
|------|---------|---------|
| 2025-10-30 | 1.0 | Initial release - CreateBookApprovedSimple endpoint |

---

## 🚀 Next Steps

1. ✅ Configure K2 Service Instance
2. ✅ Test SmartObject in K2 Designer
3. ⏳ Implement in K2 SmartForm
4. ⏳ Deploy to K2 Workflow
5. ⏳ Add Authentication
6. ⏳ Deploy to Production
