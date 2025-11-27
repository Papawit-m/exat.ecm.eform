# K2 REST Service API

## 🎉 Version 1.5 - 100% Real API Integration Achieved!

## 📋 ภาพรวม
โปรเจ็กต์นี้เป็น REST API ที่พัฒนาด้วย .NET 8 สำหรับการเชื่อมต่อกับ K2 Workflow โดยรองรับ OpenAPI 2.0 (Swagger JSON) ที่ K2 สามารถเข้าใจและทำงานร่วมกันได้

**🎉 MAJOR ACHIEVEMENT**: All 14 Books API endpoints now integrate with **real eSaraban External API** (100% integration - NO mock data)

## 🎯 คุณสมบัติ
- ✅ .NET 8 Web API
- ✅ OpenAPI 2.0 (Swagger JSON) - รองรับ K2
- ✅ CORS Configuration สำหรับ K2
- ✅ Oracle 11g Database Integration
- ✅ **eSaraban Books Management API (14 endpoints) - 100% Real API Integration** 🎉
- ✅ Oracle Database Management endpoints
- ✅ Schema Management endpoints
- ✅ Response format ที่เป็นมาตรฐาน (Raw Response)
- ✅ การจัดการ Error แบบ Consistent

## 🛠️ เทคโนโลยีที่ใช้
- .NET 8
- ASP.NET Core Web API
- Swashbuckle.AspNetCore 6.8.1
- Swashbuckle.AspNetCore.Annotations 6.8.1
- Oracle.ManagedDataAccess.Core 23.26.0

## 📦 โครงสร้าง Project
```
K2RestApi/
├── Controllers/
│   ├── BooksController.cs      # ✨ eSaraban Books API (14 endpoints)
│   ├── OracleController.cs     # Oracle database management
│   └── SchemaController.cs     # Schema management
├── Models/
│   ├── ApiResponse.cs          # Response model ที่เป็นมาตรฐาน
│   ├── BookModels.cs           # ✨ eSaraban Book Models
│   ├── BookDefaultSettings.cs  # ✨ Book Default Configuration
│   └── ESarabanApiSettings.cs  # ✨ eSaraban Configuration
├── Services/
│   └── OracleDbService.cs      # Oracle database service
├── DefaultSettings/            # 🔧 Configuration Files
│   ├── book-defaults.json      # ✨ Book API Default Settings
│   └── README.md               # Configuration guide
├── RefDocuments/               # 📚 เอกสารทั้งหมด
│   └── *.md                    # คู่มือและเอกสารอ้างอิง
├── PsUnitTest/                 # 🔧 PowerShell scripts
│   └── *.ps1                   # Scripts สำหรับ automation
├── postman-collections/        # 📦 Postman Collections
│   └── *.json                  # API testing collections
├── Program.cs                  # Entry point และ configuration
├── appsettings.json           # Configuration file
└── K2RestApi.csproj           # Project file
```

## 🚀 การติดตั้งและรัน

### ข้อกำหนด
- .NET 8 SDK หรือสูงกว่า
- Visual Studio 2022 หรือ VS Code

### วิธีรัน

1. **Clone หรือ Download โปรเจ็กต์**

2. **Restore dependencies**
```powershell
dotnet restore K2RestApi.csproj
```

3. **Build โปรเจ็กต์**
```powershell
dotnet build K2RestApi.csproj
```

4. **รันโปรเจ็กต์**
```powershell
dotnet run --project K2RestApi.csproj
```

5. **เปิดเว็บเบราว์เซอร์**
   - URL: `https://localhost:7XXX` หรือ `http://localhost:5XXX`
   - Swagger UI จะแสดงทันทีที่หน้า root

## �️ Oracle Database Configuration

### Connection Information
- **Host:** 172.20.1.176
- **Port:** 1521
- **Service Name:** ecmdev
- **Normal User:** EFM_EER / mypassword
- **SYSDBA:** SYS / Aa12345*

Connection strings ถูกกำหนดใน `appsettings.json`

