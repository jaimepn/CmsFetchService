using CmsFetchService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CmsFetchService.API.Controllers
{
    [ApiController]
    [Route("cms")]
    public class WebHookController : ControllerBase
    {

        [HttpPost(Name = "events")]
        public async Task<IActionResult> ReceiveEventsAsync([FromBody] List<CmsEventDto> events)
        {
            // TODO logic

            return Accepted();
        }
    }
}
