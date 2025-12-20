using System.Text.Json;

public class AppConfig
{
    public string? Description { get; set; } = null;
    public string WebSocketUrl { get; set; } = "ws://localhost:8080/api/ws";
    public string VrcLauncherPath { get; set; } = @"C:\Program Files (x86)\Steam\steamapps\common\VRChat\launch.exe";

    public static AppConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            var defaultConfig = new AppConfig();

            var json = JsonSerializer.Serialize(
                defaultConfig,
                new JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(path, json);

            return defaultConfig;
        }

        var text = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppConfig>(text)
               ?? new AppConfig();
    }
}
