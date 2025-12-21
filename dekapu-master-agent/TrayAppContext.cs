using System.Text.Json;

public class TrayAppContext : ApplicationContext
{
    private readonly NotifyIcon _icon;
    private readonly ToolStripMenuItem _exitItem;
    private readonly ConnectionStatusForm _statusForm;
    private readonly LauncherService _launcherService;

    public TrayAppContext(AppConfig config)
    {
        _launcherService = new LauncherService(config.VrcLauncherPath);

        _exitItem = new ToolStripMenuItem("終了", null, (_, _) => ExitThread());

        _icon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip
            {
                Items = { _exitItem }
            }
        };

        _icon.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                // Nothing todo
            }
        };

        _statusForm = new ConnectionStatusForm();
        _statusForm.Show();

        var ws = new WebSocketClientService(
            config.WebSocketUrl,
            config.Description,
            OnMessage,
            OnConnected,
            OnDisconnected
        );

        Task.Run(() => ws.StartAsync(UpdateStatus));
    }

    private void UpdateStatus(string text)
    {
        _statusForm.UpdateStatus(text);
    }

    private void OnConnected()
    {
        if (_statusForm.InvokeRequired)
        {
            _statusForm.Invoke(() => _statusForm.Hide());
        }
        else
        {
            _statusForm.Hide();
        }
    }

    private void OnDisconnected()
    {
        if (_statusForm.InvokeRequired)
        {
            _statusForm.Invoke(() =>
            {
                if (!_statusForm.Visible)
                    _statusForm.Show();
            });
        }
        else
        {
            if (!_statusForm.Visible)
                _statusForm.Show();
        }
    }


    private void OnMessage(string json)
    {
        var options =
            JsonSerializer.Deserialize<LaunchOptions>(json, Json.Options);

        if (options == null)
            return;

        Task.Run(() =>
        {
            _launcherService.ConfirmAndLaunch(options);
        });
    }


    protected override void Dispose(bool disposing)
    {
        _icon.Visible = false;
        base.Dispose(disposing);
    }
}
