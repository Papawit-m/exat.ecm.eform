using Microsoft.EntityFrameworkCore;

namespace EXAT.ECM.EER.ESARABAN.DAL
{
    public class OracleDbContext : DbContext
    {
        public OracleDbContext(DbContextOptions<OracleDbContext> options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}
