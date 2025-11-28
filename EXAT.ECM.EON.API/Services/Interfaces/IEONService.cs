using EXAT.ECM.EON.API.Models;

namespace EXAT.ECM.EON.API.Services.Interfaces
{
    public interface IEONService
    {
        Task<List<EON_SUMMARY_REPORT>> GetEONSummaryAsync(EONParameterModel request);
        Task<List<EON_REQUEST_REPORT>> GetEONRequestAsync(EONParameterModel request);
    }
}
