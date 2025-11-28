using EXAT.ECM.Business.Configurations;
using EXAT.ECM.Business.DAL;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.Business
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<OracleDbContext>(options =>options.UseOracle(Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING")));
            //builder.Configuration.GetSection(AsposeOption.Asposes);

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
