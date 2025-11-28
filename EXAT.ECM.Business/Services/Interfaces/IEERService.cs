using EXAT.ECM.Business.Models.EER;

namespace EXAT.ECM.Business.Services.Interfaces
{
    public interface IEERService
    {
        Task<EER_HEADER_SUMMARY_REPORT> GetEERSummaryAsync(EERParameterModel request);
        Task<EER_HEADER_REQUEST_REPORT> GetEERRequestFormAsync(EERParameterModel request);

    }
}
