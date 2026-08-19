using System;
using Microsoft.Extensions.Caching.Memory;
using Munters.Server.Services;
using Munters.Server.Models;

var builder = WebApplication.CreateBuilder(args);

// Ensure console logging is available during startup
builder.Logging.AddConsole();

// Bind to container port 8080 (HTTP) on all interfaces. Skip HTTPS in container to avoid
// requiring a server certificate inside the container. For production, configure a real cert
// and enable HTTPS (or terminate HTTPS at the load balancer).
builder.WebHost.UseUrls("http://0.0.0.0:8080");

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
// We will expose a minimal OpenAPI document at /openapi for the Swagger UI to consume.

// Log whether the Giphy API key is present in configuration (do not print the key)
var giphyKeyPresent = !string.IsNullOrEmpty(builder.Configuration["Giphy:ApiKey"]);
Console.WriteLine($"GIPHY API KEY PRESENT: {giphyKeyPresent}");

// Bind Giphy options from configuration so IOptions<GiphyOptions> is populated
builder.Services.Configure<GiphyOptions>(builder.Configuration.GetSection("Giphy"));

// Read resolved Giphy options now for startup logging and client config
var giphyOptions = builder.Configuration.GetSection("Giphy").Get<GiphyOptions>() ?? new GiphyOptions();
var resolvedBaseUrl = string.IsNullOrWhiteSpace(giphyOptions.BaseUrl) ? "https://api.giphy.com/" : giphyOptions.BaseUrl;
// Use the configured logger to emit startup information
var startupLoggerFactory = LoggerFactory.Create(lb => lb.AddConsole());
var startupLogger = startupLoggerFactory.CreateLogger("Startup");
startupLogger.LogInformation("Starting Munters.Server in environment: {Env}", builder.Environment.EnvironmentName);
startupLogger.LogInformation("GIPHY API KEY PRESENT: {Present}", !string.IsNullOrEmpty(builder.Configuration["Giphy:ApiKey"]));
startupLogger.LogInformation("GIPHY BaseUrl resolved to: {BaseUrl}", resolvedBaseUrl);

// Register Typed HttpClient for GiphyApiClient
builder.Services.AddHttpClient<GiphyApiClient>(client =>
{
    // Use the bound GiphyOptions.BaseUrl when configuring the typed HttpClient
    var giphyBase = resolvedBaseUrl;
    client.BaseAddress = new Uri(giphyBase);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Register Decorator Pattern using Dependency Injection
builder.Services.AddScoped<IGiphyService>(provider =>
{
    var apiClient = provider.GetRequiredService<GiphyApiClient>();
    var memoryCache = provider.GetRequiredService<IMemoryCache>();
    var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Munters.Server.Services.CachedGiphyService>>();

    // expects appsettings.json entry like "Giphy": { "CacheDurationMinutes": "5" }
    var cfg = provider.GetRequiredService<IConfiguration>();
    var minutes = cfg.GetValue<int?>("Giphy:CacheDurationMinutes") ?? 5;
    var cacheDuration = TimeSpan.FromMinutes(minutes);

    return new CachedGiphyService(apiClient, memoryCache, cacheDuration, logger);
});

// Configure CORS for local development and for a React client running in a container
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteClient", policy =>
    {
        // Allow origins can be configured via appsettings or environment variable:
        // Cors:AllowedOrigins as an array or Cors:AllowedOrigins as comma-separated string
        var configured = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        var csv = builder.Configuration["Cors:AllowedOrigins"];
        string[] origins;
        if (configured != null && configured.Length > 0)
            origins = configured;
        else if (!string.IsNullOrWhiteSpace(csv))
            origins = csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        else
            origins = new[] { "http://localhost:5173", "http://localhost:3000", "http://host.docker.internal:3000", "http://munters.client:80" };

        // Log resolved CORS origins
        startupLogger.LogInformation("Resolved CORS allowed origins: {Origins}", string.Join(',', origins));

        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Serve a minimal OpenAPI JSON and Swagger UI (development only)
if (app.Environment.IsDevelopment())
{
    // Minimal OpenAPI document
    app.MapGet("/openapi", () =>
    {
        var paths = new Dictionary<string, object>
        {
            ["/api/giphy/trending"] = new Dictionary<string, object>
            {
                ["get"] = new Dictionary<string, object>
                {
                    ["summary"] = "Fetch trending GIFs",
                    ["responses"] = new Dictionary<string, object>
                    {
                        ["200"] = new Dictionary<string, object> { ["description"] = "OK" }
                    }
                }
            },
            ["/api/giphy/search"] = new Dictionary<string, object>
            {
                ["get"] = new Dictionary<string, object>
                {
                    ["summary"] = "Search GIFs",
                    ["parameters"] = new[] { new Dictionary<string, object> { ["name"] = "q", ["in"] = "query", ["required"] = true, ["schema"] = new Dictionary<string, object> { ["type"] = "string" } } },
                    ["responses"] = new Dictionary<string, object>
                    {
                        ["200"] = new Dictionary<string, object> { ["description"] = "OK" }
                    }
                }
            }
        };

        var doc = new Dictionary<string, object>
        {
            ["openapi"] = "3.0.0",
            ["info"] = new Dictionary<string, object> { ["title"] = "Munters API", ["version"] = "v1" },
            ["paths"] = paths
        };

        return Results.Json(doc);
    });

    // Serve Swagger UI (swagger-ui bundled from CDN) at /swagger
    app.MapGet("/swagger", async ctx =>
    {
        const string html = "<!doctype html>\n<html lang=\"en\">\n  <head>\n    <meta charset=\"utf-8\"/>\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"/>\n    <title>API Docs - Swagger UI</title>\n    <link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/4.18.3/swagger-ui.css\" />\n  </head>\n  <body>\n    <div id=\"swagger-ui\"></div>\n    <script src=\"https://cdnjs.cloudflare.com/ajax/libs/swagger-ui/4.18.3/swagger-ui-bundle.js\"></script>\n    <script>\n      window.onload = function() {\n        const ui = SwaggerUIBundle({\n          url: '/openapi',\n          dom_id: '#swagger-ui',\n          presets: [SwaggerUIBundle.presets.apis],\n          layout: 'BaseLayout'\n        });\n        window.ui = ui;\n      };\n    </script>\n  </body>\n</html>";
        ctx.Response.ContentType = "text/html; charset=utf-8";
        await ctx.Response.WriteAsync(html);
    });

    app.MapGet("/swagger/index.html", async ctx => ctx.Response.Redirect("/swagger", permanent: false));
}

// Log resolved urls and other info at app startup
app.Logger.LogInformation("Application starting. Environment: {Env}", app.Environment.EnvironmentName);
app.Logger.LogInformation("Listening URLs: {Urls}", string.Join(';', app.Urls));
app.Logger.LogInformation("Using Giphy BaseUrl: {BaseUrl}", resolvedBaseUrl);

app.UseCors("AllowViteClient");
app.UseRouting();
app.MapControllers();

app.Run();
