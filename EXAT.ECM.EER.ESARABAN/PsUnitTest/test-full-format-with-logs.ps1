# =============================================================================
# TEST: CREATE BOOKS - FULL FORMAT WITH REAL-TIME LOGS
# =============================================================================
# Description: Tests all 4 Full Format endpoints with real-time API log viewer
# Features:
#   - Auto start/stop API server
#   - Real-time log monitoring
#   - Detailed error diagnostics
#   - Comprehensive test results
# =============================================================================

param(
    [switch]$SkipApiStart = $false,
    [switch]$KeepApiRunning = $false
)

$ErrorActionPreference = "Continue"

# Configuration
$baseUrl = "http://localhost:5152"
$testFile = "ExamBodyRequest\books-create-full-format-example.json"
$apiStartupWaitSeconds = 8
$logCheckIntervalMs = 500
$testDelaySeconds = 2

# Colors
$colorHeader = "Magenta"
$colorSuccess = "Green"
$colorWarning = "Yellow"
$colorError = "Red"
$colorInfo = "Cyan"
$colorDebug = "Gray"

# Helper Functions
function Write-Header {
    param([string]$Text)
    Write-Host "`n========================================" -ForegroundColor $colorHeader
    Write-Host $Text -ForegroundColor $colorHeader
    Write-Host "========================================`n" -ForegroundColor $colorHeader
}

function Write-Section {
    param([string]$Text)
    Write-Host "---------------------------------------" -ForegroundColor DarkGray
    Write-Host $Text -ForegroundColor White
}

function Write-Success {
    param([string]$Text)
    Write-Host "[OK] $Text" -ForegroundColor $colorSuccess
}

function Write-Fail {
    param([string]$Text)
    Write-Host "[X] $Text" -ForegroundColor $colorError
}

function Write-Info {
    param([string]$Text)
    Write-Host $Text -ForegroundColor $colorInfo
}

function Write-Debug {
    param([string]$Text)
    Write-Host "    $Text" -ForegroundColor $colorDebug
}

function Stop-ApiServer {
    Write-Host "Stopping API server..." -ForegroundColor $colorWarning
    Get-Process | Where-Object { $_.ProcessName -eq "K2RestApi" } | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Seconds 2
}

function Start-ApiServer {
    Write-Host "Starting API server..." -ForegroundColor $colorInfo
    
    # Create temp log file
    $script:logFile = Join-Path $env:TEMP "k2restapi-test-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
    
    # Start API in background with logging
    $startScript = @"
cd '$PSScriptRoot\..'
`$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project K2RestApi.csproj 2>&1 | Tee-Object -FilePath '$($script:logFile)'
"@
    
    $script:apiProcess = Start-Process powershell -ArgumentList `
        "-NoProfile", `
        "-ExecutionPolicy", "Bypass", `
        "-Command", $startScript `
        -WindowStyle Hidden `
        -PassThru
    
    Write-Debug "API Process ID: $($script:apiProcess.Id)"
    Write-Debug "Log file: $script:logFile"
    
    # Wait for API to start
    Write-Host "Waiting $apiStartupWaitSeconds seconds for API to start..." -ForegroundColor $colorInfo
    
    $startTime = Get-Date
    $timeout = $apiStartupWaitSeconds
    $apiReady = $false
    
    while (((Get-Date) - $startTime).TotalSeconds -lt $timeout) {
        Start-Sleep -Milliseconds $logCheckIntervalMs
        
        # Check if log file exists and contains "Now listening"
        if (Test-Path $script:logFile) {
            $logContent = Get-Content $script:logFile -Raw -ErrorAction SilentlyContinue
            if ($logContent -match "Now listening on") {
                $apiReady = $true
                break
            }
        }
    }
    
    if ($apiReady) {
        Write-Success "API server started successfully"
        return $true
    } else {
        Write-Fail "API server failed to start within $timeout seconds"
        return $false
    }
}

function Get-ApiLogs {
    param(
        [int]$TailLines = 50,
        [string]$Filter = ""
    )
    
    if (-not (Test-Path $script:logFile)) {
        return @()
    }
    
    $logs = Get-Content $script:logFile -Tail $TailLines -ErrorAction SilentlyContinue
    
    if ($Filter) {
        $logs = $logs | Where-Object { $_ -match $Filter }
    }
    
    return $logs
}

