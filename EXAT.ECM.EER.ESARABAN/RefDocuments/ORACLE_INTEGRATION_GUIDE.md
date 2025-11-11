# Oracle 11g Integration Guide

## 📋 ภาพรวม
เอกสารนี้อธิบายการเชื่อมต่อและใช้งาน Oracle 11g Database ใน K2 REST Service API

## 🔧 ข้อมูลการเชื่อมต่อ

### Oracle Database Server
- **Host:** 172.20.1.176
- **Port:** 1521 (default)
- **Service Name:** ecmdev
- **Database Version:** Oracle 11g

### Credentials

#### Normal User
- **Username:** EFM_EER
- **Password:** mypassword
- **Usage:** ใช้สำหรับ operations ทั่วไป (SELECT, INSERT, UPDATE, DELETE)

#### SYSDBA User
- **Username:** SYS
- **Password:** Aa12345*
- **Privilege:** SYSDBA
- **Usage:** ใช้สำหรับ administrative operations

## 📦 Packages ที่ติดตั้ง

```xml
<PackageReference Include="Oracle.ManagedDataAccess.Core" Version="23.26.0" />
```

## 🏗️ Architecture

### Services
- **OracleDbService** - Service หลักสำหรับจัดการ Oracle Database
  - Interface: `IOracleDbService`
  - Implementation: `OracleDbService`

### Controllers
- **OracleController** - API endpoints สำหรับ Oracle operations

## 🚀 API Endpoints

### 1. Test Connection
```http
GET /api/oracle/test-connection
```
ทดสอบการเชื่อมต่อฐานข้อมูล

**Response:**
```json
{
  "success": true,
  "message": "Oracle database connection successful",
  "data": {
    "status": "Connected",
    "databaseType": "Oracle 11g",
    "user": "EFM_EER",
    "serviceName": "ecmdev"
  }
}
```

### 2. Get Database Version
```http
GET /api/oracle/version
```
ดึงข้อมูล version ของ Oracle Database

**Response:**
```json
{
  "success": true,
  "message": "Database version retrieved successfully",
  "data": {
    "version": "Oracle Database 11g Enterprise Edition Release 11.2.0.4.0 - 64bit Production",
    "timestamp": "2025-10-30T00:00:00Z"
  }
}
```

### 3. Get Current Database Time
```http
GET /api/oracle/current-time
```
ดึงเวลาปัจจุบันจาก Database

**Response:**
```json
{
  "success": true,
  "message": "Database time retrieved successfully",
  "data": {
    "databaseTime": "2025-10-30T12:34:56",
    "serverTime": "2025-10-30T19:34:56",
    "utcTime": "2025-10-30T12:34:56Z"
  }
}
```

### 4. Execute Custom SQL Query
```http
POST /api/oracle/execute-query
Content-Type: application/json

{
  "query": "SELECT * FROM YOUR_TABLE WHERE ROWNUM <= 10",
  "useSysDba": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "Query executed successfully. 10 rows returned.",
  "data": [
    {
      "COLUMN1": "value1",
      "COLUMN2": "value2"
    }
  ]
}
```

### 5. Get List of Tables
```http
GET /api/oracle/tables
```
ดึงรายชื่อ tables ทั้งหมดใน schema

**Response:**
```json
{
  "success": true,
  "message": "Found 15 tables in the schema",
  "data": [
    "TABLE1",
    "TABLE2",
    "TABLE3"
  ]
}
```

### 6. Get Table Structure
```http
GET /api/oracle/tables/{tableName}/structure
```
ดึงโครงสร้างของ table (columns, data types)

**Example:**
```http
GET /api/oracle/tables/EMPLOYEES/structure
```

**Response:**
```json
{
  "success": true,
  "message": "Table structure for EMPLOYEES retrieved successfully",
  "data": [
    {
      "columnName": "ID",
      "dataType": "NUMBER",
      "length": 22,
      "nullable": "N"
    },
    {
      "columnName": "NAME",
      "dataType": "VARCHAR2",
      "length": 100,
      "nullable": "Y"
    }
  ]
}
```

