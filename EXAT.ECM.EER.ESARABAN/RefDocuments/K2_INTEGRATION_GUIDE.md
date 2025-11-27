# คู่มือการเชื่อมต่อ K2 REST Service

## 📖 ภาพรวม
เอกสารนี้อธิบายวิธีการเชื่อมต่อ K2 Workflow กับ REST API นี้

## 🔧 ขั้นตอนการตั้งค่า K2

### 1. เตรียม REST API

#### 1.1 Deploy API
```powershell
# Build และ Publish
dotnet publish K2RestApi.csproj -c Release -o ./publish

# หรือรันในโหมด Development
dotnet run --project K2RestApi.csproj
```

#### 1.2 ตรวจสอบ Swagger JSON
เปิดเบราว์เซอร์ไปที่:
```
https://localhost:7157/swagger/v1/swagger.json
```

คุณควรเห็น JSON ที่มีโครงสร้างประมาณนี้:
```json
{
  "swagger": "2.0",
  "info": {
    "title": "K2 REST Service API",
    "version": "v1"
  },
  "paths": {
    "/api/employees": { ... },
    "/api/health": { ... }
  }
}
```

### 2. สร้าง Service Instance ใน K2

#### 2.1 เปิด K2 Management
1. เข้าไปที่ K2 Management Console
2. ไปที่ **Service Instances**
3. คลิก **New Service Instance**

#### 2.2 เลือก REST Service Type
1. เลือก **REST Service Broker** จากรายการ
2. กรอกข้อมูล:
   - **Display Name**: `K2RestServiceAPI`
   - **Description**: `REST API for Employee Management`

#### 2.3 กำหนด Service URL
```
Base URL: https://localhost:7157
Swagger URL: https://localhost:7157/swagger/v1/swagger.json
```

**หมายเหตุ**: สำหรับ Production ควรใช้ URL ที่เป็นจริง

### 3. Import Swagger Definition

#### 3.1 Import โดยอัตโนมัติ
1. ใน Service Instance Configuration
2. เลือก **Import from Swagger/OpenAPI**
3. ใส่ URL: `https://localhost:7157/swagger/v1/swagger.json`
4. คลิก **Import**

K2 จะอ่าน Swagger JSON และสร้าง:
- SmartObjects
- Methods
- Properties
- Input/Output Parameters

#### 3.2 ตรวจสอบ SmartObjects ที่สร้าง
หลังจาก Import แล้วจะได้:
- **Books Workflow SmartObjects** พร้อม Methods:
  - CreateApprovedWorkflow
  - CreateNonCompliantWorkflow
  - CreateUnderConstructionWorkflow
  - GetFinalOrganizations
  - GetFinalOrganizationsNoAlert
- **Books Create SmartObjects** พร้อม Methods อื่นๆ:
  - CreateApprovedSimple
  - CreateNonCompliantSimple
  - CreateUnderConstructionSimple
  - GenerateCode
  - TransferBook

### 4. Configure Authentication (ถ้าจำเป็น)

#### 4.1 No Authentication (Development)
สำหรับ Development mode ไม่จำเป็นต้องตั้งค่า Authentication

#### 4.2 Basic Authentication
```csharp
// ใน Program.cs เพิ่ม:
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
```

#### 4.3 Bearer Token (JWT)
```csharp
// ใน Program.cs เพิ่ม:
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
```

### 5. ทดสอบการเชื่อมต่อ

#### 5.1 ทดสอบใน K2 SmartObject Tester
1. เปิด SmartObject Tester
2. เลือก SmartObject ที่สร้างจาก REST API
3. เลือก Method ตามตัวอย่างด้านล่าง
4. คลิก **Execute**

---

## 📚 วิธีการ Call API แต่ละตัวอย่างละเอียด

หมายเหตุสำคัญ:
- สำหรับ API กลุ่ม Workflow ทั้ง 3 ตัว (approved, non-compliant, under-construction) ตอนนี้ "Request Body" ใช้โครงสร้างเดียวกับ API แบบ Simple Create ทุกประการ เพื่อให้ใช้งานได้ง่ายขึ้นใน K2
- ฟิลด์สำหรับโอนย้ายเอกสาร original_org_code และ destination_org_code สามารถส่งได้ 2 แบบ: (1) ส่งใน Body เหมือนเดิม หรือ (2) ส่งเป็น Query String เช่น ?original_org_code=J10100&destination_org_code=J10200
- ถ้าไม่ระบุ original_org_code/destination_org_code ระบบจะกำหนดค่าเริ่มต้นเป็น original_org_code="J10100" และ destination_org_code="J10000"
- โครงสร้างไฟล์ใช้ฟิลด์ file_name, file_content (Base64), file_extension แทนรูปแบบเดิม
- รองรับการส่ง tranfer_id (optional) ผ่าน Query เพื่อกำหนดรหัสโอนย้ายเองได้ เช่น &tranfer_id=TRF-20251031-0001 (สะกดตามสเปค: tranfer_id ไม่มี s)

