# =============================================================================
# K2 REST API - REAL-TIME LOG VIEWER
# =============================================================================
# Description: Monitor K2RestApi logs in real-time
# Usage:
#   .\watch-api-logs.ps1                    # Watch all logs
#   .\watch-api-logs.ps1 -Filter "DEBUG"    # Watch DEBUG logs only
#   .\watch-api-logs.ps1 -Lines 100         # Show last 100 lines
# =============================================================================

param(
    [string]$Filter = "",
    [int]$Lines = 50,
    [int]$RefreshMs = 1000,
    [switch]$Follow = $true
)

$ErrorActionPreference = "SilentlyContinue"

# Find most recent log file
$logPattern = Join-Path $env:TEMP "k2restapi-test-*.log"
$logFiles = Get-ChildItem $logPattern | Sort-Object LastWriteTime -Descending

if ($logFiles.Count -eq 0) {
    Write-Host "No log files found. Please run the API first." -ForegroundColor Red
    Write-Host "Log pattern: $logPattern" -ForegroundColor Gray
    exit 1
}

$logFile = $logFiles[0].FullName
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "K2 REST API - REAL-TIME LOG VIEWER" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Log file: $logFile" -ForegroundColor Gray
Write-Host "Filter: $(if($Filter){"'$Filter'"}else{"(none)"})" -ForegroundColor Gray
Write-Host "Press Ctrl+C to exit`n" -ForegroundColor Yellow

if (-not $Follow) {
    # Show logs once and exit
    $logs = Get-Content $logFile -Tail $Lines
    
    if ($Filter) {
        $logs = $logs | Where-Object { $_ -match $Filter }
    }
    
    foreach ($log in $logs) {
        $color = "White"
        
        if ($log -match "\[DEBUG\]") {
            $color = "Cyan"
        } elseif ($log -match "error|exception|fail" -and $log -notmatch "0 error") {
            $color = "Red"
        } elseif ($log -match "warn") {
            $color = "Yellow"
        } elseif ($log -match "success|pass") {
            $color = "Green"
        }
        
        Write-Host $log -ForegroundColor $color
    }
    
    exit 0
}

# Follow mode - watch for new logs
$lastPosition = 0

if (Test-Path $logFile) {
    $fileInfo = Get-Item $logFile
    $lastPosition = $fileInfo.Length
}

Write-Host "Watching for new logs..." -ForegroundColor Green
Write-Host ""

while ($true) {
    if (-not (Test-Path $logFile)) {
        Start-Sleep -Milliseconds $RefreshMs
        continue
    }
    
    $fileInfo = Get-Item $logFile
    $currentSize = $fileInfo.Length
    
    if ($currentSize -gt $lastPosition) {
        $stream = [System.IO.File]::OpenRead($logFile)
        $stream.Seek($lastPosition, [System.IO.SeekOrigin]::Begin) | Out-Null
        
        $reader = New-Object System.IO.StreamReader($stream)
        
        while ($reader.Peek() -ge 0) {
            $line = $reader.ReadLine()
            
            if ($Filter -and $line -notmatch $Filter) {
                continue
            }
            
            # Colorize output
            $color = "White"
            $timestamp = ""
            
            # Extract timestamp if exists
            if ($line -match "^\[(\d{2}:\d{2}:\d{2})\]") {
                $timestamp = $matches[1]
            }
            
            # Determine color based on content
            if ($line -match "\[DEBUG\]") {
                $color = "Cyan"
            } elseif ($line -match "error|exception|fail" -and $line -notmatch "0 error") {
                $color = "Red"
            } elseif ($line -match "warn") {
                $color = "Yellow"
            } elseif ($line -match "Calling eSaraban API|POST|GET") {
                $color = "Magenta"
            } elseif ($line -match "Response|StatusCode|book_id|book_code") {
                $color = "Green"
            } elseif ($line -match "info:") {
                $color = "Gray"
            }
            
            # Format output with timestamp highlighted
            if ($timestamp) {
                Write-Host "[$timestamp] " -NoNewline -ForegroundColor DarkGray
                $lineWithoutTimestamp = $line -replace "^\[\d{2}:\d{2}:\d{2}\]\s*", ""
                Write-Host $lineWithoutTimestamp -ForegroundColor $color
            } else {
                Write-Host $line -ForegroundColor $color
            }
        }
        
        $reader.Close()
        $stream.Close()
        
        $lastPosition = $currentSize
    }
    
    Start-Sleep -Milliseconds $RefreshMs
}