## 💻 การใช้งานใน Code

### ตัวอย่าง: Execute Query
```csharp
public class MyController : ControllerBase
{
    private readonly IOracleDbService _oracleDbService;

    public MyController(IOracleDbService oracleDbService)
    {
        _oracleDbService = oracleDbService;
    }

    [HttpGet("my-data")]
    public async Task<IActionResult> GetMyData()
    {
        var query = "SELECT * FROM MY_TABLE WHERE STATUS = :status";
        var parameters = new[]
        {
            new OracleParameter("status", "ACTIVE")
        };

        var dataTable = await _oracleDbService.ExecuteQueryAsync(query, parameters);
        
        // Process dataTable
        return Ok(dataTable);
    }
}
```

### ตัวอย่าง: Execute Non-Query (INSERT/UPDATE/DELETE)
```csharp
[HttpPost("create-record")]
public async Task<IActionResult> CreateRecord(MyModel model)
{
    var query = @"INSERT INTO MY_TABLE (ID, NAME, STATUS) 
                  VALUES (:id, :name, :status)";
    
    var parameters = new[]
    {
        new OracleParameter("id", model.Id),
        new OracleParameter("name", model.Name),
        new OracleParameter("status", "ACTIVE")
    };

    var rowsAffected = await _oracleDbService.ExecuteNonQueryAsync(query, parameters);
    
    return Ok(new { RowsAffected = rowsAffected });
}
```

### ตัวอย่าง: Execute Scalar
```csharp
[HttpGet("count")]
public async Task<IActionResult> GetRecordCount()
{
    var query = "SELECT COUNT(*) FROM MY_TABLE";
    var count = await _oracleDbService.ExecuteScalarAsync(query);
    
    return Ok(new { Count = count });
}
```

### ตัวอย่าง: ใช้ SYSDBA Connection
```csharp
[HttpGet("system-info")]
public async Task<IActionResult> GetSystemInfo()
{
    // ใช้ SYSDBA สำหรับ query ที่ต้องการสิทธิ์สูง
    var query = "SELECT * FROM V$SESSION";
    var dataTable = await _oracleDbService.ExecuteQueryAsync(
        query, 
        parameters: null, 
        useSysDba: true
    );
    
    return Ok(dataTable);
}
```

## 🔒 Security Best Practices

### 1. Connection String Security
```json
// appsettings.json (Development)
{
  "ConnectionStrings": {
    "OracleConnection": "Data Source=...;User Id=EFM_EER;Password=mypassword;"
  }
}
```

**สำหรับ Production:**
- ใช้ Azure Key Vault หรือ AWS Secrets Manager
- ใช้ Environment Variables
- ไม่เก็บ password ในไฟล์ configuration

### 2. SQL Injection Prevention
```csharp
// ✅ GOOD - ใช้ Parameters
var query = "SELECT * FROM USERS WHERE USERNAME = :username";
var parameters = new[] { new OracleParameter("username", userInput) };

// ❌ BAD - String concatenation
var query = $"SELECT * FROM USERS WHERE USERNAME = '{userInput}'";
```

### 3. Least Privilege Principle
- ใช้ Normal User (EFM_EER) สำหรับ operations ทั่วไป
- ใช้ SYSDBA เฉพาะเมื่อจำเป็นจริงๆ
- จำกัดสิทธิ์ของ user ตามความจำเป็น

### 4. Connection Pooling
Oracle.ManagedDataAccess.Core มี connection pooling อัตโนมัติ:
```csharp
// Connection จะถูก pool และ reuse โดยอัตโนมัติ
using var connection = _oracleDbService.GetConnection();
```

## 📊 Database Tables (ตัวอย่าง)

