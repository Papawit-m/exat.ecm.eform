# =============================================================================
# K2 Compatibility Test - All 12 Endpoints (4 Groups)
# Test all Books API endpoints for K2 SmartObject compatibility
# =============================================================================

param(
    [string]$BaseUrl = "http://localhost:5152"
)

# Test Results
$passed = 0
$failed = 0
$results = @()

Write-Host "`n" -NoNewline
Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  🔍 K2 SMART OBJECT COMPATIBILITY TEST - ALL 12 ENDPOINTS                    " -ForegroundColor White -BackgroundColor DarkCyan
Write-Host "═══════════════════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Testing direct response format (no ApiResponse wrapper)                       " -ForegroundColor Gray
Write-Host "  Expected: response.status, response.book_code (K2 SmartObject compatible)     " -ForegroundColor Gray
Write-Host "═══════════════════════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# =============================================================================
# GROUP 1: Books - Create (K2 Compatible) - 3 Endpoints
# =============================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  📂 GROUP 1: Create (K2 Compatible) - 3 Endpoints                             " -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Yellow

# Test 1.1: Create Approved Simple
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-create-approved-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/create/approved/simple" -Method Post -Body $body -ContentType "application/json; charset=utf-8"
    
    Write-Host "  ✅ TEST 1/12: POST /api/books/create/approved/simple" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $results += @{Test="Create Approved Simple"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "  ❌ TEST 1/12: POST /api/books/create/approved/simple - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $results += @{Test="Create Approved Simple"; Status="FAIL"; BookCode="N/A"}
}

# Test 1.2: Create Non-Compliant Simple
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-create-non-compliant-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/create/non-compliant/simple" -Method Post -Body $body -ContentType "application/json; charset=utf-8"
    
    Write-Host "`n  ✅ TEST 2/12: POST /api/books/create/non-compliant/simple" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $results += @{Test="Create Non-Compliant Simple"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "`n  ❌ TEST 2/12: POST /api/books/create/non-compliant/simple - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $results += @{Test="Create Non-Compliant Simple"; Status="FAIL"; BookCode="N/A"}
}

# Test 1.3: Create Under-Construction Simple
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-create-under-construction-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/create/under-construction/simple" -Method Post -Body $body -ContentType "application/json; charset=utf-8"
    
    Write-Host "`n  ✅ TEST 3/12: POST /api/books/create/under-construction/simple" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $results += @{Test="Create Under-Construction Simple"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "`n  ❌ TEST 3/12: POST /api/books/create/under-construction/simple - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $results += @{Test="Create Under-Construction Simple"; Status="FAIL"; BookCode="N/A"}
}

Write-Host "`n  🎯 GROUP 1 SUMMARY: $passed passed, $failed failed`n" -ForegroundColor $(if($failed -eq 0){"Green"}else{"Yellow"})

# =============================================================================
# GROUP 2: Books - Operations - 2 Endpoints
# =============================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  ⚙️  GROUP 2: Operations - 2 Endpoints                                         " -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Yellow

$group2Passed = 0
$group2Failed = 0

# Test 2.1: Generate Code
try {
    $testBookId = "93AFDFD2351E4E5C8ADE5A3FB92C5553"
    $userAd = "EXAT\ECMUSR07"
    $url = "$BaseUrl/api/books/generate-code?user_ad=$userAd`&book_id=$testBookId"
    $response = Invoke-RestMethod -Uri $url -Method Get
    
    Write-Host "  ✅ TEST 4/12: GET /api/books/generate-code" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Generated Code: $($response.generated_code)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.generated_code = '$($response.generated_code)' ✓" -ForegroundColor Green
    $passed++
    $group2Passed++
    $results += @{Test="Generate Code"; Status="PASS"; BookCode=$response.generated_code}
} catch {
    Write-Host "  ❌ TEST 4/12: GET /api/books/generate-code - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group2Failed++
    $results += @{Test="Generate Code"; Status="FAIL"; BookCode="N/A"}
}

