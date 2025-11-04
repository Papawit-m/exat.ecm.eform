# ============================================================================
# Complete Books API Test Script
# Version: 1.3.1
# Date: November 4, 2025
# Description: Comprehensive test suite for all Books API endpoints
# ============================================================================

param(
    [string]$BaseUrl = "http://localhost:5152",
    [string]$UserAd = "EXAT\ECMUSR07",
    [switch]$SkipRealApiTests,
    [switch]$Verbose
)

# Color scheme
$ColorHeader = "Cyan"
$ColorSuccess = "Green"
$ColorError = "Red"
$ColorWarning = "Yellow"
$ColorInfo = "White"
$ColorDetail = "Gray"

# Test counters
$script:TotalTests = 0
$script:PassedTests = 0
$script:FailedTests = 0
$script:SkippedTests = 0

# Test results storage
$script:TestResults = @()

# ============================================================================
# Helper Functions
# ============================================================================

function Write-TestHeader {
    param([string]$Title)
    Write-Host "`n========================================" -ForegroundColor $ColorHeader
    Write-Host $Title -ForegroundColor $ColorHeader
    Write-Host "========================================`n" -ForegroundColor $ColorHeader
}

function Write-TestSection {
    param([string]$Section)
    Write-Host "`n--- $Section ---" -ForegroundColor $ColorInfo
}

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [object]$Body = $null,
        [hashtable]$ExpectedFields = @{},
        [string]$ExpectWrapper = "none", # "none", "ApiResponse"
        [switch]$SkipTest
    )
    
    $script:TotalTests++
    
    if ($SkipTest) {
        Write-Host "⏭️  SKIPPED: $Name" -ForegroundColor $ColorWarning
        $script:SkippedTests++
        $script:TestResults += @{
            Name = $Name
            Status = "SKIPPED"
            Method = $Method
            Url = $Url
        }
        return $null
    }
    
    try {
        Write-Host "🧪 Testing: $Name" -ForegroundColor $ColorInfo
        if ($Verbose) {
            Write-Host "   Method: $Method" -ForegroundColor $ColorDetail
            Write-Host "   URL: $Url" -ForegroundColor $ColorDetail
        }
        
        $params = @{
            Uri = $Url
            Method = $Method
            ContentType = "application/json; charset=utf-8"
            ErrorAction = "Stop"
        }
        
        if ($Body -ne $null) {
            if ($Body -is [string]) {
                $params.Body = $Body
            } else {
                $params.Body = $Body | ConvertTo-Json -Depth 10
            }
            
            if ($Verbose) {
                Write-Host "   Body: $($params.Body.Substring(0, [Math]::Min(100, $params.Body.Length)))..." -ForegroundColor $ColorDetail
            }
        }
        
        $response = Invoke-RestMethod @params
        
        # Check wrapper format
        $actualData = $response
        if ($ExpectWrapper -eq "ApiResponse") {
            if (-not $response.PSObject.Properties["success"]) {
                throw "Expected ApiResponse wrapper with 'success' field, but not found"
            }
            if (-not $response.success) {
                throw "API returned success=false: $($response.message) (Code: $($response.error_code))"
            }
            $actualData = $response.data
            
            if ($Verbose) {
                Write-Host "   Wrapper: ApiResponse (success: $($response.success))" -ForegroundColor $ColorDetail
            }
        } else {
            if ($Verbose) {
                Write-Host "   Wrapper: None (direct response)" -ForegroundColor $ColorDetail
            }
        }
        
        # Validate expected fields
        $missingFields = @()
        foreach ($field in $ExpectedFields.Keys) {
            if (-not $actualData.PSObject.Properties[$field]) {
                $missingFields += $field
            } elseif ($Verbose) {
                $value = $actualData.$field
                if ($value -is [string] -and $value.Length -gt 50) {
                    $value = $value.Substring(0, 50) + "..."
                }
                Write-Host "   ✓ $field = $value" -ForegroundColor $ColorDetail
            }
        }
        
        if ($missingFields.Count -gt 0) {
            throw "Missing expected fields: $($missingFields -join ', ')"
        }
        
        Write-Host "   ✅ PASSED" -ForegroundColor $ColorSuccess
        $script:PassedTests++
        
        $script:TestResults += @{
            Name = $Name
            Status = "PASSED"
            Method = $Method
            Url = $Url
            Response = $response
        }
        
        return $response
        
    } catch {
        Write-Host "   ❌ FAILED: $($_.Exception.Message)" -ForegroundColor $ColorError
        if ($Verbose) {
            Write-Host "   Details: $_" -ForegroundColor $ColorDetail
        }
        $script:FailedTests++
        
        $script:TestResults += @{
            Name = $Name
            Status = "FAILED"
            Method = $Method
            Url = $Url
            Error = $_.Exception.Message
        }
        
        return $null
    }
}

