using EXAT.ECM.PRS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.PRS.API.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) 
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Mapping Model

            modelBuilder.Entity<PRS_HEADER_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<PRS_DETAIL_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<PRS_HEADER_REQUEST_REPORT>().HasNoKey();
            modelBuilder.Entity<PRS_DETAIL_REQUEST_REPORT>().HasNoKey();
            #endregion
        }
    }
}
    