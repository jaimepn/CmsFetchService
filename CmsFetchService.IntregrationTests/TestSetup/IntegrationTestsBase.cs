namespace CmsFetchService.IntegrationTests.TestSetup;

using CmsFetchService.API.Controllers;
using CmsFetchService.Core.Entities;
using CmsFetchService.Core.Models;
using CmsFetchService.Infrastructure.Auth;
using CmsFetchService.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Http.Json;

public class IntegrationTestsBase : WebApplicationFactory<CmsFetchService.API.Controllers.CmsRecordsController>
{

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
   
        builder.ConfigureServices(services =>
        {

            services.AddControllers()
                .AddApplicationPart(typeof(WebHookController).Assembly).AddControllersAsServices(); ;

            var appDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (appDescriptor != null) services.Remove(appDescriptor);

            var readDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ReadOnlyDbContext>));
            if (readDescriptor != null) services.Remove(readDescriptor);

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_dbPath}");
            });

            services.AddDbContext<ReadOnlyDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_dbPath}")
                       .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.EnsureCreated();

            services.AddAuthentication("Basic")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", null);
        });
    }

    public HttpClient CreateAuthenticatedClient(string user, string pass)
    {
        var client = CreateClient();
        var authVal = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{user}:{pass}"));
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authVal);
        return client;
    }

    public async Task CmsEventsSend(List<CmsEventDto> events)
    {
        var cmsClient = CreateAuthenticatedClient("cms", "cmspass");
        var hookResponse = await cmsClient.PostAsJsonAsync("/cms", events);
        hookResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    public async Task<CmsRecord> TestPublishedRecordExists(string id, int version)
    {
        CmsRecord? found = null;
        var userClient = CreateAuthenticatedClient("user", "userpass");
        for (int i = 0; i < 10; i++)
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

    public async Task<CmsRecord> TestRecordExists(string id, int version)
    {
        CmsRecord? found = null;
        var userClient = CreateAuthenticatedClient("admin", "adminpass");
        for (int i = 0; i < 10; i++)
        {
            using var response = await userClient.GetAsync("/api/cmsrecords/records");
            var content = await response.Content.ReadFromJsonAsync<List<CmsRecord>>();
            found = content?.FirstOrDefault(x => x.Id == id && x.Version == version);
            if (found != null) break;
            await Task.Delay(100);
        }
        found.Should().NotBeNull();
        found.Version.Should().Be(version);
        return found!;
    }

    public static CmsEventDto CreateWebhookPayload(string id, int version, CmsEventType eventType)
    {
        return new CmsEventDto
        {
            Id = id,
            Type = eventType,
            Version = version,
            Payload = "{\"title\": \"Test\"}"
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Server?.Dispose();
            SqliteConnection.ClearAllPools();
            foreach (var file in new[] { _dbPath, _dbPath + "-wal", _dbPath + "-shm" })
                if (File.Exists(file)) File.Delete(file);
        }
        base.Dispose(disposing);
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.CmsRecordEntries.RemoveRange(db.CmsRecordEntries);
        await db.SaveChangesAsync();
    }
}
