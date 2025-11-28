namespace EXAT.ECM.EService.API.Handlers
{
    public class LoggingHttpClientHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHttpClientHandler> _logger;

        public LoggingHttpClientHandler(ILogger<LoggingHttpClientHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Log Request
            _logger.LogInformation(
                "🌐 Outgoing HTTP Request: {Method} {Uri}",
                request.Method,
                request.RequestUri);

            _logger.LogDebug(
                "Request Headers: {Headers}",
                string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));

            if (request.Content != null)
            {
                var content = await request.Content.ReadAsStringAsync(cancellationToken);
                if (!string.IsNullOrEmpty(content))
                {
                    _logger.LogDebug("Request Body: {Body}", content);
                }
            }

            // Send Request
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            HttpResponseMessage? response = null;
            Exception? exception = null;

            try
            {
                response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                // Log Response
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

                var logLevel = response.IsSuccessStatusCode ? LogLevel.Information : LogLevel.Warning;

                _logger.Log(
                    logLevel,
                    "🌐 HTTP Response: {StatusCode} ({ElapsedMs}ms) from {Uri}",
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    request.RequestUri);

                if (!string.IsNullOrEmpty(responseBody) && responseBody.Length < 10000)
                {
                    _logger.LogDebug("Response Body: {Body}", responseBody);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                exception = ex;

                _logger.LogError(
                    ex,
                    "❌ HTTP Request Failed after {ElapsedMs}ms: {Method} {Uri} - {Error}",
                    stopwatch.ElapsedMilliseconds,
                    request.Method,
                    request.RequestUri,
                    ex.Message);

                throw;
            }
        }
    }
}
