using CmsFetchService.Core.Extensions;
using CmsFetchService.Core.Models;
using CmsFetchService.Infrastructure.Persistence;
using CmsFetchService.Infrastructure.Queue;


namespace CmsFetchService.Core.Application
{
    public class CmsEventProcessor(
        ICmsQueue _queue,
        IServiceScopeFactory _scopeFactory,
        ILogger<CmsEventProcessor> _logger) : BackgroundService
    {

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CmsEventProcessor starting...");

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await foreach (var cmsEvent in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessEventAsync(db, cmsEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventObj}", cmsEvent);
                }
            }
        }

        private async Task ProcessEventAsync(AppDbContext db, CmsEventDto cmsEvent)
        {
            var existing = await db.CmsRecordEntries.FindAsync(cmsEvent.Id);

            if (existing != null && cmsEvent.Version <= existing.Version)
            {
                _logger.LogWarning("Ignoring incoming CmsEvent as it is an older version: {EventObj}", cmsEvent);
                return;
            }

            if (existing == null)
            {
                _logger.LogInformation("Adding new Cms Record with id: {Id}", cmsEvent.Id);
                var recordEntity = cmsEvent.ToEntity();
                db.CmsRecordEntries.Add(recordEntity);
            }
            else 
            {
                _logger.LogInformation("Updating existing Cms Record with id: {Id}", cmsEvent.Id);
                existing.Payload = cmsEvent.Payload;
                existing.Version = cmsEvent.Version;
                existing.LastUpdated = cmsEvent.Timestamp;
                existing.IsPublished = (cmsEvent.Type == CmsEventType.Publish);
            }

            await db.SaveChangesAsync();
        }

    }


}
