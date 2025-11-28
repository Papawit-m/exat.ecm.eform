# =============================================================================
# QUICK TEST: Single endpoint with detailed logs
# =============================================================================
# Usage: .\quick-test-endpoint.ps1 -Endpoint "approved"
# =============================================================================

param(
    [ValidateSet("approved", "non-compliant", "under-construction", "original")]
    [string]$Endpoint = "approved",
    [string]$BaseUrl = "http://localhost:5152",
    [string]$TestFile = "ExamBodyRequest\books-create-full-format-example.json"
)

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "QUICK TEST: CREATE BOOK - $($Endpoint.ToUpper())" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

# Map endpoint name to URL
$endpointUrls = @{
    "approved" = "$BaseUrl/api/books/create/approved"
    "non-compliant" = "$BaseUrl/api/books/create/non-compliant"
    "under-construction" = "$BaseUrl/api/books/create/under-construction"
    "original" = "$BaseUrl/api/books/create/original"
}

$url = $endpointUrls[$Endpoint]

Write-Host "Endpoint: $url" -ForegroundColor Cyan
Write-Host "Test File: $TestFile`n" -ForegroundColor Gray

# Check if API is running
Write-Host "Checking API status..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$BaseUrl/api/oracle/test-connection" -Method Get -TimeoutSec 3 -ErrorAction Stop
    Write-Host "[OK] API is running`n" -ForegroundColor Green
} catch {
    Write-Host "[X] API is not running!" -ForegroundColor Red
    Write-Host "Please start API: dotnet run --project K2RestApi.csproj`n" -ForegroundColor Yellow
    exit 1
}

# Load request body
$bodyPath = Join-Path $PSScriptRoot "..\$TestFile"
if (-not (Test-Path $bodyPath)) {
    Write-Host "[X] Test file not found: $bodyPath" -ForegroundColor Red
    exit 1
}

$body = Get-Content $bodyPath -Raw -Encoding UTF8
Write-Host "Request body loaded: $($body.Length) bytes`n" -ForegroundColor Gray

# Send request
Write-Host "Sending POST request..." -ForegroundColor Yellow
Write-Host "URL: $url" -ForegroundColor Gray

try {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    
    $response = Invoke-RestMethod `
        -Uri $url `
        -Method Post `
        -Body $body `
        -ContentType "application/json; charset=utf-8" `
        -ErrorAction Stop
    
    $sw.Stop()
    
    Write-Host "`n[OK] SUCCESS (took $($sw.ElapsedMilliseconds)ms)" -ForegroundColor Green
    Write-Host "`nResponse:" -ForegroundColor Cyan
    Write-Host "  status: $($response.status)" -ForegroundColor White
    Write-Host "  statusCode: $($response.statusCode)" -ForegroundColor White
    Write-Host "  message: $($response.message)" -ForegroundColor Gray
    Write-Host "  book_id: $($response.book_id)" -ForegroundColor Yellow
    Write-Host "  book_code: $($response.book_code)" -ForegroundColor Yellow
    Write-Host "  file_count: $($response.file_count)" -ForegroundColor Gray
    Write-Host "  attach_count: $($response.attach_count)" -ForegroundColor Gray
    Write-Host "  history_count: $($response.history_count)" -ForegroundColor Gray
    Write-Host "  reference_count: $($response.reference_count)" -ForegroundColor Gray
    Write-Host "  reference_attach_count: $($response.reference_attach_count)" -ForegroundColor Gray
    
    # Show full response as JSON
    Write-Host "`nFull Response (JSON):" -ForegroundColor Cyan
    $response | ConvertTo-Json -Depth 10 | Write-Host -ForegroundColor Gray
    
    Write-Host "`n========================================" -ForegroundColor Magenta
    Write-Host "TEST PASSED" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Magenta
    
    exit 0
    
} catch {
    $sw.Stop()
    
    $statusCode = "N/A"
    $errorDetail = $_.Exception.Message
    $errorBody = $null
    
    if ($_.Exception.Response) {
        $statusCode = [int]$_.Exception.Response.StatusCode
        
        # Try to read error response body
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $errorBody = $reader.ReadToEnd()
            $reader.Close()
            
            if ($errorBody) {
                try {
                    $errorJson = $errorBody | ConvertFrom-Json
                    $errorDetail = $errorJson.message
                } catch {
                    $errorDetail = $errorBody
                }
            }
        } catch {
            # Could not read error body
        }
    }
    
    Write-Host "`n[X] FAILED (took $($sw.ElapsedMilliseconds)ms)" -ForegroundColor Red
    Write-Host "`nError Details:" -ForegroundColor Yellow
    Write-Host "  HTTP Status: $statusCode" -ForegroundColor Red
    Write-Host "  Error Message: $errorDetail" -ForegroundColor Red
    
    if ($errorBody) {
        Write-Host "`nError Response Body:" -ForegroundColor Yellow
        Write-Host $errorBody -ForegroundColor Red
    }
    
    Write-Host "`nException Details:" -ForegroundColor Yellow
    Write-Host "  Type: $($_.Exception.GetType().Name)" -ForegroundColor Gray
    Write-Host "  Message: $($_.Exception.Message)" -ForegroundColor Gray
    
    if ($_.Exception.InnerException) {
        Write-Host "  Inner Exception: $($_.Exception.InnerException.Message)" -ForegroundColor Gray
    }
    
    Write-Host "`n========================================" -ForegroundColor Magenta
    Write-Host "TEST FAILED" -ForegroundColor Red
    Write-Host "========================================`n" -ForegroundColor Magenta
    
    exit 1
}
