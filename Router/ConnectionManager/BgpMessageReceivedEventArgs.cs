namespace Router
{
    public class BgpMessageReceivedEventArgs<T>
    {
        public int SenderId { get; init; }

        public T BgpMessage { get; init; }
    }
}