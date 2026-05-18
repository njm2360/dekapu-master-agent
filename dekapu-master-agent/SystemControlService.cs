using System.Diagnostics;

public static class SystemControlService
{
    public static void Shutdown() =>
        Process.Start("shutdown", "/s /t 0");

    public static void Restart() =>
        Process.Start("shutdown", "/r /t 0");
}
