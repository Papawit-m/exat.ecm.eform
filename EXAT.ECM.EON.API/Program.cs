using EXAT.ECM.EON.API.Configurations;
using EXAT.ECM.EON.API.DAL;
using EXAT.ECM.EON.API.Services;
using EXAT.ECM.EON.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AsposeOption>(builder.Configuration.GetSection(AsposeOption.Asposes));


// set license *ทันที* ก่อนเรียกใช้ Aspose ตัวอื่น
var cfg = builder.Configuration.GetSection(AsposeOption.Asposes).Get<AsposeOption>();
var license = new Aspose.Words.License();
// ถ้าไฟล์อยู่ที่ /app/AsposeEON.Total.NET.lic
var licenseFile = Path.Combine(builder.Environment.ContentRootPath, cfg.LicensePath);
license.SetLicense(licenseFile);
AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);


// Add services to the container.
builder.Services.AddScoped<IEONService, EONService>();

//builder.Services.AddDbContext<OracleDbContext>(options =>
//    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

builder.Services.AddDbContext<OracleDbContext>(options =>
        options.UseOracle(Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING"))
    );

//AllowAllOrigins //AllowAll
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