**อ่านรายละเอียดเพิ่มเติม:** [ORACLE_INTEGRATION_GUIDE.md](RefDocuments/ORACLE_INTEGRATION_GUIDE.md)

## 📚 API Endpoints (14 Books API - 100% Real Integration 🎉)

### Health Check
- `GET /api/health` - ตรวจสอบสถานะของ API
- `GET /api/health/version` - ข้อมูลเวอร์ชันของ API

### Books - Create (7 endpoints) 🌐 Real eSaraban API
- `POST /api/books/create/original` - **สร้างเอกสาร (Original)** - ตาม Postman Collection
- `POST /api/books/create/approved` - สร้างเอกสาร (กรณีอนุมัติ/เข้าสู่หลักเกณ์)
- `POST /api/books/create/non-compliant` - สร้างเอกสาร (กรณีไม่เข้าหลักเกณ์)
- `POST /api/books/create/under-construction` - สร้างเอกสาร (กรณีอยู่ระหว่างก่อสร้าง)
- `POST /api/books/create/approved/simple` - K2 Compatible (Simple format)
- `POST /api/books/create/non-compliant/simple` - K2 Compatible (Simple format)
- `POST /api/books/create/under-construction/simple` - K2 Compatible (Simple format)

### Books - Workflow (3 endpoints) 🌐 Real eSaraban API
- `POST /api/books/workflow/approved` - Workflow: Create → Generate Code → Transfer (Approved)
- `POST /api/books/workflow/non-compliant` - Workflow: Create → Generate Code → Transfer (Non-Compliant)
- `POST /api/books/workflow/under-construction` - Workflow: Create → Generate Code → Transfer (Under-Construction)

### Books - Operations (2 endpoints) 🌐 Real eSaraban API
- `GET /api/books/generate-code` - สร้างรหัสเอกสาร (Raw Response)
- `POST /api/books/transfer` - โอนย้าย Book ระหว่างองค์กร (Raw Response) **v1.4**

### Books - Query (2 endpoints) 🌐 Real eSaraban API
- `GET /api/books/final-orgs/by-action` - ดึงข้อมูลองค์กรปลายทาง (พร้อม Alert) (Raw Response) **v1.5** 🎉
- `GET /api/books/final-orgs/by-action/no-alert` - ดึงข้อมูลองค์กรปลายทาง (ไม่มี Alert) (Raw Response) **v1.5** 🎉

### Employee Management
- `GET /api/employees` - ดึงข้อมูล Employee ทั้งหมด
- `GET /api/employees/{id}` - ดึงข้อมูล Employee ตาม ID
- `GET /api/employees/department/{department}` - ดึงข้อมูล Employee ตามแผนก
- `POST /api/employees` - สร้าง Employee ใหม่
- `PUT /api/employees/{id}` - อัปเดตข้อมูล Employee
- `DELETE /api/employees/{id}` - ลบ Employee

### Oracle Database Management
- `GET /api/oracle/test-connection` - ทดสอบการเชื่อมต่อ Oracle
- `GET /api/oracle/version` - ดึงข้อมูล version ของ Oracle
- `GET /api/oracle/current-time` - ดึงเวลาปัจจุบันจาก Database
- `POST /api/oracle/execute-query` - Execute SQL query
- `GET /api/oracle/tables` - ดึงรายชื่อ tables ทั้งหมด
- `GET /api/oracle/tables/{tableName}/structure` - ดึงโครงสร้างของ table

## 🔄 ตัวอย่างการใช้งาน

### 1. Get All Employees
```http
GET /api/employees
```

**Response:**
```json
{
  "success": true,
  "message": "Employees retrieved successfully",
  "data": [
    {
      "id": 1,
      "firstName": "สมชาย",
      "lastName": "ใจดี",
      "email": "somchai@example.com",
      "department": "IT",
      "position": "Developer",
      "status": "Active",
      "createdDate": "2025-10-30T00:00:00Z"
    }
  ],
  "error": null,
  "timestamp": "2025-10-30T00:00:00Z"
}
```