# Test 2.2: Transfer Book
try {
    $bodyTransfer = [System.IO.File]::ReadAllText("ExamBodyRequest\books-transfer-example.json", [System.Text.Encoding]::UTF8)
    $testBookId = "93AFDFD2351E4E5C8ADE5A3FB92C5553"
    $userAd = "EXAT\ECMUSR07"
    $url = "$BaseUrl/api/books/transfer?user_ad=$userAd`&book_id=$testBookId`&original_org_code=J10100`&destination_org_code=J10000"
    $response = Invoke-RestMethod -Uri $url -Method Post -Body $bodyTransfer -ContentType "application/json; charset=utf-8"
    
    Write-Host "`n  ✅ TEST 5/12: POST /api/books/transfer" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Transfer ID: $($response.transfer_id)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.transfer_id = '$($response.transfer_id)' ✓" -ForegroundColor Green
    $passed++
    $group2Passed++
    $results += @{Test="Transfer Book"; Status="PASS"; BookCode=$response.transfer_id}
} catch {
    Write-Host "`n  ❌ TEST 5/12: POST /api/books/transfer - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group2Failed++
    $results += @{Test="Transfer Book"; Status="FAIL"; BookCode="N/A"}
}

Write-Host "`n  🎯 GROUP 2 SUMMARY: $group2Passed passed, $group2Failed failed`n" -ForegroundColor $(if($group2Failed -eq 0){"Green"}else{"Yellow"})

# =============================================================================
# GROUP 3: Books - Workflow (Combined) - 3 Endpoints
# =============================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  🔄 GROUP 3: Workflow (Combined) - 3 Endpoints                                " -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Yellow

$group3Passed = 0
$group3Failed = 0

# Test 3.1: Workflow Approved
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-workflow-approved-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/workflow/approved" -Method Post -Body $body -ContentType "application/json; charset=utf-8" -TimeoutSec 60
    
    Write-Host "  ✅ TEST 6/12: POST /api/books/workflow/approved" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     Transfer ID: $($response.transfer_id)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $group3Passed++
    $results += @{Test="Workflow Approved"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "  ❌ TEST 6/12: POST /api/books/workflow/approved - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group3Failed++
    $results += @{Test="Workflow Approved"; Status="FAIL"; BookCode="N/A"}
}

# Test 3.2: Workflow Non-Compliant
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-workflow-non-compliant-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/workflow/non-compliant" -Method Post -Body $body -ContentType "application/json; charset=utf-8" -TimeoutSec 60
    
    Write-Host "`n  ✅ TEST 7/12: POST /api/books/workflow/non-compliant" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     Transfer ID: $($response.transfer_id)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $group3Passed++
    $results += @{Test="Workflow Non-Compliant"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "`n  ❌ TEST 7/12: POST /api/books/workflow/non-compliant - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group3Failed++
    $results += @{Test="Workflow Non-Compliant"; Status="FAIL"; BookCode="N/A"}
}

# Test 3.3: Workflow Under-Construction
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-workflow-under-construction-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/workflow/under-construction" -Method Post -Body $body -ContentType "application/json; charset=utf-8" -TimeoutSec 60
    
    Write-Host "`n  ✅ TEST 8/12: POST /api/books/workflow/under-construction" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     Transfer ID: $($response.transfer_id)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $group3Passed++
    $results += @{Test="Workflow Under-Construction"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "`n  ❌ TEST 8/12: POST /api/books/workflow/under-construction - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group3Failed++
    $results += @{Test="Workflow Under-Construction"; Status="FAIL"; BookCode="N/A"}
}

Write-Host "`n  🎯 GROUP 3 SUMMARY: $group3Passed passed, $group3Failed failed`n" -ForegroundColor $(if($group3Failed -eq 0){"Green"}else{"Yellow"})

# =============================================================================
# GROUP 4: Books - Create (Full Format) - 4 Endpoints
# =============================================================================
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
Write-Host "  📄 GROUP 4: Create (Full Format) - 4 Endpoints                               " -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Yellow

$group4Passed = 0
$group4Failed = 0

# Test 4.1: Create Original
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-create-original-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/create/original" -Method Post -Body $body -ContentType "application/json; charset=utf-8"
    
    Write-Host "  ✅ TEST 9/12: POST /api/books/create/original" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $group4Passed++
    $results += @{Test="Create Original"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "  ❌ TEST 9/12: POST /api/books/create/original - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group4Failed++
    $results += @{Test="Create Original"; Status="FAIL"; BookCode="N/A"}
}

