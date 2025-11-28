<#
.SYNOPSIS
    Test /api/hello endpoint - eSaraban UAT API connection test

.DESCRIPTION
    Script to test the new /api/hello endpoint that verifies connection to eSaraban UAT API.
    
    Features:
    - Auto start/stop K2RestApi
    - Call GET /api/hello endpoint
    - Display formatted response
    - Show connection details
    - Color-coded output

.EXAMPLE
    .\PsUnitTest\test-hello-endpoint.ps1
    
    Test the hello endpoint with default settings

.EXAMPLE
    .\PsUnitTest\test-hello-endpoint.ps1 -NoAutoStart
    
    Test without auto-starting API (assumes API is already running)

.NOTES
    Author: K2RestApi Team
    Date: 2025-11-04

    Requirement:
    - PowerShell 5.1+ (tested on Windows PowerShell)
    - .NET 8 SDK (if auto-start)
    - K2RestApi project in repository root
#>

[CmdletBinding()]
param(
    [Parameter()]
    [switch]$NoAutoStart,
    
    [Parameter()]
    [int]$ApiPort = 5152
)

# Color definitions
$ColorSuccess = "Green"
$ColorInfo = "Cyan"
$ColorWarning = "Yellow"
$ColorError = "Red"
$ColorHeader = "Magenta"

# Banner
Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor $ColorHeader
Write-Host "║                                                           ║" -ForegroundColor $ColorHeader
Write-Host "║           Test /api/hello Endpoint                        ║" -ForegroundColor $ColorHeader
Write-Host "║           eSaraban UAT API Connection Test                ║" -ForegroundColor $ColorHeader
Write-Host "║                                                           ║" -ForegroundColor $ColorHeader
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor $ColorHeader
Write-Host ""

# Project root (assume repo root contains K2RestApi.csproj)
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path "$scriptPath\.." | Select-Object -ExpandProperty Path
$projectFile = Join-Path $repoRoot "K2RestApi.csproj"

# If not found, try parent again (in case script is in PsUnitTest folder)
if (-not (Test-Path $projectFile)) {
    $repoRoot = Resolve-Path "$scriptPath\..\.." | Select-Object -ExpandProperty Path -ErrorAction SilentlyContinue
    $projectFile = Join-Path $repoRoot "K2RestApi.csproj"
}

# Check if project file exists
if (-not (Test-Path $projectFile)) {
    Write-Host "❌ ERROR: Project file not found: $projectFile" -ForegroundColor $ColorError
    Write-Host "   Ensure you run this script from inside the repository or adjust the path." -ForegroundColor $ColorError
    exit 1
}

Write-Host "📁 Project: " -NoNewline -ForegroundColor $ColorInfo
Write-Host "$projectFile" -ForegroundColor White
Write-Host "🌐 API Port: " -NoNewline -ForegroundColor $ColorInfo
Write-Host $ApiPort -ForegroundColor White
Write-Host ""

# API URL
$apiBaseUrl = "http://localhost:$ApiPort"
$helloEndpoint = "$apiBaseUrl/api/hello"

# Function to check if API is running
function Test-ApiRunning {
    try {
        $response = Invoke-WebRequest -Uri "$apiBaseUrl/swagger/index.html" -Method Get -TimeoutSec 2 -ErrorAction Stop
        return $response.StatusCode -eq 200
    }
    catch {
        return $false
    }
}

# Start API if needed
$apiProcess = $null
$shouldStopApi = $false

