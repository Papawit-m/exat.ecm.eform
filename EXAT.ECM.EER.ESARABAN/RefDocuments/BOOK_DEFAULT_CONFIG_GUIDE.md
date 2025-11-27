# คู่มือการตั้งค่า Book Default Configuration

## 📋 ภาพรวม

ระบบ Book API มี Configuration สำหรับการตั้งค่า default values ของ Request Body ทั้ง 4 endpoints:
- `/api/books/create/original`
- `/api/books/create/approved`
- `/api/books/create/non-compliant`
- `/api/books/create/under-construction`

Configuration นี้ช่วยให้สามารถปรับแต่งค่า default ได้ง่ายโดยไม่ต้องแก้ไข code

---

## 📁 ไฟล์ที่เกี่ยวข้อง

### 1. **Models/BookDefaultSettings.cs**
   - Model class สำหรับรับค่า configuration
   - ประกอบด้วย:
     - `BookDataDefaults` - ค่า default สำหรับข้อมูลหลักของเอกสาร
     - `BookFileDefaults` - ค่า default สำหรับไฟล์เอกสาร
     - `BookHistoryDefaults` - ค่า default สำหรับประวัติการดำเนินการ
     - `EndpointDefaults` - ค่า default เฉพาะของแต่ละ endpoint

### 2. **appsettings.json**
   - ไฟล์ configuration หลักที่เก็บค่า default ทั้งหมด
   - อยู่ใน section `"BookDefaultSettings"`

### 3. **Controllers/BooksController.cs**
   - Controller ที่ใช้ configuration
   - มี helper methods สำหรับ apply defaults และ generate book code

---

## 🔧 โครงสร้าง Configuration

### **BookDefaultSettings Section**

```json
{
  "BookDefaultSettings": {
    "BookData": { ... },           // ค่า default ทั่วไปสำหรับ BookData
    "BookFile": { ... },           // ค่า default สำหรับ BookFile
    "BookHistory": { ... },        // ค่า default สำหรับ BookHistory
    "Endpoints": {                 // ค่า default เฉพาะของแต่ละ endpoint
      "Original": { ... },
      "Approved": { ... },
      "NonCompliant": { ... },
      "UnderConstruction": { ... }
    }
  }
}
```

---

## 📝 รายละเอียด Configuration

### **1. BookData Defaults (ค่า default ทั่วไป)**

```json
"BookData": {
  "RegistrationBookId": null,
  "RegistrationBookNameTh": null,
  "RegistrationBookNameEn": null,
  "RegistrationBookOrgId": null,
  "RegistrationBookOrgCode": null,
  "RegistrationBookOrgNameTh": null,
  "RegistrationBookOrgNameEn": null,
  "RegistrationBookOrgShtName": null,
  "BookTypeId": 1,              // ประเภทหนังสือ (default: 1)
  "SendTypeId": 1,              // ประเภทการส่ง (default: 1)
  "FormatId": 1,                // รูปแบบ (default: 1)
  "SubFormatId": null,
  "SpeedId": 2,                 // ความเร็ว (default: 2)
  "SecretId": 1,                // ความลับ (default: 1)
  "OptionDateId": 1,            // ตัวเลือกวันที่ (default: 1)
  "OptionLanguageId": 1,        // ภาษา (default: 1)
  "OptionNoId": 1,              // ตัวเลือกเลขที่ (default: 1)
  "StatusId": 1,                // สถานะ (default: 1)
  "RequestOrgCode": null,
  "CreatePage": 1,              // หน้าสร้าง (default: 1)
  "IsCircular": false           // เป็นหนังสือเวียนหรือไม่ (default: false)
}
```

**หมายเหตุ**: 
- ค่า `null` จะไม่ถูก apply (ใช้ค่าจาก request body)
- ค่า default จะถูก apply เมื่อ field ใน request มีค่า `0` หรือ `null`

---

### **2. BookFile Defaults (ค่า default สำหรับไฟล์)**

