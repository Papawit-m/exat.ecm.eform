using EXAT.ECM.EER.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.EER.API.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options) 
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Mapping Model

            modelBuilder.Entity<EER_HEADER_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<EER_DETAIL_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<EER_HEADER_REQUEST_REPORT>().HasNoKey();
            #endregion
        }
    }
}
    