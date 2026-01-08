using System.Reflection;
using System.Text.Json;

public class TrayAppContext : ApplicationContext
{
    private readonly NotifyIcon _icon;
    private readonly ToolStripMenuItem _exitItem;
    private readonly ConnectionStatusForm _statusForm;
    private readonly LauncherService _launcherService;
    private readonly WebSocketClient _ws;
    private readonly string? _description;

    public TrayAppContext(AppConfig config)
    {
        _description = config.Description;
        _launcherService = new LauncherService(config.VrcLauncherPath);

        _exitItem = new ToolStripMenuItem("終了", null, (_, _) => ExitThread());

        var asm = Assembly.GetExecutingAssembly();
        using var stream =
        asm.GetManifestResourceStream("dekapu_master_agent.icon.ico")
        ?? throw new InvalidOperationException("icon resource not found");

        _icon = new NotifyIcon
        {
            Icon = new Icon(stream),
            Text = "dekapu-master-agent",
            Visible = true,
            ContextMenuStrip = new ContextMenuStrip
            {
                Items = { _exitItem }
            }
        };

        _statusForm = new ConnectionStatusForm();
        _statusForm.Show();

        _ws = new WebSocketClient(config.WebSocketUrl);
        _ws.StateChanged += OnStateChanged;
        _ws.MessageReceived += OnMessage;

        Task.Run(() => _ws.StartAsync());
    }

    private void OnStateChanged(ConnectionState state, int? retry)
    {
        void Update()
        {
            switch (state)
            {
                case ConnectionState.Connecting:
                    _statusForm.UpdateStatus("接続中…");
                    break;

                case ConnectionState.Connected:
                    _statusForm.Hide();
                    SendAgentInfo();
                    break;

                case ConnectionState.Reconnecting:
                    _statusForm.UpdateStatus(
                    $"再接続待機中 ({retry}s)"
                    );
                    if (!_statusForm.Visible)
                        _statusForm.Show();
                    break;

                case ConnectionState.Disconnected:
                    if (!_statusForm.Visible)
                        _statusForm.Show();
                    break;
            }
        }

        if (_statusForm.InvokeRequired)
            _statusForm.Invoke(Update);
        else
            Update();
    }

    private void OnMessage(string message)
    {
        var options =
        JsonSerializer.Deserialize<LaunchOptions>(message, Json.Options);

        if (options == null)
            return;

        Task.Run(() =>
        {
            if (options.DirectLaunch || _launcherService.Confirm(options))
            {
                _launcherService.Launch(options);
            }
        });
    }

    private void SendAgentInfo()
    {
        var json = JsonSerializer.Serialize(
        SystemInfo.GetReport(_description),
        Json.Options
        );

        _ = _ws.SendAsync(json);
    }

    protected override void Dispose(bool disposing)
    {
        _ws.Stop();
        _icon.Visible = false;
        base.Dispose(disposing);
    }
}