### ตัวอย่าง Schema
```sql
-- สร้าง table ตัวอย่าง
CREATE TABLE EMPLOYEES (
    ID NUMBER PRIMARY KEY,
    FIRST_NAME VARCHAR2(50) NOT NULL,
    LAST_NAME VARCHAR2(50) NOT NULL,
    EMAIL VARCHAR2(100) UNIQUE,
    DEPARTMENT VARCHAR2(50),
    POSITION VARCHAR2(50),
    STATUS VARCHAR2(20) DEFAULT 'ACTIVE',
    CREATED_DATE DATE DEFAULT SYSDATE
);

-- Insert ข้อมูลตัวอย่าง
INSERT INTO EMPLOYEES VALUES (1, 'สมชาย', 'ใจดี', 'somchai@example.com', 'IT', 'Developer', 'ACTIVE', SYSDATE);
INSERT INTO EMPLOYEES VALUES (2, 'สมหญิง', 'รักงาน', 'somying@example.com', 'HR', 'HR Manager', 'ACTIVE', SYSDATE);
COMMIT;
```

## 🧪 Testing

### ทดสอบผ่าน Swagger UI
1. รัน API: `dotnet run --project K2RestApi.csproj`
2. เปิด browser: http://localhost:5152
3. ไปที่ section "Oracle"
4. ทดสอบ endpoints ต่างๆ

### ทดสอบผ่าน cURL
```bash
# Test connection
curl http://localhost:5152/api/oracle/test-connection

# Get version
curl http://localhost:5152/api/oracle/version

# Execute query
curl -X POST http://localhost:5152/api/oracle/execute-query \
  -H "Content-Type: application/json" \
  -d '{
    "query": "SELECT * FROM EMPLOYEES WHERE ROWNUM <= 5",
    "useSysDba": false
  }'

# Get tables
curl http://localhost:5152/api/oracle/tables

# Get table structure
curl http://localhost:5152/api/oracle/tables/EMPLOYEES/structure
```

## 🔍 Troubleshooting

### ปัญหา: Cannot connect to Oracle
**วิธีแก้:**
1. ตรวจสอบ network connectivity: `ping 172.20.1.176`
2. ตรวจสอบ firewall settings
3. ตรวจสอบว่า Oracle Listener รันอยู่
4. ตรวจสอบ Service Name ว่าถูกต้อง

### ปัญหา: ORA-12154: TNS:could not resolve the connect identifier
**วิธีแก้:**
- ตรวจสอบ Connection String
- ตรวจสอบ Service Name
- ใช้ EZ Connect format

### ปัญหา: ORA-01017: invalid username/password
**วิธีแก้:**
- ตรวจสอบ username/password
- ตรวจสอบว่า user ถูก lock หรือไม่
- ตรวจสอบว่า password expired หรือไม่

### ปัญหา: ORA-28009: connection as SYS should be as SYSDBA
**วิธีแก้:**
- ใช้ connection string ที่มี `DBA Privilege=SYSDBA`
- หรือใช้ `useSysDba: true` parameter

## 📝 Configuration Reference

### appsettings.json
```json
{
  "ConnectionStrings": {
    "OracleConnection": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=172.20.1.176)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ecmdev)));User Id=EFM_EER;Password=mypassword;",
    "OracleConnectionSYSDBA": "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=172.20.1.176)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ecmdev)));User Id=SYS;Password=Aa12345*;DBA Privilege=SYSDBA;"
  },
  "OracleSettings": {
    "Host": "172.20.1.176",
    "Port": 1521,
    "ServiceName": "ecmdev",
    "NormalUser": {
      "Username": "EFM_EER",
      "Password": "mypassword"
    },
    "SysDBA": {
      "Username": "SYS",
      "Password": "Aa12345*"
    }
  }
}
```

## 🎯 Next Steps

1. ✅ **Oracle Integration Complete**
2. ทดสอบการเชื่อมต่อ
3. สร้าง CRUD operations สำหรับ tables ที่ต้องการ
4. เพิ่ม Business Logic
5. เพิ่ม Error Handling และ Logging
6. เพิ่ม Unit Tests

---
**อัปเดท:** 30 ตุลาคม 2025  
**Oracle Version:** 11g  
**Database:** ecmdev@172.20.1.176
