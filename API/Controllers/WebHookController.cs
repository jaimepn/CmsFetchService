using CmsFetchService.Core.Models;
using CmsFetchService.Infrastructure.Queue;
using Microsoft.AspNetCore.Mvc;

namespace CmsFetchService.API.Controllers
{
    [ApiController]
    [Route("cms")]
    public class WebHookController(ICmsQueue _queue) : ControllerBase
    {

        [HttpPost(Name = "events")]
        public async Task<IActionResult> ReceiveEventsAsync([FromBody] List<CmsEventDto> events)
        {
            if (events == null || events.Count == 0) return BadRequest();

            foreach (var cmsEvent in events)
            {
                await _queue.AddEventToQueueAsync(cmsEvent);
            }

            return Accepted();
        }
    }
}