function Get-TestBodyFromFile {
    param([string]$FileName)
    
    $filePath = Join-Path $PSScriptRoot "..\ExamBodyRequest\$FileName"
    if (-not (Test-Path $filePath)) {
        Write-Warning "Test body file not found: $filePath"
        return $null
    }
    
    return Get-Content $filePath -Raw -Encoding UTF8
}

# ============================================================================
# Main Test Execution
# ============================================================================

Write-TestHeader "BOOKS API COMPLETE TEST SUITE - v1.3.1"

Write-Host "Configuration:" -ForegroundColor $ColorInfo
Write-Host "  Base URL: $BaseUrl" -ForegroundColor $ColorDetail
Write-Host "  User AD: $UserAd" -ForegroundColor $ColorDetail
Write-Host "  Skip Real API: $SkipRealApiTests" -ForegroundColor $ColorDetail
Write-Host "  Verbose: $Verbose" -ForegroundColor $ColorDetail

# ============================================================================
# TEST GROUP 1: Create Endpoints (K2 Compatible - Direct Response)
# ============================================================================

Write-TestHeader "TEST GROUP 1: Create Endpoints (K2 Compatible - Simple Format)"

$createBody = Get-TestBodyFromFile "books-create-k2-approved-simple-example.json"

# Test 1.1: Create Approved (Simple)
$test1_1 = Test-Endpoint `
    -Name "Create Book - Approved (Simple)" `
    -Method "POST" `
    -Url "$BaseUrl/api/books/create/approved/simple" `
    -Body $createBody `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        created_date = ""
    } `
    -ExpectWrapper "none"

$bookId1 = if ($test1_1) { $test1_1.book_id } else { $null }

# Test 1.2: Create Non-Compliant (Simple)
$test1_2 = Test-Endpoint `
    -Name "Create Book - Non-Compliant (Simple)" `
    -Method "POST" `
    -Url "$BaseUrl/api/books/create/non-compliant/simple" `
    -Body $createBody `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        created_date = ""
    } `
    -ExpectWrapper "none"

$bookId2 = if ($test1_2) { $test1_2.book_id } else { $null }

# Test 1.3: Create Under Construction (Simple)
$test1_3 = Test-Endpoint `
    -Name "Create Book - Under Construction (Simple)" `
    -Method "POST" `
    -Url "$BaseUrl/api/books/create/under-construction/simple" `
    -Body $createBody `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        created_date = ""
    } `
    -ExpectWrapper "none"

$bookId3 = if ($test1_3) { $test1_3.book_id } else { $null }

# ============================================================================
# TEST GROUP 2: Create Endpoints (Full Format)
# ============================================================================

Write-TestHeader "TEST GROUP 2: Create Endpoints (Full Format)"

# Test 2.1: Create Original
$test2_1 = Test-Endpoint `
    -Name "Create Book - Original (Full)" `
    -Method "POST" `
    -Url "$BaseUrl/api/books/create/original" `
    -Body $createBody `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        created_date = ""
    } `
    -ExpectWrapper "none"

# Test 2.2: Create Approved (Full)
$test2_2 = Test-Endpoint `
    -Name "Create Book - Approved (Full)" `
    -Method "POST" `
    -Url "$BaseUrl/api/books/create/approved" `
    -Body $createBody `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        created_date = ""
    } `
    -ExpectWrapper "none"

# Test 2.3: Create Non-Compliant (Full)
$test2_3 = Test-Endpoint `
    -Name "Create Book - Non-Compliant (Full)" `
    -Method "POST" `
    -Url "$BaseUrl/api/books/create/non-compliant" `
    -Body $createBody `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        created_date = ""
    } `
    -ExpectWrapper "none"

# Test 2.4: Create Under Construction (Full)
$test2_4 = Test-Endpoint `
    -Name "Create Book - Under Construction (Full)" `
    -Method "POST" `
    -Url "$BaseUrl/api/books/create/under-construction" `
    -Body $createBody `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        created_date = ""
    } `
    -ExpectWrapper "none"

# ============================================================================
# TEST GROUP 3: Generate Code Endpoint
# ============================================================================

Write-TestHeader "TEST GROUP 3: Generate Code Endpoint"

$testBookId = if ($bookId1) { $bookId1 } else { "TEST-BOOK-ID-12345678" }

# Test 3.1: Generate Code
$generateCodeUrl = "$BaseUrl/api/books/generate-code?user_ad=$UserAd&book_id=$testBookId"
$test3_1 = Test-Endpoint `
    -Name "Generate Book Code" `
    -Method "GET" `
    -Url $generateCodeUrl `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        book_code = ""
        to_date = ""
    } `
    -ExpectWrapper "none" `
    -SkipTest:$SkipRealApiTests

$bookCode = if ($test3_1) { $test3_1.book_code } else { $null }

# ============================================================================
# TEST GROUP 4: Transfer Endpoint (Raw Response with Wrapper)
# ============================================================================

