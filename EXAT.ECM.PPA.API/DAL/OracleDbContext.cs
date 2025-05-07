using EXAT.ECM.PPA.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.PPA.API.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) 
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Mapping Model

            modelBuilder.Entity<PPA_HEADER_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<PPA_DETAIL_SUMMARY_REPORT>().HasNoKey();
            #endregion
        }
    }
}
    