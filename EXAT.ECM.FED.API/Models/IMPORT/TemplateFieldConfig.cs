namespace EXAT.ECM.FED.API.Models.IMPORT
{
    /// <summary>
    /// คลาสสำหรับเก็บข้อมูลการตั้งค่าของแต่ละฟิลด์ใน Template
    /// </summary>
    public class TemplateFieldConfig
    {
        /// <summary>
        /// ชื่อฟิลด์ข้อมูลทางตรรกะ (เช่น 'CardNumber', 'TransactionDate')
        /// </summary>
        public required string FieldName { get; init; }

        /// <summary>
        /// ชื่อ Header ในไฟล์ CSV ที่ตรงกับฟิลด์นี้
        /// </summary>
        public string? SourceColumnName { get; init; }

        /// <summary>
        /// ลำดับคอลัมน์ในไฟล์ Excel ที่ตรงกับฟิลด์นี้
        /// </summary>
        public int? SourceColumnIndex { get; init; }

        /// <summary>
        /// ฟิลด์นี้จำเป็นต้องมีข้อมูลหรือไม่
        /// </summary>
        public required bool IsRequired { get; init; }

        /// <summary>
        /// รูปแบบวันที่สำหรับ parse ค่าเมื่อฟิลด์เป็นชนิดวันที่ (เช่น 'dd/MM/yyyy', 'yyyy-MM-dd HH:mm:ss')
        /// ถ้าไม่ระบุ จะไม่พยายาม parse เป็นวันที่
        /// </summary>
        public string? FormatDate { get; init; }

        /// <summary>
        /// ประเภทปี: AD (ค.ศ.), BE (พ.ศ.), AUTO (ตรวจสอบอัตโนมัติ)
        /// </summary>
        public string? YearType { get; init; }
    }
}