### 🔄 API 1: POST /api/books/workflow/approved

**คำอธิบาย:** สร้างเอกสาร Workflow แบบอนุมัติ/เข้าหลักเกณ์ (ทำ 3 ขั้นตอน: Create → Generate Code → Transfer)

#### วิธี Call ผ่าน HTTP
```http
POST http://localhost:5152/api/books/workflow/approved?original_org_code=ORG001&destination_org_code=ORG002&tranfer_id=TRF-20251031-0001
Content-Type: application/json

{
  "user_ad": "EXAT\\TESTUSER01",
  "book_subject": "ทดสอบ Workflow - Approved",
  "book_to": "ผู้อำนวยการ ฝ่ายวิศวกรรม",
  "registrationbook_id": "101",
  "bookFile": [
    {
      "file_name": "approved-doc.pdf",
      "file_content": "JVBERi0xLjQKJeLjz9MK...",
      "file_extension": ".pdf"
    }
  ],
  "bookAttach": [
    {
      "file_name": "attachment.pdf",
      "file_content": "JVBERi0xLjQKJeLjz9MK...",
      "file_extension": ".pdf"
    }
  ]
}
```

#### วิธี Call ใน K2 Workflow

**Step 1: เตรียมข้อมูล**
```
SmartObject: BooksWorkflow
Method: CreateApprovedWorkflow
```

**Step 2: Input Mapping**
```
user_ad = "EXAT\\" + ProcessData.Username
book_subject = ProcessData.Subject
book_to = ProcessData.RecipientName
registrationbook_id = ProcessData.RegistrationID
original_org_code = ProcessData.SourceOrganization
destination_org_code = ProcessData.DestinationOrganization
bookFile = ProcessData.Files (Collection)
bookAttach = ProcessData.Attachments (Collection, Optional)
```

**Step 3: Output Mapping**
```
ProcessData.BookID = Response.data.book_id
ProcessData.BookCode = Response.data.book_code
ProcessData.FileCount = Response.data.file_count
ProcessData.AttachCount = Response.data.attach_count
ProcessData.TransferStatus = Response.data.transfer_status
ProcessData.WorkflowType = Response.data.workflow_type
```

#### Response ที่คาดหวัง
```json
{
  "success": true,
  "message": "Workflow ทำงานสำเร็จครบทั้ง 3 ขั้นตอน",
  "data": {
    "book_id": "35d29ccb-d526-4a75-af66-6b56a08a48e4",
    "book_code": "APV-20251030-1712",
    "file_count": 1,
    "attach_count": 1,
    "create_message": "เอกสารถูกสร้างสำเร็จ",
    "generated_code": "DOC-20251030-xxxxx",
    "transfer_status": "COMPLETED",
    "workflow_type": "APPROVED",
    "executed_by": "EXAT\\TESTUSER01"
  }
}
```

#### ฟิลด์ที่จำเป็น (Required)
- ✅ `user_ad` - Username (รูปแบบ: EXAT\\username)
- ✅ `book_subject` - หัวข้อเอกสาร
- ✅ `book_to` - ผู้รับเอกสาร
- ✅ `registrationbook_id` - รหัสทะเบียน
- ⭕ `original_org_code` - ถ้าไม่ส่งจะใช้ค่าเริ่มต้น "J10100"
- ⭕ `destination_org_code` - ถ้าไม่ส่งจะใช้ค่าเริ่มต้น "J10000"
- ✅ `bookFile` - ไฟล์เอกสาร (อย่างน้อย 1 ไฟล์)

#### ฟิลด์ที่ Optional
- ⭕ `bookAttach` - ไฟล์แนบเพิ่มเติม

---

### 🔄 API 2: POST /api/books/workflow/non-compliant

