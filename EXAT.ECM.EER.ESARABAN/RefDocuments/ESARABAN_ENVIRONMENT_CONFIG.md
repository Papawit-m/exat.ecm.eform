# eSaraban API Environment Configuration Guide

## 📋 Overview
คู่มือการตั้งค่า environment configuration สำหรับการเชื่อมต่อกับ eSaraban External Service API

## 🔧 Configuration Files

### 1. `appsettings.json` (Development/UAT)
```json
{
  "ESarabanApiSettings": {
    "BaseUrl": "http://api-uat.exat.co.th/esrb-external-api",
    "Timeout": 30,
    "Environment": "UAT",
    "Endpoints": {
      "BooksCreate": "/api/books/create",
      "BooksGenerateCode": "/api/books/generate-code",
      "BooksTransfer": "/api/books/transfer",
      "BooksFinalOrgs": "/api/books/final-orgs",
      "BooksFinalOrgsByAction": "/api/books/final-orgs/by-action",
      "BooksFinalOrgsByActionNoAlert": "/api/books/final-orgs/by-action/no-alert"
    }
  }
}
```

### 2. `appsettings.Production.json` (Production)
```json
{
  "ESarabanApiSettings": {
    "BaseUrl": "http://api.exat.co.th/esrb-external-api",
    "Timeout": 30,
    "Environment": "PRODUCTION",
    "Endpoints": {
      "BooksCreate": "/api/books/create",
      "BooksGenerateCode": "/api/books/generate-code",
      "BooksTransfer": "/api/books/transfer",
      "BooksFinalOrgs": "/api/books/final-orgs",
      "BooksFinalOrgsByAction": "/api/books/final-orgs/by-action",
      "BooksFinalOrgsByActionNoAlert": "/api/books/final-orgs/by-action/no-alert"
    }
  }
}
```

## 🏗️ Model Classes

### ESarabanApiSettings.cs
```csharp
namespace K2RestApi.Models
{
    public class ESarabanApiSettings
    {
        public string BaseUrl { get; set; }
        public int Timeout { get; set; }
        public string Environment { get; set; }
        public ESarabanEndpoints Endpoints { get; set; }
    }

    public class ESarabanEndpoints
    {
        public string BooksCreate { get; set; }
        public string BooksGenerateCode { get; set; }
        public string BooksTransfer { get; set; }
        public string BooksFinalOrgs { get; set; }
        public string BooksFinalOrgsByAction { get; set; }
        public string BooksFinalOrgsByActionNoAlert { get; set; }
    }
}
```

## 🚀 Usage in Controllers

### Method 1: Using IOptions (Recommended)
```csharp
using Microsoft.Extensions.Options;
using K2RestApi.Models;

public class BooksController : ControllerBase
{
    private readonly ESarabanApiSettings _esarabanSettings;

    public BooksController(IOptions<ESarabanApiSettings> esarabanSettings)
    {
        _esarabanSettings = esarabanSettings.Value;
    }

    [HttpPost("create/approved")]
    public async Task<IActionResult> CreateBookApproved()
    {
        var baseUrl = _esarabanSettings.BaseUrl;
        var endpoint = _esarabanSettings.Endpoints.BooksCreate;
        var fullUrl = $"{baseUrl}{endpoint}";
        
        // Use fullUrl to call external API
        _logger.LogInformation($"Calling eSaraban API: {fullUrl}");
        
        // Your logic here
    }
}
```

### Method 2: Using IConfiguration (Alternative)
```csharp
using Microsoft.Extensions.Configuration;

public class BooksController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public BooksController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("create/approved")]
    public async Task<IActionResult> CreateBookApproved()
    {
        var baseUrl = _configuration["ESarabanApiSettings:BaseUrl"];
        var endpoint = _configuration["ESarabanApiSettings:Endpoints:BooksCreate"];
        var fullUrl = $"{baseUrl}{endpoint}";
        
        // Use fullUrl to call external API
    }
}
```

## 📡 API Endpoints Configuration

| Endpoint Name | URL Path | HTTP Method | Description |
|--------------|----------|-------------|-------------|
| **BooksCreate** | `/api/books/create` | POST | สร้าง Book ใหม่ (3 variations: approved, non-compliant, under-construction) |
| **BooksGenerateCode** | `/api/books/generate-code` | GET | สร้างรหัสเอกสาร |
| **BooksTransfer** | `/api/books/transfer` | POST | โอนย้าย Book ระหว่างองค์กร |
| **BooksFinalOrgs** | `/api/books/final-orgs` | GET | ดึงข้อมูลองค์กรปลายทาง (base) |
| **BooksFinalOrgsByAction** | `/api/books/final-orgs/by-action` | GET | ดึงข้อมูลองค์กรปลายทาง (พร้อม Alert) |
| **BooksFinalOrgsByActionNoAlert** | `/api/books/final-orgs/by-action/no-alert` | GET | ดึงข้อมูลองค์กรปลายทาง (ไม่มี Alert) |

## 🌍 Environment Variables

### Full URL Construction
```
Full URL = BaseUrl + Endpoint

UAT Example:
http://api-uat.exat.co.th/esrb-external-api + /api/books/create
= http://api-uat.exat.co.th/esrb-external-api/api/books/create

Production Example:
http://api.exat.co.th/esrb-external-api + /api/books/generate-code
= http://api.exat.co.th/esrb-external-api/api/books/generate-code
```

## 🔐 Environment-Specific Settings

