using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using EXAT.ECM.EER.ESARABAN.Services;
using EXAT.ECM.EER.ESARABAN.Models;
using EXAT.ECM.EER.ESARABAN.Middleware;
using Microsoft.EntityFrameworkCore;
using EXAT.ECM.EER.ESARABAN.DAL;

var builder = WebApplication.CreateBuilder(args);

// ---------- Config files ----------
builder.Configuration.AddJsonFile(
    "DefaultSettings/book-defaults.json",
    optional: false,              // ต้องมีไฟล์นี้
    reloadOnChange: true);        // แก้ไฟล์แล้วรีโหลด

// ---------- Services ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

//builder.Services.AddDbContext<OracleDbContext>(options =>
//    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

builder.Services.AddDbContext<OracleDbContext>(options =>
        options.UseOracle(Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING"))
    );

// Strongly-typed options
builder.Services.Configure<ESarabanApiSettings>(
    builder.Configuration.GetSection("ESarabanApiSettings"));
builder.Services.Configure<BookDefaultSettings>(
    builder.Configuration.GetSection("BookDefaultSettings"));

// DI: domain services
builder.Services.AddScoped<IOracleDbService, OracleDbService>();
builder.Services.AddScoped<IApiLogService, ApiLogService>();

// HttpClient for eSaraban
builder.Services.AddHttpClient<ESarabanApiService>((sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var settings = cfg.GetSection("ESarabanApiSettings").Get<ESarabanApiSettings>();
    if (!string.IsNullOrWhiteSpace(settings?.BaseUrl))
        client.BaseAddress = new Uri(settings!.BaseUrl);

    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "K2RestApi/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();

    // ⚠️ เฉพาะตอน Dev เท่านั้น
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    // ไม่ใช้ proxy (เปิดใช้ถ้าจำเป็น)
    handler.UseProxy = false;
    return handler;
});

// Swagger/OpenAPI (บังคับออกเป็น v2 สำหรับ K2)
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "K2 REST Service API",
        Version = "v1",
        Description = "REST API for K2 Integration (OpenAPI 2.0/Swagger)",
        Contact = new OpenApiContact { Name = "API Support", Email = "support@example.com" }
    });
    o.EnableAnnotations();
});

builder.Services.AddCors(o =>
{
    o.AddPolicy("K2CorsPolicy", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Swagger JSON (บังคับเป็น v2)
app.UseSwagger(c => { c.SerializeAsV2 = true; });

// Swagger UI = /swagger
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "K2 REST Service API v1");
    c.RoutePrefix = "swagger"; // => UI อยู่ที่ /swagger
});

// root -> /swagger (กัน 404 ที่หน้าแรก)
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseHttpsRedirection();
app.UseCors("K2CorsPolicy");

// Logging middleware (ก่อน Authorization)
app.UseApiLogging();

app.UseAuthorization();
app.MapControllers();

app.Run();