**คำอธิบาย:** สร้างเอกสาร Workflow แบบไม่เข้าหลักเกณ์ (ทำ 3 ขั้นตอน: Create → Generate Code → Transfer)

#### วิธี Call ผ่าน HTTP
```http
POST http://localhost:5152/api/books/workflow/non-compliant?original_org_code=ORG003&destination_org_code=ORG004&tranfer_id=TRF-20251031-0002
Content-Type: application/json

{
  "user_ad": "EXAT\\ADMIN01",
  "book_subject": "ทดสอบ Workflow - Non-Compliant",
  "book_to": "ผู้จัดการ ฝ่ายบริหาร",
  "registrationbook_id": "201",
  "bookFile": [
    {
      "file_name": "non-compliant.pdf",
      "file_content": "JVBERi0xLjQKJeLjz9MK...",
      "file_extension": ".pdf"
    }
  ]
}
```

#### วิธี Call ใน K2 Workflow

**Input Mapping** (เหมือนกับ API 1)
```
user_ad = "EXAT\\" + ProcessData.Username
book_subject = ProcessData.Subject
book_to = ProcessData.RecipientName
registrationbook_id = ProcessData.RegistrationID
original_org_code = ProcessData.SourceOrganization
destination_org_code = ProcessData.DestinationOrganization
bookFile = ProcessData.Files
```

#### Response ที่คาดหวัง
```json
{
  "success": true,
  "message": "Workflow ทำงานสำเร็จครบทั้ง 3 ขั้นตอน",
  "data": {
    "book_id": "0c6f9e40-4c72-4b99-a627-a1f3b23bf432",
    "book_code": "NCL-20251030-7115",
    "file_count": 1,
    "attach_count": 0,
    "transfer_status": "COMPLETED",
    "workflow_type": "NON_COMPLIANT",
    "executed_by": "EXAT\\ADMIN01"
  }
}
```

**สังเกต:** 
- Book Code จะเริ่มด้วย `NCL-` (Non-Compliant)
- ฟิลด์ที่จำเป็นเหมือนกับ API 1

---

### 🔄 API 3: POST /api/books/workflow/under-construction

**คำอธิบาย:** สร้างเอกสาร Workflow แบบระหว่างก่อสร้าง (ทำ 3 ขั้นตอน: Create → Generate Code → Transfer)

#### วิธี Call ผ่าน HTTP
```http
POST http://localhost:5152/api/books/workflow/under-construction?original_org_code=ORG005&destination_org_code=ORG006&tranfer_id=TRF-20251031-0003
Content-Type: application/json

{
  "user_ad": "EXAT\\ENGINEER01",
  "book_subject": "ทดสอบ Workflow - Under Construction",
  "book_to": "วิศวกร ฝ่ายก่อสร้าง",
  "registrationbook_id": "301",
  "bookFile": [
    {
      "file_name": "construction-plan.pdf",
      "file_content": "JVBERi0xLjQKJeLjz9MK...",
      "file_extension": ".pdf"
    }
  ],
  "bookAttach": [
    {
      "file_name": "site-photo1.jpg",
      "file_content": "/9j/4AAQSkZJRgABAQEA...",
      "file_extension": ".jpg"
    },
    {
      "file_name": "site-photo2.jpg",
      "file_content": "/9j/4AAQSkZJRgABAQEA...",
      "file_extension": ".jpg"
    }
  ]
}
```

#### วิธี Call ใน K2 Workflow

**Input Mapping** (เหมือนกับ API 1 และ 2)
```
SmartObject: BooksWorkflow
Method: CreateUnderConstructionWorkflow
```

#### Response ที่คาดหวัง
```json
{
  "success": true,
  "message": "Workflow ทำงานสำเร็จครบทั้ง 3 ขั้นตอน",
  "data": {
    "book_id": "d8ff26c2-6b81-41c3-9062-bdf4a8115ad8",
    "book_code": "UNC-20251030-4494",
    "file_count": 1,
    "attach_count": 2,
    "transfer_status": "COMPLETED",
    "workflow_type": "UNDER_CONSTRUCTION",
    "executed_by": "EXAT\\ENGINEER01"
  }
}
```

**สังเกต:** 
- Book Code จะเริ่มด้วย `UNC-` (Under-Construction)
- สามารถแนบไฟล์ได้หลายไฟล์ใน `bookAttach`

---

### 🔍 API 4: GET /api/books/final-orgs/by-action

