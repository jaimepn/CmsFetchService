using CmsFetchService.Core.Models;
using CmsFetchService.IntegrationTests.TestSetup;
using FluentAssertions;

namespace CmsFetchService.IntegrationTests
{
    public class WebHookControllerTests : IntegrationTestsBase
    {

        [Fact]
        public async Task PublishEvent_Success_PublishesRecord()
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
        public async Task UpdateEvent_NextVersionIncoming_Updates()
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
        public async Task UpdateEvent_PreviousVersionIncoming_Ignores()
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

        [Fact]
        public async Task MultipleOutOfOrderRequests_Accepted_KeepsLatestVersion()
        {
            // Arrange
            await ResetDatabaseAsync();
            var itemId = $"item_{Guid.NewGuid()}";
            var v1 = CreateWebhookPayload(itemId, 1, CmsEventType.Update);
            var v2 = CreateWebhookPayload(itemId, 2, CmsEventType.Update);
            var v3 = CreateWebhookPayload(itemId, 3, CmsEventType.Update);

            // Act
            await Task.WhenAll(
                CmsEventsSend([v3]),
                CmsEventsSend([v1]),
                CmsEventsSend([v2])
            );

            // Assert
            var record = await TestRecordExists(itemId, 3);
            record.Should().NotBeNull();
        }

    }
}
