using EXAT.ECM.EService.API.Model.Requests;
using System.Text.Json.Serialization;
namespace EXAT.ECM.EService.API.Model.Responses
{

    /// <summary>
    /// K2-Specific Response Models
    /// All properties use PascalCase for K2 SmartObject compatibility
    /// </summary>

    // Base K2 API Response
    public class K2ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorDetail { get; set; }
    }

    // K2 Access Token Response
    public class K2AccessTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string ExpiredIn { get; set; } = string.Empty;
        public string ExpiresAt { get; set; } = string.Empty;
        public string ExpiresDuration { get; set; } = string.Empty;
    }

    // K2 Login Response Data
    public class K2LoginResponseData
    {
        public string AccessToken { get; set; } = string.Empty;
        public string ExpiresAt { get; set; } = string.Empty;
        public string ExpiresDuration { get; set; } = string.Empty;
        public K2LoginData LoginData { get; set; } = new();
        public K2MemberResponseData? MemberByEmail { get; set; }
        public K2MemberResponseData? MemberByCustomerId { get; set; }
    }

    // K2 Login Data (Converted from snake_case to PascalCase)
    public class K2LoginData
    {
        public string MemberId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; // Computed: FirstName + LastName
        public string CreatedAt { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string Active { get; set; } = string.Empty;
        public bool IsConsentLatest { get; set; }
    }

    // K2 Member Response Data
    public class K2MemberResponseData
    {
        public string MemberId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty; // Computed: FirstName + LastName
        public string PhoneNo { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string Active { get; set; } = string.Empty;
        public bool IsConsentLatest { get; set; }

        // Addresses
        public K2Address? ContactAddress { get; set; }
        public K2Address? TaxAddress { get; set; }
        public List<K2Address> Addresses { get; set; } = new();

        // Metadata
        public string CreatedAt { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedAt { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    // K2 Address Data (Converted from snake_case to PascalCase)
    public class K2Address
    {
        public string HouseNo { get; set; } = string.Empty;
        public string VillageNo { get; set; } = string.Empty;
        public string VillageName { get; set; } = string.Empty;
        public string Road { get; set; } = string.Empty;
        public string Lane { get; set; } = string.Empty;
        public string SubDistrict { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string AddressType { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty; // Computed: Complete address string
    }

    // K2 Error Response
    public class K2ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorDetail { get; set; } = string.Empty;
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    }

    
    public class K2Response<T>
    {
        /// <summary>
        /// Status code (0 = success, other = error)
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Data payload
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Total record count (for pagination)
        /// </summary>
        public int? TotalRecords { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Create a success response
        /// </summary>
        public static K2Response<T> Success(T data, string message = "Success")
        {
            return new K2Response<T>
            {
                StatusCode = 0,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Create a success response with pagination
        /// </summary>
        public static K2Response<T> Success(T data, int totalRecords, string message = "Success")
        {
            return new K2Response<T>
            {
                StatusCode = 0,
                Message = message,
                Data = data,
                TotalRecords = totalRecords
            };
        }

        /// <summary>
        /// Create an error response
        /// </summary>
        public static K2Response<T> Error(int statusCode, string message)
        {
            return new K2Response<T>
            {
                StatusCode = statusCode,
                Message = message,
                Data = default
            };
        }

        /// <summary>
        /// Create an error response with default status code
        /// </summary>
        public static K2Response<T> Error(string message)
        {
            return Error(1, message);
        }
    }

    /// <summary>
    /// K2 SmartObject List Response
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class K2ListResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<T> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;

        public static K2ListResponse<T> Success(List<T> items, int totalRecords, int pageNumber = 1, int pageSize = 10, string message = "Success")
        {
            return new K2ListResponse<T>
            {
                StatusCode = 0,
                Message = message,
                Items = items,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public static K2ListResponse<T> Error(int statusCode, string message)
        {
            return new K2ListResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                Items = new List<T>(),
                TotalRecords = 0
            };
        }
    }

}
