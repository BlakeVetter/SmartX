using Smart.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Allow large file uploads
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 20 * 1024 * 1024; // 20MB
});

// CORS so the Blazor client (on a different port) can call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowClient");

// In-memory storage (for Part 1)
var sensors = new List<SensorRegistration>();
var telemetry = new List<TelemetryPacket<float>>();

// Ensure uploads folder exists
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);

// ============================================================
// ENDPOINTS
// ============================================================

app.MapPost("/api/sensors/register", (SensorRegistration reg) =>
{
    if (string.IsNullOrWhiteSpace(reg.DeviceMacAddress))
        return Results.BadRequest("MAC is required.");
    sensors.Add(reg);
    return Results.Ok(new { message = "Sensor registered", total = sensors.Count });
});

app.MapGet("/api/sensors", () => Results.Ok(sensors));

app.MapPost("/api/telemetry/ingest", (TelemetryPacket<float> packet) =>
{
    telemetry.Add(packet);
    return Results.Ok(new { message = "Telemetry received", total = telemetry.Count });
});

app.MapGet("/api/telemetry/recent", (int take = 50) =>
    Results.Ok(telemetry.OrderByDescending(t => t.Timestamp).Take(take).ToList()));

app.MapPost("/api/sensors/{mac}/attachments", async (string mac, HttpRequest request) =>
{
    if (!request.HasFormContentType) return Results.BadRequest("Expected multipart/form-data.");
    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    if (file is null || file.Length == 0) return Results.BadRequest("No file.");

    var safeMac = string.Concat(mac.Where(char.IsLetterOrDigit));
    var savedName = $"{safeMac}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
    var fullPath = Path.Combine(uploadsPath, savedName);

    using (var stream = File.Create(fullPath)) { await file.CopyToAsync(stream); }

    return Results.Ok(new { message = "File uploaded", savedAs = savedName });
});

// Demo endpoint showing operator overloading + recursion
app.MapGet("/api/demo/advanced", () =>
{
    var m1 = new SmartMeter("METER-A", 120.5);
    var m2 = new SmartMeter("METER-B", 80.25);
    var aggregate = m1 + m2;

    var root = new DeploymentNode("Facility");
    var zone1 = new DeploymentNode("Zone 1");
    zone1.Children.Add(new DeploymentNode("Sub-Zone B"));
    root.Children.Add(zone1);

    var isValid = DeploymentNode.ValidateDeploymentPath(root, "Sub-Zone B");

    return Results.Ok(new
    {
        aggregateMeter = aggregate.DeviceMac,
        aggregateWatts = aggregate.Wattage,
        nodeValid = isValid
    });
});

app.Run();