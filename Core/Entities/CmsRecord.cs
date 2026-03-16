using CmsFetchService.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CmsFetchService.Core.Entities
{
    [Index(nameof(Id), IsUnique = true)]
    public class CmsRecord
    {
        [Key]
        public required string Id { get; set; }
        public string? Payload { get; set; }
        public int Version { get; set; }

        public bool IsPublished { get; set; }
        public bool IsManuallyDisabled { get; set; } = false;
        public DateTimeOffset LastUpdated { get; set; }
    }
}
