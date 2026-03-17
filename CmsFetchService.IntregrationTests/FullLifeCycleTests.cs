using CmsFetchService.Core.Entities;
using CmsFetchService.Core.Models;
using CmsFetchService.IntegrationTests.TestSetup;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace CmsFetchService.IntegrationTests
{
    public class FullLifeCycleTests : IntegrationTestsBase
    {

        [Fact]
        public async Task PublishEvent_Success_RecordIsPublished()
        {
            // Arrange
            await ResetDatabaseAsync();
            var itemId = $"item_{Guid.NewGuid()}";

            // Act
            await CmsEventsSend([CreateWebhookPayload(itemId, version: 1, CmsEventType.Publish)]);

            // Assert
            await TestPublishedRecordExists(itemId, 1);
        }

        [Fact]
        public async Task UpdateEvent_Success_RecordIsUpdated()
        {
            // Arrange
            await ResetDatabaseAsync();
            var itemId = $"item_{Guid.NewGuid()}";
            await CmsEventsSend([CreateWebhookPayload(itemId, version: 1, CmsEventType.Publish)]);
            await TestPublishedRecordExists(itemId, version: 1);

            // Act
            var v2 = CreateWebhookPayload(itemId, version: 2, CmsEventType.Update);
            v2.Payload = "{\"title\": \"Page Updated\"}";
            await CmsEventsSend([v2]);

            // Assert
            var record = await TestPublishedRecordExists(itemId, version: 2);
            record.Payload.Should().Be(v2.Payload);
        }

        [Fact]
        public async Task UpdateEvent_PreviousVersion_Ignores()
        {
            // Arrange
            await ResetDatabaseAsync();
            var itemId = $"item_{Guid.NewGuid()}";
            await CmsEventsSend([CreateWebhookPayload(itemId, version: 2, CmsEventType.Publish)]);
            await TestPublishedRecordExists(itemId, version: 2);

            // Act
            await CmsEventsSend([CreateWebhookPayload(itemId, version: 1, CmsEventType.Update)]);

            // Assert
            await TestPublishedRecordExists(itemId, version: 2);
        }

        private static CmsEventDto CreateWebhookPayload(string id, int version, CmsEventType eventType)
        {
            return new CmsEventDto
            {
                Id = id,
                Type = eventType,
                Version = version,
                Payload = "{\"title\": \"Test\"}"
            };
        }

        private async Task CmsEventsSend(List<CmsEventDto> events) 
        {
            var cmsClient = CreateAuthenticatedClient("cms", "cmspass");
            var hookResponse = await cmsClient.PostAsJsonAsync("/cms", events);
            hookResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        private async Task<CmsRecord> TestPublishedRecordExists(string id, int version)
        {
            CmsRecord? found = null;
            var userClient = CreateAuthenticatedClient("user", "userpass");
            for (int i = 0; i < 20; i++)
            {
                using var response = await userClient.GetAsync("/api/cmsrecords/content");
                var content = await response.Content.ReadFromJsonAsync<List<CmsRecord>>();
                found = content?.FirstOrDefault(x => x.Id == id && x.Version == version);
                if (found != null) break;
                await Task.Delay(100);
            }
            found.Should().NotBeNull();
            found.Version.Should().Be(version);
            found.IsPublished.Should().BeTrue();
            return found!;
        }


    }
}
