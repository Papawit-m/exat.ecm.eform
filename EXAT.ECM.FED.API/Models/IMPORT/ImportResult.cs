namespace EXAT.ECM.FED.API.Models.IMPORT
{
    public class ImportResult
    {
        public string Status { get; set; } = "E";
        public string Message { get; set; } = string.Empty;
        public string? ErrorFileUrl { get; set; }
    }
}
