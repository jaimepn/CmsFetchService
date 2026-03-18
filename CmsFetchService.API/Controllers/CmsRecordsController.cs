using CmsFetchService.Infrastructure.Persistence.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CmsFetchService.API.Controllers
{

    [ApiController]
    [Route("api/cmsrecords")]
    [Tags("Record API")]
    public class CmsRecordsController(ICmsRepository repository) : ControllerBase
    {
        [EndpointSummary("Admin: View All Records")]
        [Authorize(Roles = "Admin")]
        [HttpGet("records")]
        public async Task<IActionResult> GetAll()
        {
            var records = await repository.GetAllAsync();
            return Ok(records);
        }

        [EndpointSummary("View Published Records")]
        [Authorize(Roles = "User")]
        [HttpGet("content")]
        public async Task<IActionResult> GetPublished()
        {
            var records = await repository.GetPublishedAsync();
            return Ok(records);
        }

        [EndpointSummary("Admin: Manual Disable")]
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/disable")]
        public async Task<IActionResult> ManualDisable(string id)
        {
            var record = await repository.GetByIdAsync(id);
            if (record == null) return NotFound();

            record.IsManuallyDisabled = true;

            await repository.UpsertAsync(record);
            await repository.SaveChangesAsync();
            return Ok(record);
        }

        [EndpointSummary("Admin: Delete")]
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecord(string id)
        {
            var existing = await repository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }
            await repository.DeleteAsync(id);
            await repository.SaveChangesAsync();
            return NoContent();
        }

    }
}