```json
"BookFile": {
  "FileExtension": ".pdf",              // นามสกุลไฟล์ default
  "FilePath": "/documents/books",       // path สำหรับเก็บไฟล์
  "FileUrl": null,
  "AlfrescoParentId": null,
  "AlfrescoFolderName": "Books",        // ชื่อ folder ใน Alfresco
  "AlfrescoNodeType": "cm:content"      // ประเภท node ใน Alfresco
}
```

---

### **3. BookHistory Defaults (ค่า default สำหรับประวัติ)**

```json
"BookHistory": {
  "Action": "CREATE",                               // Action default
  "ActionBy": null,
  "Remark": "สร้างเอกสารผ่าน K2 REST API"          // หมายเหตุ default
}
```

---

### **4. Endpoint-Specific Defaults (ค่า default เฉพาะแต่ละ endpoint)**

#### **Original Endpoint**
```json
"Original": {
  "BookCodePrefix": "BK-",                          // Prefix รหัสเอกสาร
  "StatusId": 1,                                    // Status ID เฉพาะ
  "HistoryAction": "CREATE_ORIGINAL",               // Action ที่บันทึกใน History
  "Description": "สร้างเอกสารทั่วไป (General Purpose)",
  "CustomDefaults": {
    "create_page": 1
  }
}
```

#### **Approved Endpoint**
```json
"Approved": {
  "BookCodePrefix": "APV-",
  "StatusId": 2,
  "HistoryAction": "CREATE_APPROVED",
  "Description": "สร้างเอกสารกรณีอนุมัติ/เข้าสู่หลักเกณ์",
  "CustomDefaults": {
    "create_page": 2,
    "status_note": "อนุมัติและเข้าสู่หลักเกณ์แล้ว"
  }
}
```

#### **NonCompliant Endpoint**
```json
"NonCompliant": {
  "BookCodePrefix": "NCL-",
  "StatusId": 3,
  "HistoryAction": "CREATE_NON_COMPLIANT",
  "Description": "สร้างเอกสารกรณีไม่เข้าหลักเกณ์",
  "CustomDefaults": {
    "create_page": 3,
    "status_note": "ไม่เข้าหลักเกณ์"
  }
}
```

#### **UnderConstruction Endpoint**
```json
"UnderConstruction": {
  "BookCodePrefix": "UNC-",
  "StatusId": 4,
  "HistoryAction": "CREATE_UNDER_CONSTRUCTION",
  "Description": "สร้างเอกสารกรณีอยู่ระหว่างก่อสร้าง",
  "CustomDefaults": {
    "create_page": 4,
    "status_note": "อยู่ระหว่างก่อสร้าง"
  }
}
```

---

## 🔄 กลไกการทำงาน

### **1. Apply Defaults Process**

เมื่อมี request เข้ามา ระบบจะ:

1. **อ่านค่า Configuration** จาก `appsettings.json`
2. **เรียก `ApplyDefaults(request, endpointType)`**
   - `endpointType` เป็น: `"original"`, `"approved"`, `"non-compliant"`, หรือ `"under-construction"`
3. **Apply defaults ตามลำดับ**:
   - BookData defaults (ทั่วไป + เฉพาะ endpoint)
   - BookFile defaults
   - BookHistory defaults
   - BookAttachment defaults
   - BookReferenceAttachment defaults

### **2. Book Code Generation**

รหัสเอกสารถูกสร้างโดย:
```csharp
string bookCode = GenerateBookCode(endpointType);
```

รูปแบบ: `{Prefix}-YYYYMMDD-XXXX`
- **Prefix**: มาจาก configuration (`BookCodePrefix`)
- **YYYYMMDD**: วันที่ปัจจุบัน
- **XXXX**: เลขสุ่ม 4 หัก (1000-9999)

**ตัวอย่าง**:
- Original: `BK-20251030-5809`
- Approved: `APV-20251030-4173`
- NonCompliant: `NCL-20251030-9323`
- UnderConstruction: `UNC-20251030-8240`

