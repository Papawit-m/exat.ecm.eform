using EXAT.ECM.EService.API.Model.Configuration;
using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.EService.API.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options)
        { }
        public DbSet<AccessSessionEntity> AccessSession { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Mapping Model
            modelBuilder.Entity<AccessSessionEntity>().HasNoKey();
            #endregion
        }
    }
}
