# Raw Response Format Change - Transfer & Query Endpoints

**Date**: November 4, 2025  
**Type**: Response Format Change  
**Breaking Change**: ⚠️ **YES** - Response structure changed  
**Affected Endpoints**: 3 endpoints

---

## Overview

Changed response format from **K2 Compatible (Direct)** to **Raw Response (with ApiResponse wrapper)** for Transfer and Final Organizations query endpoints.

### Why This Change?

- Transfer and Query endpoints should return **standard API format** with `ApiResponse<T>` wrapper
- Provides consistent error handling structure
- Separates API metadata from business data
- Better debugging and monitoring capabilities

---

## Affected Endpoints

### 1. POST `/api/books/transfer`
**Purpose**: โอนย้าย Book ระหว่างองค์กร

### 2. GET `/api/books/final-orgs/by-action`
**Purpose**: ดึงข้อมูลองค์กรปลายทาง (พร้อม Alert)

### 3. GET `/api/books/final-orgs/by-action/no-alert`
**Purpose**: ดึงข้อมูลองค์กรปลายทาง (ไม่มี Alert)

---

## Response Format Changes

### Before (K2 Compatible - Direct Response)

**Success Response**:
```json
{
  "status": "S",
  "status_code": "200",
  "message": "Success: book transferred.",
  "book_id": "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6",
  "transfer_id": "550e8400-e29b-41d4-a716-446655440000",
  "original_org_code": "J10000",
  "destination_org_code": "J20000",
  "transfer_status": "COMPLETED"
}
```

**Error Response**:
```json
{
  "status": "E",
  "status_code": "500",
  "message": "Internal server error: Connection timeout"
}
```

### After (Raw Response - with ApiResponse Wrapper)

**Success Response**:
```json
{
  "success": true,
  "message": "Book transferred successfully",
  "data": {
    "status": "S",
    "status_code": "200",
    "message": "Success: book transferred.",
    "book_id": "A1B2C3D4E5F6G7H8I9J0K1L2M3N4O5P6",
    "transfer_id": "550e8400-e29b-41d4-a716-446655440000",
    "original_org_code": "J10000",
    "destination_org_code": "J20000",
    "transfer_status": "COMPLETED"
  },
  "error_code": null
}
```

**Error Response**:
```json
{
  "success": false,
  "message": "Internal server error: Connection timeout",
  "data": null,
  "error_code": "TRANSFER_ERROR"
}
```

---

## Code Changes

### 1. POST `/api/books/transfer`

**Before**:
```csharp
// K2 Compatible: Return direct response without ApiResponse wrapper
return Ok(response);

// Error
var errorResponse = new TransferBookResponse
{
    Status = "E",
    StatusCode = "500",
    Message = $"Internal server error: {ex.Message}"
};
return StatusCode(500, errorResponse);
```

**After**:
```csharp
// Raw Response: Return with ApiResponse wrapper
return Ok(ApiResponse<TransferBookResponse>.SuccessResponse(
    response, 
    "Book transferred successfully"
));

// Error
return StatusCode(500, ApiResponse<object>.ErrorResponse(
    $"Internal server error: {ex.Message}",
    "TRANSFER_ERROR"
));
```

### 2. GET `/api/books/final-orgs/by-action`

**Before**:
```csharp
// K2 Compatible: Return direct format (without ApiResponse wrapper)
return Ok(response);

// Error
var errorResponse = new FinalOrgsResponse
{
    Status = "E",
    StatusCode = "500",
    Books = new List<OrganizationInfo>()
};
return StatusCode(500, errorResponse);
```

**After**:
```csharp
// Raw Response: Return with ApiResponse wrapper
return Ok(ApiResponse<FinalOrgsResponse>.SuccessResponse(
    response, 
    "Final organizations retrieved successfully"
));

// Error
return StatusCode(500, ApiResponse<object>.ErrorResponse(
    $"Error retrieving final organizations: {ex.Message}",
    "GET_FINAL_ORGS_ERROR"
));
```

### 3. GET `/api/books/final-orgs/by-action/no-alert`

**Before**:
```csharp
// K2 Compatible: Return direct format (without ApiResponse wrapper)
return Ok(response);

// Error
var errorResponse = new FinalOrgsResponse
{
    Status = "E",
    StatusCode = "500",
    Books = new List<OrganizationInfo>()
};
return StatusCode(500, errorResponse);
```

**After**:
```csharp
// Raw Response: Return with ApiResponse wrapper
return Ok(ApiResponse<FinalOrgsResponse>.SuccessResponse(
    response, 
    "Final organizations retrieved successfully (no alert)"
));

// Error
return StatusCode(500, ApiResponse<object>.ErrorResponse(
    $"Error retrieving final organizations: {ex.Message}",
    "GET_FINAL_ORGS_NO_ALERT_ERROR"
));
```

---

## Response Structure Comparison

### ApiResponse<T> Structure

```csharp
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }
}
```

### Access Pattern Changes

