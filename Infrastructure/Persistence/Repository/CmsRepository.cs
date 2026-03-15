using CmsFetchService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CmsFetchService.Infrastructure.Persistence.Repository
{
    public class CmsRepository(AppDbContext context) : ICmsRepository
    {

        public async Task<CmsRecord?> GetByIdAsync(string id) =>
            await context.CmsRecordEntries.FindAsync(id);

        public async Task<List<CmsRecord>> GetAllAsync() =>
            await context.CmsRecordEntries.ToListAsync();

        public async Task<List<CmsRecord>> GetPublishedAsync() =>
            await context.CmsRecordEntries.Where(r => r.IsPublished && !r.IsManuallyDisabled).ToListAsync();

        public async Task UpsertAsync(CmsRecord record)
        {
            var existing = await context.CmsRecordEntries.AnyAsync(r => r.Id == record.Id);
            if (!existing)
            {
                context.CmsRecordEntries.Add(record);
            }
            else
            {
                context.CmsRecordEntries.Update(record);
            }
        }

        public async Task SaveChangesAsync() => await context.SaveChangesAsync();

    }
}
