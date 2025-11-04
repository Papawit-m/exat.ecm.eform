# Test Books Create API - 3 สายงาน
# สคริปต์สำหรับทดสอบการสร้างเอกสาร 3 กรณี
# Created: October 30, 2025

<#
.SYNOPSIS
    ทดสอบการเรียกใช้ Books Create API ทั้ง 3 endpoints

.DESCRIPTION
    สคริปต์นี้ทดสอบ 3 กรณีการสร้างเอกสาร:
    1. กรณี อนุมัติ/เข้าสู่หลักเกณ์
    2. กรณี แบบไม่เข้าหลักเกณ์
    3. กรณี อยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา

.PARAMETER BaseUrl
    Base URL ของ API (default: http://localhost:5152)

.PARAMETER UserAD
    Active Directory username (default: EXAT\ECMUSR07)

.EXAMPLE
    .\test-books-create.ps1
    
.EXAMPLE
    .\test-books-create.ps1 -BaseUrl "https://api.example.com" -UserAD "EXAT\ECMUSR01"
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
        $Response | ConvertTo-Json -Depth 10 | Write-Host
        
        # แสดง Book ID และ Book Code ถ้ามี
        if ($Response.data -and $Response.data.bookId) {
            Write-ColorOutput "`n💾 Book ID: $($Response.data.bookId)" "Green"
            Write-ColorOutput "📋 Book Code: $($Response.data.bookCode)" "Green"
        }
    } else {
        Write-ColorOutput "❌ Status: FAILED" "Red"
        Write-ColorOutput "⚠️  Error: $ErrorMessage" "Red"
    }
}

Write-ColorOutput "`n🚀 Books Create API Test Script - 3 สายงาน" "Magenta"
Write-ColorOutput "Base URL: $BaseUrl" "Gray"
Write-ColorOutput "User AD: $UserAD`n" "Gray"

# =============================================================================
# Test 1: กรณี อนุมัติ/เข้าสู่หลักเกณ์
# =============================================================================
Write-Section "Test 1: สร้างเอกสาร - กรณีอนุมัติ/เข้าสู่หลักเกณ์"

try {
    $headers = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
    
    $approvedBody = @{
        bookTitle = "เอกสารทดสอบ - กรณีอนุมัติ - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        bookTypeId = "TYPE001-APPROVED"
        registrationBookId = "REG001-APPROVED"
        bookYear = (Get-Date).Year
        orgCode = "J10100"
        approvalDocumentNo = "อว. 1234/2568"
        approvalDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
        certificateNo = "รง. 5678/2568"
        certificateIssuer = "กรมโยธาธิการและผังเมือง"
        certificateIssueDate = ((Get-Date).AddDays(-5)).ToString("yyyy-MM-ddTHH:mm:ssZ")
        remarks = "เอกสารผ่านการอนุมัติจากหน่วยงานที่เกี่ยวข้องครบถ้วน"
        attachedDocuments = @(
            "approval-doc-001.pdf",
            "certificate-002.pdf"
        )
    } | ConvertTo-Json -Depth 5
    
    Write-ColorOutput "📤 Request Body:" "Yellow"
    Write-Host $approvedBody
    
    $approvedUrl = "$BaseUrl/api/books/create/approved?user_ad=$([System.Uri]::EscapeDataString($UserAD))"
    Write-ColorOutput "`n🌐 Calling: POST $approvedUrl" "Cyan"
    
    $response = Invoke-RestMethod -Uri $approvedUrl `
        -Method POST `
        -Headers $headers `
        -Body $approvedBody `
        -ErrorAction Stop
    
    Show-Result -Status "Success" -Response $response
    
} catch {
    Show-Result -Status "Failed" -ErrorMessage $_.Exception.Message
}

# รอก่อนทดสอบถัดไป
Start-Sleep -Seconds 2

# =============================================================================
# Test 2: กรณี แบบไม่เข้าหลักเกณ์
# =============================================================================
Write-Section "Test 2: สร้างเอกสาร - กรณีไม่เข้าหลักเกณ์"

try {
    $headers = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
    
    $nonCompliantBody = @{
        bookTitle = "เอกสารทดสอบ - กรณีไม่เข้าหลักเกณ์ - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        bookTypeId = "TYPE002-NONCOMPLIANT"
        registrationBookId = "REG002-NONCOMPLIANT"
        bookYear = (Get-Date).Year
        orgCode = "J10100"
        nonCompliantReason = "ไม่มีหนังสือรับรองจากหน่วยงานที่เกี่ยวข้อง"
        nonCompliantDetails = "โครงการยังไม่ได้รับการอนุมัติจากกรมโยธาธิการและผังเมือง เนื่องจากยังไม่สามารถหาเอกสารประกอบการพิจารณาได้ครบถ้วน"
        requiresReview = $true
        reviewerOrgCode = "J10000"
        reviewerName = "นายสมชาย ใจดี"
        referenceDocuments = "หนังสือที่ กท 1234/2568 ลงวันที่ 15 ตุลาคม 2568"
        remarks = "ต้องการให้หน่วยงานทบทวนและให้คำแนะนำเพิ่มเติม"
    } | ConvertTo-Json -Depth 5
    
    Write-ColorOutput "📤 Request Body:" "Yellow"
    Write-Host $nonCompliantBody
    
    $nonCompliantUrl = "$BaseUrl/api/books/create/non-compliant?user_ad=$([System.Uri]::EscapeDataString($UserAD))"
    Write-ColorOutput "`n🌐 Calling: POST $nonCompliantUrl" "Cyan"
    
    $response = Invoke-RestMethod -Uri $nonCompliantUrl `
        -Method POST `
        -Headers $headers `
        -Body $nonCompliantBody `
        -ErrorAction Stop
    
    Show-Result -Status "Success" -Response $response
    
} catch {
    Show-Result -Status "Failed" -ErrorMessage $_.Exception.Message
}

