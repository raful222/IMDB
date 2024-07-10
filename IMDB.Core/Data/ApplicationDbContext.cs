using IMDB.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace IMDB.Core.Data
{
    /*
     * Add-Migration -OutputDir "Data/Migrations" Initial -Project IMDB.Core -StartupProject IMDB
     * Update-Database -Project IMDB.Core -StartupProject IMDB
     * Remove-Migration -Force -Project IMDB.Core -StartupProject IMDB
     */
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
        }
        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AddTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }
        private void AddTimestamps()
        {
            var now = DateTime.Now;

            foreach (var auditableEntity in ChangeTracker.Entries<IAuditable>())
            {
                if (auditableEntity.State == EntityState.Added)
                {
                    auditableEntity.Entity.CreatedAt = now;
                    auditableEntity.Entity.UpdatedAt = now;
                }

                if (auditableEntity.State == EntityState.Modified)
                {
                    auditableEntity.Property(nameof(IAuditable.CreatedAt)).IsModified = false;
                    auditableEntity.Entity.UpdatedAt = now;
                }
            }
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        public virtual DbSet<Actor> Actors { get; set; }

    }
}