### Development/UAT
- **BaseUrl**: `http://api-uat.exat.co.th/esrb-external-api`
- **Environment**: `UAT`
- **Swagger**: Enabled
- **Logging**: Information level

### Production
- **BaseUrl**: `http://api.exat.co.th/esrb-external-api`
- **Environment**: `PRODUCTION`
- **Swagger**: Disabled
- **Logging**: Warning level

## 🧪 Testing with PowerShell

### Test Script: `test-esaraban-api-with-config.ps1`
```powershell
# Environment Configuration
$baseUrl = "http://api-uat.exat.co.th/esrb-external-api"
$localApiUrl = "http://localhost:5152/api/books"

# Test Create Book
$response = Invoke-RestMethod -Uri "$localApiUrl/create/approved?user_ad=EXAT\ECMUSR07" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### Run All Tests
```powershell
cd PsUnitTest
.\test-esaraban-api-with-config.ps1
```

## 📝 Configuration Registration in Program.cs

```csharp
// Register eSaraban API Settings
builder.Services.Configure<ESarabanApiSettings>(
    builder.Configuration.GetSection("ESarabanApiSettings"));
```

## 🔄 Switching Environments

### Using Command Line
```powershell
# Run in Development (default)
dotnet run

# Run in Production
dotnet run --environment Production

# Run in UAT
dotnet run --environment UAT
```

### Using Environment Variable
```powershell
# PowerShell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run

# CMD
set ASPNETCORE_ENVIRONMENT=Production
dotnet run
```

## ⚙️ Advanced Configuration

### Custom Timeout per Endpoint
```json
{
  "ESarabanApiSettings": {
    "BaseUrl": "http://api-uat.exat.co.th/esrb-external-api",
    "Timeout": 30,
    "Environment": "UAT",
    "Endpoints": {
      "BooksCreate": "/api/books/create",
      "BooksGenerateCode": "/api/books/generate-code"
    },
    "EndpointTimeouts": {
      "BooksCreate": 60,
      "BooksTransfer": 45,
      "BooksGenerateCode": 15
    }
  }
}
```

### Retry Policy Configuration
```json
{
  "ESarabanApiSettings": {
    "BaseUrl": "http://api-uat.exat.co.th/esrb-external-api",
    "RetryPolicy": {
      "MaxRetries": 3,
      "RetryDelaySeconds": 2,
      "EnableExponentialBackoff": true
    }
  }
}
```

## 🐛 Troubleshooting

### Issue: Configuration not loading
**Solution**: Verify JSON syntax in appsettings.json
```powershell
# Validate JSON
Get-Content appsettings.json | ConvertFrom-Json
```

### Issue: Cannot connect to eSaraban API
**Solution**: Check BaseUrl and network connectivity
```powershell
# Test connectivity
Test-NetConnection api-uat.exat.co.th -Port 80
Invoke-WebRequest -Uri "http://api-uat.exat.co.th/esrb-external-api" -Method Get
```

### Issue: Wrong environment loaded
**Solution**: Check ASPNETCORE_ENVIRONMENT variable
```powershell
# Check current environment
$env:ASPNETCORE_ENVIRONMENT

# Set correct environment
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

## 📚 Related Documentation
- [BOOKS_CREATE_API_GUIDE.md](./BOOKS_CREATE_API_GUIDE.md) - Books API creation guide
- [K2_INTEGRATION_GUIDE.md](./K2_INTEGRATION_GUIDE.md) - K2 integration guide
- [ESARABAN_API_ENDPOINTS.md](./ESARABAN_API_ENDPOINTS.md) - eSaraban API endpoints reference

## 🎯 Best Practices

1. **Never commit production passwords** to version control
2. **Use Azure Key Vault** or similar for production secrets
3. **Validate configuration** on application startup
4. **Log configuration errors** but don't expose sensitive data
5. **Use different base URLs** for each environment
6. **Set appropriate timeouts** based on endpoint complexity
7. **Implement retry policies** for production resilience

## 🔗 Example: Complete Implementation

```csharp
using Microsoft.Extensions.Options;
using K2RestApi.Models;

public class BooksController : ControllerBase
{
    private readonly ESarabanApiSettings _esarabanSettings;
    private readonly ILogger<BooksController> _logger;
    private readonly HttpClient _httpClient;

    public BooksController(
        IOptions<ESarabanApiSettings> esarabanSettings,
        ILogger<BooksController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _esarabanSettings = esarabanSettings.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(_esarabanSettings.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_esarabanSettings.Timeout);
    }

    [HttpPost("create/approved")]
    public async Task<IActionResult> CreateBookApproved(
        [FromQuery] string user_ad,
        [FromBody] CreateBookApprovedRequest request)
    {
        try
        {
            var endpoint = _esarabanSettings.Endpoints.BooksCreate;
            
            _logger.LogInformation(
                $"Calling eSaraban API [{_esarabanSettings.Environment}]: " +
                $"{_esarabanSettings.BaseUrl}{endpoint}");

            // TODO: Implement actual HTTP call to eSaraban API
            // var response = await _httpClient.PostAsJsonAsync(endpoint, request);
            
            return Ok(ApiResponse<object>.SuccessResponse(
                new { message = "Configuration loaded successfully" }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling eSaraban API");
            return StatusCode(500, ApiResponse<object>.ErrorResponse(
                "Internal server error", "ESARABAN_API_ERROR"));
        }
    }
}
```

---

**Last Updated**: October 30, 2025  
**Version**: 1.0  
**Environment**: UAT & Production
