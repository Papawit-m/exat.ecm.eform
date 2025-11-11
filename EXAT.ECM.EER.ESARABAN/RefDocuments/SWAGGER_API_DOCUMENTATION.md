# Swagger API Documentation Guide

## 📋 Overview

คู่มือการเข้าถึงและใช้งาน Swagger API Documentation สำหรับ eSaraban Books API

**API Base URL (DEV)**: `http://localhost:5152`  
**Swagger UI URL**: `http://localhost:5152`  
**Swagger JSON URL**: `http://localhost:5152/swagger/v1/swagger.json`  
**OpenAPI Version**: 2.0 (K2 Compatible)

---

## 🌐 Accessing Swagger UI

### **Development Environment**

1. เปิด browser
2. ไปที่ `http://localhost:5152`
3. Swagger UI จะแสดงทันที (configured at root path `/`)

### **Production Environment**

| Environment | Swagger UI URL |
|------------|----------------|
| **DEV** | `http://localhost:5152` |
| **UAT** | `http://api-uat.exat.co.th/esrb-external-api` |
| **PROD** | `http://api.exat.co.th/esrb-external-api` |

---

## 📚 API Endpoints

### **Books - Create (K2 Compatible)** ⭐ แนะนำ

#### **POST** `/api/books/create/approved/simple`
สร้างเอกสารแบบง่าย - กรณีอนุมัติ/เข้าสู่หลักเกณ์ (K2 SmartObject Compatible)

**Tag**: `Books - Create (K2 Compatible)`

**Request Body**:
```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบสร้างเอกสารผ่าน K2 SmartObject",
  "book_to": "ผู้อำนวยการใหญ่",
  "registrationbook_id": "E1786792382247A49DD27072718DB187",
  "parent_bookid": "PARENT_BOOK_001",
  "parent_orgid": "ORG_456",
  "parent_positionname": "ผู้จัดการฝ่ายบริหาร"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "เอกสารถูกสร้างสำเร็จ",
  "data": {
    "status": "S",
    "statusCode": "200",
    "message": "Success: generate book.",
    "book_id": "DF4E19B272DE4FD78880B4CE65CECD75",
    "book_code": "APV-20251030-5810",
    "book_subject": "ทดสอบสร้างเอกสารผ่าน K2 SmartObject",
    "book_to": "ผู้อำนวยการใหญ่",
    "registrationbook_id": "E1786792382247A49DD27072718DB187",
    "parent_bookid": "PARENT_BOOK_001",
    "parent_orgid": "ORG_456",
    "parent_positionname": "ผู้จัดการฝ่ายบริหาร",
    "booktype_id": 93,
    "created_by": "EXAT\\ECMUSR07",
    "created_date": "2025-10-30T20:06:30.5011076+07:00"
  }
}
```

---

### **Books - Create** (Full API)

#### **POST** `/api/books/create/original`
สร้างเอกสาร - Original (ตาม Postman Collection)

**Tag**: `Books - Create`

#### **POST** `/api/books/create/approved`
สร้างเอกสาร - กรณีอนุมัติ/เข้าสู่หลักเกณ์

**Tag**: `Books - Create`

#### **POST** `/api/books/create/non-compliant`
สร้างเอกสาร - กรณีไม่เข้าหลักเกณ์

**Tag**: `Books - Create`

#### **POST** `/api/books/create/under-construction`
สร้างเอกสาร - กรณีอยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา

**Tag**: `Books - Create`

---

### **Books - Operations**

#### **GET** `/api/books/generate-code`
สร้างรหัสเอกสาร (Generate Code)

**Query Parameters**:
- `user_ad` (required): Active Directory username
- `book_id` (required): Book ID (GUID format)

#### **POST** `/api/books/transfer`
โอนย้าย Book ระหว่างองค์กร

**Query Parameters**:
- `user_ad` (required)
- `book_id` (required)
- `tranfer_id` (optional)
- `original_org_code` (required)
- `destination_org_code` (required)

---

### **Books - Query**

#### **GET** `/api/books/final-orgs/by-action`
ดึงข้อมูลองค์กรปลายทาง (พร้อม Alert)

