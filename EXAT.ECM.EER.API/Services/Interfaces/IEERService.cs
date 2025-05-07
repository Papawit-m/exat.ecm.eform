using EXAT.ECM.EER.API.Models;

namespace EXAT.ECM.EER.API.Services.Interfaces
{
    public interface IEERService
    {
        Task<EER_HEADER_SUMMARY_REPORT> GetEERSummaryAsync(EERParameterModel request);
        Task<EER_HEADER_REQUEST_REPORT> GetEERRequestFormAsync(EERParameterModel request);

    }
}
