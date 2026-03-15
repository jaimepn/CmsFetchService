using CmsFetchService.Core.Models;

namespace CmsFetchService.Infrastructure.Queue
{
    public interface ICmsQueue
    {
        ValueTask AddEventToQueueAsync(CmsEventDto cmsEvent);

        IAsyncEnumerable<CmsEventDto> DequeueAllAsync(CancellationToken ct);
    }
}