---

## 🛠️ วิธีการปรับแต่ง Configuration

### **ตัวอย่างที่ 1: เปลี่ยน Book Code Prefix**

**Before:**
```json
"Original": {
  "BookCodePrefix": "BK-"
}
```

**After:**
```json
"Original": {
  "BookCodePrefix": "DOC-"
}
```

**Result**: รหัสเอกสารจะเป็น `DOC-20251030-XXXX`

---

### **ตัวอย่างที่ 2: เปลี่ยนค่า default ของ Speed**

**Before:**
```json
"BookData": {
  "SpeedId": 2
}
```

**After:**
```json
"BookData": {
  "SpeedId": 3
}
```

**Result**: ถ้า request body ไม่ส่ง `speed_id` มา ระบบจะใช้ค่า `3`

---

### **ตัวอย่างที่ 3: เพิ่ม Custom Default สำหรับ Approved**

**Before:**
```json
"Approved": {
  "CustomDefaults": {
    "create_page": 2
  }
}
```

**After:**
```json
"Approved": {
  "CustomDefaults": {
    "create_page": 2,
    "status_note": "อนุมัติและเข้าสู่หลักเกณ์แล้ว",
    "is_urgent": 1
  }
}
```

**Result**: Field `is_urgent` จะถูกเพิ่มเข้าไปใน `BookData` โดยอัตโนมัติ

---

### **ตัวอย่างที่ 4: เปลี่ยน History Action**

**Before:**
```json
"NonCompliant": {
  "HistoryAction": "CREATE_NON_COMPLIANT"
}
```

**After:**
```json
"NonCompliant": {
  "HistoryAction": "CREATE_REJECTED"
}
```

**Result**: ใน `BookHistory` field `action` จะมีค่าเป็น `"CREATE_REJECTED"`

---

## 📌 กฎการ Apply Defaults

### **1. BookData Fields**
- **Integer fields** (booktype_id, sendtype_id, etc.):
  - Apply default เมื่อค่าใน request = `0`
  - ไม่ apply เมื่อมีค่ามากกว่า `0`

- **String fields**:
  - Apply default เมื่อค่าใน request = `null` หรือ empty string
  - ไม่ apply เมื่อมีค่าแล้ว

### **2. Endpoint-Specific Defaults**
- มีความสำคัญ**สูงกว่า** general defaults
- `StatusId` จาก endpoint config จะ override `StatusId` ทั่วไป
- `CustomDefaults` สามารถเพิ่ม field ใหม่ได้

### **3. Null Handling**
- Configuration ที่มีค่า `null` จะ**ไม่ถูก apply**
- เหมาะสำหรับ field ที่ต้องการให้ user ส่งมาเอง

---

## 🎯 Use Cases และ Best Practices

### **Use Case 1: ตั้งค่า Default Organization**

```json
"BookData": {
  "RegistrationBookOrgCode": "J10000",
  "RegistrationBookOrgNameTh": "สำนักงานใหญ่",
  "RequestOrgCode": "J10000"
}
```

**เมื่อไหร่ใช้**: เมื่อส่วนใหญ่ของเอกสารมาจากองค์กรเดียวกัน

---

### **Use Case 2: ตั้งค่า Default ตาม Environment**

**Development** (`appsettings.Development.json`):
```json
"BookData": {
  "BookTypeId": 999,
  "StatusId": 0
}
```

**Production** (`appsettings.Production.json`):
```json
"BookData": {
  "BookTypeId": 1,
  "StatusId": 1
}
```

---

### **Use Case 3: Custom Fields สำหรับ Special Case**

```json
"UnderConstruction": {
  "CustomDefaults": {
    "create_page": 4,
    "project_type": "construction",
    "requires_consultant": true,
    "consultant_level": "senior"
  }
}
```

---

## ⚙️ Code Implementation

### **Program.cs - Register Configuration**

```csharp
// Register Book Default Settings
builder.Services.Configure<BookDefaultSettings>(
    builder.Configuration.GetSection("BookDefaultSettings"));
```

