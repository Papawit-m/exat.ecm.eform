using EXAT.ECM.FED.API.Models;
using EXAT.ECM.FED.API.Models.IMPORT;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data;

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

            modelBuilder.Entity<FED_INCOMPT_FUEL_TAXINV>().HasNoKey();
            modelBuilder.Entity<DETAIL_FED_INCOMPT_FUEL_TAXINV>().HasNoKey();

            modelBuilder.Entity<PoliceFuelExceed>().HasNoKey();
            modelBuilder.Entity<DETAIL_PoliceFuelExceed>().HasNoKey();

            modelBuilder.Entity<Header_Report>().HasNoKey();
            modelBuilder.Entity<FuelFleetCard>().HasNoKey();
            modelBuilder.Entity<DETAIL_FuelFleetCard>().HasNoKey();
            modelBuilder.Entity<DETAIL2_FuelFleetCard>().HasNoKey();

            modelBuilder.Entity<T_TEMP_FED_IMPORT_FLEETCARD>().HasNoKey();
            modelBuilder.Entity<T_TEMP_FED_FILE>().HasNoKey();
            modelBuilder.Entity<T_VALIDATE_EXCEL>().HasNoKey();
            modelBuilder.Entity<ImportResult>().HasNoKey();
            modelBuilder.Entity<T_TEMP_FED_IMPORT_FLEETCARD_ERROR>().HasNoKey();
            modelBuilder.Entity<T_TEMP_FED_IMPORT_FLEETCARD_ERROR_LIST>().HasNoKey();


            
            #endregion
        }

        /// <summary>
        /// เปิดคอนเน็กชันใหม่จาก connection string ของ DbContext (เหมาะสำหรับ using/Dispose)
        /// </summary>
        public async Task<OracleConnection> GetOpenConnectionAsync(CancellationToken ct = default)
        {
            // ดึง connection string จาก EF Core
            var baseConn = (OracleConnection)Database.GetDbConnection();
            var conn = new OracleConnection(baseConn.ConnectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);
            return conn; // ผู้เรียกต้องปิด/Dispose เอง (using)
        }

        /// <summary>
        /// เวอร์ชัน Sync (บางเคสอาจอยากใช้)
        /// </summary>
        public OracleConnection GetOpenConnection()
        {
            var baseConn = (OracleConnection)Database.GetDbConnection();
            var conn = new OracleConnection(baseConn.ConnectionString);
            conn.Open();
            return conn;
        }

        public OracleConnection GetOracleConnection()
        {
            var conn = (OracleConnection)this.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                conn.Open();
            return conn;
        }
    }
}
    