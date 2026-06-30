using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FeedicoApiExample;

/// <summary>
/// Minimal Feedico REST client — merchants (networks) and coupons.
/// </summary>
public sealed class FeedicoClient : IDisposable
{
    private const string ApiRoot = "https://api.feedico.io/api/v1";

    private readonly HttpClient _http;
    private readonly string _token;

    public FeedicoClient(string token, HttpClient? http = null)
    {
        _token = token.Trim();
        if (_token.Length == 0)
            throw new ArgumentException("API token is required.", nameof(token));

        _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(90) };
    }

    public Task<JsonDocument> ListMerchantsAsync(
        int page = 1,
        int pageSize = 50,
        string? provider = null,
        string? firmName = null,
        CancellationToken cancellationToken = default)
        => PostAsync($"{ApiRoot}/me/networks", ListBody(page, pageSize, provider, firmName), cancellationToken);

    public Task<JsonDocument> ListCouponsAsync(
        int page = 1,
        int pageSize = 50,
        string? provider = null,
        string? firmName = null,
        CancellationToken cancellationToken = default)
        => PostAsync($"{ApiRoot}/me/coupons", ListBody(page, pageSize, provider, firmName), cancellationToken);

    public static List<JsonElement> ExtractRows(JsonDocument document)
    {
        var root = document.RootElement;
        if (root.ValueKind == JsonValueKind.Array)
            return root.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.Object).ToList();

        foreach (var key in new[] { "networks", "coupons", "items" })
        {
            if (root.TryGetProperty(key, out var nested) && nested.ValueKind == JsonValueKind.Array)
                return nested.EnumerateArray().Where(e => e.ValueKind == JsonValueKind.Object).ToList();
        }

        return new List<JsonElement>();
    }

    private async Task<JsonDocument> PostAsync(string url, object body, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(raw);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"Invalid JSON from Feedico (HTTP {(int)response.StatusCode}).");
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = TryReadError(document) ?? $"HTTP {(int)response.StatusCode}";
            document.Dispose();
            throw new InvalidOperationException(message);
        }

        return document;
    }

    private static string? TryReadError(JsonDocument document)
    {
        var root = document.RootElement;
        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
            return error.GetString();
        if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
            return message.GetString();
        return null;
    }

    private static object ListBody(int page, int pageSize, string? provider, string? firmName) => new
    {
        page = Math.Max(1, page),
        pageSize = Math.Clamp(pageSize, 1, 500),
        provider,
        firmName = firmName ?? string.Empty,
    };

    public void Dispose() => _http.Dispose();
}
