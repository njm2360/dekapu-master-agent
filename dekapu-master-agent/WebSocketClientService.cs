using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

public class WebSocketClientService
{
    private readonly Uri _uri;
    private readonly Action<string> _onMessage;
    private readonly Action _onConnected;
    private readonly Action _onDisconnected;

    private readonly string? _description;

    private CancellationTokenSource _cts = new();

    public WebSocketClientService(
        string url,
        string? description,
        Action<string> onMessage,
        Action onConnected,
        Action onDisconnected)
    {
        _uri = new Uri(url);
        _description = description;
        _onMessage = onMessage;
        _onConnected = onConnected;
        _onDisconnected = onDisconnected;
    }


    public async Task StartAsync(Action<string> statusCallback)
    {
        int retrySeconds = 1;

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                using var ws = new ClientWebSocket();
                statusCallback("接続中…");
                await ws.ConnectAsync(_uri, _cts.Token);

                retrySeconds = 1;
                _onConnected();

                await SendHelloAsync(ws);

                var buffer = new byte[4096];
                while (ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(buffer, _cts.Token);
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _onMessage(msg);
                }
            }
            catch
            {
                _onDisconnected();
                statusCallback($"再接続待機中（{retrySeconds}s）");
                await Task.Delay(TimeSpan.FromSeconds(retrySeconds), _cts.Token);
                retrySeconds = Math.Min(retrySeconds * 2, 60);
            }
        }
    }

    private async Task SendHelloAsync(ClientWebSocket ws)
    {
        var json = JsonSerializer.Serialize(
            SystemInfo.GetReport(_description),
            Json.Options
        );

        var bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(
            bytes,
            WebSocketMessageType.Text,
            true,
            _cts.Token);
    }

}
