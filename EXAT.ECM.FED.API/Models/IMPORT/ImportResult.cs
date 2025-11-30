namespace EXAT.ECM.FED.API.Models.IMPORT
{
    public class ImportResult
    {
        public string Status { get; set; } = "E";
        public string Message { get; set; } = string.Empty;
        public string RecordCount { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public string FileName { get; set; }
        public string ColumnCount { get; set; }
        public string FilesCount { get; set; }
        public string ProgressId { get; set; }
        public string TotalRows { get; set; }   
        public string ImportBatchId { get; set; }
        public string Source { get; set; }
        public string Headers { get; set; }
        public string FileDetails { get; set; }
        public string Errors { get; set; }
        public string Total { get; set; }
        public string Inserted { get; set; }    
        public string Failed { get; set; }
        public string FilesFound { get; set; }        
        public string? ErrorFileUrl { get; set; }
    }
}
