using EXAT.ECM.FED.API.Configurations;
using EXAT.ECM.FED.API.DAL;
using EXAT.ECM.FED.API.Services;
using EXAT.ECM.FED.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// ---------- Options ----------
builder.Services.Configure<AsposeOption>(builder.Configuration.GetSection(AsposeOption.Asposes));

// ---------- Caching----------
builder.Services.AddMemoryCache(); 

// ---------- DI Services ----------
builder.Services.AddScoped<IFEDService, FEDService>();
builder.Services.AddScoped<IFleetCardRepository, FleetCardRepository>();
builder.Services.AddScoped<ILoggingService, DbLoggingService>();
builder.Services.AddScoped<IConfigService, ConfigServiceTemplateImportBankFED>();

// ---------- DbContext ----------
//builder.Services.AddDbContext<OracleDbContext>(options =>
//    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

// Env var:

builder.Services.AddDbContext<OracleDbContext>(options =>
    options.UseOracle(Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")));

// ---------- CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ---------- EPPlus License ----------
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// ---------- MVC / Swagger ----------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---------- Middleware pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ถ้ามี Auth ค่อยเปิดบรรทัดนี้ก่อน Authorization
// app.UseAuthentication();

app.UseCors("AllowAll");       
app.UseAuthorization();

app.MapControllers();

app.Run();