**Query Parameters**:
- `user_ad` (required)
- `book_id` (required)

#### **GET** `/api/books/final-orgs/by-action/no-alert`
ดึงข้อมูลองค์กรปลายทาง (ไม่มี Alert)

**Query Parameters**:
- `user_ad` (required)
- `book_id` (required)

---

## 🧪 Testing with Swagger UI

### **Step-by-Step Guide**

1. **เปิด Swagger UI**
   - ไปที่ `http://localhost:5152`

2. **เลือก Endpoint**
   - คลิกที่ **Books - Create (K2 Compatible)**
   - เลือก **POST /api/books/create/approved/simple**

3. **คลิก "Try it out"**

4. **กรอก Request Body**
   ```json
   {
     "user_ad": "EXAT\\ECMUSR07",
     "book_subject": "ทดสอบ Swagger UI",
     "book_to": "ผอ.",
     "registrationbook_id": "E1786792382247A49DD27072718DB187"
   }
   ```

5. **คลิก "Execute"**

6. **ดู Response**
   - **Response Code**: 200
   - **Response Body**: JSON with book_id, book_code, etc.

---

## 📥 Download Swagger JSON

### **For K2 REST Service Configuration**

**URL**: `http://localhost:5152/swagger/v1/swagger.json`

### **Using curl**:
```bash
curl -o swagger.json http://localhost:5152/swagger/v1/swagger.json
```

### **Using PowerShell**:
```powershell
Invoke-WebRequest -Uri "http://localhost:5152/swagger/v1/swagger.json" `
  -OutFile "swagger.json"
```

### **Using Browser**:
1. เปิด `http://localhost:5152/swagger/v1/swagger.json`
2. คลิกขวา → Save As → `swagger.json`

---

## 🔧 Swagger Configuration

### **Program.cs Configuration**

```csharp
// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "eSaraban Books API",
        Version = "v1",
        Description = "eSaraban External Service API - Books Management"
    });
    
    // Enable annotations
    c.EnableAnnotations();
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Use Swagger at root path
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "eSaraban Books API v1");
    c.RoutePrefix = string.Empty; // Swagger UI at root
});
```

### **OpenAPI 2.0 Format**

Swagger JSON is generated in **OpenAPI 2.0** format for K2 compatibility:

```json
{
  "swagger": "2.0",
  "info": {
    "title": "eSaraban Books API",
    "version": "v1"
  },
  "paths": {
    "/api/books/create/approved/simple": {
      "post": {
        "tags": ["Books - Create (K2 Compatible)"],
        "summary": "สร้างเอกสารแบบง่าย",
        "parameters": [...],
        "responses": {...}
      }
    }
  }
}
```

---

## 📊 Response Codes

| Code | Status | Description |
|------|--------|-------------|
| **200** | OK | Success - เอกสารถูกสร้างสำเร็จ |
| **400** | Bad Request | ข้อมูลไม่ถูกต้อง (validation error) |
| **404** | Not Found | ไม่พบข้อมูล |
| **500** | Internal Server Error | เกิดข้อผิดพลาดภายในระบบ |

---

## 🔍 Request/Response Examples

### **Example 1: Minimal Request**

**Request**:
```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบ",
  "book_to": "ผอ.",
  "registrationbook_id": "RB001"
}
```

**Response**:
```json
{
  "success": true,
  "data": {
    "status": "S",
    "book_id": "A1B2C3D4E5F6...",
    "book_code": "APV-20251030-1234",
    "booktype_id": 93
  }
}
```

### **Example 2: Full Request with Optional Fields**

**Request**:
```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบสร้างเอกสารแบบเต็ม",
  "book_to": "ผู้อำนวยการใหญ่",
  "registrationbook_id": "E1786792382247A49DD27072718DB187",
  "parent_bookid": "PARENT_BOOK_001",
  "parent_orgid": "AG0101",
  "parent_positionname": "ผู้จัดการฝ่ายบริหาร"
}
```

