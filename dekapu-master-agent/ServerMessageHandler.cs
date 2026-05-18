using System.Text.Json;

public class ServerMessageHandler(LauncherService launcherService, Control uiContext)
{
    private readonly LauncherService _launcherService = launcherService;
    private readonly Control _uiContext = uiContext;

    public void Handle(string message)
    {
        Message<JsonElement>? msg;
        try
        {
            msg = JsonSerializer.Deserialize<Message<JsonElement>>(message, Json.Options);
        }
        catch (JsonException)
        {
            return;
        }
        if (msg is null) return;

        switch (msg.Command)
        {
            case Command.Launch:
                HandleLaunch(msg.Body);
                break;
            case Command.Shutdown:
                SystemControlService.Shutdown();
                break;
            case Command.Restart:
                SystemControlService.Restart();
                break;
        }
    }

    private void HandleLaunch(JsonElement? body)
    {
        if (body is null) return;

        LaunchOptions? options;
        try
        {
            options = body.Value.Deserialize<LaunchOptions>(Json.Options);
        }
        catch (JsonException)
        {
            return;
        }
        if (options is null) return;

        _ = Task.Run(() =>
        {
            try
            {
                bool confirmed = options.DirectLaunch || ConfirmOnUiThread(options);
                if (confirmed)
                {
                    _launcherService.Launch(options);
                }
            }
            catch
            {
            }
        });
    }

    private bool ConfirmOnUiThread(LaunchOptions options)
    {
        if (!_uiContext.InvokeRequired)
            return _launcherService.Confirm(options);

        return (bool)_uiContext.Invoke(() => _launcherService.Confirm(options));
    }
}
