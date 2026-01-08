using System.Diagnostics;

public class LauncherService(string launcherPath)
{
    private readonly string _launcherPath = launcherPath;

    public bool Confirm(LaunchOptions options)
    {
        var args = BuildArguments(options);

        string instanceText =
            options.Instance != null
                ? options.Instance.DisplayLabel
                : "未指定";

        string profileText =
            options.Profile.HasValue
                ? options.Profile.Value.ToString()
                : "0";

        var result = MessageBox.Show(
            $"以下の内容で起動します。\n\n" +
            $"インスタンス: {instanceText}\n" +
            $"プロファイル: {profileText}\n" +
            $"引数: {args}",
            "起動確認",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Question
        );

        return result == DialogResult.OK;
    }

    public void Launch(LaunchOptions options)
    {
        if (IsLauncherRunning())
        {
            MessageBox.Show(
                "ランチャーが起動中です。",
                "起動エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        var args = BuildArguments(options);

        Process.Start(new ProcessStartInfo
        {
            FileName = _launcherPath,
            Arguments = args,
            UseShellExecute = false
        });
    }

    private static string BuildArguments(LaunchOptions options)
    {
        var args = new List<string>();

        // profile
        if (options.Profile.HasValue)
            args.Add($"--profile={options.Profile.Value}");

        // instance
        if (options.Instance != null)
            args.Add(GetLaunchUrl(options.Instance));

        // basic
        if (options.NoVr)
            args.Add("--no-vr");

        if (options.Fps.HasValue)
            args.Add($"--fps={options.Fps.Value}");

        if (!string.IsNullOrWhiteSpace(options.Midi))
            args.Add($"--midi={options.Midi}");

        if (options.Osc != null)
            args.Add($"--osc={options.Osc.InPort}:{options.Osc.OutIp}:{options.Osc.OutPort}");

        // performance
        if (!string.IsNullOrWhiteSpace(options.Affinity))
            args.Add($"--affinity={options.Affinity}");

        if (options.ProcessPriority.HasValue)
            args.Add($"--process-priority={options.ProcessPriority.Value}");

        if (options.MainThreadPriority.HasValue)
            args.Add($"--main-thread-priority={options.MainThreadPriority.Value}");

        // debug
        if (options.WatchAvatars)
            args.Add("--watch-avatars");

        if (options.WatchWorlds)
            args.Add("--watch-worlds");

        if (options.DebugGui)
            args.Add("--enable-debug-gui");

        if (options.SdkLogLevels)
            args.Add("--enable-sdk-log-levels");

        if (options.UdonDebugLogging)
            args.Add("--enable-udon-debug-logging");

        // extra args
        if (!string.IsNullOrWhiteSpace(options.ExtraArgs))
            args.Add(options.ExtraArgs);

        return string.Join(' ', args);
    }

    private static string GetLaunchUrl(InstanceInfo instance)
    {
        return $"vrchat://launch?id={instance.Id}";
    }

    private bool IsLauncherRunning()
    {
        var targetExe = Path.GetFileName(_launcherPath);

        return Process.GetProcessesByName(
            Path.GetFileNameWithoutExtension(targetExe)
        ).Length != 0;
    }
}
