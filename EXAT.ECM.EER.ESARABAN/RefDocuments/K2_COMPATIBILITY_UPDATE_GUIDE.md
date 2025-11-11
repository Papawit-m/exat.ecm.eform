# K2 SmartObject Compatibility Update Guide

**Date**: November 1, 2025  
**Status**: In Progress  
**Purpose**: Update all API endpoints to return K2-compatible direct response format (without ApiResponse wrapper)

---

## 📊 Summary

### Total Endpoints: 14
- ✅ **Already K2 Compatible**: 2 endpoints (Books - Query)
- 📝 **Need Update**: 12 endpoints

---

## 🎯 What Needs to Change

### ❌ BEFORE (Current - Not K2 Compatible):
```json
{
  "success": true,
  "message": "...",
  "data": {
    "book_id": "...",
    "book_code": "..."
  }
}
```

### ✅ AFTER (K2 Compatible):
```json
{
  "book_id": "...",
  "book_code": "...",
  "status": "S",
  "statusCode": "200"
}
```

---

## 📋 Endpoints to Update

### 1️⃣ Books - Create (K2 Compatible) - 3 Endpoints

#### ✅ POST `/api/books/create/approved/simple`
- **Current**: Returns `ApiResponse<CreateBookSimpleResponse>`
- **Change to**: Return `CreateBookSimpleResponse` directly
- **Model**: Add `status` and `statusCode` fields

#### ✅ POST `/api/books/create/non-compliant/simple`
- **Current**: Returns `ApiResponse<CreateBookSimpleResponse>`
- **Change to**: Return `CreateBookSimpleResponse` directly

#### ✅ POST `/api/books/create/under-construction/simple`
- **Current**: Returns `ApiResponse<CreateBookSimpleResponse>`
- **Change to**: Return `CreateBookSimpleResponse` directly

---

### 2️⃣ Books - Create (Full Format) - 4 Endpoints

#### 📝 POST `/api/books/create/original`
- **Current**: Returns `ApiResponse<ESarabanCreateBookResponse>`
- **Change to**: Return `ESarabanCreateBookResponse` directly
- **Model**: Add `status` and `statusCode` fields

#### 📝 POST `/api/books/create/approved`
- **Current**: Returns `ApiResponse<ESarabanCreateBookResponse>`
- **Change to**: Return `ESarabanCreateBookResponse` directly

#### 📝 POST `/api/books/create/non-compliant`
- **Current**: Returns `ApiResponse<ESarabanCreateBookResponse>`
- **Change to**: Return `ESarabanCreateBookResponse` directly

#### 📝 POST `/api/books/create/under-construction`
- **Current**: Returns `ApiResponse<ESarabanCreateBookResponse>`
- **Change to**: Return `ESarabanCreateBookResponse` directly

---

### 3️⃣ Books - Workflow (Combined) - 3 Endpoints

#### 📝 POST `/api/books/workflow/approved`
- **Current**: Returns `ApiResponse<CreateGenerateTransferResponse>`
- **Change to**: Return `CreateGenerateTransferResponse` directly
- **Model**: Add `status` and `statusCode` fields

#### 📝 POST `/api/books/workflow/non-compliant`
- **Current**: Returns `ApiResponse<CreateGenerateTransferResponse>`
- **Change to**: Return `CreateGenerateTransferResponse` directly

#### 📝 POST `/api/books/workflow/under-construction`
- **Current**: Returns `ApiResponse<CreateGenerateTransferResponse>`
- **Change to**: Return `CreateGenerateTransferResponse` directly

---

### 4️⃣ Books - Operations - 2 Endpoints

#### 📝 GET `/api/books/generate-code`
- **Current**: Returns `ApiResponse<GenerateCodeResponse>`
- **Change to**: Return `GenerateCodeResponse` directly
- **Model**: Add `status` and `statusCode` fields