**Before (Direct Access)**:
```javascript
// K2 SmartObject or JavaScript
var bookId = response.book_id;
var status = response.status;
var transferId = response.transfer_id;
```

**After (Nested Access)**:
```javascript
// Standard API clients
var bookId = response.data.book_id;
var status = response.data.status;
var transferId = response.data.transfer_id;

// Check for errors
if (!response.success) {
    console.error(response.message);
    console.error(response.error_code);
}
```

---

## Impact Analysis

### Breaking Changes

| Aspect | Impact | Severity |
|--------|--------|----------|
| **Response Structure** | Property access path changed from `response.field` to `response.data.field` | 🔴 HIGH |
| **Error Handling** | Error format changed, now includes `success`, `error_code` | 🔴 HIGH |
| **K2 SmartObject** | SmartObject mappings need to be updated | 🔴 HIGH |
| **API Clients** | All clients need to update response parsing | 🔴 HIGH |

### Who Is Affected?

1. **K2 SmartObject Users**: Must update SmartObject method mappings
2. **JavaScript/TypeScript Clients**: Must update response handling code
3. **PowerShell Scripts**: Must update property access (e.g., `$response.data.book_id`)
4. **Postman Collections**: Need to update test assertions
5. **Integration Tests**: Must update expected response structure

---

## Migration Guide

### For PowerShell Scripts

**Before**:
```powershell
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/transfer" -Method Post -Body $body
Write-Host "Book ID: $($response.book_id)"
Write-Host "Transfer ID: $($response.transfer_id)"
```

**After**:
```powershell
$response = Invoke-RestMethod -Uri "http://localhost:5152/api/books/transfer" -Method Post -Body $body

# Check for success
if ($response.success) {
    Write-Host "Book ID: $($response.data.book_id)"
    Write-Host "Transfer ID: $($response.data.transfer_id)"
} else {
    Write-Error "Error: $($response.message) (Code: $($response.error_code))"
}
```

### For K2 SmartObject

**Before - Direct Mapping**:
```
Response Properties:
- book_id (map to: book_id)
- transfer_id (map to: transfer_id)
- status (map to: status)
```

**After - Nested Mapping**:
```
Response Properties:
- book_id (map to: data.book_id)
- transfer_id (map to: data.transfer_id)
- status (map to: data.status)
- success (map to: success)
- error_code (map to: error_code)
```

### For JavaScript/TypeScript

**Before**:
```typescript
const response = await fetch('/api/books/transfer', {
    method: 'POST',
    body: JSON.stringify(requestData)
});
const data = await response.json();
console.log(data.book_id);
console.log(data.transfer_id);
```

**After**:
```typescript
const response = await fetch('/api/books/transfer', {
    method: 'POST',
    body: JSON.stringify(requestData)
});
const result = await response.json();

if (result.success) {
    console.log(result.data.book_id);
    console.log(result.data.transfer_id);
} else {
    console.error(`Error: ${result.message} (${result.error_code})`);
}
```

---

## Testing Examples

### Test 1: Transfer Book

**Request**:
```bash
POST http://localhost:5152/api/books/transfer?user_ad=EXAT\ECMUSR07&book_id=ABC123&original_org_code=J10000&destination_org_code=J20000
Content-Type: application/json

{
  "transfer_reason": "ตามคำสั่งผู้บริหาร",
  "transfer_note": "เร่งด่วน"
}
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Book transferred successfully",
  "data": {
    "status": "S",
    "status_code": "200",
    "message": "Success: book transferred.",
    "book_id": "ABC123",
    "transfer_id": "550e8400-e29b-41d4-a716-446655440000",
    "original_org_code": "J10000",
    "destination_org_code": "J20000",
    "transfer_reason": "ตามคำสั่งผู้บริหาร",
    "transfer_note": "เร่งด่วน",
    "transfer_status": "COMPLETED",
    "transferred_by": "EXAT\\ECMUSR07",
    "transferred_date": "2025-11-04T15:30:00"
  },
  "error_code": null
}
```

**PowerShell Test**:
```powershell
$body = @{
    transfer_reason = "ตามคำสั่งผู้บริหาร"
    transfer_note = "เร่งด่วน"
} | ConvertTo-Json

$response = Invoke-RestMethod `
    -Uri "http://localhost:5152/api/books/transfer?user_ad=EXAT\ECMUSR07&book_id=ABC123&original_org_code=J10000&destination_org_code=J20000" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"

