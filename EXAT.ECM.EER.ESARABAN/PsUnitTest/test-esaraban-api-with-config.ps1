# eSaraban API Configuration Test Script
# ทดสอบการเรียกใช้ API endpoints พร้อม environment configuration

# ===================================
# Environment Configuration
# ===================================
$baseUrl = "http://api-uat.exat.co.th/esrb-external-api"
$localApiUrl = "http://localhost:5152/api/books"
$userAd = "EXAT\ECMUSR07"

# ===================================
# Color Output Functions
# ===================================
function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Cyan
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Header {
    param([string]$Message)
    Write-Host "`n========================================" -ForegroundColor Magenta
    Write-Host " $Message" -ForegroundColor Magenta
    Write-Host "========================================`n" -ForegroundColor Magenta
}

# ===================================
# Test Functions
# ===================================

function Test-CreateBookApproved {
    Write-Header "TEST 1: สร้างเอกสาร - กรณีอนุมัติ/เข้าสู่หลักเกณ์"
    
    $body = @{
        bookTitle = "เอกสารทดสอบ - กรณีอนุมัติ"
        bookTypeId = "TYPE001"
        registrationBookId = "REG001"
        bookYear = 2567
        orgCode = "J10000"
        approvalDocumentNo = "อว.0001/2567"
        approvalDate = "2024-01-15"
        certificateNo = "CERT-2024-0001"
    } | ConvertTo-Json

    Write-Info "Endpoint: POST $localApiUrl/create/approved"
    Write-Info "User AD: $userAd"
    
    try {
        $response = Invoke-RestMethod -Uri "$localApiUrl/create/approved?user_ad=$userAd" `
            -Method Post `
            -Body $body `
            -ContentType "application/json"
        
        Write-Success "Response received!"
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    }
    catch {
        Write-Error "Request failed: $_"
    }
}

function Test-CreateBookNonCompliant {
    Write-Header "TEST 2: สร้างเอกสาร - กรณีไม่เข้าหลักเกณ์"
    
    $body = @{
        bookTitle = "เอกสารทดสอบ - กรณีไม่เข้าหลักเกณ์"
        bookTypeId = "TYPE002"
        registrationBookId = "REG002"
        bookYear = 2567
        orgCode = "J10100"
        nonCompliantReason = "ไม่มีเอกสารรับรองจากหน่วยงานที่เกี่ยวข้อง"
        nonCompliantDetails = "รอการตรวจสอบเพิ่มเติมจากกองวิศวกรรม"
        requiresReview = $true
        reviewerOrgCode = "J10100"
    } | ConvertTo-Json

    Write-Info "Endpoint: POST $localApiUrl/create/non-compliant"
    Write-Info "User AD: $userAd"
    
    try {
        $response = Invoke-RestMethod -Uri "$localApiUrl/create/non-compliant?user_ad=$userAd" `
            -Method Post `
            -Body $body `
            -ContentType "application/json"
        
        Write-Success "Response received!"
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    }
    catch {
        Write-Error "Request failed: $_"
    }
}