function Show-ApiLogs {
    param(
        [string]$Title = "API LOGS",
        [int]$Lines = 30
    )
    
    Write-Host "`n--- $Title ---" -ForegroundColor $colorInfo
    
    $logs = Get-ApiLogs -TailLines $Lines
    
    foreach ($log in $logs) {
        $color = $colorDebug
        
        if ($log -match "\[DEBUG\]") {
            $color = "White"
        } elseif ($log -match "error|exception|fail" -and $log -notmatch "0 error") {
            $color = $colorError
        } elseif ($log -match "warn") {
            $color = $colorWarning
        } elseif ($log -match "info") {
            $color = $colorInfo
        }
        
        Write-Host $log -ForegroundColor $color
    }
    
    Write-Host "--- End of logs ---`n" -ForegroundColor $colorInfo
}

# Main Script
Write-Header "TEST: CREATE BOOKS - FULL FORMAT WITH LOGS"

Write-Info "Test Time: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Info "Test File: $testFile"
Write-Info "Base URL: $baseUrl`n"

# Check if API is already running
$apiRunning = $false
try {
    $null = Invoke-RestMethod -Uri "$baseUrl/api/oracle/test-connection" -Method Get -TimeoutSec 2 -ErrorAction Stop
    $apiRunning = $true
    Write-Success "API is already running"
} catch {
    Write-Info "API is not running"
}

# Start API if needed
if (-not $apiRunning -and -not $SkipApiStart) {
    $started = Start-ApiServer
    
    if (-not $started) {
        Write-Fail "Cannot start API. Exiting."
        exit 1
    }
    
    # Verify API is responding
    try {
        $health = Invoke-RestMethod -Uri "$baseUrl/api/oracle/test-connection" -Method Get -TimeoutSec 5 -ErrorAction Stop
        Write-Success "API health check passed"
    } catch {
        Write-Fail "API health check failed: $($_.Exception.Message)"
        Show-ApiLogs -Title "STARTUP LOGS" -Lines 50
        Stop-ApiServer
        exit 1
    }
}

# Load test request body
Write-Section "Loading Test Data"
$bodyPath = Join-Path $PSScriptRoot "..\$testFile"

if (-not (Test-Path $bodyPath)) {
    Write-Fail "Test file not found: $bodyPath"
    if (-not $SkipApiStart) { Stop-ApiServer }
    exit 1
}

$body = Get-Content $bodyPath -Raw -Encoding UTF8
Write-Success "Request body loaded ($($body.Length) bytes)"

# Define test endpoints
$endpoints = @(
    @{
        Name = "4. CREATE APPROVED"
        Url = "$baseUrl/api/books/create/approved"
        Desc = "Approved/Compliant Case"
    },
    @{
        Name = "5. CREATE NON-COMPLIANT"
        Url = "$baseUrl/api/books/create/non-compliant"
        Desc = "Non-Compliant Case"
    },
    @{
        Name = "6. CREATE UNDER-CONSTRUCTION"
        Url = "$baseUrl/api/books/create/under-construction"
        Desc = "Under-Construction Case"
    },
    @{
        Name = "7. CREATE ORIGINAL"
        Url = "$baseUrl/api/books/create/original"
        Desc = "Original Format"
    }
)

# Run tests
Write-Header "RUNNING TESTS"
$results = @()
$testNumber = 1

