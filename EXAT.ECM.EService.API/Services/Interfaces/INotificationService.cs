using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;

namespace EXAT.ECM.EService.API.Services.Interfaces
{
    public interface INotificationService
    {
        Task<TFNNotiResponse> InsertNotiAsync(TFNNotiRequest request, CancellationToken cancellationToken = default);
        Task<TFNNotiResponse> UpdateNotiAsync(int notiId, TFNNotiRequest request, CancellationToken cancellationToken = default);
        Task<TFNNotiResponse> UpdateStatusAsync(int notiId, TFNNotiRequest request, CancellationToken cancellationToken = default);
    }
}
