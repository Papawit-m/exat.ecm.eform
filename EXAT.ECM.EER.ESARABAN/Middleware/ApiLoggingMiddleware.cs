using System.Diagnostics;
using System.Text.Json;
using System.Text;
using EXAT.ECM.EER.ESARABAN.Services;

namespace EXAT.ECM.EER.ESARABAN.Middleware
{
    /// <summary>
    /// Middleware สำหรับบันทึก API Log อัตโนมัติทุก Request
    /// </summary>
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiLoggingMiddleware> _logger;

        public ApiLoggingMiddleware(RequestDelegate next, ILogger<ApiLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IApiLogService apiLogService)
        {
            // Skip logging for Swagger endpoints
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/favicon.ico"))
            {
                await _next(context);
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var requestBody = await GetRequestBodyAsync(context.Request);
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            string? username = null;
            int statusCode = 200;
            string? errorMessage = null;
            string? responseContent = null;

            try
            {
                // Extract username from request body or query string
                username = ExtractUsernameFromRequest(context.Request, requestBody);

                // Process the request
                await _next(context);

                stopwatch.Stop();
                statusCode = context.Response.StatusCode;

                // Read response body
                responseBody.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(responseBody, Encoding.UTF8, leaveOpen: true))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
                responseBody.Seek(0, SeekOrigin.Begin);

                // Log all responses (only for API endpoints)
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    // Determine success flag and log level based on status code
                    var successFlag = (statusCode >= 200 && statusCode < 400) ? "Y" : "N";
                    var logLevel = statusCode switch
                    {
                        >= 500 => "ERROR",
                        >= 400 => "WARNING",
                        _ => "INFO"
                    };

                    // Build full request path with query string
                    var fullPath = context.Request.Path.Value ?? "";
                    if (context.Request.QueryString.HasValue)
                    {
                        fullPath += context.Request.QueryString.Value;
                    }

                    await apiLogService.LogSuccessAsync(
                        endpoint: context.Request.Path.Value ?? "",
                        httpMethod: context.Request.Method,
                        username: username ?? "Anonymous",
                        statusCode: statusCode,
                        executionTime: stopwatch.ElapsedMilliseconds,
                        requestBody: TruncateString(requestBody, 4000),
                        responseData: TruncateString(responseContent, 4000),
                        successFlag: successFlag,
                        logLevel: logLevel,
                        requestPath: fullPath
                    );
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                statusCode = 500;
                errorMessage = ex.Message;

                _logger.LogError(ex, $"API Error: {context.Request.Path}");

                // Build full request path with query string
                var fullPath = context.Request.Path.Value ?? "";
                if (context.Request.QueryString.HasValue)
                {
                    fullPath += context.Request.QueryString.Value;
                }

                // Log error (only for API endpoints)
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await apiLogService.LogErrorAsync(
                        endpoint: context.Request.Path.Value ?? "",
                        httpMethod: context.Request.Method,
                        username: username ?? "Anonymous",
                        ex: ex,
                        requestBody: TruncateString(requestBody, 4000),
                        requestPath: fullPath
                    );
                }

                // Re-throw the exception to let the error handler deal with it
                throw;
            }
            finally
            {
                // Copy response back to original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        /// <summary>
        /// อ่าน Request Body
        /// </summary>
        private async Task<string> GetRequestBodyAsync(HttpRequest request)
        {
            try
            {
                if (request.ContentLength == null || request.ContentLength == 0)
                    return string.Empty;

                request.EnableBuffering();

                using var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read request body");
                return string.Empty;
            }
        }

        /// <summary>
        /// Extract username จาก Query String หรือ Request Body
        /// </summary>
        private string? ExtractUsernameFromRequest(HttpRequest request, string? requestBody)
        {
            // Try query string first (for endpoints like Transfer, Generate Code)
            if (request.Query.ContainsKey("user_ad"))
            {
                return request.Query["user_ad"].ToString();
            }

            // Fallback to JSON body
            try
            {
                if (string.IsNullOrEmpty(requestBody))
                    return null;

                using var document = JsonDocument.Parse(requestBody);
                if (document.RootElement.TryGetProperty("user_ad", out var userAdElement))
                {
                    return userAdElement.GetString();
                }
            }
            catch
            {
                // Ignore parsing errors
            }

            return null;
        }

        /// <summary>
        /// Truncate string to specified length
        /// </summary>
        private string? TruncateString(string? input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            if (input.Length <= maxLength)
                return input;

            return input.Substring(0, maxLength) + "... (truncated)";
        }
    }

    /// <summary>
    /// Extension method สำหรับ register middleware
    /// </summary>
    public static class ApiLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiLoggingMiddleware>();
        }
    }
}
