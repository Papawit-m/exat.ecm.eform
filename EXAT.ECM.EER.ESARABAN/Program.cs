using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using EXAT.ECM.EER.ESARABAN.Services;
using EXAT.ECM.EER.ESARABAN.Models;
using EXAT.ECM.EER.ESARABAN.Middleware;
using EXAT.ECM.EER.ESARABAN.Middleware;
using EXAT.ECM.EER.ESARABAN.Models;
using EXAT.ECM.EER.ESARABAN.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Book Default Settings configuration file
builder.Configuration.AddJsonFile(
    "DefaultSettings/book-defaults.json",
    optional: false,
    reloadOnChange: true);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Register eSaraban API Settings
builder.Services.Configure<ESarabanApiSettings>(
    builder.Configuration.GetSection("ESarabanApiSettings"));

// Register Book Default Settings (from DefaultSettings/book-defaults.json)
builder.Services.Configure<BookDefaultSettings>(
    builder.Configuration.GetSection("BookDefaultSettings"));

// Register Oracle Database Service
builder.Services.AddScoped<IOracleDbService, OracleDbService>();

// Register API Log Service
builder.Services.AddScoped<IApiLogService, ApiLogService>();

// Register eSaraban API Service with HttpClient
builder.Services.AddHttpClient<ESarabanApiService>((serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var settings = config.GetSection("ESarabanApiSettings").Get<ESarabanApiSettings>();

    if (settings != null && !string.IsNullOrEmpty(settings.BaseUrl))
    {
        client.BaseAddress = new Uri(settings.BaseUrl);
    }

    // Set timeout to 30 seconds
    client.Timeout = TimeSpan.FromSeconds(30);

    // Add default headers
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "K2RestApi/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // Bypass SSL certificate validation for UAT environment (if needed)
    // WARNING: Remove this in production!
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    // Use system proxy settings with explicit configuration
    // Temporarily disabled for testing - direct connection works
    handler.UseProxy = false;
    //handler.UseDefaultCredentials = true;
    //handler.Proxy = System.Net.WebRequest.GetSystemWebProxy();
    //handler.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;

    return handler;
});

// Configure Swagger for OpenAPI 2.0 (K2 compatible)
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "K2 REST Service API",
        Version = "v1",
        Description = "REST API for K2 Integration with OpenAPI 2.0 (Swagger JSON) support",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@example.com"
        }
    });

    // Enable annotations for better documentation
    options.EnableAnnotations();
});

// Configure CORS for K2
builder.Services.AddCors(options =>
{
    options.AddPolicy("K2CorsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable Swagger UI and JSON endpoint
app.UseSwagger(c =>
{
    c.SerializeAsV2 = true; // Force OpenAPI 2.0 format for K2 compatibility
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "K2 REST Service API V1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
});

app.UseHttpsRedirection();
app.UseCors("K2CorsPolicy");

// Add API Logging Middleware (before authorization)
app.UseApiLogging();

app.UseAuthorization();
app.MapControllers();

app.Run();
