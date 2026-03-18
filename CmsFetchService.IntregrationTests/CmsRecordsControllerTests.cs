using CmsFetchService.Core.Entities;
using CmsFetchService.Core.Models;
using CmsFetchService.IntegrationTests.TestSetup;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace CmsFetchService.IntegrationTests
{
    public class CmsRecordsControllerTests : IntegrationTestsBase
    {
        [Theory]
        [InlineData("user", "userpass")]
        [InlineData("cms", "cmspass")]
        public async Task GetAllRecords_NotAdmin_Forbidden(string userName, string password)
        {
            // Arrange / Act
            var client = CreateAuthenticatedClient(userName, password);
            var response = await client.DeleteAsync($"/api/cmsrecords/records");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetAllRecords_WrongCredentials_Unauthorized()
        {
            // Arrange / Act
            var client = CreateAuthenticatedClient("admin", "wrong-password");
            var response = await client.DeleteAsync($"/api/cmsrecords/records");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteRecord_Success_RemovesFromDb()
        {
            // Arrange
            await ResetDatabaseAsync();
            var itemId = $"item_{Guid.NewGuid()}";
            await CmsEventsSend([CreateWebhookPayload(itemId, version: 1, CmsEventType.Update)]);
            await TestRecordExists(itemId, version: 1);

            // Act
            var client = CreateAuthenticatedClient("admin", "adminpass");
            var response = await client.DeleteAsync($"/api/cmsrecords/{itemId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            var retryResponse = await client.DeleteAsync($"/api/cmsrecords/{itemId}");
            retryResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetUserContent_RecordIsDisabledByAdmin_DoesntReturn()
        {
            // Arrange
            await ResetDatabaseAsync();
            var itemId = $"item_{Guid.NewGuid()}";
            await CmsEventsSend([CreateWebhookPayload(itemId, version: 1, CmsEventType.Update)]);
            await TestRecordExists(itemId, version: 1);

            var client = CreateAuthenticatedClient("admin", "adminpass");
            var disableResponse = await client.PatchAsync($"/api/cmsrecords/{itemId}/disable", null);
            disableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            var response = await client.GetAsync($"/api/cmsrecords/content");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<List<CmsRecord>>();
            content.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAdminContent_RecordIsDisabledByAdmin_Returns()
        {
            // Arrange
            await ResetDatabaseAsync();
            var itemId = $"item_{Guid.NewGuid()}";
            await CmsEventsSend([CreateWebhookPayload(itemId, version: 1, CmsEventType.Update)]);
            await TestRecordExists(itemId, version: 1);

            var client = CreateAuthenticatedClient("admin", "adminpass");
            var disableResponse = await client.PatchAsync($"/api/cmsrecords/{itemId}/disable", null);
            disableResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act
            var response = await client.GetAsync($"/api/cmsrecords/records");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadFromJsonAsync<List<CmsRecord>>();
            content.Should().Contain(x => x.Id == itemId && x.IsManuallyDisabled == true);
        }


    }
}