**คำอธิบาย:** ดึงข้อมูลองค์กรปลายทาง **พร้อมส่ง Alert** ไปยังองค์กรที่เกี่ยวข้อง

#### วิธี Call ผ่าน HTTP
```http
GET http://localhost:5152/api/books/final-orgs/by-action?user_ad=EXAT\TESTUSER01&book_id=35d29ccb-d526-4a75-af66-6b56a08a48e4
```

#### วิธี Call ใน K2 Workflow

**Step 1: เตรียม Query**
```
SmartObject: BooksFinalOrgs
Method: GetFinalOrganizations
```

**Step 2: Input Mapping**
```
user_ad = "EXAT\\" + ProcessData.Username
book_id = ProcessData.BookID (จาก Workflow API ที่สร้างไว้)
```

**Step 3: Output Mapping**
```
ProcessData.BookID = Response.data.bookId
ProcessData.HasAlert = Response.data.hasAlert
ProcessData.AlertMessage = Response.data.alertMessage
ProcessData.Organizations = Response.data.organizations (Collection)
```

#### Response ที่คาดหวัง
```json
{
  "success": true,
  "message": "ดึงข้อมูลสำเร็จ",
  "data": {
    "bookId": "35d29ccb-d526-4a75-af66-6b56a08a48e4",
    "hasAlert": true,
    "alertMessage": "การแจ้งเตือนถูกส่งไปยังองค์กรที่เกี่ยวข้องเรียบร้อยแล้ว",
    "organizations": [
      {
        "org_code": "ORG002",
        "org_name": "ฝ่ายวิศวกรรม",
        "org_type": "DEPARTMENT",
        "contact_person": "นายสมชาย ใจดี",
        "contact_email": "somchai@exat.co.th"
      }
    ]
  }
}
```

#### Query Parameters (Required)
- ✅ `user_ad` - Username (รูปแบบ: EXAT\username)
- ✅ `book_id` - รหัสเอกสาร (GUID)

**สังเกต:**
- ⚠️ `hasAlert` = `true` → ระบบจะส่ง Alert ไปยังองค์กร
- 🔔 `alertMessage` จะมีข้อความยืนยันการส่ง alert

---

### 🔍 API 5: GET /api/books/final-orgs/by-action/no-alert

**คำอธิบาย:** ดึงข้อมูลองค์กรปลายทาง **โดยไม่ส่ง Alert** (ใช้สำหรับ query ข้อมูลเท่านั้น)

#### วิธี Call ผ่าน HTTP
```http
GET http://localhost:5152/api/books/final-orgs/by-action/no-alert?user_ad=EXAT\TESTUSER01&book_id=35d29ccb-d526-4a75-af66-6b56a08a48e4
```

#### วิธี Call ใน K2 Workflow

**Step 1: เตรียม Query**
```
SmartObject: BooksFinalOrgs
Method: GetFinalOrganizationsNoAlert
```

**Step 2: Input Mapping** (เหมือนกับ API 4)
```
user_ad = "EXAT\\" + ProcessData.Username
book_id = ProcessData.BookID
```

**Step 3: Output Mapping**
```
ProcessData.BookID = Response.data.bookId
ProcessData.HasAlert = Response.data.hasAlert
ProcessData.Organizations = Response.data.organizations
```

#### Response ที่คาดหวัง
```json
{
  "success": true,
  "message": "ดึงข้อมูลสำเร็จ",
  "data": {
    "bookId": "35d29ccb-d526-4a75-af66-6b56a08a48e4",
    "hasAlert": false,
    "alertMessage": null,
    "organizations": [
      {
        "org_code": "ORG002",
        "org_name": "ฝ่ายวิศวกรรม",
        "org_type": "DEPARTMENT",
        "contact_person": "นายสมชาย ใจดี",
        "contact_email": "somchai@exat.co.th"
      }
    ]
  }
}
```

**สังเกต:**
- ✅ `hasAlert` = `false` → ระบบ**ไม่**ส่ง Alert
- ⭕ `alertMessage` = `null` → ไม่มีข้อความแจ้งเตือน
- 📊 ข้อมูล organizations เหมือนกับ API 4

---

## 🎯 เปรียบเทียบ API 4 vs API 5

