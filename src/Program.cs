using Microsoft.EntityFrameworkCore;
using MonProjetPFE.Models;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages + Controllers
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// EF Core SQLite (fichier sous /app/data/db.sqlite en conteneur)
var conn = builder.Configuration.GetConnectionString("Default")
           ?? "Data Source=data/db.sqlite";
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite(conn));

// HttpClient + Jenkins service minimaliste
builder.Services.AddHttpClient("jenkins", client => {
    var baseUrl = builder.Configuration["Jenkins:BaseUrl"]?.TrimEnd('/');
    if (!string.IsNullOrWhiteSpace(baseUrl))
        client.BaseAddress = new Uri(baseUrl);
    var user = builder.Configuration["Jenkins:User"];
    var token = builder.Configuration["Jenkins:ApiToken"];
    if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(token))
    {
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{token}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
    }
});

// Build
var app = builder.Build();

// DB init (dev local): crée le fichier si absent
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseStaticFiles();
app.MapRazorPages();

// API minimal pour déclencher/poller Jenkins
app.MapPost("/api/workflows/{actionName}", async (
    string actionName, HttpContext http, IHttpClientFactory f, IConfiguration cfg) =>
{
    var client = f.CreateClient("jenkins");
    var job = cfg[$"Jenkins:Jobs:{actionName}"];
    if (string.IsNullOrWhiteSpace(job))
        return Results.NotFound(new { error = "Unknown action/job" });

    Dictionary<string, string>? parameters = null;
    if (http.Request.ContentLength > 0)
        parameters = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(http.Request.Body);

    var url = (parameters is { Count: > 0 })
        ? $"/job/{Uri.EscapeDataString(job)}/buildWithParameters"
        : $"/job/{Uri.EscapeDataString(job)}/build";

    var resp = await client.PostAsync(url, parameters is { Count: > 0 }
        ? new FormUrlEncodedContent(parameters) : null);
    if (!resp.IsSuccessStatusCode)
        return Results.Problem($"Jenkins trigger failed: {resp.StatusCode}");

    // Poll queue item to get build number
    if (resp.Headers.Location is null) return Results.Ok(new { job, buildNumber = (int?)null });
    var queueUrl = resp.Headers.Location.ToString() + "api/json";

    for (int i = 0; i < 30; i++)
    {
        var json = await client.GetStringAsync(queueUrl);
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("executable", out var exec) &&
            exec.TryGetProperty("number", out var num))
            return Results.Ok(new { job, buildNumber = num.GetInt32() });
        await Task.Delay(1000);
    }
    return Results.Ok(new { job, buildNumber = (int?)null });
});

app.MapGet("/api/workflows/{actionName}/{buildNumber:int}/status", async (
    string actionName, int buildNumber, IHttpClientFactory f, IConfiguration cfg) =>
{
    var client = f.CreateClient("jenkins");
    var job = cfg[$"Jenkins:Jobs:{actionName}"];
    if (string.IsNullOrWhiteSpace(job))
        return Results.NotFound(new { error = "Unknown action/job" });

    var json = await client.GetStringAsync($"/job/{Uri.EscapeDataString(job)}/{buildNumber}/api/json");
    using var doc = JsonDocument.Parse(json);
    var resultProp = doc.RootElement.TryGetProperty("result", out var res) ? res.GetString() : null;
    var status = resultProp ?? "RUNNING";
    return Results.Ok(new { job, buildNumber, status });
});

app.Run();
