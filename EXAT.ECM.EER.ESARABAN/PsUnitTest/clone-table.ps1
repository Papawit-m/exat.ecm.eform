# PowerShell Script for Cloning S_API_ESARABAN_LOG from S_API_ESERVICE_LOG
# Date: 2025-10-30
# Schema: EFM_EER

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Clone Table: S_API_ESARABAN_LOG" -ForegroundColor Cyan
Write-Host "  From: S_API_ESERVICE_LOG" -ForegroundColor Cyan
Write-Host "  Schema: EFM_EER" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# API Base URL
$apiUrl = "http://localhost:5152"

# Function to make API call
function Invoke-ApiCall {
    param(
        [string]$Endpoint,
        [string]$Method = "GET",
        [object]$Body = $null
    )
    
    try {
        $params = @{
            Uri = "$apiUrl$Endpoint"
            Method = $Method
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json)
        }
        
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
        return $null
    }
}

# Step 1: Check if source table exists
Write-Host "[1/5] Checking if source table exists..." -ForegroundColor Yellow
$sourceCheck = Invoke-ApiCall -Endpoint "/api/oracle/tables/S_API_ESERVICE_LOG/structure"

if ($sourceCheck -and $sourceCheck.success) {
    Write-Host "✓ Source table S_API_ESERVICE_LOG found" -ForegroundColor Green
    Write-Host "  Columns: $($sourceCheck.data.Count)" -ForegroundColor Gray
}
else {
    Write-Host "✗ Source table S_API_ESERVICE_LOG not found!" -ForegroundColor Red
    Write-Host "Please check the source table name and try again." -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 2: Check if target table already exists
Write-Host "[2/5] Checking if target table exists..." -ForegroundColor Yellow
$targetCheck = Invoke-ApiCall -Endpoint "/api/schema/tables/S_API_ESARABAN_LOG/exists"

if ($targetCheck -and $targetCheck.data.exists) {
    Write-Host "⚠ Table S_API_ESARABAN_LOG already exists!" -ForegroundColor Yellow
    $confirm = Read-Host "Do you want to drop and recreate it? (yes/no)"
    
    if ($confirm -eq "yes") {
        Write-Host "  Dropping existing table..." -ForegroundColor Yellow
        $dropResult = Invoke-ApiCall -Endpoint "/api/schema/tables/S_API_ESARABAN_LOG?useSysDba=false" -Method "DELETE"
        
        if ($dropResult -and $dropResult.success) {
            Write-Host "✓ Table dropped successfully" -ForegroundColor Green
        }
        else {
            Write-Host "✗ Failed to drop table" -ForegroundColor Red
            exit 1
        }
    }
    else {
        Write-Host "Operation cancelled." -ForegroundColor Yellow
        exit 0
    }
}
else {
    Write-Host "✓ Target table does not exist (ready to create)" -ForegroundColor Green
}

Write-Host ""

# Step 3: Clone the table structure
Write-Host "[3/5] Cloning table structure..." -ForegroundColor Yellow

$cloneBody = @{
    sourceTable = "S_API_ESERVICE_LOG"
    newTable = "S_API_ESARABAN_LOG"
    includeData = $false
    useSysDba = $false
}

$cloneResult = Invoke-ApiCall -Endpoint "/api/schema/tables/clone" -Method "POST" -Body $cloneBody

if ($cloneResult -and $cloneResult.success) {
    Write-Host "✓ Table S_API_ESARABAN_LOG created successfully!" -ForegroundColor Green
    Write-Host "  Source: $($cloneResult.data.sourceTable)" -ForegroundColor Gray
    Write-Host "  Target: $($cloneResult.data.newTable)" -ForegroundColor Gray
    Write-Host "  Include Data: $($cloneResult.data.includeData)" -ForegroundColor Gray
    Write-Host "  Rows Copied: $($cloneResult.data.rowsCopied)" -ForegroundColor Gray
}
else {
    Write-Host "✗ Failed to clone table" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Step 4: Verify the new table
Write-Host "[4/5] Verifying new table..." -ForegroundColor Yellow
$verifyResult = Invoke-ApiCall -Endpoint "/api/schema/tables/S_API_ESARABAN_LOG/exists"

if ($verifyResult -and $verifyResult.data.exists) {
    Write-Host "✓ Table S_API_ESARABAN_LOG verified successfully" -ForegroundColor Green
}
else {
    Write-Host "⚠ Table verification failed" -ForegroundColor Yellow
}

Write-Host ""

# Step 5: Get new table structure
Write-Host "[5/5] Getting new table structure..." -ForegroundColor Yellow
$structureResult = Invoke-ApiCall -Endpoint "/api/oracle/tables/S_API_ESARABAN_LOG/structure"

if ($structureResult -and $structureResult.success) {
    Write-Host "✓ Table structure retrieved" -ForegroundColor Green
    Write-Host "  Total Columns: $($structureResult.data.Count)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Columns:" -ForegroundColor Cyan
    
    foreach ($column in $structureResult.data) {
        $nullable = if ($column.Nullable -eq "Y") { "NULL" } else { "NOT NULL" }
        Write-Host "  - $($column.ColumnName) : $($column.DataType)($($column.Length)) $nullable" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  ✓ Clone operation completed!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Add Primary Key (if needed)" -ForegroundColor Gray
Write-Host "2. Add Indexes (if needed)" -ForegroundColor Gray
Write-Host "3. Add Foreign Keys (if needed)" -ForegroundColor Gray
Write-Host "4. Add Comments (if needed)" -ForegroundColor Gray
Write-Host ""
Write-Host "SQL Examples:" -ForegroundColor Yellow
Write-Host "  Add Primary Key:" -ForegroundColor Gray
Write-Host "  ALTER TABLE S_API_ESARABAN_LOG ADD CONSTRAINT PK_S_API_ESARABAN_LOG PRIMARY KEY (ID)" -ForegroundColor Gray
Write-Host ""
Write-Host "  Add Index:" -ForegroundColor Gray
Write-Host "  CREATE INDEX IDX_S_API_ESARABAN_LOG_1 ON S_API_ESARABAN_LOG (COLUMN_NAME)" -ForegroundColor Gray
Write-Host ""
