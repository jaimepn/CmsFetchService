using CmsFetchService.Core.Application;
using CmsFetchService.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CmsFetchService.Infrastructure.Persistence.Repository
{
    public class CmsRepository(AppDbContext _writeContext, ReadOnlyDbContext _readContext, ILogger<CmsRepository> _logger) : ICmsRepository
    {

        public async Task<CmsRecord?> GetByIdAsync(string id) =>
            await _readContext.CmsRecordEntries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        public async Task<List<CmsRecord>> GetAllAsync() =>
            await _readContext.CmsRecordEntries.ToListAsync();

        public async Task<List<CmsRecord>> GetPublishedAsync() =>
            await _readContext.CmsRecordEntries.Where(r => r.IsPublished && !r.IsManuallyDisabled).ToListAsync();

        public async Task UpsertAsync(CmsRecord record)
        {
            var existing = await _writeContext.CmsRecordEntries.FirstOrDefaultAsync(x => x.Id == record.Id);
            if (existing == null)
            {
                _writeContext.CmsRecordEntries.Add(record);
            }
            else
            {
                if (record.Version >= existing.Version)
                {
                    _writeContext.Entry(existing).CurrentValues.SetValues(record);
                }
                else
                {
                    _logger.LogWarning("Ignoring previous version for event {EventId} - Current version {CurrentV}, Incoming: {IncomingV}",
                        record.Id, existing.Version, record.Version);
                }
            }
        }
        public async Task DeleteAsync(string id)
        {
            var entity = await _writeContext.CmsRecordEntries.FindAsync(id);
            if (entity != null)
            {
                _writeContext.CmsRecordEntries.Remove(entity);
            }
        }

        public async Task SaveChangesAsync() => await _writeContext.SaveChangesAsync();

    }
}
