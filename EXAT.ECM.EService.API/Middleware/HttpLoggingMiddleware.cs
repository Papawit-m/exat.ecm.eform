using System.Diagnostics;
using System.Text;

namespace EXAT.ECM.EService.API.Middleware
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;

        public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // Log Request
            await LogRequest(context.Request);

            // Capture original response body stream
            var originalBodyStream = context.Response.Body;

            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                await _next(context);

                // Log Response
                await LogResponse(context.Response, stopwatch.ElapsedMilliseconds);

                // Copy response back to original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task LogRequest(HttpRequest request)
        {
            request.EnableBuffering();

            var body = string.Empty;
            if (request.ContentLength > 0)
            {
                using var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }

            _logger.LogInformation(
                "📥 HTTP Request {Method} {Path} {QueryString}",
                request.Method,
                request.Path,
                request.QueryString);

            _logger.LogDebug(
                "Request Headers: {Headers}",
                string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}")));

            if (!string.IsNullOrEmpty(body))
            {
                _logger.LogDebug("Request Body: {Body}", body);
            }
        }

        private async Task LogResponse(HttpResponse response, long elapsedMs)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var body = await new StreamReader(response.Body).ReadToEndAsync();
            response.Body.Seek(0, SeekOrigin.Begin);

            var logLevel = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

            _logger.Log(
                logLevel,
                "📤 HTTP Response {StatusCode} ({ElapsedMs}ms)",
                response.StatusCode,
                elapsedMs);

            if (!string.IsNullOrEmpty(body) && body.Length < 10000)
            {
                _logger.LogDebug("Response Body: {Body}", body);
            }
        }
    }
}
