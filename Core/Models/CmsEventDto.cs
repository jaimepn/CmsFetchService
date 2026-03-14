using System.Text.Json.Serialization;

namespace CmsFetchService.Core.Models
{
    public class CmsEventDto
    {
        public required string Id { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CmsEventType Type { get; set; }
        public int Version { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string? Payload { get; set; }
    }
}
