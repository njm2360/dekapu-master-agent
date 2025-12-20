public class LaunchOptions
{
    // Target
    public InstanceInfo? Instance { get; set; }

    // Basic
    public int? Profile { get; set; }
    public bool NoVr { get; set; } = true;
    public int? Fps { get; set; }
    public string? Midi { get; set; }
    public OscConfig? Osc { get; set; }

    // Performance
    public string? Affinity { get; set; }
    public string? ProcessPriority { get; set; }

    // Debug
    public bool WatchWorlds { get; set; } = false;
    public bool WatchAvatars { get; set; } = false;
    public bool DebugGui { get; set; } = false;
    public bool SdkLogLevels { get; set; } = false;
    public bool UdonDebugLogging { get; set; } = false;

    // Extra
    public string[]? ExtraArgs { get; set; }
}
