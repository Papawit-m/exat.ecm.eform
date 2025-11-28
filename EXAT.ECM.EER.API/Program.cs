using EXAT.ECM.EER.API.Configurations;
using EXAT.ECM.EER.API.DAL;
using EXAT.ECM.EER.API.Services;
using EXAT.ECM.EER.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AsposeOption>(builder.Configuration.GetSection(AsposeOption.Asposes));

// Add services to the container.
builder.Services.AddScoped<IEERService, EERService>();

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
