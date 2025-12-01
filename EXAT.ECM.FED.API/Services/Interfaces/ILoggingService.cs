namespace EXAT.ECM.FED.API.Services.Interfaces
{
    public interface ILoggingService
    {
        /// <summary>
        /// บันทึกข้อผิดพลาด (Exception) ลงในแหล่งจัดเก็บ
        /// </summary>
        /// <param name="logLevel">ระดับของ Log</param>
        /// <param name="ex">Exception ที่เกิดขึ้น</param>
        /// <param name="message">ข้อความสรุปเพิ่มเติม</param>
        /// <param name="contextInfo">ข้อมูลแวดล้อมที่เกี่ยวข้อง</param>
        Task LogErrorAsync(string logLevel, Exception ex, string message, string? contextInfo = null);

        /// <summary>
        /// บันทึกข้อความทั่วไป (INFO) ลงในแหล่งจัดเก็บ
        /// </summary>
        /// <param name="message">ข้อความหลัก</param>
        /// <param name="contextInfo">ข้อมูลแวดล้อมเพิ่มเติม</param>
        Task LogInfoAsync(string message, string? contextInfo = null);
    }
}
