using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;

namespace EXAT.ECM.EService.API.Services.Interfaces
{
    public interface ISessionService
    {
        Task<SessionTokenResponse> CreateSessionAsync(string deviceId, string? deviceName, DeviceInfo deviceInfo);
        Task<SessionTokenResponse?> GetSessionByDeviceIdAsync(string deviceId);
        Task<SessionTokenResponse?> GetSessionByTokenAsync(string token);
        Task<bool> ClearSessionAsync(string token);
        Task<int> ClearAllSessionsAsync();

    }
}