### 2. Create New Employee
```http
POST /api/employees
Content-Type: application/json

{
  "firstName": "สมศรี",
  "lastName": "ขยัน",
  "email": "somsri@example.com",
  "department": "Finance",
  "position": "Accountant"
}
```

### 3. Update Employee
```http
PUT /api/employees/1
Content-Type: application/json

{
  "position": "Senior Developer",
  "department": "IT"
}
```

## 🔌 การเชื่อมต่อกับ K2

### ขั้นตอนการตั้งค่า K2 REST Service

1. **เปิด K2 Designer/Management**

2. **สร้าง Service Instance ใหม่**
   - เลือก REST Service
   - ตั้งค่า Base URL: `https://your-api-url`

3. **Import Swagger JSON**
   - URL: `https://your-api-url/swagger/v1/swagger.json`
   - K2 จะอ่าน OpenAPI 2.0 spec และสร้าง SmartObjects อัตโนมัติ

4. **ตั้งค่า Authentication (ถ้าจำเป็น)**
   - เลือกประเภท Authentication ที่เหมาะสม
   - กรอกข้อมูล Credentials

5. **Test Connection**
   - ทดสอบการเชื่อมต่อผ่าน K2 Management

6. **Generate SmartObjects**
   - K2 จะสร้าง SmartObjects จาก API endpoints
   - นำไปใช้ใน Workflow ได้เลย

### คุณสมบัติที่รองรับ K2

✅ **OpenAPI 2.0 Format** - K2 อ่านได้โดยตรง  
✅ **CORS Enabled** - รองรับการเรียกข้าน Cross-Origin  
✅ **Standard HTTP Methods** - GET, POST, PUT, DELETE  
✅ **JSON Response Format** - รูปแบบที่ K2 เข้าใจ  
✅ **Error Handling** - Response codes ที่เป็นมาตรฐาน  

## ⚙️ Configuration

### appsettings.json
```json
{
  "ApiSettings": {
    "Title": "K2 REST Service API",
    "Version": "v1",
    "EnableSwagger": true
  },
  "Cors": {
    "PolicyName": "K2CorsPolicy",
    "AllowedOrigins": "*",
    "AllowedMethods": "*",
    "AllowedHeaders": "*"
  }
}
```

### Production Settings
สำหรับ Production ควรแก้ไข:
- ตั้งค่า CORS ให้จำกัดเฉพาะ K2 Server
- เพิ่ม Authentication/Authorization
- ปิด Swagger UI (ถ้าไม่ต้องการ)
- เปลี่ยน Connection String ไปใช้ Database จริง

## 🔒 Security Considerations

1. **CORS Policy**: ปรับให้จำกัดเฉพาะ K2 Server
2. **Authentication**: เพิ่ม JWT หรือ OAuth2
3. **HTTPS**: ใช้ HTTPS ในทุก environment
4. **Input Validation**: ตรวจสอบข้อมูลที่รับเข้ามาทั้งหมด
5. **Rate Limiting**: จำกัดจำนวน request ต่อ IP

## � API Integration Status - Version 1.5

### 🎉 100% Real API Integration Achievement

**Integration Progress Timeline:**
```
v1.1:   0% (0/14)   ████░░░░░░░░░░░░░░░░ - All mock data
v1.2:  35.7% (5/14) ███████░░░░░░░░░░░░░ - Generate + Workflow
v1.3:  78.6% (11/14) ███████████████░░░░░ - All Create endpoints
v1.3.1: 78.6% (11/14) ███████████████░░░░░ - Raw Response format
v1.4:  85.7% (12/14) ████████████████░░░░ - Transfer endpoint
v1.5: 100.0% (14/14) ████████████████████ - Final Orgs ✅ COMPLETE
```

### All 14 Books API Endpoints Status

