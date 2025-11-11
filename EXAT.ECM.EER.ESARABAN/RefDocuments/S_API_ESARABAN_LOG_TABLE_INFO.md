# ✅ Table Creation Summary: S_API_ESARABAN_LOG

## สถานะ: สำเร็จแล้ว
**วันที่สร้าง:** 30 ตุลาคม 2025

---

## 📊 ข้อมูล Table

### ข้อมูลทั่วไป
- **Schema:** EFM_EER
- **Table Name:** S_API_ESARABAN_LOG
- **Source Table:** S_API_ESERVICE_LOG
- **Total Columns:** 17
- **Data Included:** No (Structure only)

### โครงสร้าง Columns

| Column Name | Data Type | Length | Nullable |
|------------|-----------|--------|----------|
| LOG_ID | NUMBER | 22 | NOT NULL |
| LOG_LEVEL | VARCHAR2 | 20 | NOT NULL |
| ENDPOINT | VARCHAR2 | 200 | NULL |
| HTTP_METHOD | VARCHAR2 | 10 | NULL |
| REQUEST_PATH | VARCHAR2 | 500 | NULL |
| REQUEST_PARAMETERS | CLOB | 4000 | NULL |
| USERNAME | VARCHAR2 | 100 | NULL |
| CUSTOMER_ID | VARCHAR2 | 50 | NULL |
| EMAIL | VARCHAR2 | 200 | NULL |
| STATUS_CODE | NUMBER | 22 | NULL |
| SUCCESS_FLAG | CHAR | 1 | NULL |
| MESSAGE | VARCHAR2 | 4000 | NULL |
| ERROR_MESSAGE | CLOB | 4000 | NULL |
| EXECUTION_TIME | NUMBER | 22 | NULL |
| REQUEST_TIMESTAMP | TIMESTAMP(6) | 11 | NULL |
| RESPONSE_TIMESTAMP | TIMESTAMP(6) | 11 | NULL |
| CREATED_DATE | TIMESTAMP(6) | 11 | NULL |

---

## 🔧 วิธีการสร้าง

### ผ่าน REST API
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

### Response
```json
{
  "success": true,
  "message": "Table S_API_ESARABAN_LOG created successfully from S_API_ESERVICE_LOG",
  "data": {
    "sourceTable": "S_API_ESERVICE_LOG",
    "newTable": "S_API_ESARABAN_LOG",
    "includeData": false,
    "rowsCopied": 0,
    "createdAt": "2025-10-30T07:38:23.7551418Z"
  }
}
```

### PowerShell Command
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

### SQL Equivalent
```sql
CREATE TABLE S_API_ESARABAN_LOG AS 
SELECT * FROM S_API_ESERVICE_LOG WHERE 1=0;
```

---

## 📝 คำอธิบาย Columns

### Log Identification
- **LOG_ID**: Primary identifier สำหรับแต่ละ log entry
- **LOG_LEVEL**: ระดับของ log (INFO, WARNING, ERROR, etc.)

### Request Information
- **ENDPOINT**: API endpoint ที่ถูกเรียก
- **HTTP_METHOD**: HTTP method (GET, POST, PUT, DELETE)
- **REQUEST_PATH**: Full path ของ request
- **REQUEST_PARAMETERS**: Parameters ที่ส่งมาพร้อม request (JSON format)

### User Information
- **USERNAME**: ชื่อผู้ใช้ที่เรียก API
- **CUSTOMER_ID**: รหัสลูกค้า
- **EMAIL**: อีเมลของผู้ใช้

### Response Information
- **STATUS_CODE**: HTTP status code (200, 404, 500, etc.)
- **SUCCESS_FLAG**: Flag บอกความสำเร็จ (Y/N)
- **MESSAGE**: ข้อความตอบกลับ
- **ERROR_MESSAGE**: ข้อความ error (ถ้ามี)

