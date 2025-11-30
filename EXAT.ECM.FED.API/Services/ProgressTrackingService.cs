using Microsoft.Extensions.Caching.Memory;

namespace EXAT.ECM.FED.API.Services
{
    public interface IProgressTrackingService
    {
        void InitializeProgress(string progressId, int totalRows);
        void UpdateProgress(string progressId, int processedRows);
        ImportProgress? GetProgress(string progressId);
        void CompleteProgress(string progressId, int inserted, int failed, List<object> errors);
        void SetError(string progressId, string errorMessage);
    }

    public class ImportProgress
    {
        public int TotalRows { get; set; }
        public int ProcessedRows { get; set; }
        public int InsertedRows { get; set; }
        public int FailedRows { get; set; }
        public string Status { get; set; } = "Processing"; // Processing, Completed, Error
        public double PercentComplete => TotalRows > 0 ? (ProcessedRows * 100.0 / TotalRows) : 0;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<object> Errors { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }

    public class ProgressTrackingService : IProgressTrackingService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        public ProgressTrackingService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void InitializeProgress(string progressId, int totalRows)
        {
            var progress = new ImportProgress
            {
                TotalRows = totalRows,
                ProcessedRows = 0,
                StartTime = DateTime.UtcNow,
                Status = "Processing"
            };
            _cache.Set(progressId, progress, _cacheExpiration);
        }

        public void UpdateProgress(string progressId, int processedRows)
        {
            if (_cache.TryGetValue(progressId, out ImportProgress? progress) && progress != null)
            {
                progress.ProcessedRows = processedRows;
                _cache.Set(progressId, progress, _cacheExpiration);
            }
        }

        public ImportProgress? GetProgress(string progressId)
        {
            _cache.TryGetValue(progressId, out ImportProgress? progress);
            return progress;
        }

        public void CompleteProgress(string progressId, int inserted, int failed, List<object> errors)
        {
            if (_cache.TryGetValue(progressId, out ImportProgress? progress) && progress != null)
            {
                progress.Status = "Completed";
                progress.InsertedRows = inserted;
                progress.FailedRows = failed;
                progress.Errors = errors;
                progress.EndTime = DateTime.UtcNow;
                _cache.Set(progressId, progress, _cacheExpiration);
            }
        }

        public void SetError(string progressId, string errorMessage)
        {
            if (_cache.TryGetValue(progressId, out ImportProgress? progress) && progress != null)
            {
                progress.Status = "Error";
                progress.ErrorMessage = errorMessage;
                progress.EndTime = DateTime.UtcNow;
                _cache.Set(progressId, progress, _cacheExpiration);
            }
        }
    }
}
