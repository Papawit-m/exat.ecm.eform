using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;

namespace EXAT.ECM.EService.API.Services.Interfaces
{
    public interface ITagUsageService
    {
        Task<TagUsageResponse?> SearchTagUsageAsync(TagUsageRequest request, CancellationToken cancellationToken = default);
    }
}