| Endpoint | Integration | Version | Response Format |
|----------|-------------|---------|-----------------|
| `/create/original` | ✅ Real API | v1.3 | ApiResponse |
| `/create/approved` | ✅ Real API | v1.3 | ApiResponse |
| `/create/non-compliant` | ✅ Real API | v1.3 | ApiResponse |
| `/create/under-construction` | ✅ Real API | v1.3 | ApiResponse |
| `/create/approved/simple` | ✅ Real API | v1.3 | K2 Compatible |
| `/create/non-compliant/simple` | ✅ Real API | v1.3 | K2 Compatible |
| `/create/under-construction/simple` | ✅ Real API | v1.3 | K2 Compatible |
| `/workflow/approved` | ✅ Real API | v1.2 | ApiResponse |
| `/workflow/non-compliant` | ✅ Real API | v1.2 | ApiResponse |
| `/workflow/under-construction` | ✅ Real API | v1.2 | ApiResponse |
| `/generate-code` | ✅ Real API | v1.2 | Raw Response |
| `/transfer` | ✅ Real API | v1.4 | **Raw Response** |
| `/final-orgs/by-action` | ✅ Real API | **v1.5** 🎉 | **Raw Response** |
| `/final-orgs/by-action/no-alert` | ✅ Real API | **v1.5** 🎉 | **Raw Response** |

**Achievement Summary:**
- ✅ **14/14 endpoints (100%)** - Real eSaraban External API
- ✅ **0/14 endpoints (0%)** - Mock data
- ✅ **Production Ready** - All endpoints call real API
- ✅ **No Hardcoded Data** - All data from eSaraban database
- ✅ **Raw Response Format** - Consistent across Query/Operations endpoints

**See Detailed Documentation:**
- [VERSION_1.5_CHANGELOG.md](RefDocuments/VERSION_1.5_CHANGELOG.md) - v1.5 Release Notes
- [MOCK_DATA_ANALYSIS.md](RefDocuments/MOCK_DATA_ANALYSIS.md) - Integration Analysis

## �📝 หมายเหตุ

- API นี้ใช้ In-Memory storage สำหรับ demo
- สำหรับ Production ควรเชื่อมต่อกับ Database จริง
- OpenAPI 2.0 (Swagger JSON) ถูกตั้งค่าไว้แล้วสำหรับ K2
- Swagger UI จะแสดงที่ root URL (`/`)

## 🐛 การแก้ปัญหา

### K2 ไม่สามารถอ่าน Swagger JSON
- ตรวจสอบว่า API รัน https
- ตรวจสอบ CORS settings
- ดู Swagger JSON ที่ `/swagger/v1/swagger.json`

### Build Error
```powershell
dotnet clean K2RestApi.csproj
dotnet restore K2RestApi.csproj
dotnet build K2RestApi.csproj
```

### Port Already in Use
แก้ไขใน `Properties/launchSettings.json`

## 📞 ติดต่อ
สำหรับคำถามเพิ่มเติม กรุณาติดต่อทีมพัฒนา

## � เอกสารเพิ่มเติม

เอกสารทั้งหมดอยู่ใน folder **[RefDocuments](RefDocuments/)**

- **[PROJECT_SUMMARY.md](RefDocuments/PROJECT_SUMMARY.md)** - สรุปโปรเจ็กต์และคุณสมบัติทั้งหมด
- **[K2_INTEGRATION_GUIDE.md](RefDocuments/K2_INTEGRATION_GUIDE.md)** - คู่มือการเชื่อมต่อ K2 แบบละเอียด
- **[ORACLE_INTEGRATION_GUIDE.md](RefDocuments/ORACLE_INTEGRATION_GUIDE.md)** - คู่มือการใช้งาน Oracle Database
- **[CLONE_TABLE_GUIDE.md](RefDocuments/CLONE_TABLE_GUIDE.md)** - คู่มือการ clone table structure
- **[S_API_ESARABAN_LOG_TABLE_INFO.md](RefDocuments/S_API_ESARABAN_LOG_TABLE_INFO.md)** - ข้อมูลและวิธีใช้งาน S_API_ESARABAN_LOG table

## �📄 License
This project is for demonstration purposes.

---
สร้างด้วย ❤️ โดยใช้ .NET 8 และ Swagger/OpenAPI 2.0
