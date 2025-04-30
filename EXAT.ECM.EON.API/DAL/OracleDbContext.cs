using EXAT.ECM.EON.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.EON.API.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Mapping Model

            modelBuilder.Entity<EON_SUMMARY_REPORT>().HasNoKey();
            modelBuilder.Entity<EON_REQUEST_REPORT>().HasNoKey();
            #endregion
        }
    }
}
