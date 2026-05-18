public class AppConfig
{
    public string? Description { get; set; } = null;
    public string WebSocketUrl { get; set; } = "ws://127.0.0.1:8080/ws/agent";
    public string VrcLauncherPath { get; set; } = @"C:\Program Files (x86)\Steam\steamapps\common\VRChat\launch.exe";
}
