# eSaraban External Service API - Selected Endpoints

## 📋 Overview
เอกสารนี้สรุป 4 endpoints หลักที่ใช้งานจาก eSaraban External Service API

**Base URL**: `{{baseUrl}}` (กำหนดใน Postman environment)

---

## 🔹 1. Create Book - `/api/books/create`

### Endpoint Details
- **Method**: `POST`
- **URL**: `{{baseUrl}}/api/books/create?user_ad={user_ad}`
- **Content-Type**: `application/json`
- **Accept**: `text/plain`

### Query Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `user_ad` | string | ✅ Yes | Active Directory username (e.g., `EXAT\ECMUSR07`) |

### Request Body Structure
```json
{
  "book_title": "string",
  "book_type_id": "string",
  "registration_book_id": "string",
  "book_year": "integer",
  "org_code": "string",
  "create_by": "string"
}
```

### Response
- **Success**: HTTP 200
- **Not Found**: HTTP 404
- **Server Error**: HTTP 500

### Example Usage
```powershell
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "text/plain"
}

$body = @{
    book_title = "Test Document"
    book_type_id = "TYPE001"
    registration_book_id = "REG001"
    book_year = 2025
    org_code = "J10100"
    create_by = "EXAT\ECMUSR07"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://your-api.com/api/books/create?user_ad=EXAT\ECMUSR07" `
    -Method POST `
    -Headers $headers `
    -Body $body
```

---

## 🔹 2. Generate Code - `/api/books/generate-code`

### Endpoint Details
- **Method**: `GET`
- **URL**: `{{baseUrl}}/api/books/generate-code`

### Query Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `user_ad` | string | ✅ Yes | Active Directory username |
| `book_id` | string | ✅ Yes | Book ID (GUID format) |

### Response
- **Success**: HTTP 200 - Returns generated code
- **Not Found**: HTTP 404
- **Server Error**: HTTP 500

### Example Usage
```powershell
$user_ad = "EXAT\ECMUSR07"
$book_id = "269B1ABF2ABE46968B78F099EAC90588"

$response = Invoke-RestMethod -Uri "https://your-api.com/api/books/generate-code?user_ad=$user_ad&book_id=$book_id" `
    -Method GET
```

---

## 🔹 3. Transfer Book - `/api/books/transfer`

### Endpoint Details
- **Method**: `POST`
- **URL**: `{{baseUrl}}/api/books/transfer`
- **Content-Type**: `application/json`
- **Accept**: `text/plain`

### Query Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `user_ad` | string | ✅ Yes | Active Directory username |
| `book_id` | string | ✅ Yes | Book ID to transfer |
| `tranfer_id` | string | ❌ No | Transfer ID (can be null) |
| `original_org_code` | string | ✅ Yes | Source organization code |
| `destination_org_code` | string | ✅ Yes | Destination organization code |

### Request Body Structure
```json
{
  "transfer_reason": "string",
  "transfer_note": "string",
  "create_by": "string"
}
```

### Response
- **Success**: HTTP 200
- **Not Found**: HTTP 404
- **Server Error**: HTTP 500

### Example Usage
```powershell
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "text/plain"
}

$body = @{
    transfer_reason = "Organization restructure"
    transfer_note = "Moving to new department"
    create_by = "EXAT\ECMUSR07"
} | ConvertTo-Json

$params = @{
    user_ad = "EXAT\ECMUSR07"
    book_id = "269B1ABF2ABE46968B78F099EAC90588"
    tranfer_id = "null"
    original_org_code = "J10100"
    destination_org_code = "J10000"
}

$queryString = ($params.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join "&"

$response = Invoke-RestMethod -Uri "https://your-api.com/api/books/transfer?$queryString" `
    -Method POST `
    -Headers $headers `
    -Body $body
```

---

## 🔹 4. Final Organizations - `/api/books/final-orgs`

### Endpoint Variations

#### 4.1 By Action with Alert
- **Method**: `GET`
- **URL**: `{{baseUrl}}/api/books/final-orgs/by-action`

#### 4.2 By Action without Alert
- **Method**: `GET`
- **URL**: `{{baseUrl}}/api/books/final-orgs/by-action/no-alert`

### Query Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `user_ad` | string | ✅ Yes | Active Directory username |
| `book_id` | string | ✅ Yes | Book ID |

### Response
- **Success**: HTTP 200 - Returns list of final organizations
- **Not Found**: HTTP 404
- **Server Error**: HTTP 500

### Example Usage
```powershell
# With Alert
$user_ad = "EXAT\ECMUSR07"
$book_id = "269B1ABF2ABE46968B78F099EAC90588"

$response = Invoke-RestMethod -Uri "https://your-api.com/api/books/final-orgs/by-action?user_ad=$user_ad&book_id=$book_id" `
    -Method GET

# Without Alert
$response = Invoke-RestMethod -Uri "https://your-api.com/api/books/final-orgs/by-action/no-alert?user_ad=$user_ad&book_id=$book_id" `
    -Method GET
```

---

## 🔧 Common Response Codes

| Code | Description |
|------|-------------|
| 200 | Success - Request completed successfully |
| 404 | Not Found - Resource not found |
| 500 | Server Error - Internal server error |

---

## 📝 Notes

### Authentication
- All endpoints require `user_ad` parameter with Active Directory username
- Format: `DOMAIN\USERNAME` (e.g., `EXAT\ECMUSR07`)

### GUID Format
- `book_id`, `book_type_id`, etc. use GUID format
- Example: `269B1ABF2ABE46968B78F099EAC90588`

### Organization Codes
- Organization codes follow format: `{Letter}{5 digits}`
- Examples: `J10100`, `J10000`

### Best Practices
1. Always URL-encode special characters in query parameters
2. Handle errors appropriately (404, 500)
3. Use proper Content-Type headers for POST requests
4. Validate GUID formats before making requests
5. Log all API calls for troubleshooting

---

## 🔗 Related Documentation
- [K2 Integration Guide](./K2_INTEGRATION_GUIDE.md)
- [Oracle Integration Guide](./ORACLE_INTEGRATION_GUIDE.md)
- [Project Summary](./PROJECT_SUMMARY.md)

---

*Last Updated: October 30, 2025*
