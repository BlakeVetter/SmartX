using System.Net.Http.Json;
using Smart.Shared;

namespace SmartX.Client.Services;

public class SensorApiClient
{
    private readonly HttpClient _http;
    public SensorApiClient(HttpClient http) => _http = http;

    // --- Sensors ---

    public async Task<bool> RegisterSensorAsync(SensorRegistration reg)
    {
        var resp = await _http.PostAsJsonAsync("api/sensors/register", reg);
        return resp.IsSuccessStatusCode;
    }

    public async Task<List<SensorRegistration>> GetSensorsAsync()
        => await _http.GetFromJsonAsync<List<SensorRegistration>>("api/sensors") ?? new();

    // ✅ NEW: Update an existing sensor
    public async Task<bool> UpdateSensorAsync(string mac, SensorRegistration reg)
    {
        var resp = await _http.PutAsJsonAsync($"api/sensors/{mac}", reg);
        return resp.IsSuccessStatusCode;
    }

    // ✅ NEW: Delete a sensor
    public async Task<bool> DeleteSensorAsync(string mac)
    {
        var resp = await _http.DeleteAsync($"api/sensors/{mac}");
        return resp.IsSuccessStatusCode;
    }

    // --- Telemetry ---

    public async Task<List<TelemetryPacket<float>>> GetRecentTelemetryAsync()
        => await _http.GetFromJsonAsync<List<TelemetryPacket<float>>>("api/telemetry/recent") ?? new();

    public async Task<bool> SendTelemetryAsync(TelemetryPacket<float> packet)
    {
        var resp = await _http.PostAsJsonAsync("api/telemetry/ingest", packet);
        return resp.IsSuccessStatusCode;
    }

    // --- File Upload ---

    public async Task<bool> UploadAttachmentAsync(string mac, Stream stream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(stream), "file", fileName);
        var resp = await _http.PostAsync($"api/sensors/{mac}/attachments", content);
        return resp.IsSuccessStatusCode;
    }
}