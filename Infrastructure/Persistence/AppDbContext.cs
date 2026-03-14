using CmsFetchService.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Xml;

namespace CmsFetchService.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<CmsRecord> CmsRecordEntries => Set<CmsRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Ensure we have a default value for our manual flag
            modelBuilder.Entity<CmsRecord>()
                .Property(e => e.IsManuallyDisabled)
                .HasDefaultValue(false);
        }
    }
}
