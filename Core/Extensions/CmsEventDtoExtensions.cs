using CmsFetchService.Core.Entities;
using CmsFetchService.Core.Models;

namespace CmsFetchService.Core.Extensions
{
    public static class CmsEventDtoExtensions
    {
        public static CmsRecord ToEntity(this CmsEventDto dto)
        {
            return new CmsRecord
            {
                Id = dto.Id,
                Payload = dto.Payload,
                Version = dto.Version,
                Type = dto.Type,
                IsPublished = false,
                LastUpdated = dto.Timestamp
            };
        }
    }
}
