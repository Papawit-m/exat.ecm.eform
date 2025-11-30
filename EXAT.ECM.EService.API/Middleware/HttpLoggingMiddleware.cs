using EXAT.ECM.EService.API.Services.Interfaces;
using System.Diagnostics;
using System.Text;

namespace EXAT.ECM.EService.API.Middleware
{
    public class HttpLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpLoggingMiddleware> _logger;
        private readonly IAccessSessionService _sessionService;

        public HttpLoggingMiddleware(RequestDelegate next, ILogger<HttpLoggingMiddleware> logger, IAccessSessionService sessionService)
        {
            _next = next;
            _logger = logger;
            _sessionService = sessionService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            // // 1) Log Request ก่อน
            await LogRequest(context.Request);

            //  2) เตรียมดัก Response
            var originalBodyStream = context.Response.Body;

            try
            {
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // 3) ตรวจสอบ Token / Device ก่อนจะไปต่อใน pipeline
                var validationPassed = await ValidateAccessAsync(context);

                if (validationPassed)
                {
                    // ผ่านเงื่อนไข → เข้าสู่ middleware / endpoint ถัดไป
                    await _next(context);
                }
                // ถ้าไม่ผ่าน เราก็ตั้ง StatusCode + เขียนข้อความตอบกลับใน ValidateAccessAsync แล้ว

                // 4) Log Response
                await LogResponse(context.Response, stopwatch.ElapsedMilliseconds);

                // 5) Copy response กลับไปยัง original stream
                await responseBody.CopyToAsync(originalBodyStream);
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }

        /// <summary>
        /// ตรวจสอบสิทธิ์จาก Token + DeviceId
        /// </summary>
        private async Task<bool> ValidateAccessAsync(HttpContext context)
        {
            var request = context.Request;

            // -------------------------
            // 1) ดึง Token จาก Query
            // -------------------------
            string token = request.Query["token"].ToString();
            if (string.IsNullOrEmpty(token))
                token = request.Query["accessToken"].ToString();

            // -------------------------
            // 2) ดึง Token จาก Route
            // -------------------------
            if (string.IsNullOrEmpty(token))
            {
                if (request.RouteValues.TryGetValue("token", out var routeToken))
                {
                    token = routeToken?.ToString();
                }
                else if (request.RouteValues.TryGetValue("accessToken", out var routeAToken))
                {
                    token = routeAToken?.ToString();
                }
            }

            // -------------------------
            // 3) ดึง Token จาก Header
            // -------------------------
            if (string.IsNullOrEmpty(token))
            {
                token = request.Headers["X-Access-Token"];
            }

            // DeviceId รับจาก Header (ให้ client แนบมา)
            string deviceId = request.Headers["X-Device-Id"];

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(deviceId))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing token or device id.");
                _logger.LogWarning("Unauthorized: Missing token or device id. Path={Path}", request.Path);
                return false;
            }

            var session = await _sessionService.GetSessionAsync(token);

            if (session == null || session.IS_ACTIVE == 0)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or inactive token.");
                _logger.LogWarning("Unauthorized: Invalid or inactive token. Token={Token}", token);
                return false;
            }

            // ถ้า DeviceId ใน session ยังว่าง → ผูกกับเครื่องแรกที่เข้ามา
            if (string.IsNullOrEmpty(session.DEVICE_ID))
            {
                await _sessionService.UpdateDeviceId(token, deviceId);
                _logger.LogInformation("Bind token to first device. Token={Token}, DeviceId={DeviceId}", token, deviceId);
            }
            else if (!string.Equals(session.DEVICE_ID, deviceId, StringComparison.OrdinalIgnoreCase))
            {
                // ถ้า DeviceId ไม่ตรง → ไม่อนุญาตให้ใช้ link นี้จากอีกเครื่อง
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("This link is already used on another device.");
                _logger.LogWarning(
                    "Forbidden: Token used from another device. Token={Token}, OriginalDevice={OriginalDevice}, CurrentDevice={CurrentDevice}",
                    token,
                    session.DEVICE_ID,
                    deviceId);
                return false;
            }

            return true;
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