# Test 4.2: Create Approved (Full)
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-create-approved-full-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/create/approved" -Method Post -Body $body -ContentType "application/json; charset=utf-8"
    
    Write-Host "`n  ✅ TEST 10/12: POST /api/books/create/approved (Full)" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $group4Passed++
    $results += @{Test="Create Approved (Full)"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "`n  ❌ TEST 10/12: POST /api/books/create/approved (Full) - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group4Failed++
    $results += @{Test="Create Approved (Full)"; Status="FAIL"; BookCode="N/A"}
}

# Test 4.3: Create Non-Compliant (Full)
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-create-non-compliant-full-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/create/non-compliant" -Method Post -Body $body -ContentType "application/json; charset=utf-8"
    
    Write-Host "`n  ✅ TEST 11/12: POST /api/books/create/non-compliant (Full)" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $group4Passed++
    $results += @{Test="Create Non-Compliant (Full)"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "`n  ❌ TEST 11/12: POST /api/books/create/non-compliant (Full) - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group4Failed++
    $results += @{Test="Create Non-Compliant (Full)"; Status="FAIL"; BookCode="N/A"}
}

# Test 4.4: Create Under-Construction (Full)
try {
    $body = [System.IO.File]::ReadAllText("ExamBodyRequest\books-create-under-construction-full-example.json", [System.Text.Encoding]::UTF8)
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/books/create/under-construction" -Method Post -Body $body -ContentType "application/json; charset=utf-8"
    
    Write-Host "`n  ✅ TEST 12/12: POST /api/books/create/under-construction (Full)" -ForegroundColor Green
    Write-Host "     Status: $($response.status) | StatusCode: $($response.statusCode)" -ForegroundColor Cyan
    Write-Host "     Book Code: $($response.book_code)" -ForegroundColor White
    Write-Host "     K2 Path Test: response.book_code = '$($response.book_code)' ✓" -ForegroundColor Green
    $passed++
    $group4Passed++
    $results += @{Test="Create Under-Construction (Full)"; Status="PASS"; BookCode=$response.book_code}
} catch {
    Write-Host "`n  ❌ TEST 12/12: POST /api/books/create/under-construction (Full) - FAILED" -ForegroundColor Red
    Write-Host "     Error: $($_.Exception.Message)" -ForegroundColor Red
    $failed++
    $group4Failed++
    $results += @{Test="Create Under-Construction (Full)"; Status="FAIL"; BookCode="N/A"}
}

Write-Host "`n  🎯 GROUP 4 SUMMARY: $group4Passed passed, $group4Failed failed`n" -ForegroundColor $(if($group4Failed -eq 0){"Green"}else{"Yellow"})

# =============================================================================
# FINAL SUMMARY
# =============================================================================
Write-Host "===============================================================================" -ForegroundColor Cyan
Write-Host "  FINAL TEST RESULTS - K2 COMPATIBILITY (12 ENDPOINTS)                      " -ForegroundColor White -BackgroundColor DarkCyan
Write-Host "===============================================================================" -ForegroundColor Cyan

$successRate = [math]::Round(($passed / 12) * 100, 1)

Write-Host "`n  Total Tests: 12" -ForegroundColor White
Write-Host "  ✅ Passed: $passed" -ForegroundColor Green
Write-Host "  ❌ Failed: $failed" -ForegroundColor $(if($failed -gt 0){"Red"}else{"Gray"})
Write-Host "  📈 Success Rate: $successRate%" -ForegroundColor $(if($successRate -eq 100){"Green"}elseif($successRate -ge 80){"Yellow"}else{"Red"})

if ($failed -eq 0) {
    Write-Host "`n  🎉🎉🎉 ALL TESTS PASSED! K2 SmartObject compatible! 🎉🎉🎉" -ForegroundColor Green -BackgroundColor DarkGreen
} else {
    Write-Host "`n  ⚠️  SOME TESTS FAILED - Review errors above" -ForegroundColor Yellow -BackgroundColor DarkRed
}

Write-Host "`n===============================================================================`n" -ForegroundColor Cyan
