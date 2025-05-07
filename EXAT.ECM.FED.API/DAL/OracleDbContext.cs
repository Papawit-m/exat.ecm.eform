using EXAT.ECM.FED.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.FED.API.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) 
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Mapping Model
            modelBuilder.Entity<FED_VEHICLE_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_HEADER_DAILYVEHIUSE_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_DETAIL_DAILYVEHIUSE_REPORT>().HasNoKey();

            modelBuilder.Entity<FED_HEADER_MONTHLYVEHIUSE_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_DETAIL_MONTHLYVEHIUSE_REPORT>().HasNoKey();

            modelBuilder.Entity<FED_HEADER_DriverUsageVehicle_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_DETAIL_DriverUsageVehicle_REPORT>().HasNoKey();

            modelBuilder.Entity<FED_HEADER_MachineUse_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_DETAIL_MachineUse_REPORT>().HasNoKey();
            #endregion
        }
    }
}
    