| Feature | /by-action (API 4) | /by-action/no-alert (API 5) |
|---------|-------------------|----------------------------|
| **ส่ง Alert** | ✅ ส่ง | ❌ ไม่ส่ง |
| **hasAlert** | `true` | `false` |
| **alertMessage** | มีข้อความ | `null` |
| **Organizations Data** | ✅ เหมือนกัน | ✅ เหมือนกัน |
| **Use Case** | User Action, ต้องการแจ้งเตือน | Query ข้อมูล, แสดงผลเท่านั้น |

---

## 💡 ตัวอย่าง K2 Workflow ที่ใช้ทั้ง 5 APIs

### Workflow 1: สร้างเอกสารและดึงข้อมูลองค์กร (Approved)

```
Start
  ↓
[รับข้อมูลจาก Form]
  ↓
[Call API 1: CreateApprovedWorkflow]
  ├─ Input: user_ad, book_subject, book_to, files
  ├─ Output: book_id, book_code
  ↓
[Decision: Success?]
  ├─ Yes → Continue
  └─ No → [Error Handling] → End
  ↓
[Wait 2 seconds] (ให้ระบบประมวลผล)
  ↓
[Call API 4: GetFinalOrganizations (with Alert)]
  ├─ Input: user_ad, book_id
  ├─ Output: organizations, alertMessage
  ↓
[Send Email Notification to Organizations]
  ↓
[Log Activity]
  ↓
End
```

### Workflow 2: Query ข้อมูลองค์กรโดยไม่ส่ง Alert

```
Start
  ↓
[Get Book ID from Database/Form]
  ↓
[Call API 5: GetFinalOrganizationsNoAlert]
  ├─ Input: user_ad, book_id
  ├─ Output: organizations
  ↓
[Display Organizations in UI]
  ↓
End
```

### Workflow 3: Combined Workflow (3 Types)

```
Start
  ↓
[Decision: Document Type?]
  ├─ Approved → [Call API 1: /workflow/approved]
  ├─ Non-Compliant → [Call API 2: /workflow/non-compliant]
  └─ Under-Construction → [Call API 3: /workflow/under-construction]
  ↓
[Get book_id from Response]
  ↓
[Decision: Send Alert?]
  ├─ Yes → [Call API 4: /by-action]
  └─ No → [Call API 5: /by-action/no-alert]
  ↓
[Process Results]
  ↓
End
```

---

## 🔧 Tips สำหรับการใช้งานใน K2

### 1. การจัดการ Error
```
Try {
  Call API
} Catch {
  If (Error Type = Timeout) {
    Wait 5 seconds
    Retry (Max 3 times)
  } Else {
    Log Error
    Send Notification to Admin
  }
}
```

### 2. การ Validate Input ก่อน Call API
```
If (user_ad is empty) → Show Error
If (book_subject is empty) → Show Error
If (bookFile count = 0) → Show Error
If (org_codes are invalid) → Show Error
```

### 3. การใช้ Collection สำหรับ Multiple Files
```
// สร้าง Collection
Create Collection: ProcessData.Files

// เพิ่มไฟล์
Add Item to Collection:
  - file_name = "file1.pdf"
  - file_content = [Base64 String]
  - file_extension = ".pdf"

// ส่งไปที่ API
bookFile = ProcessData.Files
```

### 4. การ Log Activity
```
Log Entry:
  - Timestamp: Now()
  - User: ProcessData.Username
  - API: "POST /api/books/workflow/approved"
  - BookID: Response.book_id
  - BookCode: Response.book_code
  - Status: "SUCCESS"
```

---

## ✅ Checklist การทดสอบ

- [ ] Test API 1: Create Approved Workflow
  - [ ] With bookFile only
  - [ ] With bookFile + bookAttach
  - [ ] Test required fields validation
  - [ ] Test file upload
- [ ] Test API 2: Create Non-Compliant Workflow
  - [ ] Different user
  - [ ] Verify NCL- prefix
- [ ] Test API 3: Create Under-Construction Workflow
  - [ ] Multiple attachments
  - [ ] Verify UNC- prefix
- [ ] Test API 4: Get Organizations with Alert
  - [ ] Verify hasAlert = true
  - [ ] Verify alertMessage present
- [ ] Test API 5: Get Organizations without Alert
  - [ ] Verify hasAlert = false
  - [ ] Verify alertMessage = null
- [ ] Test Combined Workflow
  - [ ] Create → Query flow
  - [ ] Error handling
  - [ ] Retry logic

---

### 6. ใช้งานใน K2 Workflow