### Performance & Timing
- **EXECUTION_TIME**: เวลาที่ใช้ในการ execute (milliseconds)
- **REQUEST_TIMESTAMP**: เวลาที่รับ request
- **RESPONSE_TIMESTAMP**: เวลาที่ส่ง response
- **CREATED_DATE**: เวลาที่สร้าง log record

---

## 🎯 การใช้งาน

### 1. Insert Log Entry
```sql
INSERT INTO S_API_ESARABAN_LOG (
    LOG_ID, LOG_LEVEL, ENDPOINT, HTTP_METHOD, REQUEST_PATH,
    USERNAME, CUSTOMER_ID, STATUS_CODE, SUCCESS_FLAG,
    MESSAGE, EXECUTION_TIME, REQUEST_TIMESTAMP, CREATED_DATE
) VALUES (
    SEQ_API_LOG.NEXTVAL, -- สมมติว่ามี sequence
    'INFO',
    '/api/esaraban/submit',
    'POST',
    '/api/esaraban/submit?id=123',
    'ADMIN',
    'CUST001',
    200,
    'Y',
    'Request processed successfully',
    150,
    SYSTIMESTAMP,
    SYSTIMESTAMP
);
COMMIT;
```

### 2. Query Logs
```sql
-- ดู logs ล่าสุด 10 records
SELECT * FROM S_API_ESARABAN_LOG
ORDER BY CREATED_DATE DESC
FETCH FIRST 10 ROWS ONLY;

-- ดู error logs
SELECT * FROM S_API_ESARABAN_LOG
WHERE SUCCESS_FLAG = 'N'
ORDER BY CREATED_DATE DESC;

-- ดู logs ตาม user
SELECT * FROM S_API_ESARABAN_LOG
WHERE USERNAME = 'ADMIN'
ORDER BY CREATED_DATE DESC;

-- ดู performance (slow requests)
SELECT * FROM S_API_ESARABAN_LOG
WHERE EXECUTION_TIME > 1000 -- มากกว่า 1 วินาที
ORDER BY EXECUTION_TIME DESC;
```

### 3. Statistics Query
```sql
-- นับจำนวน requests ต่อ endpoint
SELECT ENDPOINT, COUNT(*) as TOTAL_REQUESTS
FROM S_API_ESARABAN_LOG
GROUP BY ENDPOINT
ORDER BY TOTAL_REQUESTS DESC;

-- Success rate
SELECT 
    COUNT(*) as TOTAL,
    SUM(CASE WHEN SUCCESS_FLAG = 'Y' THEN 1 ELSE 0 END) as SUCCESS_COUNT,
    ROUND(SUM(CASE WHEN SUCCESS_FLAG = 'Y' THEN 1 ELSE 0 END) * 100.0 / COUNT(*), 2) as SUCCESS_RATE
FROM S_API_ESARABAN_LOG;

-- Average execution time
SELECT 
    ENDPOINT,
    AVG(EXECUTION_TIME) as AVG_TIME,
    MIN(EXECUTION_TIME) as MIN_TIME,
    MAX(EXECUTION_TIME) as MAX_TIME
FROM S_API_ESARABAN_LOG
GROUP BY ENDPOINT
ORDER BY AVG_TIME DESC;
```

---

## 🔄 Next Steps (แนะนำ)

### 1. สร้าง Sequence สำหรับ LOG_ID
```sql
CREATE SEQUENCE SEQ_S_API_ESARABAN_LOG
START WITH 1
INCREMENT BY 1
NOCACHE
NOCYCLE;
```

### 2. เพิ่ม Primary Key
```sql
ALTER TABLE S_API_ESARABAN_LOG 
ADD CONSTRAINT PK_S_API_ESARABAN_LOG PRIMARY KEY (LOG_ID);
```