foreach ($ep in $endpoints) {
    Write-Section "Test $testNumber/$($endpoints.Count): $($ep.Name)"
    Write-Info "$($ep.Desc)"
    Write-Debug "POST $($ep.Url)"
    
    # Clear log buffer before test
    if (Test-Path $script:logFile) {
        $logSizeBefore = (Get-Item $script:logFile).Length
    }
    
    try {
        $response = Invoke-RestMethod `
            -Uri $ep.Url `
            -Method Post `
            -Body $body `
            -ContentType "application/json; charset=utf-8" `
            -ErrorAction Stop
        
        Write-Success "SUCCESS"
        Write-Host "  status: $($response.status)" -ForegroundColor $colorInfo
        Write-Host "  statusCode: $($response.statusCode)" -ForegroundColor $colorInfo
        Write-Host "  message: $($response.message)" -ForegroundColor $colorDebug
        Write-Host "  book_id: $($response.book_id)" -ForegroundColor $colorSuccess
        Write-Host "  book_code: $($response.book_code)" -ForegroundColor $colorSuccess
        Write-Host "  file_count: $($response.file_count)" -ForegroundColor $colorDebug
        Write-Host "  attach_count: $($response.attach_count)" -ForegroundColor $colorDebug
        
        $results += @{
            Name = $ep.Name
            Status = "PASS"
            BookId = $response.book_id
            Code = $response.book_code
            FileCount = $response.file_count
            AttachCount = $response.attach_count
            StatusCode = $response.statusCode
        }
        
        # Show relevant logs for successful call
        if ($script:logFile) {
            Start-Sleep -Milliseconds 500
            $recentLogs = Get-ApiLogs -TailLines 10 -Filter "DEBUG|eSaraban"
            if ($recentLogs.Count -gt 0) {
                Write-Host "`n  Recent API Logs:" -ForegroundColor $colorInfo
                foreach ($log in $recentLogs) {
                    Write-Host "    $log" -ForegroundColor $colorDebug
                }
            }
        }
        
    } catch {
        $statusCode = "N/A"
        $errorDetail = $_.Exception.Message
        
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
        
        Write-Fail "FAILED (HTTP $statusCode)"
        Write-Host "  Error: $errorDetail" -ForegroundColor $colorError
        
        $results += @{
            Name = $ep.Name
            Status = "FAIL"
            BookId = "N/A"
            Code = "N/A"
            FileCount = 0
            AttachCount = 0
            StatusCode = $statusCode
        }
        
        # Show detailed logs for failed call
        if ($script:logFile) {
            Start-Sleep -Milliseconds 500
            Write-Host "`n  API Debug Logs:" -ForegroundColor $colorWarning
            $debugLogs = Get-ApiLogs -TailLines 20 -Filter "DEBUG|ERROR|Exception"
            foreach ($log in $debugLogs) {
                $logColor = if ($log -match "ERROR|Exception") { $colorError } else { $colorWarning }
                Write-Host "    $log" -ForegroundColor $logColor
            }
        }
    }
    
    Write-Host ""
    $testNumber++
    
    # Delay between tests
    if ($testNumber -le $endpoints.Count) {
        Start-Sleep -Seconds $testDelaySeconds
    }
}

# Display summary
Write-Header "TEST RESULTS SUMMARY"

foreach ($r in $results) {
    $icon = if ($r.Status -eq "PASS") { "[PASS]" } else { "[FAIL]" }
    $color = if ($r.Status -eq "PASS") { $colorSuccess } else { $colorError }
    
    Write-Host "$icon $($r.Name)" -ForegroundColor $color
    Write-Host "      Status Code: $($r.StatusCode)" -ForegroundColor $colorDebug
    Write-Host "      book_id: $($r.BookId)" -ForegroundColor $colorDebug
    Write-Host "      book_code: $($r.Code)" -ForegroundColor $colorDebug
    Write-Host "      files: $($r.FileCount), attachments: $($r.AttachCount)" -ForegroundColor $colorDebug
}

$passCount = ($results | Where-Object { $_.Status -eq "PASS" }).Count
$totalCount = $results.Count
$passRate = if ($totalCount -gt 0) { [math]::Round(($passCount / $totalCount) * 100, 0) } else { 0 }

Write-Host ""
$resultColor = if ($passCount -eq $totalCount) { $colorSuccess } elseif ($passCount -gt 0) { $colorWarning } else { $colorError }
Write-Host "TOTAL: $passCount/$totalCount PASSED ($passRate%)" -ForegroundColor $resultColor

# Show full logs if any test failed
$failedCount = $totalCount - $passCount
if ($failedCount -gt 0) {
    Write-Host "`nNote: $failedCount test(s) failed. Showing detailed API logs..." -ForegroundColor $colorWarning
    Show-ApiLogs -Title "DETAILED API LOGS" -Lines 50
}

# Cleanup
if (-not $SkipApiStart -and -not $KeepApiRunning) {
    Write-Host "`nCleaning up..." -ForegroundColor $colorInfo
    Stop-ApiServer
    
    if ($script:logFile -and (Test-Path $script:logFile)) {
        Write-Debug "Log file saved: $script:logFile"
        Write-Debug "To view full logs: Get-Content '$($script:logFile)'"
    }
}

Write-Host "========================================`n" -ForegroundColor $colorHeader

# Exit with appropriate code
if ($passCount -eq $totalCount) {
    exit 0
} else {
    exit 1
}
