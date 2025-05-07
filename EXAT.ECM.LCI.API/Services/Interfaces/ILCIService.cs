using EXAT.ECM.LCI.API.Models;

namespace EXAT.ECM.LCI.API.Services.Interfaces
{
    public interface ILCIService
    {
        Task<LCI_HEADER_SUMMARY_REPORT> GetLCISummaryAsync(LCIParameterModel request);
        Task<LCI_HEADER_REQUEST_REPORT> GetLCIRequestAsync(LCIParameterModel request);
    }
}