### 3. สร้าง Indexes สำหรับ Performance
```sql
-- Index สำหรับการค้นหาตามวันที่
CREATE INDEX IDX_S_API_ESARABAN_LOG_DATE 
ON S_API_ESARABAN_LOG (CREATED_DATE DESC);

-- Index สำหรับการค้นหาตาม username
CREATE INDEX IDX_S_API_ESARABAN_LOG_USER 
ON S_API_ESARABAN_LOG (USERNAME);

-- Index สำหรับการค้นหาตาม endpoint
CREATE INDEX IDX_S_API_ESARABAN_LOG_ENDPOINT 
ON S_API_ESARABAN_LOG (ENDPOINT);

-- Index สำหรับการค้นหา error
CREATE INDEX IDX_S_API_ESARABAN_LOG_FLAG 
ON S_API_ESARABAN_LOG (SUCCESS_FLAG);
```

### 4. เพิ่ม Default Values
```sql
ALTER TABLE S_API_ESARABAN_LOG 
MODIFY (CREATED_DATE DEFAULT SYSTIMESTAMP);

ALTER TABLE S_API_ESARABAN_LOG 
MODIFY (SUCCESS_FLAG DEFAULT 'Y');
```

### 5. เพิ่ม Check Constraints
```sql
ALTER TABLE S_API_ESARABAN_LOG 
ADD CONSTRAINT CHK_SUCCESS_FLAG CHECK (SUCCESS_FLAG IN ('Y', 'N'));

ALTER TABLE S_API_ESARABAN_LOG 
ADD CONSTRAINT CHK_HTTP_METHOD CHECK (HTTP_METHOD IN ('GET', 'POST', 'PUT', 'DELETE', 'PATCH'));
```

### 6. เพิ่ม Table Comment
```sql
COMMENT ON TABLE S_API_ESARABAN_LOG IS 'API Log table for ESARABAN service';

COMMENT ON COLUMN S_API_ESARABAN_LOG.LOG_ID IS 'Unique log identifier';
COMMENT ON COLUMN S_API_ESARABAN_LOG.LOG_LEVEL IS 'Log level: INFO, WARNING, ERROR';
COMMENT ON COLUMN S_API_ESARABAN_LOG.SUCCESS_FLAG IS 'Success flag: Y=Success, N=Failed';
```

### 7. สร้าง Trigger สำหรับ Auto-increment LOG_ID
```sql
CREATE OR REPLACE TRIGGER TRG_S_API_ESARABAN_LOG_BI
BEFORE INSERT ON S_API_ESARABAN_LOG
FOR EACH ROW
BEGIN
    IF :NEW.LOG_ID IS NULL THEN
        SELECT SEQ_S_API_ESARABAN_LOG.NEXTVAL INTO :NEW.LOG_ID FROM DUAL;
    END IF;
    
    IF :NEW.CREATED_DATE IS NULL THEN
        :NEW.CREATED_DATE := SYSTIMESTAMP;
    END IF;
END;
/
```

---

## 📚 เอกสารที่เกี่ยวข้อง

- **CLONE_TABLE_GUIDE.md** - คู่มือการ clone table
- **ORACLE_INTEGRATION_GUIDE.md** - คู่มือการเชื่อมต่อ Oracle
- **README.md** - คู่มือโปรเจ็กต์หลัก

---

## 🔍 Verification Commands

### ตรวจสอบว่า table มีอยู่
```sql
SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = 'S_API_ESARABAN_LOG';
```

### ตรวจสอบโครงสร้าง
```sql
DESC S_API_ESARABAN_LOG;
```

### ตรวจสอบจำนวน records
```sql
SELECT COUNT(*) FROM S_API_ESARABAN_LOG;
```

### ผ่าน API
```bash
# Check existence
curl http://localhost:5152/api/schema/tables/S_API_ESARABAN_LOG/exists

# Get structure
curl http://localhost:5152/api/oracle/tables/S_API_ESARABAN_LOG/structure
```

---

**สร้างเมื่อ:** 2025-10-30T07:38:23Z  
**สถานะ:** ✅ Active  
**Schema:** EFM_EER  
**Database:** Oracle 11g (ecmdev@172.20.1.176)
