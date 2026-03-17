using CmsFetchService.Core.Extensions;
using CmsFetchService.Core.Models;
using CmsFetchService.Infrastructure.Persistence.Repository;
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

            await foreach (var cmsEvent in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ICmsRepository>();
                    await ProcessEventAsync(repo, cmsEvent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing event {EventObj}", cmsEvent);
                }
            }
        }

        private async Task ProcessEventAsync(ICmsRepository repo, CmsEventDto cmsEvent)
        {
            var existing = await repo.GetByIdAsync(cmsEvent.Id);

            if (cmsEvent.Type == CmsEventType.Delete)
            {
                _logger.LogInformation("Deleting Cms Record with id: {Id}", cmsEvent.Id);
                await repo.DeleteAsync(cmsEvent.Id);
                await repo.SaveChangesAsync();
                return;
            }
            else 
            {
                var recordEntity = cmsEvent.ToEntity();
                if (cmsEvent.Type == CmsEventType.Publish)
                {
                    recordEntity.IsPublished = true;
                }
                else if (cmsEvent.Type == CmsEventType.UnPublish)
                {
                    recordEntity.IsPublished = false;
                }
                else if (existing != null)
                {
                    recordEntity.IsPublished = existing.IsPublished;
                }
                 await repo.UpsertAsync(recordEntity);
            }
            await repo.SaveChangesAsync();
        }

    }


}