### **BooksController.cs - Inject และใช้งาน**

```csharp
private readonly BookDefaultSettings _bookDefaults;

public BooksController(
    ILogger<BooksController> logger,
    IOptions<BookDefaultSettings> bookDefaults)
{
    _logger = logger;
    _bookDefaults = bookDefaults.Value;
}

// Apply defaults
ApplyDefaults(request, "original");

// Generate book code
var bookCode = GenerateBookCode("original");
```

---

## 🧪 การทดสอบ Configuration

### **1. ทดสอบ Default Values**

**Request Body** (ไม่ส่ง optional fields):
```json
{
  "user_ad": "testuser",
  "book": {
    "book_owner": "Test Owner",
    "book_subject": "Test Subject",
    "book_to": "Test Recipient",
    "registrationbook_id": "REG001",
    "booktype_id": 0,
    "sendtype_id": 0
  }
}
```

**Expected Result** (หลัง apply defaults):
```json
{
  "booktype_id": 1,    // จาก BookData.BookTypeId
  "sendtype_id": 1,    // จาก BookData.SendTypeId
  "format_id": 1,      // จาก BookData.FormatId
  "speed_id": 2,       // จาก BookData.SpeedId
  "secret_id": 1,      // จาก BookData.SecretId
  "status_id": 1       // จาก Endpoints.Original.StatusId
}
```

---

### **2. ทดสอบ Endpoint-Specific Values**

**Endpoint**: `/api/books/create/approved`

**Expected**:
- `book_code`: `APV-YYYYMMDD-XXXX`
- `status_id`: `2` (จาก Endpoints.Approved.StatusId)
- `history.action`: `CREATE_APPROVED`

---

### **3. ทดสอบ Custom Defaults**

**Configuration**:
```json
"Approved": {
  "CustomDefaults": {
    "is_urgent": 1
  }
}
```

**Expected**: Field `is_urgent` = `1` ใน BookData

---

## 📊 ตาราง Summary

| Configuration Section | Purpose | Apply เมื่อไหร่ |
|----------------------|---------|----------------|
| `BookData` | ค่า default ทั่วไป | Field = 0 หรือ null |
| `BookFile` | ค่า default ไฟล์ | Field = null |
| `BookHistory` | ค่า default ประวัติ | Field = null |
| `Endpoints.{Type}` | ค่า default เฉพาะ endpoint | Override general defaults |
| `CustomDefaults` | ค่า custom เพิ่มเติม | Field ไม่มีค่า |

---

## 🚀 การ Deploy Configuration

### **Development**
```bash
# แก้ไข appsettings.Development.json
dotnet build
dotnet run
```

### **Production**
```bash
# แก้ไข appsettings.Production.json
dotnet publish -c Release
# Deploy ไปยัง server
```

---

## 📝 Checklist การตั้งค่า

- [ ] ตรวจสอบค่า default ใน `BookData`
- [ ] ตรวจสอบ `BookCodePrefix` ของแต่ละ endpoint
- [ ] ตรวจสอบ `StatusId` เฉพาะของแต่ละ endpoint
- [ ] ตรวจสอบ `HistoryAction` names
- [ ] ทดสอบ Custom Defaults
- [ ] Backup `appsettings.json` ก่อนแก้ไข
- [ ] ทดสอบทุก endpoint หลังแก้ config

---

## 🔗 Related Files

- `Models/BookDefaultSettings.cs` - Configuration model
- `Models/BookModels.cs` - Data models
- `Controllers/BooksController.cs` - Controller implementation
- `appsettings.json` - Configuration file
- `appsettings.Production.json` - Production config

---

## 📞 Support

หากมีคำถามหรือปัญหา กรุณาติดต่อ:
- **Development Team**: dev-team@example.com
- **Documentation**: [API Documentation](http://localhost:5152)

---

**Version**: 1.0  
**Last Updated**: 2025-01-30  
**Author**: Development Team
