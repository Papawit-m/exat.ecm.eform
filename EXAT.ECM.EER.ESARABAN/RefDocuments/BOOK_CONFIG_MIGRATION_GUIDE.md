# การย้าย BookDefaultSettings ไปยัง DefaultSettings Folder

## 📋 สรุปการเปลี่ยนแปลง

เอกสารนี้อธิบายการย้าย Configuration `BookDefaultSettings` ออกจาก `appsettings.json` ไปยังไฟล์แยกใน folder `DefaultSettings/`

---

## 🎯 วัตถุประสงค์

1. **แยก Configuration** - แยกค่า default ออกจาก appsettings.json เพื่อความเป็นระเบียบ
2. **จัดการง่าย** - แก้ไขค่า default ได้โดยไม่ต้องแตะ appsettings.json หลัก
3. **Hot Reload** - รองรับการ reload configuration โดยไม่ต้อง restart application
4. **Scope ชัดเจน** - ใช้งานเฉพาะ 4 endpoints ที่กำหนด

---

## 📁 โครงสร้างไฟล์

### **ก่อนเปลี่ยนแปลง**
```
K2RestApi/
├── appsettings.json                 # มี BookDefaultSettings อยู่ที่นี่
├── Controllers/
│   └── BooksController.cs
└── Models/
    └── BookDefaultSettings.cs
```

### **หลังเปลี่ยนแปลง**
```
K2RestApi/
├── appsettings.json                 # ลบ BookDefaultSettings ออกแล้ว
├── DefaultSettings/                 # 🆕 Folder ใหม่
│   ├── book-defaults.json          # 🆕 Configuration สำหรับ Book API
│   └── README.md                   # 🆕 คู่มือการใช้งาน
├── Controllers/
│   └── BooksController.cs
└── Models/
    └── BookDefaultSettings.cs
```

---

## 🔄 การเปลี่ยนแปลงในแต่ละไฟล์

### **1. appsettings.json**

#### ก่อน:
```json
{
  "ESarabanApiSettings": { ... },
  "BookDefaultSettings": {
    "BookData": { ... },
    "BookFile": { ... },
    "BookHistory": { ... },
    "Endpoints": { ... }
  }
}
```

#### หลัง:
```json
{
  "ESarabanApiSettings": { ... }
}
```

**การเปลี่ยนแปลง**: ✅ ลบ section `BookDefaultSettings` ทั้งหมดออก

---

### **2. DefaultSettings/book-defaults.json** (ไฟล์ใหม่)

```json
{
  "BookDefaultSettings": {
    "BookData": {
      "RegistrationBookId": null,
      "RegistrationBookNameTh": null,
      "BookTypeId": 1,
      "SendTypeId": 1,
      "FormatId": 1,
      "SpeedId": 2,
      "SecretId": 1,
      ...
    },
    "BookFile": {
      "FileExtension": ".pdf",
      "FilePath": "/documents/books",
      ...
    },
    "BookHistory": {
      "Action": "CREATE",
      "Remark": "สร้างเอกสารผ่าน K2 REST API"
    },
    "Endpoints": {
      "Original": {
        "BookCodePrefix": "BK-",
        "StatusId": 1,
        "HistoryAction": "CREATE_ORIGINAL",
        ...
      },
      "Approved": { ... },
      "NonCompliant": { ... },
      "UnderConstruction": { ... }
    }
  }
}
```

**การเปลี่ยนแปลง**: ✅ สร้างไฟล์ใหม่พร้อม Configuration ทั้งหมด

---

### **3. Program.cs**

#### ก่อน:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register Book Default Settings
builder.Services.Configure<BookDefaultSettings>(
    builder.Configuration.GetSection("BookDefaultSettings"));
```

#### หลัง:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Book Default Settings configuration file
builder.Configuration.AddJsonFile(
    "DefaultSettings/book-defaults.json",
    optional: false,
    reloadOnChange: true);

// Add services to the container
builder.Services.AddControllers();

// Register Book Default Settings (from DefaultSettings/book-defaults.json)
builder.Services.Configure<BookDefaultSettings>(
    builder.Configuration.GetSection("BookDefaultSettings"));
```

**การเปลี่ยนแปลง**: 
- ✅ เพิ่มการ load ไฟล์ `DefaultSettings/book-defaults.json`
- ✅ ตั้งค่า `reloadOnChange: true` เพื่อ hot reload

---

### **4. K2RestApi.csproj**

#### ก่อน:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="..." />
  </ItemGroup>
</Project>
```

#### หลัง:
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <EnableDefaultContentItems>false</EnableDefaultContentItems>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="..." />
  </ItemGroup>
  <ItemGroup>
    <Content Include="DefaultSettings\**\*.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
    </Content>
    <Content Include="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="appsettings.*.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
```

**การเปลี่ยนแปลง**:
- ✅ เพิ่ม `EnableDefaultContentItems=false` เพื่อควบคุม content items
- ✅ กำหนดให้ copy ไฟล์ใน `DefaultSettings/` ไปยัง output directory
- ✅ กำหนดให้ copy appsettings.json และ appsettings.*.json

