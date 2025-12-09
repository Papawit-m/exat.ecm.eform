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
builder.Services.AddScoped<IProgressTrackingService, ProgressTrackingService>(); 
builder.Services.AddScoped<IBatchInsertService, BatchInsertService>();

// ---------- DbContext ----------
//builder.Services.AddDbContext<OracleDbContext>(options =>
//    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

// Env var:

builder.Services.AddDbContext<OracleDbContext>(options =>
<<<<<<< HEAD
    //options.UseOracle(Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")));
options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));
=======
    options.UseOracle(Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")));

>>>>>>> b4a977611be1183cc2db9e8d2ca46969d0ae8b70
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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // รองรับภาษาไทยและ Unicode ทุกภาษา - ไม่ escape Unicode characters
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        // เพิ่ม options อื่นๆ ถ้าต้องการ
        options.JsonSerializerOptions.WriteIndented = false; // compact JSON
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// ---------- Set Aspose License ----------
var asposeConfig = builder.Configuration.GetSection("AsposeConfig");
var licensePath = asposeConfig.GetValue<string>("LicensePath");
SetAsposeLicense(app.Environment, licensePath);
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


static void SetAsposeLicense(IWebHostEnvironment env, string licenseRelativePath)
{
    try
    {
        string licenseFullPath = Path.Combine(env.ContentRootPath, licenseRelativePath.TrimStart('/'));
        if (!File.Exists(licenseFullPath))
        {
            Console.WriteLine($"❌ Aspose License file not found: {licenseFullPath}");
            return;
        }
        var wordLic = new Aspose.Words.License();
        wordLic.SetLicense(licenseFullPath);
        var pdfLic = new Aspose.Pdf.License();
        pdfLic.SetLicense(licenseFullPath);
        Console.WriteLine("✅ Aspose License set successfully from: " + licenseFullPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error setting Aspose License: " + ex.Message);
    }
}