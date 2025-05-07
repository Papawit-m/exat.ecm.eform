using EXAT.ECM.PPA.API.Models;

namespace EXAT.ECM.PPA.API.Services.Interfaces
{
    public interface IPPAService
    {
        Task<PPA_HEADER_SUMMARY_REPORT> GetPPASummaryAsync(PPAParameterModel request);
    }
}
