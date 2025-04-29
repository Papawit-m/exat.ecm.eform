using EXAT.ECM.Business.Models.PRS;

namespace EXAT.ECM.Business.Services.Interfaces
{
    public interface IPRSService
    {
        Task<PRS_HEADER_SUMMARY_REPORT> GetPRSSummaryAsync(PRSParameterModel request);
        Task<PRS_HEADER_REQUEST_REPORT> GetPRSRequestFormAsync(PRSParameterModel request);
    }
}
