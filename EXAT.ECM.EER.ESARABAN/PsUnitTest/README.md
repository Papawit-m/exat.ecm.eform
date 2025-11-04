# 🔧 PowerShell Scripts

โฟลเดอร์นี้เก็บ PowerShell scripts สำหรับการทดสอบและ automation ต่างๆ ของโปรเจ็กต์ K2 REST Service API

## 📄 Scripts ทั้งหมด

### API Testing Scripts (Enhanced with Real-Time Logs) 🆕

#### test-full-format-with-logs.ps1 ⭐ NEW (v1.5.1)
- **คำอธิบาย:** Complete test suite สำหรับ Create Books Full Format endpoints (4 endpoints) พร้อม real-time log monitoring
- **การใช้งาน:**
  ```powershell
  # Run all tests with auto API start/stop
  .\PsUnitTest\test-full-format-with-logs.ps1
  
  # Use existing API instance
  .\PsUnitTest\test-full-format-with-logs.ps1 -SkipApiStart
  
  # Keep API running after tests
  .\PsUnitTest\test-full-format-with-logs.ps1 -KeepApiRunning
  ```
- **คุณสมบัติ:**
  - ✅ Auto start/stop API server
  - ✅ Real-time log capture and display
  - ✅ Detailed DEBUG logs from eSaraban API calls
  - ✅ Color-coded log output (Error/Warning/Info/Debug)
  - ✅ Comprehensive error diagnostics
  - ✅ Test result summary with pass/fail statistics
  - ✅ Log file preservation for debugging
  - ✅ Tests all 4 Full Format endpoints
