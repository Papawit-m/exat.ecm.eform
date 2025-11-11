namespace EXAT.ECM.EER.ESARABAN.Models
{
    /// <summary>
    /// Base API Response model for K2 integration
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error details if any
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Error code if any
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Timestamp of the response
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Helper method to create a success response
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message ?? "Operation completed successfully",
                Data = data,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Helper method to create an error response
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string error, string? errorCode = null, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message ?? "Operation failed",
                Error = error,
                ErrorCode = errorCode,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
