using EXAT.ECM.EService.API.DAL;
using EXAT.ECM.EService.API.Handlers;
using EXAT.ECM.EService.API.Middleware;
using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Services.Implementations;
using EXAT.ECM.EService.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register LoggingHttpClientHandler
builder.Services.AddTransient<LoggingHttpClientHandler>();

// Register Oracle Logger Service
builder.Services.AddScoped<IOracleLoggerService, OracleLoggerService>();

// Configure EXAT API settings
builder.Services.Configure<ExatApiSettings>(builder.Configuration.GetSection(ExatApiSettings.SectionName));

//builder.Services.AddScoped<IAccessSessionService, AccessSessionService>();
//builder.Services.AddScoped<ISessionService, SessionService>();

// Register AES Encryption Service
builder.Services.AddSingleton<IEncryptionService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var settings = configuration.GetSection(ExatApiSettings.SectionName).Get<ExatApiSettings>();
    var logger = provider.GetRequiredService<ILogger<IEncryptionService>>();

    if (settings == null)
    {
        logger.LogError("❌ ExatApiSettings is null! Using NoOpEncryptionService as fallback.");
        return new NoOpEncryptionService(provider.GetRequiredService<ILogger<NoOpEncryptionService>>());
    }

    if (settings.UseEncryption)
    {
        var aesLogger = provider.GetRequiredService<ILogger<AesEncryptionService>>();
        logger.LogInformation("🔐 AES Encryption Service enabled (Production mode)");
        return new AesEncryptionService(settings.AesSecretKey, settings.AesSecretIvKey, aesLogger);
    }
    else
    {
        var noOpLogger = provider.GetRequiredService<ILogger<NoOpEncryptionService>>();
        logger.LogInformation("🔓 AES Encryption Service disabled (UAT mode)");
        return new NoOpEncryptionService(noOpLogger);
    }
});

builder.Services.AddSingleton<ISessionService, SessionService>();

//builder.Services.AddDbContext<OracleDbContext>(options =>
//    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

builder.Services.AddDbContext<OracleDbContext>(options =>
        options.UseOracle(Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")));

builder.Services.Configure<TFMNotiSettings>(builder.Configuration.GetSection("TFMNoti"));

builder.Services.Configure<ThaiEpassApiSettings>(builder.Configuration.GetSection("ThaiEpassApi"));
builder.Services.AddHttpClient<IThaiEpassAuthService, ThaiEpassAuthService>();
builder.Services.AddHttpClient<ITagUsageService, TagUsageService>(); 
builder.Services.AddHttpClient<ICustomerSearchService, CustomerSearchService>();

builder.Services.AddHttpClient<INotificationService, NotificationService>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<TFMNotiSettings>>().Value;

    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
});
// Add HttpClient for EXAT API service with logging

builder.Services.AddHttpClient<IExatApiService, ExatApiService>(client =>
{
    var settings = builder.Configuration.GetSection(ExatApiSettings.SectionName).Get<ExatApiSettings>();
    if (settings != null)
    {
        client.BaseAddress = new Uri(settings.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);

        // Debug: Log the BaseAddress being set
        Console.WriteLine($"🔧 HttpClient BaseAddress set to: {client.BaseAddress}");
        Console.WriteLine($"🔧 Settings BaseUrl from config: {settings.BaseUrl}");
    }
    else
    {
        Console.WriteLine("⚠️  WARNING: ExatApiSettings is NULL!");
    }
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        // IMPORTANT: Bypass SSL certificate validation for UAT environment
        // This is necessary because EXAT UAT API uses self-signed certificates
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    Console.WriteLine("⚠️  SSL Certificate Validation DISABLED for EXAT UAT API");
    Console.WriteLine("    This should only be used in Development/UAT environments");

    return handler;
})
.AddHttpMessageHandler<LoggingHttpClientHandler>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Add HTTP Logging Middleware
app.UseMiddleware<HttpLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