#### 6.1 สร้าง Workflow ใหม่
1. เปิด K2 Designer/Studio
2. สร้าง Workflow ใหม่
3. ลาก SmartObject ที่สร้างไว้มาใช้

#### 6.2 ตัวอย่าง: Create Book Approved Workflow
```
Start → Get Form Data → Call REST API (CreateApprovedWorkflow) → Query Organizations → Send Notification → End
```

**Step: Call REST API**
- SmartObject: BooksWorkflow
- Method: CreateApprovedWorkflow
- Input Mapping:
  - user_ad: `"EXAT\\" + ProcessData.Username`
  - book_subject: `ProcessData.Subject`
  - book_to: `ProcessData.RecipientName`
  - registrationbook_id: `ProcessData.RegistrationID`
  - original_org_code: `ProcessData.SourceOrg`
  - destination_org_code: `ProcessData.DestinationOrg`
  - bookFile: `ProcessData.Files`
  - bookAttach: `ProcessData.Attachments`
- Output Mapping:
  - Save Response: `ProcessData.BookID`, `ProcessData.BookCode`

#### 6.3 ตัวอย่าง: Query Organizations Workflow
```
Start → Get Book ID → Call REST API (GetFinalOrganizations) → Loop Organizations → Process Each → End
```

**Step: Call REST API**
- SmartObject: BooksFinalOrgs
- Method: GetFinalOrganizations (with Alert)
- Input Mapping:
  - user_ad: `"EXAT\\" + ProcessData.Username`
  - book_id: `ProcessData.BookID`
- Output Mapping:
  - Save to Collection: `ProcessData.Organizations`

**Step: Loop Through Organizations**
- Use For Each activity
- Loop through Organizations Collection
- Access properties: org_code, org_name, contact_person, etc.

### 7. Error Handling

#### 7.1 Handle API Errors
REST API ส่ง error ในรูปแบบ:
```json
{
  "success": false,
  "message": "Employee not found",
  "error": "No employee with ID 999",
  "timestamp": "2025-10-30T00:00:00Z"
}
```

ใน K2 Workflow:
1. เพิ่ม Error Handling Path
2. ตรวจสอบ `Success` property
3. ถ้า `Success = false` ให้ไปที่ Error Handling

#### 7.2 Retry Logic
เพิ่ม Retry mechanism สำหรับ transient errors:
```
Try Call API → If Error → Wait 5 sec → Retry (max 3 times) → If still error → Escalate
```

### 8. Best Practices

#### 8.1 Performance
- ใช้ Caching สำหรับข้อมูลที่ไม่เปลี่ยนบ่อย
- Implement pagination สำหรับ list ที่มีข้อมูลเยอะ
- ใช้ Async methods ใน K2

#### 8.2 Security
- ใช้ HTTPS เสมอ
- Implement proper Authentication
- Validate input data
- จำกัด CORS origins

#### 8.3 Monitoring
- Log ทุก API call
- ตั้ง alerts สำหรับ errors
- Monitor response time
- Track usage statistics

## 🔍 Troubleshooting

### ปัญหา: K2 ไม่สามารถเชื่อมต่อ
**วิธีแก้:**
1. ตรวจสอบ Firewall settings
2. ตรวจสอบ SSL certificate
3. ตรวจสอบ CORS configuration
4. ดู K2 Server logs

### ปัญหา: Swagger JSON ไม่ถูกต้อง
**วิธีแก้:**
1. ตรวจสอบว่า `c.SerializeAsV2 = true` ตั้งค่าแล้ว
2. Validate Swagger JSON ที่ https://editor.swagger.io
3. ตรวจสอบ data types ทั้งหมด

### ปัญหา: Response ไม่ตรงกับที่คาดหวัง
**วิธีแก้:**
1. Test API ผ่าน Postman หรือ Swagger UI
2. ตรวจสอบ response mapping
3. ดู K2 execution logs

### ปัญหา: Timeout
**วิธีแก้:**
1. เพิ่ม timeout setting ใน K2
2. Optimize API performance
3. ใช้ async patterns

## 📞 ติดต่อและสนับสนุน

หากพบปัญหาหรือต้องการความช่วยเหลือ:
- ตรวจสอบ API logs
- ตรวจสอบ K2 Server logs
- ติดต่อทีมพัฒนา

---
อัพเดทล่าสุด: 30 ตุลาคม 2025
