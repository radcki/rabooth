using Microsoft.EntityFrameworkCore;
using raBooth.Web.Core.DataAccess.Configuration;
using raBooth.Web.Core.Entities;

namespace raBooth.Web.Core.DataAccess
{
    public interface IDatabaseContext
    {
        public DbSet<Collage> Collages { get; }
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
    public class DatabaseContext : DbContext, IDatabaseContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Collage> Collages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CollageEntityTypeConfiguration).Assembly);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
#if DEBUG
            //builder.UseLoggerFactory(_loggerFactory);
            builder.EnableSensitiveDataLogging();
#endif
        }
    }
}
