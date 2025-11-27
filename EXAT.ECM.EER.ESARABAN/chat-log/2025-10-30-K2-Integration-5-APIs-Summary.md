# Chat Log - K2 Integration Guide Summary for 5 APIs

**วันที่:** 30 ตุลาคม 2025  
**หัวข้อ:** สรุปข้อมูล K2_INTEGRATION_GUIDE.md สำหรับ 5 APIs  
**ผู้สนทนา:** User & GitHub Copilot

---

## 📋 คำถาม

**User:** สรุปข้อมูล K2_INTEGRATION_GUILD.md ของ API ดังนี้

### Workflow APIs (3 endpoints)
1. `POST /api/books/workflow/approved` - Workflow แบบอนุมัติ/เข้าหลักเกณ์
2. `POST /api/books/workflow/non-compliant` - Workflow แบบไม่เข้าหลักเกณ์
3. `POST /api/books/workflow/under-construction` - Workflow แบบระหว่างก่อสร้าง

### Query APIs (2 endpoints)
4. `GET /api/books/final-orgs/by-action` - ดึงข้อมูลองค์กรปลายทาง (มี Alert)
5. `GET /api/books/final-orgs/by-action/no-alert` - ดึงข้อมูลองค์กรปลายทาง (ไม่มี Alert)

---

## 💡 สรุปคำตอบ

### 🎯 ภาพรวมการเชื่อมต่อ K2 SmartObject

**Swagger JSON Endpoint:**
- Development: `http://localhost:5152/swagger/v1/swagger.json`
- Production: `http://your-server:port/swagger/v1/swagger.json`

**สิ่งสำคัญ:**
- ✅ **ไม่ต้องมีไฟล์ swagger.yml** - K2 อ่าน JSON โดยตรง
- ✅ OpenAPI 2.0 (K2 Compatible)
- ✅ Auto-generate SmartObjects จาก API ทั้งหมด

---

## 🔧 ขั้นตอนการตั้งค่า K2 (4 Steps)

### **Step 1: เริ่ม API Server**
```powershell
dotnet run --project K2RestApi.csproj
```

### **Step 2: เปิด K2 Management**
1. K2 Management Console → **Service Instances**
2. คลิก **New Service Instance**
3. เลือก **REST Service Broker**

### **Step 3: กำหนด Service URL**
```
Display Name: eSaraban Books API
Base URL: http://localhost:5152
Swagger URL: http://localhost:5152/swagger/v1/swagger.json
```

### **Step 4: Import Swagger**
1. เลือก **Import from Swagger/OpenAPI**
2. ใส่ Swagger JSON URL
3. คลิก **Import** → K2 สร้าง SmartObjects อัตโนมัติ ✨

---

## 📚 SmartObjects ที่จะถูกสร้าง (5 APIs)

### 🔄 **Workflow SmartObjects (3 Methods)**

#### 1. **WorkflowApproved** SmartObject
**Method:** `CreateApprovedWorkflow`
- **HTTP:** POST `/api/books/workflow/approved`
- **Input Parameters:**
  - user_ad (string) - Active Directory username
  - book_subject (string) - หัวข้อเอกสาร
  - book_to (string) - ผู้รับเอกสาร
  - registrationbook_id (string) - รหัสทะเบียน
  - original_org_code (string) - รหัสองค์กรต้นทาง
  - destination_org_code (string) - รหัสองค์กรปลายทาง
  - bookFile (array) - ไฟล์เอกสาร
  - bookAttach (array, optional) - ไฟล์แนบ
- **Output:**
  - book_id (guid) - รหัสเอกสาร
  - book_code (string) - รหัสเอกสาร (APV-20251030-xxxx)
  - transfer_status (string) - สถานะการโอนย้าย
  - workflow_type (string) - "APPROVED"

#### 2. **WorkflowNonCompliant** SmartObject
**Method:** `CreateNonCompliantWorkflow`
- **HTTP:** POST `/api/books/workflow/non-compliant`
- **Output:**
  - book_code (string) - รหัสเอกสาร (NCL-20251030-xxxx)
  - workflow_type (string) - "NON_COMPLIANT"

#### 3. **WorkflowUnderConstruction** SmartObject
**Method:** `CreateUnderConstructionWorkflow`
- **HTTP:** POST `/api/books/workflow/under-construction`
- **Output:**
  - book_code (string) - รหัสเอกสาร (UNC-20251030-xxxx)
  - workflow_type (string) - "UNDER_CONSTRUCTION"