- **Parameters:**
  - `-SkipApiStart`: Use existing API instance (don't start new one)
  - `-KeepApiRunning`: Don't stop API after tests complete
- **Output:**
  - Console: Colored test results with embedded logs
  - File: Full API logs saved to `%TEMP%\k2restapi-test-YYYYMMDD-HHMMSS.log`
- **Tested Endpoints:**
  1. POST `/api/books/create/approved`
  2. POST `/api/books/create/non-compliant`
  3. POST `/api/books/create/under-construction`
  4. POST `/api/books/create/original`

#### watch-api-logs.ps1 ⭐ NEW (v1.5.1)
- **คำอธิบาย:** Real-time log viewer สำหรับ monitoring K2RestApi logs
- **การใช้งาน:**
  ```powershell
  # Watch all logs in real-time
  .\PsUnitTest\watch-api-logs.ps1
  
  # Watch only DEBUG logs
  .\PsUnitTest\watch-api-logs.ps1 -Filter "DEBUG"
  
  # Watch ERROR/Exception logs
  .\PsUnitTest\watch-api-logs.ps1 -Filter "ERROR|Exception"
  
  # Show last 100 lines
  .\PsUnitTest\watch-api-logs.ps1 -Lines 100
  
  # Show logs once (no follow)
  .\PsUnitTest\watch-api-logs.ps1 -Follow:$false
  ```
- **คุณสมบัติ:**
  - ✅ Real-time log streaming (follow mode)
  - ✅ Color-coded output based on log level
  - ✅ Regex filtering support
  - ✅ Timestamp highlighting
  - ✅ Auto-find latest log file
  - ✅ Refresh rate control
- **Parameters:**
  - `-Filter`: Regex pattern to filter logs
  - `-Lines`: Number of lines to show (default: 50)
  - `-RefreshMs`: Refresh interval in milliseconds (default: 1000)
  - `-Follow`: Enable/disable follow mode (default: $true)
- **Output:** Color-coded real-time logs to console

#### quick-test-endpoint.ps1 ⭐ NEW (v1.5.1)
- **คำอธิบาย:** Quick test single endpoint with detailed error diagnostics
- **การใช้งาน:**
  ```powershell
  # Test approved endpoint
  .\PsUnitTest\quick-test-endpoint.ps1 -Endpoint "approved"
  
  # Test non-compliant endpoint
  .\PsUnitTest\quick-test-endpoint.ps1 -Endpoint "non-compliant"
  
  # Test under-construction endpoint
  .\PsUnitTest\quick-test-endpoint.ps1 -Endpoint "under-construction"
  
  # Test original endpoint
  .\PsUnitTest\quick-test-endpoint.ps1 -Endpoint "original"
  ```
- **คุณสมบัติ:**
  - ✅ Fast single endpoint testing
  - ✅ Detailed response display (all fields)
  - ✅ Full JSON response output
  - ✅ Response time measurement
  - ✅ Comprehensive error diagnostics
  - ✅ API health check before test
- **Parameters:**
  - `-Endpoint`: Endpoint to test (approved/non-compliant/under-construction/original)
  - `-BaseUrl`: API base URL (default: http://localhost:5152)
  - `-TestFile`: Request body file path
- **Output:**
  - Success: Full response with all fields + JSON
  - Failure: Detailed error with HTTP status, message, and exception details

### API Testing Scripts (Legacy)

#### test-books-api-complete.ps1 ⭐ NEW (v1.3.1)
- **คำอธิบาย:** Complete test suite สำหรับ Books API ทั้งหมด 14 endpoints
- **การใช้งาน:**
  ```powershell
  # Run all tests
  .\PsUnitTest\test-books-api-complete.ps1
  
  # Run with verbose output
  .\PsUnitTest\test-books-api-complete.ps1 -Verbose
  
  # Skip real eSaraban API calls
  .\PsUnitTest\test-books-api-complete.ps1 -SkipRealApiTests
  
  # Custom base URL and user
  .\PsUnitTest\test-books-api-complete.ps1 -BaseUrl "http://api-uat.example.com" -UserAd "EXAT\USER01"
  ```
- **คุณสมบัติ:**
  - ✅ Tests all 14 Books API endpoints
  - ✅ Validates response format (Direct vs ApiResponse wrapper)
  - ✅ Colored output with test results
  - ✅ Test counters (Passed/Failed/Skipped)
  - ✅ JSON export of test results
  - ✅ Verbose mode for detailed output
  - ✅ Skip real API tests option
  - ✅ Automatic test body loading from ExamBodyRequest
- **Parameters:**
  - `-BaseUrl`: API base URL (default: http://localhost:5152)
  - `-UserAd`: Active Directory username (default: EXAT\ECMUSR07)
  - `-SkipRealApiTests`: Skip tests that call real eSaraban API
  - `-Verbose`: Show detailed test information
- **Output:**
  - Console: Colored test results with pass/fail status
  - File: JSON test results exported to `test-results-YYYYMMDD-HHMMSS.json`
- **เอกสารอ้างอิง:** [VERSION_1.3.1_CHANGELOG.md](../RefDocuments/VERSION_1.3.1_CHANGELOG.md)

### Database Management Scripts

#### clone-table.ps1
- **คำอธิบาย:** สคริปต์สำหรับ clone table structure จาก source table ไปเป็น new table
- **การใช้งาน:**
  ```powershell
  .\PsUnitTest\clone-table.ps1
  ```
- **คุณสมบัติ:**
  - ตรวจสอบว่า source table มีอยู่
  - ตรวจสอบว่า target table ไม่ซ้ำ
  - Clone table structure (ไม่รวมข้อมูล)
  - แสดงผลแบบ step-by-step
  - ตรวจสอบและแสดงโครงสร้าง table ที่สร้างแล้ว
- **เอกสารอ้างอิง:** [CLONE_TABLE_GUIDE.md](../RefDocuments/CLONE_TABLE_GUIDE.md)

## 📋 การใช้งาน

### ความต้องการ
- PowerShell 5.1 หรือสูงกว่า
- API ต้องรันอยู่ที่ http://localhost:5152
- สิทธิ์ในการ execute scripts

### การรันสคริปต์

#### วิธีที่ 1: รันจาก root folder
```powershell
.\PsUnitTest\clone-table.ps1
```

#### วิธีที่ 2: รันจากภายใน PsUnitTest folder
```powershell
cd PsUnitTest
.\clone-table.ps1
```

#### วิธีที่ 3: รันแบบ bypass execution policy
```powershell
powershell -ExecutionPolicy Bypass -File .\PsUnitTest\clone-table.ps1
```

### Execution Policy

หากพบปัญหา execution policy ให้ตั้งค่า:
```powershell
# สำหรับ current user
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# หรือ bypass ชั่วคราว
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
```

## 🛠️ การพัฒนาสคริปต์ใหม่

### Guidelines
1. **ตั้งชื่อไฟล์:** ใช้ kebab-case (เช่น `clone-table.ps1`, `backup-database.ps1`)
2. **Comments:** เพิ่ม header comments อธิบายจุดประสงค์และการใช้งาน
3. **Error Handling:** ใช้ try-catch และแสดง error messages ที่ชัดเจน
4. **User Feedback:** แสดงความคืบหน้าและผลลัพธ์ที่เข้าใจง่าย
5. **Parameters:** รองรับ parameters สำหรับความยืดหยุ่น
6. **Documentation:** เพิ่มข้อมูลใน README นี้เมื่อสร้างสคริปต์ใหม่

### Template สำหรับสคริปต์ใหม่
```powershell
# PowerShell Script: [Script Name]
# Description: [What this script does]
# Author: [Your name]
# Date: [Creation date]
# Usage: .\script-name.ps1 [-Parameter1 value1] [-Parameter2 value2]

param(
    [Parameter(Mandatory=$false)]
    [string]$Parameter1 = "default-value",
    
    [Parameter(Mandatory=$false)]
    [string]$Parameter2 = "default-value"
)

# Script configuration
$ErrorActionPreference = "Stop"
$apiUrl = "http://localhost:5152"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  [Script Name]" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Main script logic here
try {
    # Your code here
    
    Write-Host "✓ Operation completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
```

## 🔍 Testing

### การทดสอบสคริปต์
1. ตรวจสอบ syntax:
   ```powershell
   Get-Command .\PsUnitTest\script-name.ps1 -Syntax
   ```

2. ทดสอบด้วย `-WhatIf` (ถ้ารองรับ):
   ```powershell
   .\PsUnitTest\script-name.ps1 -WhatIf
   ```

3. ทดสอบด้วย verbose output:
   ```powershell
   .\PsUnitTest\script-name.ps1 -Verbose
   ```

## 📊 สคริปต์ที่วางแผนจะเพิ่ม (Future)

- [ ] `backup-database.ps1` - Backup Oracle database
- [ ] `restore-database.ps1` - Restore Oracle database
- [ ] `test-api-endpoints.ps1` - ทดสอบ API endpoints ทั้งหมด
- [ ] `deploy-api.ps1` - Deploy API to server
- [ ] `generate-test-data.ps1` - สร้างข้อมูลทดสอบ
- [ ] `cleanup-logs.ps1` - ลบ log files เก่า
- [ ] `health-check.ps1` - ตรวจสอบสถานะ API และ Database

## 🗂️ โครงสร้าง

```
PsUnitTest/
├── README.md              # ไฟล์นี้
└── clone-table.ps1        # Clone table structure script
```

## 📚 เอกสารที่เกี่ยวข้อง

- **[CLONE_TABLE_GUIDE.md](../RefDocuments/CLONE_TABLE_GUIDE.md)** - คู่มือการ clone table
- **[ORACLE_INTEGRATION_GUIDE.md](../RefDocuments/ORACLE_INTEGRATION_GUIDE.md)** - คู่มือ Oracle integration
- **[README.md](../README.md)** - โปรเจ็กต์หลัก

## 💡 Tips

1. **ใช้ ISE หรือ VS Code** สำหรับเขียนและ debug scripts
2. **ทดสอบใน Development environment** ก่อนรันใน Production
3. **เก็บ credentials** ใน secure storage ไม่ใส่ใน scripts
4. **ใช้ logging** สำหรับการติดตาม execution
5. **Version control** scripts ทั้งหมดผ่าน Git

## 🔒 Security Notes

- ⚠️ ไม่เก็บ passwords หรือ sensitive data ใน scripts
- ⚠️ ใช้ environment variables หรือ secure storage
- ⚠️ ระวังการรัน scripts ที่มา download จาก internet
- ⚠️ ตรวจสอบ code ก่อนรันเสมอ

---

**หมายเหตุ:** ต่อไปนี้ไฟล์ .ps1 ทั้งหมดต้องสร้างภายใต้โฟลเดอร์ `PsUnitTest/` เท่านั้น

**อัปเดทล่าสุด:** 30 ตุลาคม 2025

---
[← กลับไปที่ README หลัก](../README.md)
