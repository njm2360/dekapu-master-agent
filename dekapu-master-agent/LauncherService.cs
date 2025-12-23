using System.Diagnostics;
using System.Text;

public class LauncherService
{
    private readonly string _launcherPath;

    public LauncherService(string launcherPath)
    {
        _launcherPath = launcherPath;
    }

    public void ConfirmAndLaunch(LaunchOptions options)
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
            $"プロファイル: {profileText} \n" +
            $"引数: {args}",
            "起動確認",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Question
        );

        if (result != DialogResult.OK)
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = _launcherPath,
            Arguments = args,
            UseShellExecute = false
        });
    }


    private static string BuildArguments(LaunchOptions options)
    {
        var sb = new StringBuilder();

        // profile
        if (options.Profile.HasValue)
            sb.Append($"--profile={options.Profile.Value} ");

        // instance
        if (options.Instance != null)
        {
            sb.Append(GetLaunchUrl(options.Instance));
            sb.Append(' ');
        }

        // basic
        if (options.NoVr)
            sb.Append("--no-vr ");

        if (options.Fps.HasValue)
            sb.Append($"--fps={options.Fps.Value} ");

        if (!string.IsNullOrWhiteSpace(options.Midi))
            sb.Append($"--midi={options.Midi} ");

        if (options.Osc != null)
        {
            sb.Append(
                $"--osc={options.Osc.InPort}:{options.Osc.OutIp}:{options.Osc.OutPort} ");
        }

        // performance
        if (!string.IsNullOrWhiteSpace(options.Affinity))
            sb.Append($"--affinity={options.Affinity} ");

        if (options.ProcessPriority.HasValue)
            sb.Append($"--process-priority={options.ProcessPriority.Value} ");

        if (options.MainThreadPriority.HasValue)
            sb.Append($"--main-thread-priority={options.MainThreadPriority.Value} ");

        // debug
        if (options.WatchAvatars)
            sb.Append("--watch-avatars ");

        if (options.WatchWorlds)
            sb.Append("--watch-worlds ");

        if (options.DebugGui)
            sb.Append("--enable-debug-gui ");

        if (options.SdkLogLevels)
            sb.Append("--enable-sdk-log-levels ");

        if (options.UdonDebugLogging)
            sb.Append("--enable-udon-debug-logging ");

        // extra args
        if (options.ExtraArgs != null)
            sb.Append(options.ExtraArgs).Append(' ');

        return sb.ToString().Trim();
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