---

### 🔍 **Query SmartObjects (2 Methods)**

#### 4. **FinalOrgsWithAlert** SmartObject
**Method:** `GetFinalOrganizations`
- **HTTP:** GET `/api/books/final-orgs/by-action`
- **Query Parameters:**
  - user_ad (string) - Active Directory username
  - book_id (guid) - รหัสเอกสาร
- **Output:**
  - bookId (guid)
  - hasAlert (boolean) - **true** 🔔
  - alertMessage (string) - "การแจ้งเตือนถูกส่ง..."
  - organizations (array) - รายการองค์กรปลายทาง

#### 5. **FinalOrgsNoAlert** SmartObject
**Method:** `GetFinalOrganizationsNoAlert`
- **HTTP:** GET `/api/books/final-orgs/by-action/no-alert`
- **Output:**
  - hasAlert (boolean) - **false** 🔕
  - alertMessage (null) - ไม่มีการแจ้งเตือน

---

## 💡 ตัวอย่างการใช้งานใน K2 Workflow

### **Scenario 1: Approved Book Workflow**
```
Start 
  ↓
[Get Form Data]
  ↓
[Call WorkflowApproved.CreateApprovedWorkflow]
  ├─ Input: user_ad, book_subject, book_to, files
  ├─ Output: book_id, book_code
  ↓
[Decision: Success?]
  ├─ Yes → [Send Notification] → End
  └─ No → [Error Handling] → End
```

**Input Mapping ใน K2:**
```
user_ad = ProcessData.UserAD
book_subject = ProcessData.Subject
book_to = ProcessData.RecipientName
registrationbook_id = ProcessData.RegistrationID
original_org_code = ProcessData.SourceOrg
destination_org_code = ProcessData.DestinationOrg
bookFile = ProcessData.Files
```

**Output Mapping:**
```
ProcessData.BookID = Response.book_id
ProcessData.BookCode = Response.book_code
ProcessData.TransferStatus = Response.transfer_status
```

---

### **Scenario 2: Query Organizations with Alert**
```
Start
  ↓
[Get Book ID from previous step]
  ↓
[Call FinalOrgsWithAlert.GetFinalOrganizations]
  ├─ Input: user_ad, book_id
  ├─ Output: organizations, alertMessage
  ↓
[Loop Through Organizations]
  ↓
[Log Alert Message]
  ↓
End
```

---

### **Scenario 3: Combined Workflow (Create + Query)**
```
Start
  ↓
[Call WorkflowApproved] → Get book_id
  ↓
[Wait 2 seconds]
  ↓
[Call FinalOrgsNoAlert] → Get organizations (ไม่ส่ง alert)
  ↓
[Display Results to User]
  ↓
End
```

---

## ⚙️ Error Handling ใน K2

### **Response Structure**
```json
{
  "success": true/false,
  "message": "Success or error message",
  "data": { ... }
}
```

### **Error Handling Steps**
1. ตรวจสอบ `success` property
2. ถ้า `success = false`:
   - Log error message
   - แจ้งเตือนผู้ใช้
   - Retry (ถ้าเหมาะสม)
3. ถ้า timeout:
   - Wait 5 seconds
   - Retry (max 3 attempts)
   - Escalate ถ้ายังไม่สำเร็จ

---

## 🎨 Best Practices สำหรับ 5 APIs

### **1. Workflow APIs (POST)**
✅ **ควรทำ:**
- Validate input ก่อนเรียก API
- ตรวจสอบ file size และ type
- บันทึก book_id ไว้ใช้ใน step ถัดไป
- Handle timeout (API อาจใช้เวลาในการอัปโหลดไฟล์)
- Log ทุก API call

❌ **ไม่ควรทำ:**
- เรียก API ซ้ำโดยไม่จำเป็น (ทำให้เกิด duplicate books)
- ส่งไฟล์ขนาดใหญ่เกินไป
- ละเว้นการจัดการ error

### **2. Query APIs (GET)**
✅ **ควรทำ:**
- ใช้ `/by-action` เมื่อต้องการส่ง alert
- ใช้ `/no-alert` เมื่อเป็นการ query ข้อมูลเพื่อแสดงผลเท่านั้น
- Cache ผลลัพธ์ถ้าไม่เปลี่ยนบ่อย
- ตรวจสอบ hasAlert flag

