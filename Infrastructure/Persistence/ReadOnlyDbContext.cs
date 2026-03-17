using Microsoft.EntityFrameworkCore;

namespace CmsFetchService.Infrastructure.Persistence
{
    public class ReadOnlyDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
    }
}
