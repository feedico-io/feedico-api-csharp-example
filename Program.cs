using System.Text.Json;

namespace FeedicoApiExample;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Env.LoadDotEnv(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".env"));
        Env.LoadDotEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

        var token = Env.Get("FEEDICO_API_TOKEN");
        if (string.IsNullOrWhiteSpace(token))
        {
            Console.Error.WriteLine("Set FEEDICO_API_TOKEN in .env (see .env.example).");
            return 1;
        }

        var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "both";
        var page = Env.GetInt("FEEDICO_PAGE", 1);
        var pageSize = Env.GetInt("FEEDICO_PAGE_SIZE", 10);
        var provider = Env.Get("FEEDICO_PROVIDER");
        var firmName = Env.Get("FEEDICO_FIRM_NAME");

        using var client = new FeedicoClient(token);

        try
        {
            if (mode is "merchants" or "both")
                await PrintMerchantsAsync(client, page, pageSize, provider, firmName);

            if (mode is "coupons" or "both")
            {
                if (mode is "both")
                    Console.WriteLine();
                await PrintCouponsAsync(client, page, pageSize, provider, firmName);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error: " + ex.Message);
            return 1;
        }

        return 0;
    }

    private static async Task PrintMerchantsAsync(
        FeedicoClient client,
        int page,
        int pageSize,
        string? provider,
        string? firmName)
    {
        using var doc = await client.ListMerchantsAsync(page, pageSize, provider, firmName);
        var rows = FeedicoClient.ExtractRows(doc);
        Console.WriteLine($"Merchants on page {page}: {rows.Count}");
        Console.WriteLine();

        foreach (var row in rows)
        {
            var name = FirstString(row, "firmName", "name", "title") ?? "(no name)";
            var net = FirstString(row, "provider", "network") ?? "";
            var id = FirstString(row, "id", "merchantId") ?? "";
            Console.WriteLine($"- [{id}] {name} ({net})");
        }
    }

    private static async Task PrintCouponsAsync(
        FeedicoClient client,
        int page,
        int pageSize,
        string? provider,
        string? firmName)
    {
        using var doc = await client.ListCouponsAsync(page, pageSize, provider, firmName);
        var rows = FeedicoClient.ExtractRows(doc);
        Console.WriteLine($"Coupons on page {page}: {rows.Count}");
        Console.WriteLine();

        foreach (var row in rows)
        {
            var title = FirstString(row, "title", "description") ?? "(no title)";
            var code = FirstString(row, "couponCode", "code");
            var merchant = FirstString(row, "firmName", "merchantName");
            var ends = FirstString(row, "endsAt", "endDate");

            var line = $"- {title}";
            if (!string.IsNullOrEmpty(code))
                line += $" | code: {code}";
            if (!string.IsNullOrEmpty(merchant))
                line += $" | {merchant}";
            if (!string.IsNullOrEmpty(ends))
                line += $" | until {ends}";
            Console.WriteLine(line);
        }
    }

    private static string? FirstString(JsonElement row, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (row.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
            {
                var s = value.GetString();
                if (!string.IsNullOrWhiteSpace(s))
                    return s;
            }
        }
        return null;
    }
}
