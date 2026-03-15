using CmsFetchService.Core.Models;
using System.Threading.Channels;

namespace CmsFetchService.Infrastructure.Queue
{
    public class CmsQueue : ICmsQueue
    {
        // ceiling at 10k - assuming that anything above that has malicious intent
        private readonly Channel<CmsEventDto> _queue = Channel.CreateBounded<CmsEventDto>(10_000);

        public async ValueTask AddEventToQueueAsync(CmsEventDto cmsEvent)
        {
            await _queue.Writer.WriteAsync(cmsEvent);
        }

        public IAsyncEnumerable<CmsEventDto> DequeueAllAsync(CancellationToken ct)
        {
            return _queue.Reader.ReadAllAsync(ct);
        }

    }
}
