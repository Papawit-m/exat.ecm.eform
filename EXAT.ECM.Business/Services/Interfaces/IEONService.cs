using EXAT.ECM.Business.Models.EON;

namespace EXAT.ECM.Business.Services.Interfaces
{
    public interface IEONService
    {
        Task<List<EON_SUMMARY_REPORT>> GetEONSummaryAsync(EONParameterModel request);
        Task<List<EON_REQUEST_REPORT>> GetEONRequestAsync(EONParameterModel request);
    }
}