# Verify response structure
if ($response.success) {
    Write-Host "✅ Transfer Success!" -ForegroundColor Green
    Write-Host "Book ID: $($response.data.book_id)"
    Write-Host "Transfer ID: $($response.data.transfer_id)"
    Write-Host "Status: $($response.data.transfer_status)"
} else {
    Write-Host "❌ Transfer Failed!" -ForegroundColor Red
    Write-Host "Error: $($response.message)"
    Write-Host "Code: $($response.error_code)"
}
```

### Test 2: Get Final Organizations

**Request**:
```bash
GET http://localhost:5152/api/books/final-orgs/by-action?user_ad=EXAT\ECMUSR07&book_id=ABC123
```

**Expected Response (200 OK)**:
```json
{
  "success": true,
  "message": "Final organizations retrieved successfully",
  "data": {
    "status": "S",
    "status_code": "200",
    "books": [
      {
        "running_no": 1,
        "send_org_name_th": "กองกรรมสิทธิ์ที่ดิน",
        "send_date": "04-NOV-25",
        "receive_code": null,
        "receive_date": null,
        "receive_org_name_th": "J10000 ฝ่ายกรรมสิทธิ์ที่ดิน",
        "status_name_th": "รอดำเนินการรับหนังสือ",
        "receive_comment": null,
        "book_id": "ABC123"
      }
    ]
  },
  "error_code": null
}
```

**PowerShell Test**:
```powershell
$response = Invoke-RestMethod `
    -Uri "http://localhost:5152/api/books/final-orgs/by-action?user_ad=EXAT\ECMUSR07&book_id=ABC123" `
    -Method Get

if ($response.success) {
    Write-Host "✅ Query Success!" -ForegroundColor Green
    Write-Host "Organizations found: $($response.data.books.Count)"
    
    foreach ($org in $response.data.books) {
        Write-Host "  $($org.running_no). $($org.receive_org_name_th)" -ForegroundColor Cyan
    }
} else {
    Write-Host "❌ Query Failed!" -ForegroundColor Red
    Write-Host "Error: $($response.message)"
}
```

---

## Error Codes

### Transfer Endpoint

| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| `USER_AD_REQUIRED` | 400 | user_ad parameter is missing |
| `BOOK_ID_REQUIRED` | 400 | book_id parameter is missing |
| `ORIGINAL_ORG_CODE_REQUIRED` | 400 | original_org_code parameter is missing |
| `DESTINATION_ORG_CODE_REQUIRED` | 400 | destination_org_code parameter is missing |
| `REQUEST_BODY_REQUIRED` | 400 | Request body is missing |
| `TRANSFER_ERROR` | 500 | Internal server error during transfer |

### Final Organizations Endpoints

| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| `USER_AD_REQUIRED` | 400 | user_ad parameter is missing |
| `BOOK_ID_REQUIRED` | 400 | book_id parameter is missing |
| `GET_FINAL_ORGS_ERROR` | 500 | Error retrieving organizations (with alert) |
| `GET_FINAL_ORGS_NO_ALERT_ERROR` | 500 | Error retrieving organizations (no alert) |

---

## Benefits of Raw Response Format

### 1. Consistent Error Handling
```typescript
// Single error handling pattern for all endpoints
if (!response.success) {
    handleError(response.message, response.error_code);
    return;
}
// Process response.data
```

### 2. Better Monitoring
```csharp
// Log success/failure rate easily
logger.LogInformation($"API Success: {response.Success}, Code: {response.ErrorCode}");
```

### 3. Separation of Concerns
```json
{
  "success": true,        // API layer status
  "message": "...",       // API layer message
  "data": {
    "status": "S",        // Business layer status
    "message": "...",     // Business layer message
    ...                   // Business data
  }
}
```

### 4. Standard API Patterns
- Follows common REST API conventions
- Compatible with OpenAPI/Swagger standards
- Easier integration with third-party tools

---

## Rollback Plan

If this change causes issues, you can revert by:

```bash
# Revert to previous commit
git revert HEAD

# Or manually change back
# In BooksController.cs, replace:
return Ok(ApiResponse<T>.SuccessResponse(response, "..."));

# With:
return Ok(response);
```

---

## Next Steps

1. ✅ **Code Updated**: 3 endpoints modified
2. ✅ **Build Successful**: No compilation errors
3. ⏳ **Update Tests**: PowerShell test scripts need updating
4. ⏳ **Update K2 SmartObjects**: If these endpoints are used in K2
5. ⏳ **Update Documentation**: API documentation and Swagger annotations
6. ⏳ **Notify Stakeholders**: Inform all API consumers about breaking change

---

## Related Documentation

- `RefDocuments/VERSION_1.3_CHANGELOG.md` - Current version details
- `RefDocuments/K2_COMPATIBILITY_TEST_RESULTS.md` - K2 integration testing
- `RefDocuments/API_CREATE_IMPLEMENTATION.md` - Books API implementation
- `Models/ApiResponse.cs` - ApiResponse wrapper implementation

---

## Summary

| Metric | Value |
|--------|-------|
| **Endpoints Changed** | 3 |
| **Breaking Change** | ✅ YES |
| **Build Status** | ✅ SUCCESS |
| **Migration Difficulty** | 🟡 MEDIUM |
| **Recommended Action** | Update all API clients and test thoroughly |

**Version Note**: This change should be documented in a new version (e.g., v1.3.1 or v1.4) due to breaking changes.
