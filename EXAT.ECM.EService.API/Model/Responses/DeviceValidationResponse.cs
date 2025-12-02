using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Configuration;
namespace EXAT.ECM.EService.API.Model.Responses
{
    public class DeviceValidationResponse
    {
        /// <summary>
        /// Whether the device is valid/allowed
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Whether this is a new device registration
        /// </summary>
        public bool IsNewDevice { get; set; }

        /// <summary>
        /// Device information
        /// </summary>
        public DeviceInfo? DeviceInfo { get; set; }

        /// <summary>
        /// Validation message
        /// </summary>
        public string? Message { get; set; }
    }

    /// <summary>
    /// Request model for device registration from client
    /// </summary>
    public class DeviceRegistrationRequest
    {
        /// <summary>
        /// MAC Address from client device (required)
        /// </summary>
        public string MacAddress { get; set; } = string.Empty;

        /// <summary>
        /// Device name/hostname (optional)
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// Additional device information (optional)
        /// </summary>
        public string? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// Response model for session token generation
    /// </summary>
    public class SessionTokenResponse
    {
        /// <summary>
        /// Generated session token (unique identifier)
        /// </summary>
        public string SessionToken { get; set; } = string.Empty;

        /// <summary>
        /// Token expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Indicates if this is a newly created session (true) or existing session (false)
        /// </summary>
        public bool IsNewSession { get; set; }

        /// <summary>
        /// Device information associated with this token
        /// </summary>
        public DeviceInfo? DeviceInfo { get; set; }

        /// <summary>
        /// Server device information (hostname and network interfaces from the machine running the API)
        /// </summary>
        public ServerDeviceInfoModel? ServerDeviceInfo { get; set; }
    }

    public class ServerDeviceInfoModel
    {
        public string Hostname { get; set; }
        public string PrimaryMacAddress { get; set; }
        public DateTime? RetrievedAt { get; set; }
        public List<NetworkInterfaceInfo> NetworkInterfaces { get; set; }
    }

}
