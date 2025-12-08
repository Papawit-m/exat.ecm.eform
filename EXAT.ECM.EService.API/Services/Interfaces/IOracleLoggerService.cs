using EXAT.ECM.EService.API.Model.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Text.Json;

namespace EXAT.ECM.EService.API.Services.Interfaces
{
    public interface IOracleLoggerService
    {
        Task LogAsync(LogEntry logEntry);
        Task LogInformationAsync(string endpoint, string httpMethod, string requestPath, string? message = null, string? username = null, string? customerId = null, string? email = null, double? executionTime = null, object? requestData = null, object? responseData = null);
        Task LogWarningAsync(string endpoint, string httpMethod, string requestPath, string message, string? username = null, string? customerId = null, string? email = null, object? requestData = null);
        Task LogErrorAsync(string endpoint, string httpMethod, string requestPath, string message, Exception? exception = null, string? username = null, string? customerId = null, string? email = null, double? executionTime = null, object? requestData = null);
    }
}
