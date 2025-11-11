# eSaraban API Test Script
# สคริปต์สำหรับทดสอบการเรียกใช้ 4 endpoints หลักของ eSaraban External Service API
# Created: October 30, 2025

<#
.SYNOPSIS
    ทดสอบการเรียกใช้ eSaraban External Service API endpoints

.DESCRIPTION
    สคริปต์นี้ทดสอบ 4 endpoints หลัก:
    1. /api/books/create - สร้าง book ใหม่
    2. /api/books/generate-code - สร้าง code สำหรับ book
    3. /api/books/transfer - โอนย้าย book ระหว่างองค์กร
    4. /api/books/final-orgs - ดึงข้อมูลองค์กรปลายทาง

.PARAMETER BaseUrl
    Base URL ของ API (default: http://localhost:5152)

.PARAMETER UserAD
    Active Directory username (default: EXAT\ECMUSR07)

.EXAMPLE
    .\test-esaraban-api.ps1
    
.EXAMPLE
    .\test-esaraban-api.ps1 -BaseUrl "https://api.example.com" -UserAD "EXAT\ECMUSR01"
#>

param(
    [string]$BaseUrl = "http://localhost:5152",
    [string]$UserAD = "EXAT\ECMUSR07"
)

# ฟังก์ชันแสดงข้อความสี
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# ฟังก์ชันแสดงหัวข้อ
function Write-Section {
    param([string]$Title)
    Write-Host "`n$("=" * 80)" -ForegroundColor Cyan
    Write-Host " $Title" -ForegroundColor Cyan
    Write-Host "$("=" * 80)" -ForegroundColor Cyan
}

# ฟังก์ชันแสดงผลลัพธ์
function Show-Result {
    param(
        [string]$Status,
        [object]$Response,
        [string]$ErrorMessage
    )
    
    if ($Status -eq "Success") {
        Write-ColorOutput "✅ Status: SUCCESS" "Green"
        Write-ColorOutput "📦 Response:" "Yellow"
        $Response | ConvertTo-Json -Depth 5 | Write-Host
    } else {
        Write-ColorOutput "❌ Status: FAILED" "Red"
        Write-ColorOutput "⚠️  Error: $ErrorMessage" "Red"
    }
}

# ตัวแปรสำหรับเก็บ Book ID ที่สร้างขึ้น
$CreatedBookId = $null

Write-ColorOutput "`n🚀 eSaraban External Service API Test Script" "Magenta"
Write-ColorOutput "Base URL: $BaseUrl" "Gray"
Write-ColorOutput "User AD: $UserAD`n" "Gray"

# =============================================================================
# Test 1: Create Book
# =============================================================================
Write-Section "Test 1: Create Book - POST /api/books/create"

try {
    $headers = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
    
    $createBody = @{
        book_title = "Test Document - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        book_type_id = "TYPE001"
        registration_book_id = "REG001"
        book_year = (Get-Date).Year
        org_code = "J10100"
        create_by = $UserAD
    } | ConvertTo-Json
    
    Write-ColorOutput "📤 Request Body:" "Yellow"
    Write-Host $createBody
    
    $createUrl = "$BaseUrl/api/books/create?user_ad=$([System.Uri]::EscapeDataString($UserAD))"
    Write-ColorOutput "`n🌐 Calling: POST $createUrl" "Cyan"
    
    $response = Invoke-RestMethod -Uri $createUrl `
        -Method POST `
        -Headers $headers `
        -Body $createBody `
        -ErrorAction Stop
    
    Show-Result -Status "Success" -Response $response
    
    # เก็บ Book ID ที่สร้างขึ้นเพื่อใช้ใน test ถัดไป
    if ($response.book_id) {
        $CreatedBookId = $response.book_id
        Write-ColorOutput "`n💾 Saved Book ID: $CreatedBookId" "Green"
    }
    
} catch {
    Show-Result -Status "Failed" -ErrorMessage $_.Exception.Message
}

# =============================================================================
# Test 2: Generate Code
# =============================================================================
Write-Section "Test 2: Generate Code - GET /api/books/generate-code"

try {
    # ใช้ Book ID ที่สร้างขึ้น หรือใช้ค่าเริ่มต้น
    $bookId = if ($CreatedBookId) { $CreatedBookId } else { "269B1ABF2ABE46968B78F099EAC90588" }
    
    $generateUrl = "$BaseUrl/api/books/generate-code?user_ad=$([System.Uri]::EscapeDataString($UserAD))&book_id=$bookId"
    Write-ColorOutput "🌐 Calling: GET $generateUrl" "Cyan"
    
    $response = Invoke-RestMethod -Uri $generateUrl `
        -Method GET `
        -ErrorAction Stop
    
    Show-Result -Status "Success" -Response $response
    
} catch {
    Show-Result -Status "Failed" -ErrorMessage $_.Exception.Message
}

# =============================================================================
# Test 3: Transfer Book
# =============================================================================
Write-Section "Test 3: Transfer Book - POST /api/books/transfer"

try {
    $headers = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
    
    $transferBody = @{
        transfer_reason = "Test Transfer - Organization Restructure"
        transfer_note = "Automated test transfer at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        create_by = $UserAD
    } | ConvertTo-Json
    
    Write-ColorOutput "📤 Request Body:" "Yellow"
    Write-Host $transferBody
    
    # ใช้ Book ID ที่สร้างขึ้น หรือใช้ค่าเริ่มต้น
    $bookId = if ($CreatedBookId) { $CreatedBookId } else { "269B1ABF2ABE46968B78F099EAC90588" }
    
    $transferParams = @{
        user_ad = [System.Uri]::EscapeDataString($UserAD)
        book_id = $bookId
        tranfer_id = "null"
        original_org_code = "J10100"
        destination_org_code = "J10000"
    }
    
    $queryString = ($transferParams.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join "&"
    $transferUrl = "$BaseUrl/api/books/transfer?$queryString"
    
    Write-ColorOutput "`n🌐 Calling: POST $transferUrl" "Cyan"
    
    $response = Invoke-RestMethod -Uri $transferUrl `
        -Method POST `
        -Headers $headers `
        -Body $transferBody `
        -ErrorAction Stop
    
    Show-Result -Status "Success" -Response $response
    
} catch {
    Show-Result -Status "Failed" -ErrorMessage $_.Exception.Message
}

# =============================================================================
# Test 4: Final Organizations (With Alert)
# =============================================================================
Write-Section "Test 4a: Final Organizations (With Alert) - GET /api/books/final-orgs/by-action"

try {
    # ใช้ Book ID ที่สร้างขึ้น หรือใช้ค่าเริ่มต้น
    $bookId = if ($CreatedBookId) { $CreatedBookId } else { "269B1ABF2ABE46968B78F099EAC90588" }
    
    $finalOrgsUrl = "$BaseUrl/api/books/final-orgs/by-action?user_ad=$([System.Uri]::EscapeDataString($UserAD))&book_id=$bookId"
    Write-ColorOutput "🌐 Calling: GET $finalOrgsUrl" "Cyan"
    
    $response = Invoke-RestMethod -Uri $finalOrgsUrl `
        -Method GET `
        -ErrorAction Stop
    
    Show-Result -Status "Success" -Response $response
    
} catch {
    Show-Result -Status "Failed" -ErrorMessage $_.Exception.Message
}

# =============================================================================
# Test 5: Final Organizations (No Alert)
# =============================================================================
Write-Section "Test 4b: Final Organizations (No Alert) - GET /api/books/final-orgs/by-action/no-alert"

try {
    # ใช้ Book ID ที่สร้างขึ้น หรือใช้ค่าเริ่มต้น
    $bookId = if ($CreatedBookId) { $CreatedBookId } else { "269B1ABF2ABE46968B78F099EAC90588" }
    
    $finalOrgsNoAlertUrl = "$BaseUrl/api/books/final-orgs/by-action/no-alert?user_ad=$([System.Uri]::EscapeDataString($UserAD))&book_id=$bookId"
    Write-ColorOutput "🌐 Calling: GET $finalOrgsNoAlertUrl" "Cyan"
    
    $response = Invoke-RestMethod -Uri $finalOrgsNoAlertUrl `
        -Method GET `
        -ErrorAction Stop
    
    Show-Result -Status "Success" -Response $response
    
} catch {
    Show-Result -Status "Failed" -ErrorMessage $_.Exception.Message
}

# =============================================================================
# สรุปผล
# =============================================================================
Write-Section "Test Summary"

Write-ColorOutput "✅ Test completed!" "Green"
Write-ColorOutput "`n📊 Results:" "Cyan"
Write-ColorOutput "  • Test 1: Create Book" "Yellow"
Write-ColorOutput "  • Test 2: Generate Code" "Yellow"
Write-ColorOutput "  • Test 3: Transfer Book" "Yellow"
Write-ColorOutput "  • Test 4a: Final Organizations (With Alert)" "Yellow"
Write-ColorOutput "  • Test 4b: Final Organizations (No Alert)" "Yellow"

if ($CreatedBookId) {
    Write-ColorOutput "`n💾 Created Book ID: $CreatedBookId" "Green"
}

Write-ColorOutput "`n✨ All tests executed!`n" "Magenta"
