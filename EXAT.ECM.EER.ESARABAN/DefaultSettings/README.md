# DefaultSettings Folder

## 📋 ภาพรวม

Folder นี้เก็บไฟล์ Configuration สำหรับค่า Default ของ Book API endpoints

## 📁 ไฟล์ในโฟลเดอร์

### **book-defaults.json**
Configuration สำหรับค่า default ของ 4 endpoints:
- `/api/books/create/original` - สร้างเอกสารทั่วไป (Book Code: BK-YYYYMMDD-XXXX)
- `/api/books/create/approved` - สร้างเอกสารกรณีอนุมัติ (Book Code: APV-YYYYMMDD-XXXX)
- `/api/books/create/non-compliant` - สร้างเอกสารกรณีไม่เข้าหลักเกณ์ (Book Code: NCL-YYYYMMDD-XXXX)
- `/api/books/create/under-construction` - สร้างเอกสารกรณีอยู่ระหว่างก่อสร้าง (Book Code: UNC-YYYYMMDD-XXXX)

## 🔧 โครงสร้าง Configuration

```json
{
  "BookDefaultSettings": {
    "BookData": { ... },        // ค่า default ทั่วไปสำหรับ BookData
    "BookFile": { ... },        // ค่า default สำหรับ BookFile
    "BookHistory": { ... },     // ค่า default สำหรับ BookHistory
    "Endpoints": {
      "Original": { ... },
      "Approved": { ... },
      "NonCompliant": { ... },
      "UnderConstruction": { ... }
    }
  }
}
```

## 🔄 การทำงาน

1. **ไฟล์ถูก Load** เมื่อ Application เริ่มต้น (Program.cs)
2. **Support Hot Reload** - แก้ไขไฟล์แล้วจะ reload อัตโนมัติโดยไม่ต้อง restart
3. **Apply Defaults** - ค่า default จะถูก apply เมื่อ request body ไม่มีค่าหรือมีค่า 0

## 📝 วิธีการแก้ไข Configuration

### ตัวอย่างที่ 1: เปลี่ยน Book Code Prefix

**ก่อนแก้ไข:**
```json
"Original": {
  "BookCodePrefix": "BK-"
}
```

**หลังแก้ไข:**
```json
"Original": {
  "BookCodePrefix": "DOC-"
}
```

**ผลลัพธ์**: รหัสเอกสารจะเป็น `DOC-20251030-XXXX`

---

### ตัวอย่างที่ 2: เปลี่ยนค่า Default Speed

**ก่อนแก้ไข:**
```json
"BookData": {
  "SpeedId": 2
}
```

**หลังแก้ไข:**
```json
"BookData": {
  "SpeedId": 3
}
```

**ผลลัพธ์**: ถ้า request ไม่ส่ง `speed_id` มา จะใช้ค่า `3`

---

### ตัวอย่างที่ 3: เพิ่ม Custom Default

**ก่อนแก้ไข:**
```json
"Approved": {
  "CustomDefaults": {
    "create_page": 2
  }
}
```

**หลังแก้ไข:**
```json
"Approved": {
  "CustomDefaults": {
    "create_page": 2,
    "status_note": "อนุมัติแล้ว",
    "is_urgent": 1
  }
}
```

## ⚠️ ข้อควรระวัง

1. **JSON Syntax** - ตรวจสอบ syntax ให้ถูกต้อง (ใช้ JSON validator)
2. **Null Values** - ค่า `null` จะไม่ถูก apply (ให้ user ส่งค่ามาเอง)
3. **Data Types** - ตรวจสอบ type ให้ตรงกับ Model (int, string, bool)
4. **Backup** - Backup ไฟล์ก่อนแก้ไขเสมอ

## 📊 ค่า Default ที่แนะนำ

### BookData
- `BookTypeId`: 1 (หนังสือทั่วไป)
- `SendTypeId`: 1 (ส่งปกติ)
- `FormatId`: 1 (รูปแบบมาตรฐาน)
- `SpeedId`: 2 (ความเร็วปกติ)
- `SecretId`: 1 (ไม่ลับ)
- `CreatePage`: 1 (หน้าสร้าง)

### Endpoint-Specific
- **Original**: StatusId = 1
- **Approved**: StatusId = 2
- **NonCompliant**: StatusId = 3
- **UnderConstruction**: StatusId = 4

## 🔗 Related Files

- **Models/BookDefaultSettings.cs** - Configuration Model
- **Controllers/BooksController.cs** - ใช้งาน Configuration
- **Program.cs** - Load Configuration
- **RefDocuments/BOOK_DEFAULT_CONFIG_GUIDE.md** - คู่มือการใช้งานแบบละเอียด

## 🚀 Testing

หลังจากแก้ไข Configuration แล้ว:

1. **Save ไฟล์** - ระบบจะ reload อัตโนมัติ (reloadOnChange: true)
2. **ทดสอบ API** - ส่ง request ที่ไม่มีค่า optional fields
3. **ตรวจสอบ Response** - ดูว่าค่า default ถูก apply ถูกต้อง

```bash
# ตัวอย่าง Test Command (PowerShell)
$body = '{"user_ad":"testuser","book":{"book_owner":"Owner","book_subject":"Test","book_to":"Recipient","registrationbook_id":"REG001","booktype_id":0}}'
Invoke-RestMethod -Uri "http://localhost:5152/api/books/create/original" -Method Post -Body $body -ContentType "application/json"
```

## 📞 Support

หากมีคำถามหรือปัญหา:
- **Documentation**: `RefDocuments/BOOK_DEFAULT_CONFIG_GUIDE.md`
- **Model Reference**: `Models/BookDefaultSettings.cs`
- **API Documentation**: http://localhost:5152 (Swagger UI)

---

**Last Updated**: 2025-01-30  
**Version**: 1.0
