public class Message<T>
{
    public Command Command { get; set; } = Command.Unknown;
    public T? Body { get; set; }
}
