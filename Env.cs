namespace FeedicoApiExample;

internal static class Env
{
    public static void LoadDotEnv(string path)
    {
        if (!File.Exists(path))
            return;

        foreach (var line in File.ReadAllLines(path))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
                continue;

            var eq = trimmed.IndexOf('=');
            if (eq <= 0)
                continue;

            var key = trimmed[..eq].Trim();
            var value = trimmed[(eq + 1)..].Trim();
            if (key.Length == 0 || Environment.GetEnvironmentVariable(key) is not null)
                continue;

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    public static string? Get(string key, string? defaultValue = null)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return value.Trim();
    }

    public static int GetInt(string key, int defaultValue)
    {
        var raw = Get(key);
        return int.TryParse(raw, out var n) ? n : defaultValue;
    }
}
