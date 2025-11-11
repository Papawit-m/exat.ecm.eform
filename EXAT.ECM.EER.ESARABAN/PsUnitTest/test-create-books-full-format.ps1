# =============================================================================
# TEST: CREATE BOOKS - FULL FORMAT (4 endpoints)
# =============================================================================
# Description: Tests all 4 Create Books Full Format endpoints
# Endpoints:
#   4. POST /api/books/create/approved
#   5. POST /api/books/create/non-compliant
#   6. POST /api/books/create/under-construction
#   7. POST /api/books/create/original
# =============================================================================

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "TEST: CREATE BOOKS - FULL FORMAT" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

# Test configuration
$baseUrl = "http://localhost:5152"
$testFile = "ExamBodyRequest\books-create-full-format-example.json"
$apiStartupWaitSeconds = 7

Write-Host "Test Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Cyan
Write-Host "Base URL: $baseUrl" -ForegroundColor Gray
Write-Host "Test File: $testFile`n" -ForegroundColor Gray

# Check if API is already running
Write-Host "Checking API status..." -ForegroundColor Yellow
try {
    $healthCheck = Invoke-RestMethod -Uri "$baseUrl/swagger/index.html" -Method Get -TimeoutSec 2 -ErrorAction Stop
    Write-Host "✅ API is already running!`n" -ForegroundColor Green
} catch {
    Write-Host "⚠️ API is not running. Starting API server..." -ForegroundColor Yellow
    
    # Start API in background
    $apiProcess = Start-Process powershell -ArgumentList `
        "-NoProfile", `
        "-Command", `
        "cd '$PSScriptRoot\..'; dotnet run --project K2RestApi.csproj" `
        -PassThru -WindowStyle Hidden
    
    Write-Host "Waiting $apiStartupWaitSeconds seconds for API to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds $apiStartupWaitSeconds
    
    # Verify API started
    try {
        $null = Invoke-RestMethod -Uri "$baseUrl/swagger/index.html" -Method Get -TimeoutSec 2 -ErrorAction Stop
        Write-Host "✅ API started successfully!`n" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to start API. Please start manually:" -ForegroundColor Red
        Write-Host "   dotnet run --project K2RestApi.csproj`n" -ForegroundColor Gray
        exit 1
    }
}

# Load request body
Write-Host "Loading request body..." -ForegroundColor Yellow
$bodyPath = Join-Path $PSScriptRoot "..\$testFile"
if (-not (Test-Path $bodyPath)) {
    Write-Host "❌ Test file not found: $bodyPath" -ForegroundColor Red
    exit 1
}
$body = Get-Content $bodyPath -Raw -Encoding UTF8
Write-Host "✅ Request body loaded ($($body.Length) bytes)`n" -ForegroundColor Green

# Define test endpoints
$endpoints = @(
    @{
        Name = "4. CREATE APPROVED"
        Url = "$baseUrl/api/books/create/approved"
        Color = "Green"
        Desc = "Approved/Compliant Case"
    },
    @{
        Name = "5. CREATE NON-COMPLIANT"
        Url = "$baseUrl/api/books/create/non-compliant"
        Color = "Yellow"
        Desc = "Non-Compliant Case"
    },
    @{
        Name = "6. CREATE UNDER-CONSTRUCTION"
        Url = "$baseUrl/api/books/create/under-construction"
        Color = "Cyan"
        Desc = "Under-Construction Case"
    },
    @{
        Name = "7. CREATE ORIGINAL"
        Url = "$baseUrl/api/books/create/original"
        Color = "Blue"
        Desc = "Original - Postman Collection Format"
    }
)

# Run tests
$results = @()
foreach ($ep in $endpoints) {
    Write-Host "---------------------------------------" -ForegroundColor DarkGray
    Write-Host "$($ep.Name)" -ForegroundColor $ep.Color
    Write-Host "$($ep.Desc)" -ForegroundColor Gray
    Write-Host "POST $($ep.Url)" -ForegroundColor White
    
    try {
        $response = Invoke-RestMethod `
            -Uri $ep.Url `
            -Method Post `
            -Body $body `
            -ContentType "application/json; charset=utf-8" `
            -ErrorAction Stop
        
        Write-Host "✅ SUCCESS" -ForegroundColor Green
        Write-Host "  status: $($response.status)" -ForegroundColor Cyan
        Write-Host "  statusCode: $($response.statusCode)" -ForegroundColor Cyan
        Write-Host "  message: $($response.message)" -ForegroundColor Gray
        Write-Host "  book_id: $($response.book_id)" -ForegroundColor Yellow
        Write-Host "  book_code: $($response.book_code)" -ForegroundColor Yellow
        Write-Host "  file_count: $($response.file_count)" -ForegroundColor Gray
        Write-Host "  attach_count: $($response.attach_count)" -ForegroundColor Gray
        Write-Host "  history_count: $($response.history_count)" -ForegroundColor Gray
        Write-Host "  reference_count: $($response.reference_count)" -ForegroundColor Gray
        Write-Host "  reference_attach_count: $($response.reference_attach_count)" -ForegroundColor Gray
        
        $results += @{
            Name = $ep.Name
            Status = "✅ PASS"
            BookId = $response.book_id
            Code = $response.book_code
            FileCount = $response.file_count
            AttachCount = $response.attach_count
        }
    } catch {
        $errorMsg = $_.Exception.Message
        $statusCode = ""
        
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            Write-Host "❌ FAILED (HTTP $statusCode)" -ForegroundColor Red
        } else {
            Write-Host "❌ FAILED" -ForegroundColor Red
        }
        
        Write-Host "  Error: $errorMsg" -ForegroundColor Red
        
        # Try to get error response body
        if ($_.Exception.Response) {
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $errorBody = $reader.ReadToEnd() | ConvertFrom-Json
                Write-Host "  Detail: $($errorBody.message)" -ForegroundColor Yellow
            } catch {
                # Could not read error body
            }
        }
        
        $results += @{
            Name = $ep.Name
            Status = "❌ FAIL"
            BookId = "N/A"
            Code = "N/A"
            FileCount = 0
            AttachCount = 0
        }
    }
    
    Write-Host ""
}

# Display summary
Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "TEST RESULTS SUMMARY" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

foreach ($r in $results) {
    $color = if ($r.Status -like "*PASS*") { "Green" } else { "Red" }
    Write-Host "$($r.Status) $($r.Name)" -ForegroundColor $color
    Write-Host "   book_id: $($r.BookId)" -ForegroundColor Gray
    Write-Host "   book_code: $($r.Code)" -ForegroundColor Gray
    Write-Host "   files: $($r.FileCount), attachments: $($r.AttachCount)" -ForegroundColor Gray
}

$passCount = ($results | Where-Object { $_.Status -like "*PASS*" }).Count
$totalCount = $results.Count
$passRate = [math]::Round(($passCount / $totalCount) * 100, 0)

Write-Host "`nTotal: $passCount/$totalCount PASSED ($passRate%)" -ForegroundColor $(if ($passCount -eq $totalCount) { "Green" } else { "Yellow" })
Write-Host "========================================`n" -ForegroundColor Magenta

# Return results for CI/CD
if ($passCount -eq $totalCount) {
    Write-Host "✅ ALL TESTS PASSED!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "⚠️ SOME TESTS FAILED" -ForegroundColor Yellow
    exit 1
}
