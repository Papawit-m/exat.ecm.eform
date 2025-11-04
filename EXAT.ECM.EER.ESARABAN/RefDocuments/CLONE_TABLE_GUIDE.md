# สคริปต์สำหรับ Clone Table: S_API_ESARABAN_LOG จาก S_API_ESERVICE_LOG

## วิธีใช้งาน

### 1. ผ่าน API Endpoint

#### Clone Table (Structure Only)
```http
POST /api/schema/tables/clone
Content-Type: application/json

{
  "sourceTable": "S_API_ESERVICE_LOG",
  "newTable": "S_API_ESARABAN_LOG",
  "includeData": false,
  "useSysDba": false
}
```

#### Clone Table (With Data)
```http
POST /api/schema/tables/clone
Content-Type: application/json

{
  "sourceTable": "S_API_ESERVICE_LOG",
  "newTable": "S_API_ESARABAN_LOG",
  "includeData": true,
  "useSysDba": false
}
```

### 2. ผ่าน cURL

#### Clone Structure Only
```bash
curl -X POST http://localhost:5152/api/schema/tables/clone \
  -H "Content-Type: application/json" \
  -d '{
    "sourceTable": "S_API_ESERVICE_LOG",
    "newTable": "S_API_ESARABAN_LOG",
    "includeData": false,
    "useSysDba": false
  }'
```

#### Clone With Data
```bash
curl -X POST http://localhost:5152/api/schema/tables/clone \
  -H "Content-Type: application/json" \
  -d '{
    "sourceTable": "S_API_ESERVICE_LOG",
    "newTable": "S_API_ESARABAN_LOG",
    "includeData": true,
    "useSysDba": false
  }'
```

### 3. ผ่าน PowerShell

#### Clone Structure Only
```powershell
$body = @{
    sourceTable = "S_API_ESERVICE_LOG"
    newTable = "S_API_ESARABAN_LOG"
    includeData = $false
    useSysDba = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5152/api/schema/tables/clone" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

#### Clone With Data
```powershell
$body = @{
    sourceTable = "S_API_ESERVICE_LOG"
    newTable = "S_API_ESARABAN_LOG"
    includeData = $true
    useSysDba = $false
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5152/api/schema/tables/clone" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### 4. SQL Statement (Manual)

```sql
-- Clone structure only (ไม่มีข้อมูล)
CREATE TABLE S_API_ESARABAN_LOG AS 
SELECT * FROM S_API_ESERVICE_LOG WHERE 1=0;

-- Clone with data (รวมข้อมูล)
CREATE TABLE S_API_ESARABAN_LOG AS 
SELECT * FROM S_API_ESERVICE_LOG;
```

## Response ที่คาดหวัง

### Success Response
```json
{
  "success": true,
  "message": "Table S_API_ESARABAN_LOG created successfully from S_API_ESERVICE_LOG",
  "data": {
    "sourceTable": "S_API_ESERVICE_LOG",
    "newTable": "S_API_ESARABAN_LOG",
    "includeData": false,
    "rowsCopied": 0,
    "createdAt": "2025-10-30T12:34:56.789Z"
  },
  "error": null,
  "timestamp": "2025-10-30T12:34:56.789Z"
}
```

### Error Response (Table Already Exists)
```json
{
  "success": false,
  "message": "Table S_API_ESARABAN_LOG already exists",
  "data": null,
  "error": "Table already exists",
  "timestamp": "2025-10-30T12:34:56.789Z"
}
```

## Utility Endpoints

### Check if Table Exists
```http
GET /api/schema/tables/S_API_ESARABAN_LOG/exists
```

```bash
curl http://localhost:5152/api/schema/tables/S_API_ESARABAN_LOG/exists
```

### Get Table Structure/DDL
```http
GET /api/schema/tables/S_API_ESERVICE_LOG/ddl
```

```bash
curl http://localhost:5152/api/schema/tables/S_API_ESERVICE_LOG/ddl
```

### Get Table Column Structure
```http
GET /api/oracle/tables/S_API_ESERVICE_LOG/structure
```

```bash
curl http://localhost:5152/api/oracle/tables/S_API_ESERVICE_LOG/structure
```

### Drop Table (if needed to recreate)
```http
DELETE /api/schema/tables/S_API_ESARABAN_LOG?useSysDba=false
```

```bash
curl -X DELETE http://localhost:5152/api/schema/tables/S_API_ESARABAN_LOG?useSysDba=false
```

## Complete Workflow

### ขั้นตอนการสร้าง Table

1. **ตรวจสอบว่า Source Table มีอยู่**
```bash
curl http://localhost:5152/api/oracle/tables/S_API_ESERVICE_LOG/structure
```

2. **ตรวจสอบว่า New Table ยังไม่มี**
```bash
curl http://localhost:5152/api/schema/tables/S_API_ESARABAN_LOG/exists
```

3. **Clone Table Structure**
```bash
curl -X POST http://localhost:5152/api/schema/tables/clone \
  -H "Content-Type: application/json" \
  -d '{
    "sourceTable": "S_API_ESERVICE_LOG",
    "newTable": "S_API_ESARABAN_LOG",
    "includeData": false,
    "useSysDba": false
  }'
```

4. **ยืนยันว่าสร้างสำเร็จ**
```bash
curl http://localhost:5152/api/schema/tables/S_API_ESARABAN_LOG/exists
```

5. **ดูโครงสร้างของ Table ใหม่**
```bash
curl http://localhost:5152/api/oracle/tables/S_API_ESARABAN_LOG/structure
```

## หมายเหตุ

- **includeData: false** - สร้าง table โครงสร้างเดียวกัน แต่ไม่ copy ข้อมูล (แนะนำ)
- **includeData: true** - สร้าง table พร้อม copy ข้อมูลทั้งหมดจาก source table
- **useSysDba: false** - ใช้ user EFM_EER (Normal user) - เพียงพอสำหรับการสร้าง table
- **useSysDba: true** - ใช้ SYS user (ใช้เฉพาะเมื่อจำเป็น)

## ข้อควรระวัง

1. **Constraints และ Indexes จะไม่ถูก copy** - CREATE TABLE AS SELECT จะ copy เฉพาะ:
   - Column structure
   - Data types
   - NOT NULL constraints
   - Data (ถ้าระบุ includeData: true)

2. **ไม่ copy:**
   - Primary Key
   - Foreign Keys
   - Indexes
   - Triggers
   - Comments

3. หากต้องการ copy constraints/indexes ทั้งหมด ต้องใช้ DBMS_METADATA หรือ copy แยก

## Example: Complete Clone with Constraints

หากต้องการ clone พร้อม constraints ให้ run SQL แบบนี้ใน Oracle:

```sql
-- 1. Clone structure
CREATE TABLE S_API_ESARABAN_LOG AS 
SELECT * FROM S_API_ESERVICE_LOG WHERE 1=0;

-- 2. Add Primary Key (ถ้ามี)
ALTER TABLE S_API_ESARABAN_LOG 
ADD CONSTRAINT PK_S_API_ESARABAN_LOG PRIMARY KEY (column_name);

-- 3. Add Indexes (ถ้ามี)
CREATE INDEX IDX_S_API_ESARABAN_LOG_1 
ON S_API_ESARABAN_LOG (column_name);

-- 4. Add Comments (ถ้าต้องการ)
COMMENT ON TABLE S_API_ESARABAN_LOG IS 'Log table for ESARABAN API';
```

---
**Schema:** EFM_EER  
**Source Table:** S_API_ESERVICE_LOG  
**New Table:** S_API_ESARABAN_LOG  
**Date:** 2025-10-30
