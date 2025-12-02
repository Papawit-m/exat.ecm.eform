using EXAT.ECM.EService.API.DAL;
using EXAT.ECM.EService.API.Handlers;
using EXAT.ECM.EService.API.Middleware;
using EXAT.ECM.EService.API.Model.Configuration;
using EXAT.ECM.EService.API.Services.Implementations;
using EXAT.ECM.EService.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Register LoggingHttpClientHandler
builder.Services.AddTransient<LoggingHttpClientHandler>();

// Configure EXAT API settings
builder.Services.Configure<ExatApiSettings>(builder.Configuration.GetSection(ExatApiSettings.SectionName));

builder.Services.AddScoped<IAccessSessionService, AccessSessionService>();
builder.Services.AddScoped<ISessionService, SessionService>();

//builder.Services.AddDbContext<OracleDbContext>(options =>
//    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

builder.Services.AddDbContext<OracleDbContext>(options =>
        options.UseOracle(Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")));

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
