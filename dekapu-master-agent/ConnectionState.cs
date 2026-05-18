public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}

public sealed record ConnectionStateChange(
    ConnectionState State,
    int? RetrySeconds
);