Write-TestHeader "TEST GROUP 4: Transfer Endpoint (Raw Response)"

$transferBody = @{
    transfer_reason = "ทดสอบการโอนย้าย"
    transfer_note = "Test from PowerShell script"
} | ConvertTo-Json

# Test 4.1: Transfer Book
$transferUrl = "$BaseUrl/api/books/transfer?user_ad=$UserAd&book_id=$testBookId&original_org_code=J10000&destination_org_code=J20000"
$test4_1 = Test-Endpoint `
    -Name "Transfer Book" `
    -Method "POST" `
    -Url $transferUrl `
    -Body $transferBody `
    -ExpectedFields @{
        status = "S"
        book_id = ""
        transfer_id = ""
    } `
    -ExpectWrapper "ApiResponse"

# ============================================================================
# TEST GROUP 5: Final Organizations Query (Raw Response with Wrapper)
# ============================================================================

Write-TestHeader "TEST GROUP 5: Final Organizations Query (Raw Response)"

# Test 5.1: Get Final Orgs (with Alert)
$finalOrgsUrl1 = "$BaseUrl/api/books/final-orgs/by-action?user_ad=$UserAd&book_id=$testBookId"
$test5_1 = Test-Endpoint `
    -Name "Get Final Organizations (with Alert)" `
    -Method "GET" `
    -Url $finalOrgsUrl1 `
    -ExpectedFields @{
        status = "S"
        books = @()
    } `
    -ExpectWrapper "ApiResponse"

# Test 5.2: Get Final Orgs (no Alert)
$finalOrgsUrl2 = "$BaseUrl/api/books/final-orgs/by-action/no-alert?user_ad=$UserAd&book_id=$testBookId"
$test5_2 = Test-Endpoint `
    -Name "Get Final Organizations (no Alert)" `
    -Method "GET" `
    -Url $finalOrgsUrl2 `
    -ExpectedFields @{
        status = "S"
        books = @()
    } `
    -ExpectWrapper "ApiResponse"

# ============================================================================
# TEST GROUP 6: Workflow Endpoints (Combined Operations)
# ============================================================================

Write-TestHeader "TEST GROUP 6: Workflow Endpoints (Combined)"

Write-Host "NOTE: Workflow endpoints are skipped in this test (they internally call /create and /generate-code)" -ForegroundColor $ColorWarning
Write-Host "To test workflows, run them individually with proper request bodies." -ForegroundColor $ColorWarning

$script:SkippedTests += 3  # Approved, Non-Compliant, Under-Construction
$script:TotalTests += 3

# ============================================================================
# TEST SUMMARY
# ============================================================================

Write-TestHeader "TEST SUMMARY"

Write-Host "Total Tests:   $script:TotalTests" -ForegroundColor $ColorInfo
Write-Host "Passed:        $script:PassedTests" -ForegroundColor $ColorSuccess
Write-Host "Failed:        $script:FailedTests" -ForegroundColor $(if ($script:FailedTests -gt 0) { $ColorError } else { $ColorSuccess })
Write-Host "Skipped:       $script:SkippedTests" -ForegroundColor $ColorWarning

$passRate = if ($script:TotalTests -gt 0) { 
    [math]::Round(($script:PassedTests / ($script:TotalTests - $script:SkippedTests)) * 100, 2) 
} else { 
    0 
}

Write-Host "`nPass Rate:     $passRate%" -ForegroundColor $(if ($passRate -ge 80) { $ColorSuccess } elseif ($passRate -ge 50) { $ColorWarning } else { $ColorError })

# ============================================================================
# DETAILED RESULTS
# ============================================================================

if ($Verbose) {
    Write-TestHeader "DETAILED RESULTS"
    
    foreach ($result in $script:TestResults) {
        $statusColor = switch ($result.Status) {
            "PASSED" { $ColorSuccess }
            "FAILED" { $ColorError }
            "SKIPPED" { $ColorWarning }
        }
        
        Write-Host "$($result.Status): $($result.Name)" -ForegroundColor $statusColor
        Write-Host "  Method: $($result.Method) $($result.Url)" -ForegroundColor $ColorDetail
        
        if ($result.Error) {
            Write-Host "  Error: $($result.Error)" -ForegroundColor $ColorError
        }
    }
}

# ============================================================================
# EXPORT RESULTS
# ============================================================================

$resultsFile = Join-Path $PSScriptRoot "test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$script:TestResults | ConvertTo-Json -Depth 10 | Out-File $resultsFile -Encoding UTF8
Write-Host "`nTest results exported to: $resultsFile" -ForegroundColor $ColorInfo

# ============================================================================
# EXIT CODE
# ============================================================================

if ($script:FailedTests -gt 0) {
    Write-Host "`n❌ Some tests failed!" -ForegroundColor $ColorError
    exit 1
} else {
    Write-Host "`n✅ All tests passed!" -ForegroundColor $ColorSuccess
    exit 0
}