function Test-CreateBookUnderConstruction {
    Write-Header "TEST 3: สร้างเอกสาร - กรณีอยู่ระหว่างก่อสร้าง"
    
    $body = @{
        bookTitle = "เอกสารทดสอบ - โครงการก่อสร้าง"
        bookTypeId = "TYPE003"
        registrationBookId = "REG003"
        bookYear = 2567
        orgCode = "J10200"
        projectName = "โครงการก่อสร้างอาคารสำนักงาน"
        projectCode = "PROJ-2024-001"
        constructionStartDate = "2024-01-01"
        expectedCompletionDate = "2024-12-31"
        constructionProgress = 45.5
        consultantName = "บริษัท ที่ปรึกษาวิศวกรรม จำกัด"
        consultantOrgCode = "CONS001"
        requestLetterSubject = "ขอหนังสือรับรองความก้าวหน้าโครงการ"
        requestLetterDetails = "ขอหนังสือรับรองความก้าวหน้าโครงการ ณ เดือนมกราคม 2567"
        requiredDocuments = @("แบบก่อสร้าง", "ใบอนุญาตก่อสร้าง", "รายงานความก้าวหน้า")
    } | ConvertTo-Json

    Write-Info "Endpoint: POST $localApiUrl/create/under-construction"
    Write-Info "User AD: $userAd"
    
    try {
        $response = Invoke-RestMethod -Uri "$localApiUrl/create/under-construction?user_ad=$userAd" `
            -Method Post `
            -Body $body `
            -ContentType "application/json"
        
        Write-Success "Response received!"
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    }
    catch {
        Write-Error "Request failed: $_"
    }
}

function Test-GenerateCode {
    Write-Header "TEST 4: สร้างรหัสเอกสาร (Generate Code)"
    
    $bookId = "12345678-1234-1234-1234-123456789012"
    
    Write-Info "Endpoint: GET $localApiUrl/generate-code"
    Write-Info "User AD: $userAd"
    Write-Info "Book ID: $bookId"
    
    try {
        $response = Invoke-RestMethod -Uri "$localApiUrl/generate-code?user_ad=$userAd&book_id=$bookId" `
            -Method Get `
            -ContentType "application/json"
        
        Write-Success "Response received!"
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    }
    catch {
        Write-Error "Request failed: $_"
    }
}

function Test-TransferBook {
    Write-Header "TEST 5: โอนย้าย Book ระหว่างองค์กร"
    
    $bookId = "12345678-1234-1234-1234-123456789012"
    $transferId = "87654321-4321-4321-4321-210987654321"
    $originalOrgCode = "J10000"
    $destinationOrgCode = "J10100"
    
    $body = @{
        bookId = $bookId
        originalOrgCode = $originalOrgCode
        destinationOrgCode = $destinationOrgCode
        transferReason = "โอนย้ายเอกสารเพื่อดำเนินการต่อ"
        transferNote = "โอนจากสำนักงานผู้อำนวยการใหญ่ ไปยังกองวิศวกรรม"
    } | ConvertTo-Json

    Write-Info "Endpoint: POST $localApiUrl/transfer"
    Write-Info "User AD: $userAd"
    Write-Info "Book ID: $bookId"
    Write-Info "From: $originalOrgCode -> To: $destinationOrgCode"
    
    try {
        $response = Invoke-RestMethod -Uri "$localApiUrl/transfer?user_ad=$userAd&book_id=$bookId&tranfer_id=$transferId&original_org_code=$originalOrgCode&destination_org_code=$destinationOrgCode" `
            -Method Post `
            -Body $body `
            -ContentType "application/json"
        
        Write-Success "Response received!"
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    }
    catch {
        Write-Error "Request failed: $_"
    }
}

function Test-GetFinalOrgsWithAlert {
    Write-Header "TEST 6: ดึงข้อมูลองค์กรปลายทาง (พร้อม Alert)"
    
    $bookId = "12345678-1234-1234-1234-123456789012"
    
    Write-Info "Endpoint: GET $localApiUrl/final-orgs/by-action"
    Write-Info "User AD: $userAd"
    Write-Info "Book ID: $bookId"
    
    try {
        $response = Invoke-RestMethod -Uri "$localApiUrl/final-orgs/by-action?user_ad=$userAd&book_id=$bookId" `
            -Method Get `
            -ContentType "application/json"
        
        Write-Success "Response received!"
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    }
    catch {
        Write-Error "Request failed: $_"
    }
}

function Test-GetFinalOrgsNoAlert {
    Write-Header "TEST 7: ดึงข้อมูลองค์กรปลายทาง (ไม่มี Alert)"
    
    $bookId = "12345678-1234-1234-1234-123456789012"
    
    Write-Info "Endpoint: GET $localApiUrl/final-orgs/by-action/no-alert"
    Write-Info "User AD: $userAd"
    Write-Info "Book ID: $bookId"
    
    try {
        $response = Invoke-RestMethod -Uri "$localApiUrl/final-orgs/by-action/no-alert?user_ad=$userAd&book_id=$bookId" `
            -Method Get `
            -ContentType "application/json"
        
        Write-Success "Response received!"
        Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    }
    catch {
        Write-Error "Request failed: $_"
    }
}

# ===================================
# Environment Information
# ===================================
function Show-EnvironmentInfo {
    Write-Header "Environment Configuration"
    Write-Info "eSaraban Base URL: $baseUrl"
    Write-Info "Local API URL: $localApiUrl"
    Write-Info "User AD: $userAd"
    Write-Info "Environment: UAT"
    Write-Host ""
}

# ===================================
# Main Execution
# ===================================
Write-Host "`n" -NoNewline
Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   eSaraban API Configuration Test - All Endpoints            ║" -ForegroundColor Cyan
Write-Host "║   Testing Books API with Environment Configuration           ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

Show-EnvironmentInfo

# Run all tests
Test-CreateBookApproved
Test-CreateBookNonCompliant
Test-CreateBookUnderConstruction
Test-GenerateCode
Test-TransferBook
Test-GetFinalOrgsWithAlert
Test-GetFinalOrgsNoAlert

# Summary
Write-Header "Test Summary"
Write-Success "All 7 endpoint tests completed!"
Write-Info "กรุณาตรวจสอบผลลัพธ์ของแต่ละ test ด้านบน"
Write-Host ""