if (-not $NoAutoStart) {
    Write-Host "🔍 Checking if API is already running..." -ForegroundColor $ColorInfo
    
    if (Test-ApiRunning) {
        Write-Host "✅ API is already running" -ForegroundColor $ColorSuccess
        Write-Host ""
    }
    else {
        Write-Host "🚀 Starting K2RestApi..." -ForegroundColor $ColorInfo
        
        # Start the API
        $startInfo = new-object System.Diagnostics.ProcessStartInfo
        $startInfo.FileName = "dotnet"
        $startInfo.Arguments = "run --project `"$projectFile`" --urls $apiBaseUrl"
        $startInfo.WorkingDirectory = $repoRoot
        $startInfo.RedirectStandardOutput = $true
        $startInfo.RedirectStandardError = $true
        $startInfo.UseShellExecute = $false
        $startInfo.CreateNoWindow = $true

        $apiProcess = New-Object System.Diagnostics.Process
        $apiProcess.StartInfo = $startInfo
        $null = $apiProcess.Start()

        $shouldStopApi = $true

        Write-Host "⏳ Waiting for API to start..." -ForegroundColor $ColorInfo
        # Wait for API to be ready (max 30 seconds)
        $maxWaitSeconds = 30
        $waitedSeconds = 0
        $isReady = $false
        while ($waitedSeconds -lt $maxWaitSeconds) {
            Start-Sleep -Seconds 2
            $waitedSeconds += 2
            if (Test-ApiRunning) {
                $isReady = $true
                break
            }
            Write-Host "." -NoNewline -ForegroundColor $ColorInfo
        }
        Write-Host ""
        if ($isReady) {
            Write-Host "✅ API started successfully!" -ForegroundColor $ColorSuccess
            Write-Host ""
        }
        else {
            Write-Host "❌ ERROR: API failed to start within $maxWaitSeconds seconds" -ForegroundColor $ColorError
            if ($apiProcess -and -not $apiProcess.HasExited) {
                $apiProcess.Kill()
            }
            exit 1
        }
    }
}

# Test Hello Endpoint
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
Write-Host "🧪 Testing /api/hello Endpoint" -ForegroundColor $ColorHeader
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
Write-Host ""

Write-Host "📍 Endpoint: " -NoNewline -ForegroundColor $ColorInfo
Write-Host "GET $helloEndpoint" -ForegroundColor White
Write-Host "🎯 Purpose: " -NoNewline -ForegroundColor $ColorInfo
Write-Host "Test connection to eSaraban UAT API" -ForegroundColor White
Write-Host ""

try {
    Write-Host "⏳ Calling endpoint..." -ForegroundColor $ColorInfo
    $startTime = Get-Date
    $response = Invoke-RestMethod -Uri $helloEndpoint -Method Get -ErrorAction Stop
    $endTime = Get-Date
    $responseTime = ($endTime - $startTime).TotalMilliseconds

    Write-Host "✅ Response received in " -NoNewline -ForegroundColor $ColorSuccess
    Write-Host "$([math]::Round($responseTime, 2)) ms" -ForegroundColor White
    Write-Host ""

    # Display response
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
    Write-Host "📋 Response Details" -ForegroundColor $ColorHeader
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
    Write-Host ""

    # Connection status
    if ($response.success) {
        Write-Host "🟢 Connection Status: " -NoNewline -ForegroundColor $ColorSuccess
        Write-Host "SUCCESS" -ForegroundColor $ColorSuccess
    }
    else {
        Write-Host "🔴 Connection Status: " -NoNewline -ForegroundColor $ColorError
        Write-Host "FAILED" -ForegroundColor $ColorError
    }

    Write-Host "💬 Message: " -NoNewline -ForegroundColor $ColorInfo
    Write-Host $response.message -ForegroundColor White
    Write-Host ""

    # Test details
    Write-Host "🔧 Test Details:" -ForegroundColor $ColorInfo
    Write-Host "  • Endpoint Tested: " -NoNewline -ForegroundColor $ColorInfo
    Write-Host $response.endpointTested -ForegroundColor White
    Write-Host "  • User AD: " -NoNewline -ForegroundColor $ColorInfo
    Write-Host $response.userAd -ForegroundColor White
    Write-Host "  • Book ID (Test): " -NoNewline -ForegroundColor $ColorInfo
    Write-Host $response.bookId -ForegroundColor White
    Write-Host ""

    # eSaraban response
    if ($response.eSarabanResponse) {
        Write-Host "📡 eSaraban API Response:" -ForegroundColor $ColorInfo
        Write-Host "  • Status: " -NoNewline -ForegroundColor $ColorInfo
        if ($response.eSarabanResponse.status -eq "S") {
            Write-Host $response.eSarabanResponse.status -ForegroundColor $ColorSuccess
        }
        else {
            Write-Host $response.eSarabanResponse.status -ForegroundColor $ColorWarning
        }
        Write-Host "  • Status Code: " -NoNewline -ForegroundColor $ColorInfo
        Write-Host $response.eSarabanResponse.statusCode -ForegroundColor White
        Write-Host "  • Book Code: " -NoNewline -ForegroundColor $ColorInfo
        Write-Host $response.eSarabanResponse.bookCode -ForegroundColor White
        Write-Host "  • To Date: " -NoNewline -ForegroundColor $ColorInfo
        Write-Host $response.eSarabanResponse.toDate -ForegroundColor White
        Write-Host ""
    }

    # Connection info
    if ($response.connectionInfo) {
        Write-Host "🔌 Connection Configuration:" -ForegroundColor $ColorInfo
        Write-Host "  • Base URL: " -NoNewline -ForegroundColor $ColorInfo
        Write-Host $response.connectionInfo.baseUrl -ForegroundColor White
        Write-Host "  • Proxy Enabled: " -NoNewline -ForegroundColor $ColorInfo
        Write-Host $response.connectionInfo.proxyEnabled -ForegroundColor White
        Write-Host "  • SSL Validation: " -NoNewline -ForegroundColor $ColorInfo
        Write-Host $response.connectionInfo.sslValidation -ForegroundColor White
        Write-Host "  • Timeout: " -NoNewline -ForegroundColor $ColorInfo
        Write-Host $response.connectionInfo.timeout -ForegroundColor White
        Write-Host ""
    }

    # Error details (if any)
    if ($response.errorDetails) {
        Write-Host "⚠️ Error Details:" -ForegroundColor $ColorWarning
        Write-Host "  $($response.errorDetails)" -ForegroundColor $ColorWarning
        Write-Host ""
    }

    # Full JSON response
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
    Write-Host "📄 Full JSON Response" -ForegroundColor $ColorHeader
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
    Write-Host ""
    Write-Host ($response | ConvertTo-Json -Depth 10) -ForegroundColor White
    Write-Host ""

    # Summary
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
    Write-Host "📊 Test Summary" -ForegroundColor $ColorHeader
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
    Write-Host ""

    if ($response.success) {
        Write-Host "✅ TEST PASSED" -ForegroundColor $ColorSuccess
        Write-Host "   Connection to eSaraban UAT API is working!" -ForegroundColor $ColorSuccess
    }
    else {
        Write-Host "❌ TEST FAILED" -ForegroundColor $ColorError
        Write-Host "   Connection to eSaraban UAT API failed" -ForegroundColor $ColorError
    }

    Write-Host ""
}
catch {
    Write-Host "❌ ERROR: Failed to call /api/hello endpoint" -ForegroundColor $ColorError
    Write-Host "   $($_.Exception.Message)" -ForegroundColor $ColorError
    Write-Host ""
    if ($_.Exception.Response) {
        try { $statusCode = $_.Exception.Response.StatusCode.value__ } catch { $statusCode = $_.Exception.Response.StatusCode }
        Write-Host "   HTTP Status Code: $statusCode" -ForegroundColor $ColorError
        Write-Host ""
    }
}

# Cleanup
if ($shouldStopApi -and $apiProcess -and -not $apiProcess.HasExited) {
    Write-Host "🛑 Stopping K2RestApi..." -ForegroundColor $ColorInfo
    try { $apiProcess.Kill() } catch {}
    Write-Host "✅ API stopped" -ForegroundColor $ColorSuccess
    Write-Host ""
}

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
Write-Host "Test completed" -ForegroundColor $ColorHeader
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor $ColorHeader
Write-Host ""