---

### **5. DefaultSettings/README.md** (ไฟล์ใหม่)

สร้างเอกสารคู่มือการใช้งาน Configuration ที่อยู่ใน folder DefaultSettings/

**เนื้อหาประกอบด้วย**:
- ภาพรวม Configuration
- โครงสร้างไฟล์
- วิธีการแก้ไข
- ตัวอย่างการใช้งาน
- ข้อควรระวัง

---

## ✅ Endpoints ที่ได้รับผลกระทบ

Configuration นี้ใช้งานเฉพาะ **4 endpoints**:

| Endpoint | Book Code Prefix | Status ID | History Action |
|----------|------------------|-----------|----------------|
| `/api/books/create/original` | `BK-` | 1 | `CREATE_ORIGINAL` |
| `/api/books/create/approved` | `APV-` | 2 | `CREATE_APPROVED` |
| `/api/books/create/non-compliant` | `NCL-` | 3 | `CREATE_NON_COMPLIANT` |
| `/api/books/create/under-construction` | `UNC-` | 4 | `CREATE_UNDER_CONSTRUCTION` |

---

## 🔧 การทำงานของระบบ

### **1. Load Configuration**
```
Program.cs เริ่มต้น
    ↓
Load DefaultSettings/book-defaults.json
    ↓
Parse JSON → BookDefaultSettings Model
    ↓
Register ใน Dependency Injection
    ↓
Inject เข้า BooksController
```

### **2. Apply Defaults**
```
Request เข้ามาที่ Controller
    ↓
ApplyDefaults(request, "original")
    ↓
Check แต่ละ field ว่ามีค่าหรือไม่
    ↓
ถ้าไม่มีค่า (0, null) → Apply default จาก config
    ↓
ถ้ามีค่าแล้ว → ใช้ค่าจาก request
    ↓
Process ต่อไป
```

### **3. Hot Reload**
```
แก้ไข DefaultSettings/book-defaults.json
    ↓
Save ไฟล์
    ↓
System detect การเปลี่ยนแปลง (reloadOnChange: true)
    ↓
Reload configuration อัตโนมัติ
    ↓
ใช้ค่าใหม่ทันที (ไม่ต้อง restart)
```

---

## 🧪 การทดสอบ

### **Test 1: ตรวจสอบไฟล์ถูก Copy**

```bash
# Build โปรเจ็กต์
dotnet build

# ตรวจสอบว่าไฟล์ถูก copy ไปยัง output
ls bin/Debug/net8.0/DefaultSettings/
```

**ผลลัพธ์ที่คาดหวัง**:
```
book-defaults.json
README.md
```

---

### **Test 2: ทดสอบ Hot Reload**

1. **Run Application**:
```bash
dotnet run
```

2. **แก้ไข Configuration**:
แก้ไข `DefaultSettings/book-defaults.json`:
```json
"Original": {
  "BookCodePrefix": "TEST-"
}
```

3. **ทดสอบ API**:
```powershell
$body = '{"user_ad":"test","book":{"book_owner":"O","book_subject":"S","book_to":"T","registrationbook_id":"R","booktype_id":0}}'
Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/original" -Method Post -Body $body -ContentType "application/json"
```

4. **ตรวจสอบ Response**:
```json
{
  "book_code": "TEST-20251030-XXXX"  // ✅ ใช้ prefix ใหม่
}
```

---

### **Test 3: ทดสอบ Default Values**

**Request Body** (ส่งเฉพาะ required fields):
```json
{
  "user_ad": "testuser",
  "book": {
    "book_owner": "Owner",
    "book_subject": "Subject",
    "book_to": "Recipient",
    "registrationbook_id": "REG001",
    "booktype_id": 0,
    "sendtype_id": 0
  }
}
```

**Expected Result** (หลัง apply defaults):
```json
{
  "booktype_id": 1,     // จาก BookData.BookTypeId
  "sendtype_id": 1,     // จาก BookData.SendTypeId
  "format_id": 1,       // จาก BookData.FormatId
  "speed_id": 2,        // จาก BookData.SpeedId
  "secret_id": 1,       // จาก BookData.SecretId
  "status_id": 1        // จาก Endpoints.Original.StatusId
}
```

---

## 📊 ข้อดีของการเปลี่ยนแปลง

| ข้อดี | คำอธิบาย |
|-------|----------|
| **🗂️ แยกความรับผิดชอบ** | appsettings.json เก็บ config ทั่วไป, DefaultSettings/ เก็บ config เฉพาะ Book API |
| **✏️ แก้ไขง่าย** | แก้ไขได้โดยไม่ต้องกังวลว่าจะกระทบ config อื่น |
| **🔄 Hot Reload** | แก้ไขแล้วใช้งานได้ทันทีโดยไม่ต้อง restart |
| **📦 Modular** | เพิ่มไฟล์ config ใหม่ได้ง่าย (เช่น transfer-defaults.json) |
| **🎯 Scope ชัดเจน** | ใช้เฉพาะ 4 endpoints ที่กำหนด |
| **📝 มี Documentation** | มี README.md อธิบายการใช้งาน |

