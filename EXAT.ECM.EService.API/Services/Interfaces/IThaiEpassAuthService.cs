using EXAT.ECM.EService.API.Model.Responses;

namespace EXAT.ECM.EService.API.Services.Interfaces
{
    public interface IThaiEpassAuthService
    {
        Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);

        Task<ThaiEpassAuthResponse?> GetAuthResponseAsync(CancellationToken cancellationToken = default);
    }
}
