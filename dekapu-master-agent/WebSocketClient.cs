using System.Buffers;
using System.Net.WebSockets;
using System.Text;

public sealed class WebSocketClient(string url) : IDisposable
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);

    private readonly Uri _uri = new(url);
    private readonly CancellationTokenSource _cts = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private ClientWebSocket? _ws;

    public event Action<string>? MessageReceived;
    public event Action<ConnectionStateChange>? StateChanged;

    public async Task StartAsync()
    {
        int retrySeconds = 1;

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                _ws = new ClientWebSocket();

                StateChanged?.Invoke(new ConnectionStateChange(ConnectionState.Connecting, null));

                using (var connectCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token))
                {
                    connectCts.CancelAfter(ConnectTimeout);
                    await _ws.ConnectAsync(_uri, connectCts.Token);
                }

                retrySeconds = 1;
                StateChanged?.Invoke(new ConnectionStateChange(ConnectionState.Connected, null));

                await ReceiveLoopAsync(_ws);
            }
            catch
            {
                if (_cts.IsCancellationRequested) break;

                StateChanged?.Invoke(new ConnectionStateChange(ConnectionState.Reconnecting, retrySeconds));

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(retrySeconds), _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                retrySeconds = Math.Min(retrySeconds * 2, 60);
            }
            finally
            {
                _ws?.Dispose();
                _ws = null;
            }
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket ws)
    {
        while (ws.State == WebSocketState.Open)
        {
            var writer = new ArrayBufferWriter<byte>(initialCapacity: 4096);
            ValueWebSocketReceiveResult result;
            do
            {
                var memory = writer.GetMemory(sizeHint: 4096);
                result = await ws.ReceiveAsync(memory, _cts.Token);
                writer.Advance(result.Count);
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await ws.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    null,
                    _cts.Token
                );
                return;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var msg = Encoding.UTF8.GetString(writer.WrittenSpan);
                MessageReceived?.Invoke(msg);
            }
        }
    }

    public async Task SendAsync(string message)
    {
        var ws = _ws;
        if (ws is null || ws.State != WebSocketState.Open)
            return;

        var bytes = Encoding.UTF8.GetBytes(message);

        await _sendLock.WaitAsync(_cts.Token);
        try
        {
            await ws.SendAsync(
                bytes,
                WebSocketMessageType.Text,
                true,
                _cts.Token
            );
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public void Dispose()
    {
        try
        {
            _cts.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        _ws?.Dispose();
        _cts.Dispose();
        _sendLock.Dispose();
    }
}
