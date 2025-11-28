using EXAT.ECM.EService.API.Model.Requests;
using EXAT.ECM.EService.API.Model.Responses;

namespace EXAT.ECM.EService.API.Services.Interfaces
{
    public interface IExatApiService
    {
        Task<ApiResponse<AccessTokenResponse>> GetAccessTokenAsync(AccessTokenRequest request);
        Task<ApiResponse<Member>> GetMemberByCustomerIdAsync(string accessToken, string customerId);
        Task<ApiResponse<Member>> GetMemberByEmailAsync(string accessToken, string email);
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<LoginResponse>> LoginWithAccessTokenAsync(string accessToken, LoginRequest request);
        Task<ApiResponse<LoginResponse>> LoginWithTokenAsync(string accessToken, LoginTokenRequest request);
    }
}
