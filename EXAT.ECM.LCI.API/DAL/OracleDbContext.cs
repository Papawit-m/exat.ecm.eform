using EXAT.ECM.LCI.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.LCI.API.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) 
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Mapping Model

            modelBuilder.Entity<LCI_HEADER_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<LCI_DETAIL_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<LCI_HEADER_REQUEST_REPORT>().HasNoKey();
            modelBuilder.Entity<LCI_DETAIL_REQUEST_REPORT>().HasNoKey();
            #endregion
        }
    }
}
    