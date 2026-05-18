using System.Reflection;
using System.Text.Json;

public class TrayAppContext : ApplicationContext
{
    private readonly NotifyIcon _icon;
    private readonly ToolStripMenuItem _exitItem;
    private readonly ConnectionStatusForm _statusForm;
    private readonly WebSocketClient _ws;
    private readonly ServerMessageHandler _messageHandler;
    private readonly string? _description;

    public TrayAppContext(AppConfig config)
    {
        _description = config.Description;

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

        _messageHandler = new ServerMessageHandler(
            new LauncherService(config.VrcLauncherPath),
            _statusForm
        );

        _ws = new WebSocketClient(config.WebSocketUrl);
        _ws.StateChanged += OnStateChanged;
        _ws.MessageReceived += _messageHandler.Handle;

        Task.Run(() => _ws.StartAsync());
    }

    private void OnStateChanged(ConnectionStateChange change)
    {
        void Update()
        {
            switch (change.State)
            {
                case ConnectionState.Connecting:
                    _statusForm.UpdateStatus("接続中…");
                    break;

                case ConnectionState.Connected:
                    _statusForm.UpdateStatus("接続完了");
                    SendAgentInfo();
                    _ = Task.Delay(3000).ContinueWith(_ =>
                    {
                        if (_statusForm.IsDisposed) return;
                        if (_statusForm.InvokeRequired)
                            _statusForm.Invoke(() => _statusForm.Hide());
                        else
                            _statusForm.Hide();
                    });
                    break;

                case ConnectionState.Reconnecting:
                    _statusForm.UpdateStatus(
                    $"再接続待機中 ({change.RetrySeconds}s)"
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

    private void SendAgentInfo()
    {
        var message = new Message<AgentInfo>
        {
            Command = Command.AgentInfo,
            Body = AgentInfo.Collect(_description)
        };

        var json = JsonSerializer.Serialize(message, Json.Options);

        _ = _ws.SendAsync(json);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _ws.Dispose();
            _icon.Visible = false;
            _icon.ContextMenuStrip?.Dispose();
            _icon.Dispose();
            _statusForm.Dispose();
        }
        base.Dispose(disposing);
    }
}