**Response**:
```json
{
  "success": true,
  "message": "เอกสารถูกสร้างสำเร็จ",
  "data": {
    "status": "S",
    "statusCode": "200",
    "message": "Success: generate book.",
    "book_id": "DF4E19B272DE4FD78880B4CE65CECD75",
    "book_code": "APV-20251030-5810",
    "book_subject": "ทดสอบสร้างเอกสารแบบเต็ม",
    "book_to": "ผู้อำนวยการใหญ่",
    "parent_bookid": "PARENT_BOOK_001",
    "parent_orgid": "AG0101",
    "parent_positionname": "ผู้จัดการฝ่ายบริหาร",
    "booktype_id": 93
  }
}
```

### **Example 3: Error Response (Missing Required Field)**

**Request**:
```json
{
  "user_ad": "EXAT\\ECMUSR07",
  "book_subject": "ทดสอบ"
  // Missing: book_to, registrationbook_id
}
```

**Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Operation failed",
  "data": null,
  "error": "book_to is required",
  "errorCode": "BOOK_TO_REQUIRED",
  "timestamp": "2025-10-30T13:06:30.5011105Z"
}
```

---

## 🔐 Authentication (Future)

### **Planned Authentication Methods**

1. **API Key**
   ```
   Header: X-API-Key: {your-api-key}
   ```

2. **Bearer Token**
   ```
   Header: Authorization: Bearer {token}
   ```

3. **OAuth 2.0**
   ```
   Header: Authorization: Bearer {oauth-token}
   ```

---

## 📝 Swagger Annotations

### **Controller-Level Annotations**

```csharp
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BooksController : ControllerBase
{
    // ...
}
```

### **Method-Level Annotations**

```csharp
[HttpPost("create/approved/simple")]
[SwaggerOperation(
    Summary = "สร้างเอกสารแบบง่าย - กรณีอนุมัติ/เข้าสู่หลักเกณ์",
    Description = "สร้างเอกสารใหม่แบบง่าย ๆ โดยส่งเฉพาะพารามิเตอร์ที่จำเป็น",
    Tags = new[] { "Books - Create (K2 Compatible)" }
)]
[SwaggerResponse(200, "Success", typeof(ApiResponse<object>))]
[SwaggerResponse(400, "Bad Request", typeof(ApiResponse<object>))]
public async Task<IActionResult> CreateBookApprovedSimple(...)
{
    // ...
}
```

---

## 🎯 Best Practices

### **1. Always Include Content-Type**
```
Content-Type: application/json; charset=utf-8
```

### **2. Use UTF-8 Encoding**
- รองรับภาษาไทย
- K2 compatible

### **3. Validate Required Fields**
- Check all required fields before calling API
- Display user-friendly error messages

### **4. Handle Errors Gracefully**
```javascript
try {
    const response = await fetch(apiUrl, options);
    if (!response.ok) {
        const error = await response.json();
        console.error(error.errorCode, error.error);
    }
} catch (error) {
    console.error('Network error:', error);
}
```

### **5. Log All API Calls**
- Request timestamp
- User AD
- Book ID (response)
- Status code

---

## 🔄 API Versioning

### **Current Version**: v1

**Swagger Endpoint**: `/swagger/v1/swagger.json`

**Future Versions**:
- v2: `/swagger/v2/swagger.json`
- v3: `/swagger/v3/swagger.json`

**Note**: K2 Service Instance จะต้อง update Swagger URL เมื่อมี version ใหม่

---

## 📞 Support & Documentation

**Swagger UI (Live)**: `http://localhost:5152`  
**API Documentation**: See `RefDocuments/` folder  
**K2 Integration Guide**: `RefDocuments/K2_SMARTOBJECT_INTEGRATION_GUIDE.md`  
**Project README**: `README.md`

**Contact**:
- **Development Team**: api-support@exat.co.th
- **GitHub**: EXAT.ECM.EER.ESARABAN

---

## 🚀 Quick Links

- [Swagger UI](http://localhost:5152)
- [Swagger JSON](http://localhost:5152/swagger/v1/swagger.json)
- [K2 Integration Guide](./K2_SMARTOBJECT_INTEGRATION_GUIDE.md)
- [Project README](../README.md)

---

**Last Updated**: October 30, 2025  
**Version**: 1.0
