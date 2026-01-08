using System.Net.WebSockets;
using System.Text;

public sealed class WebSocketClient(string url)
{
    private readonly Uri _uri = new(url);
    private readonly CancellationTokenSource _cts = new();

    private ClientWebSocket? _ws;

    public event Action<string>? MessageReceived;
    public event Action<ConnectionState, int?>? StateChanged;

    public async Task StartAsync()
    {
        int retrySeconds = 1;

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                _ws = new ClientWebSocket();

                StateChanged?.Invoke(ConnectionState.Connecting, null);
                await _ws.ConnectAsync(_uri, _cts.Token);

                retrySeconds = 1;
                StateChanged?.Invoke(ConnectionState.Connected, null);

                var buffer = new byte[4096];
                while (_ws.State == WebSocketState.Open)
                {
                    var result = await _ws.ReceiveAsync(buffer, _cts.Token);
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    MessageReceived?.Invoke(msg);
                }
            }
            catch
            {
                StateChanged?.Invoke(
                ConnectionState.Reconnecting,
                retrySeconds
                );

                await Task.Delay(
                TimeSpan.FromSeconds(retrySeconds),
                _cts.Token
                );

                retrySeconds = Math.Min(retrySeconds * 2, 60);
            }
            finally
            {
                _ws?.Dispose();
                _ws = null;
            }
        }
    }

    public async Task SendAsync(string message)
    {
        if (_ws == null || _ws.State != WebSocketState.Open)
            return;

        var bytes = Encoding.UTF8.GetBytes(message);
        await _ws.SendAsync(
        bytes,
        WebSocketMessageType.Text,
        true,
        _cts.Token
        );
    }

    public void Stop()
    {
        _cts.Cancel();
    }
}
