using System.Text.Json;

public static class ConfigLoader
{
    public static AppConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            var defaultConfig = new AppConfig();
            File.WriteAllText(
                path,
                JsonSerializer.Serialize(defaultConfig, Json.IndentedOptions)
            );
            return defaultConfig;
        }

        var text = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppConfig>(text, Json.Options)
               ?? new AppConfig();
    }
}