#### 📝 POST `/api/books/transfer`
- **Current**: Returns `ApiResponse<TransferBookResponse>`
- **Change to**: Return `TransferBookResponse` directly
- **Model**: Add `status` and `statusCode` fields

---

### 5️⃣ Books - Query - 2 Endpoints ✅ Already Done

#### ✅ GET `/api/books/final-orgs/by-action`
- **Status**: Already returns `FinalOrgsResponse` directly
- **Format**: `{ "status": "S", "statusCode": "200", "books": [...] }`

#### ✅ GET `/api/books/final-orgs/by-action/no-alert`
- **Status**: Already returns `FinalOrgsResponse` directly
- **Format**: `{ "status": "S", "statusCode": "200", "books": [...] }`

---

## 🔧 Implementation Steps

### Step 1: Update Models (BookModels.cs)

Add status fields to all response models:

```csharp
public class CreateBookSimpleResponse
{
    // Add these fields
    [JsonPropertyName("status")]
    public string Status { get; set; } = "S";
    
    [JsonPropertyName("statusCode")]
    public string StatusCode { get; set; } = "200";
    
    // Existing fields...
    public string BookId { get; set; }
    public string BookCode { get; set; }
    // ...
}
```

Models to update:
- `CreateBookSimpleResponse`
- `ESarabanCreateBookResponse`
- `CreateGenerateTransferResponse`
- `GenerateCodeResponse`
- `TransferBookResponse`

### Step 2: Update Controllers (BooksController.cs)

Change from:
```csharp
return Ok(ApiResponse<CreateBookSimpleResponse>.SuccessResponse(
    response,
    "สร้างเอกสารสำเร็จ"
));
```

To:
```csharp
// K2 Compatible: Return direct format
response.Status = "S";
response.StatusCode = "200";
return Ok(response);
```

For error handling, change from:
```csharp
return StatusCode(500, ApiResponse<object>.ErrorResponse(
    "Internal server error",
    "INTERNAL_ERROR"
));
```

To:
```csharp
// K2 Compatible: Return direct error format
var errorResponse = new CreateBookSimpleResponse
{
    Status = "E",
    StatusCode = "500",
    BookId = string.Empty,
    // Set other required fields...
};
return StatusCode(500, errorResponse);
```

---

## 📝 Testing Checklist

After updates, test each endpoint:

- [ ] POST `/api/books/create/approved/simple`
- [ ] POST `/api/books/create/non-compliant/simple`
- [ ] POST `/api/books/create/under-construction/simple`
- [ ] POST `/api/books/create/original`
- [ ] POST `/api/books/create/approved`
- [ ] POST `/api/books/create/non-compliant`
- [ ] POST `/api/books/create/under-construction`
- [ ] POST `/api/books/workflow/approved`
- [ ] POST `/api/books/workflow/non-compliant`
- [ ] POST `/api/books/workflow/under-construction`
- [ ] GET  `/api/books/generate-code`
- [ ] POST `/api/books/transfer`

---

## 🎯 Expected Benefits

1. ✅ **K2 SmartObject Compatible** - Direct property access
2. ✅ **Simpler Mapping** - No nested `data` wrapper
3. ✅ **Better Performance** - Less JSON parsing overhead
4. ✅ **Consistent Format** - All endpoints use same pattern
5. ✅ **RESTful Standard** - Matches industry best practices
6. ✅ **Postman Collection Compatible** - Consistent with external API format

---

## ⚠️ Important Notes

1. **Backwards Compatibility**: This is a **breaking change** for existing consumers
2. **Documentation**: Update all API documentation and Swagger descriptions
3. **Testing**: Comprehensive testing required before production deployment
4. **K2 Configuration**: Update K2 SmartObject definitions after deployment

---

## 📚 Reference

- See `FinalOrgsResponse` model and `/final-orgs/by-action` endpoint for working example
- Postman Collection: Reference for expected response format
- K2 SmartObject Documentation: For property mapping guidelines
