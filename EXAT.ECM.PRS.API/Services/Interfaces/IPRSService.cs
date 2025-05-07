using EXAT.ECM.PRS.API.Models;

namespace EXAT.ECM.PRS.API.Services.Interfaces
{
    public interface IPRSService
    {
        Task<PRS_HEADER_SUMMARY_REPORT> GetPRSSummaryAsync(PRSParameterModel request);
        Task<PRS_HEADER_REQUEST_REPORT> GetPRSRequestFormAsync(PRSParameterModel request);
    }
}