---

## ⚠️ ข้อควรระวัง

### **1. ไฟล์ต้องมีอยู่จริง**
```csharp
builder.Configuration.AddJsonFile(
    "DefaultSettings/book-defaults.json",
    optional: false,  // ⚠️ ถ้าไฟล์ไม่มี application จะไม่ start
    reloadOnChange: true);
```

**แก้ไข**: ตรวจสอบว่าไฟล์อยู่ใน DefaultSettings/ และ copy ไปยัง output directory

---

### **2. JSON Syntax ต้องถูกต้อง**

**ผิด**:
```json
{
  "BookTypeId": 1,  // ⚠️ มี comma ที่ตัวสุดท้าย
}
```

**ถูก**:
```json
{
  "BookTypeId": 1
}
```

**เครื่องมือช่วย**: ใช้ JSON validator (jsonlint.com, Visual Studio Code)

---

### **3. Property Names Case-Sensitive**

```json
{
  "BookTypeId": 1,      // ✅ ถูกต้อง (Pascal Case)
  "booktypeid": 1,      // ❌ ผิด (ตัวพิมพ์เล็กทั้งหมด)
  "book_type_id": 1     // ❌ ผิด (snake_case)
}
```

---

### **4. Backup ก่อนแก้ไข**

```bash
# Backup ไฟล์ก่อนแก้ไข
cp DefaultSettings/book-defaults.json DefaultSettings/book-defaults.json.backup

# กู้คืนถ้ามีปัญหา
cp DefaultSettings/book-defaults.json.backup DefaultSettings/book-defaults.json
```

---

## 🚀 Deploy to Production

### **1. Development Environment**
```bash
# Build
dotnet build

# Run
dotnet run
```

ไฟล์ `DefaultSettings/book-defaults.json` จะถูก copy ไปยัง `bin/Debug/net8.0/DefaultSettings/`

---

### **2. Production Environment**

```bash
# Publish
dotnet publish -c Release -o ./publish

# Check files
ls ./publish/DefaultSettings/
```

**ผลลัพธ์**:
```
book-defaults.json
README.md
```

**Deploy**:
1. Copy ทั้ง folder `publish/` ไปยัง production server
2. ตรวจสอบว่า `DefaultSettings/book-defaults.json` อยู่ใน path เดียวกับ executable
3. ปรับค่า configuration ตาม environment (ถ้าจำเป็น)

---

### **3. Environment-Specific Configuration**

สามารถสร้างไฟล์เฉพาะ environment ได้:

```
DefaultSettings/
├── book-defaults.json              # Default (Development)
├── book-defaults.Production.json   # Production
└── book-defaults.Staging.json      # Staging
```

**Update Program.cs**:
```csharp
var environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile(
    $"DefaultSettings/book-defaults.{environment}.json",
    optional: true,
    reloadOnChange: true);
```

---

## 📞 Troubleshooting

### **ปัญหา: Application ไม่ start**

**Error**: `FileNotFoundException: Could not find file 'DefaultSettings/book-defaults.json'`

**แก้ไข**:
1. ตรวจสอบว่าไฟล์อยู่ใน folder `DefaultSettings/`
2. ตรวจสอบ K2RestApi.csproj ว่ามีการ copy ไฟล์
3. Build ใหม่: `dotnet build`

---

### **ปัญหา: Configuration ไม่ load**

**Symptom**: ค่า default ไม่ถูก apply

**แก้ไข**:
1. ตรวจสอบ JSON syntax ด้วย validator
2. ตรวจสอบ property names ว่าตรงกับ Model
3. ตรวจสอบ log เพื่อดู configuration errors

---

### **ปัญหา: Hot Reload ไม่ทำงาน**

**แก้ไข**:
1. ตรวจสอบว่าตั้งค่า `reloadOnChange: true`
2. Restart application
3. ตรวจสอบ file permissions

---

## 🔗 Related Documentation

- **DefaultSettings/README.md** - คู่มือการใช้งาน Configuration
- **RefDocuments/BOOK_DEFAULT_CONFIG_GUIDE.md** - คู่มือโดยละเอียด
- **Models/BookDefaultSettings.cs** - Configuration Model
- **Controllers/BooksController.cs** - Implementation

---

## 📝 Checklist การ Migrate

- [x] สร้าง folder `DefaultSettings/`
- [x] สร้างไฟล์ `DefaultSettings/book-defaults.json`
- [x] ลบ `BookDefaultSettings` section จาก `appsettings.json`
- [x] Update `Program.cs` เพื่อ load configuration file
- [x] Update `K2RestApi.csproj` เพื่อ copy files
- [x] สร้าง `DefaultSettings/README.md`
- [x] Build และทดสอบ
- [x] Update `.github/copilot-instructions.md`
- [x] สร้างเอกสาร migration guide

---

**Migration Date**: 2025-01-30  
**Version**: 1.0  
**Status**: ✅ Completed
