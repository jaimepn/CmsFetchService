using CmsFetchService.Core.Application;
using CmsFetchService.Core.Models;
using CmsFetchService.Infrastructure.Auth;
using CmsFetchService.Infrastructure.Persistence;
using CmsFetchService.Infrastructure.Persistence.Repository;
using CmsFetchService.Infrastructure.Queue;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Join(AppContext.BaseDirectory, "cms.db");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options => { 
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    // add request examples
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (context.JsonTypeInfo.Type == typeof(CmsEventDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("item_123"),
                ["type"] = new OpenApiString(CmsEventType.Update.ToString()),
                ["version"] = new OpenApiInteger(1),
                ["timestamp"] = new OpenApiString(DateTimeOffset.UtcNow.ToString("O")),
                ["payload"] = new OpenApiString("{\"title\": \"Test\"}")
            };
        }
        return Task.CompletedTask;
    });

    // add auth section
    options.AddDocumentTransformer((document, context, ct) => 
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "basic",
            Description = "Enter your credentials"
        };
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("BasicAuth", scheme);
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "BasicAuth"
                    }
                },
                Array.Empty<string>()
            }
        });
        document.Info.Description = """
            ## Authentication
            Use the **Authorize** button to test endpoints:
            - **CMS:** `cms` / `cmspass`
            - **Admin:** `admin` / `adminpass`
            - **User:** `user` / `userpass`
            """;

        return Task.CompletedTask;
    });
});

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuthentication", null);

builder.Services.AddSingleton<ICmsQueue, CmsQueue>();
builder.Services.AddHostedService<CmsEventProcessor>();
builder.Services.AddScoped<ICmsRepository, CmsRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