# รอก่อนทดสอบถัดไป
Start-Sleep -Seconds 2

# =============================================================================
# Test 3: กรณี อยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา
# =============================================================================
Write-Section "Test 3: สร้างเอกสาร - กรณีอยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา"

try {
    $headers = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
    
    $underConstructionBody = @{
        bookTitle = "เอกสารทดสอบ - กรณีอยู่ระหว่างก่อสร้าง - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        bookTypeId = "TYPE003-CONSTRUCTION"
        registrationBookId = "REG003-CONSTRUCTION"
        bookYear = (Get-Date).Year
        orgCode = "J10100"
        projectName = "โครงการก่อสร้างสะพานข้ามแม่น้ำเจ้าพระยา สายที่ 5"
        projectCode = "PRJ-2025-001"
        constructionStartDate = ((Get-Date).AddMonths(-9)).ToString("yyyy-MM-ddTHH:mm:ssZ")
        expectedCompletionDate = ((Get-Date).AddMonths(15)).ToString("yyyy-MM-ddTHH:mm:ssZ")
        constructionProgress = 45.5
        consultantName = "บริษัท ABC วิศวกรรมที่ปรึกษา จำกัด"
        consultantOrgCode = "CON001"
        consultantContactPerson = "นายสมศักดิ์ ช่างคิด"
        consultantContactPhone = "02-1234567"
        consultantEmail = "somsak@abc-consultant.com"
        requestLetterSubject = "ขอหนังสือรับรองความก้าวหน้าโครงการก่อสร้างสะพานฯ"
        requestLetterDetails = "ด้วยการทางพิเศษแห่งประเทศไทย มีความประสงค์ขอให้บริษัทฯ ออกหนังสือรับรองความก้าวหน้าของโครงการก่อสร้างสะพานข้ามแม่น้ำเจ้าพระยา สายที่ 5 ณ วันที่ 30 ตุลาคม 2568 เพื่อนำไปใช้ประกอบการพิจารณาการเบิกจ่ายงบประมาณ"
        requiredDocuments = @(
            "รายงานความก้าวหน้าโครงการ ณ 30 ตุลาคม 2568",
            "แบบรูปก่อสร้างที่อัปเดตล่าสุด (As-Built Drawing)",
            "ใบรับรองคุณภาพงานก่อสร้าง",
            "รายงานการตรวจสอบคุณภาพวัสดุ",
            "ภาพถ่ายความก้าวหน้าโครงการ"
        )
        requiredByDate = ((Get-Date).AddDays(30)).ToString("yyyy-MM-ddTHH:mm:ssZ")
        remarks = "กรุณาออกหนังสือดังกล่าวภายใน 30 วัน เนื่องจากต้องนำไปใช้ประกอบการเบิกจ่ายงบประมาณงวดที่ 3"
    } | ConvertTo-Json -Depth 5
    
    Write-ColorOutput "📤 Request Body:" "Yellow"
    Write-Host $underConstructionBody
    
    $underConstructionUrl = "$BaseUrl/api/books/create/under-construction?user_ad=$([System.Uri]::EscapeDataString($UserAD))"
    Write-ColorOutput "`n🌐 Calling: POST $underConstructionUrl" "Cyan"
    
    $response = Invoke-RestMethod -Uri $underConstructionUrl `
        -Method POST `
        -Headers $headers `
        -Body $underConstructionBody `
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
Write-ColorOutput "`n📊 Test Results:" "Cyan"
Write-ColorOutput "  • Test 1: กรณีอนุมัติ/เข้าสู่หลักเกณ์" "Yellow"
Write-ColorOutput "  • Test 2: กรณีไม่เข้าหลักเกณ์" "Yellow"
Write-ColorOutput "  • Test 3: กรณีอยู่ระหว่างก่อสร้างและขอหนังสือจากที่ปรึกษา" "Yellow"

Write-ColorOutput "`n📝 Endpoints Tested:" "Cyan"
Write-ColorOutput "  1. POST /api/books/create/approved" "White"
Write-ColorOutput "  2. POST /api/books/create/non-compliant" "White"
Write-ColorOutput "  3. POST /api/books/create/under-construction" "White"

Write-ColorOutput "`n✨ All tests executed!`n" "Magenta"

# =============================================================================
# Additional Helper Function - Export Results to File
# =============================================================================
Write-ColorOutput "💾 Tip: คุณสามารถบันทึกผลลัพธ์ได้โดยใช้:" "Gray"
Write-ColorOutput "   .\test-books-create.ps1 | Out-File -FilePath test-results.txt" "Gray"
Write-ColorOutput ""
