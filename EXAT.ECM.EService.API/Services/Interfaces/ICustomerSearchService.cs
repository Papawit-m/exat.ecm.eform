using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;

namespace EXAT.ECM.EService.API.Services.Interfaces
{
    public interface ICustomerSearchService
    {
        Task<CustomerSearchResponse?> SearchCustomerAsync(CustomerSearchRequest request, CancellationToken cancellationToken = default);
    }
}
