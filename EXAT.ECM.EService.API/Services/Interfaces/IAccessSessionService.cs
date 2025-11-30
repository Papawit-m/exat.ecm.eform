using EXAT.ECM.EService.API.Model.Configuration;

namespace EXAT.ECM.EService.API.Services.Interfaces
{
    public interface IAccessSessionService
    {
        Task<AccessSessionModel?> GetSessionAsync(string token);
        Task UpdateDeviceId(string token, string deviceId);
    }
}
