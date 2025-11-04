using System.Text.Json.Serialization;

namespace EXAT.ECM.FED.API.Models.IMPORT
{
    public class ImportResultBankFED<T>
    {

        public string? Status { get; set; }
        public string? StatusCode { get; set; }
        public string? Message { get; set; }
        
        /// <summary>
        /// HeaderId สำหรับอ้างอิงการ import (GUID)
        /// </summary>
        [JsonPropertyOrder(-100)] // ให้ serialize ก่อน property อื่น
        public string? HeaderId { get; set; }

        /// <summary>
        /// จำนวนแถวข้อมูลทั้งหมดในไฟล์
        /// </summary>
        public int TotalRowsInFile { get; set; }

        /// <summary>
        /// จำนวนแถวที่ประมวลผลสำเร็จ
        /// </summary>
        public int SuccessfulRows { get; set; }

        /// <summary>
        /// จำนวนแถวที่ประมวลผลล้มเหลว
        /// </summary>
        public int FailedRows { get; set; }

        /// <summary>
        /// รายละเอียดของข้อมูลที่สำเร็จ
        /// </summary>
        public List<T> SuccessDetails { get; set; } = new List<T>();

        /// <summary>
        /// รายละเอียดของข้อมูลที่ล้มเหลว
        /// </summary>
        public List<ImportFailureDetail> FailureDetails { get; set; } = new List<ImportFailureDetail>();
    }
}
