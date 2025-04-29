
using EXAT.ECM.Business.Models.EON;
using EXAT.ECM.Business.Models.LCI;
using EXAT.ECM.Business.Models.PPA;
using EXAT.ECM.Business.Models.PRS;
using EXAT.ECM.Business.Models.EER;
using EXAT.ECM.Business.Models.FED;

using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.Business.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Mapping Model

            // LCI
            modelBuilder.Entity<LCI_HEADER_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<LCI_DETAIL_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<LCI_HEADER_REQUEST_REPORT>().HasNoKey();
            modelBuilder.Entity<LCI_DETAIL_REQUEST_REPORT>().HasNoKey();

            // PRS
            modelBuilder.Entity<PRS_HEADER_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<PRS_DETAIL_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<PRS_HEADER_REQUEST_REPORT>().HasNoKey();
            modelBuilder.Entity<PRS_DETAIL_REQUEST_REPORT>().HasNoKey();

            // EON
            modelBuilder.Entity<EON_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<EON_REQUEST_REPORT>().HasNoKey();

            // PPA
            modelBuilder.Entity<PPA_HEADER_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<PPA_DETAIL_SUMMARY_REPORT>().HasNoKey();

            // EER
            modelBuilder.Entity<EER_HEADER_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<EER_DETAIL_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<EER_HEADER_REQUEST_REPORT>().HasNoKey();
            //modelBuilder.Entity<EER_DETAIL_REQUEST_REPORT>().HasNoKey();
            // FED
            #endregion
            modelBuilder.Entity<FED_VEHICLE_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_HEADER_DAILYVEHIUSE_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_DETAIL_DAILYVEHIUSE_REPORT>().HasNoKey();

            modelBuilder.Entity<FED_HEADER_MONTHLYVEHIUSE_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_DETAIL_MONTHLYVEHIUSE_REPORT>().HasNoKey();

            modelBuilder.Entity<FED_HEADER_DriverUsageVehicle_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_DETAIL_DriverUsageVehicle_REPORT>().HasNoKey();

            modelBuilder.Entity<FED_HEADER_MachineUse_REPORT>().HasNoKey();
            modelBuilder.Entity<FED_DETAIL_MachineUse_REPORT>().HasNoKey();


        }
    }
}
