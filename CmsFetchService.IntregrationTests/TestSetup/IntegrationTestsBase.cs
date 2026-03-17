namespace CmsFetchService.IntegrationTests.TestSetup;

using CmsFetchService.Infrastructure.Auth;
using CmsFetchService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


public class IntegrationTestsBase : WebApplicationFactory<CmsFetchService.API.Controllers.CmsRecordsController>
{

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
   
        builder.ConfigureServices(services =>
        {

            services.AddControllers()
                .AddApplicationPart(typeof(CmsFetchService.API.Controllers.WebHookController).Assembly).AddControllersAsServices(); ;

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
