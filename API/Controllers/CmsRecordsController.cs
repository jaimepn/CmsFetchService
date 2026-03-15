using CmsFetchService.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Mvc;

namespace CmsFetchService.API.Controllers
{

    [ApiController]
    [Route("api/cmsrecords")]
    public class CmsRecordsController(ICmsRepository repository) : ControllerBase
    {

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var records = await repository.GetAllAsync();
            return Ok(records);
        }

        [HttpGet]
        public async Task<IActionResult> GetPublished()
        {
            var records = await repository.GetPublishedAsync();
            return Ok(records);
        }

        [HttpPatch("{id}/disable")]
        public async Task<IActionResult> ManualDisable(string id)
        {
            var record = await repository.GetByIdAsync(id);
            if (record == null) return NotFound();

            record.IsManuallyDisabled = true;

            await repository.SaveChangesAsync();
            return Ok(record);
        }

    }
}