❌ **ไม่ควรทำ:**
- เรียก `/by-action` บ่อยเกินไป (ส่ง alert ซ้ำ)
- ใช้ผิด endpoint (ส่ง alert เมื่อไม่จำเป็น)

---

## 🔍 Troubleshooting 5 APIs

### **ปัญหา: Workflow API ไม่สำเร็จ**
**สาเหตุที่เป็นไปได้:**
- ❌ Missing required fields (user_ad, book_subject, book_to, registrationbook_id)
- ❌ Invalid file format
- ❌ File size เกินขนาด
- ❌ Invalid org_code

**วิธีแก้:**
1. Validate input ใน K2 ก่อนเรียก API
2. ตรวจสอบ error message ใน response
3. ดู API logs
4. Test ผ่าน Swagger UI

### **ปัญหา: Query API ส่ง alert ซ้ำ**
**สาเหตุ:**
- ❌ เรียก `/by-action` หลายครั้ง

**วิธีแก้:**
- ใช้ `/no-alert` สำหรับ query ธรรมดา
- บันทึก flag ว่าส่ง alert แล้ว
- ใช้ `/by-action` เฉพาะเมื่อ user action

### **ปัญหา: Timeout**
**วิธีแก้:**
1. เพิ่ม timeout ใน K2 Service Instance (default: 30 sec → 60 sec)
2. ลดขนาดไฟล์
3. ตรวจสอบ network connectivity

---

## 📈 Monitoring & Logging

### **ควร Monitor:**
- ✅ API Response Time
- ✅ Success/Failure Rate
- ✅ Books Created Count
- ✅ Alert Sent Count
- ✅ Error Types

### **ควร Log:**
- ✅ ทุก API Call (timestamp, user, parameters)
- ✅ Book IDs ที่สร้าง
- ✅ Transfer Status
- ✅ Errors และ Exceptions

---

## ✅ Checklist ก่อน Deploy Production

- [ ] ตั้งค่า Production URL
- [ ] เปิดใช้ HTTPS
- [ ] Configure Authentication
- [ ] ตรวจสอบ CORS settings
- [ ] Test ทุก API ผ่าน K2
- [ ] ตั้งค่า Logging
- [ ] ตั้งค่า Monitoring
- [ ] Backup plan
- [ ] Documentation สำหรับ users

---

## 📊 ผลการทดสอบ

**Test Results (จาก TEST_REPORT_COMPREHENSIVE_5_APIs.md):**
- ✅ 9/9 Test Cases Passed (100%)
- ✅ 3 Books Created Successfully
  - APV-20251030-1712 (Approved)
  - NCL-20251030-7115 (Non-Compliant)
  - UNC-20251030-4494 (Under-Construction)
- ✅ Response Time < 1 second
- ✅ Multi-user Support Verified
- ✅ File Handling Working (bookFile + bookAttach)
- ✅ Alert Management Tested

---

## 📞 เอกสารอ้างอิง

**เอกสารที่เกี่ยวข้อง:**
1. `RefDocuments/K2_INTEGRATION_GUIDE.md` - คู่มือ K2 แบบละเอียด
2. `RefDocuments/TEST_REPORT_COMPREHENSIVE_5_APIs.md` - ผลการทดสอบ 5 APIs
3. `RefDocuments/PROJECT_SUMMARY.md` - ภาพรวม Project
4. `.github/copilot-instructions.md` - คำแนะนำสำหรับ GitHub Copilot

---

## 🎯 สรุป

**การเชื่อมต่อ K2 SmartObject:**
- ✅ ไม่ต้องมีไฟล์ swagger.yml
- ✅ ใช้ Swagger JSON URL โดยตรง
- ✅ K2 สร้าง SmartObjects อัตโนมัติจาก 21 APIs
- ✅ ทดสอบครบถ้วนแล้ว (100% success)
- ✅ พร้อม Deploy Production

**API ทั้ง 5 ตัวพร้อมใช้งานกับ K2 SmartObject! 🎉**

---

**บันทึกโดย:** GitHub Copilot  
**วันที่บันทึก:** 30 ตุลาคม 2025  
**สถานะ:** ✅ Complete
