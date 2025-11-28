using EXAT.ECM.Business.Models.PPA;

namespace EXAT.ECM.Business.Services.Interfaces
{
    public interface IPPAService
    {
        Task<PPA_HEADER_SUMMARY_REPORT> GetPPASummaryAsync(PPAParameterModel request);
    }
}
