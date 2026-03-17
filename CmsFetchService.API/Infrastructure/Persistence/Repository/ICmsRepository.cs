using CmsFetchService.Core.Entities;

namespace CmsFetchService.Infrastructure.Persistence.Repository
{
    public interface ICmsRepository
    {
        Task<CmsRecord?> GetByIdAsync(string id);
        Task<List<CmsRecord>> GetAllAsync();
        Task<List<CmsRecord>> GetPublishedAsync();
        Task UpsertAsync(CmsRecord record);
        Task SaveChangesAsync();
        Task DeleteAsync(string id);
    }
}
