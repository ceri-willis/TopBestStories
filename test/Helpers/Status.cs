namespace TopHackerNewsStories.Helpers
{
    public class Status<T> : IStatus<T>
    {
        public T? Item { get; }

        public bool Ok { get; }

        public string Message { get; }


        public Status(T item)
        {
            Item = item;
            Ok = true;
            Message = string.Empty;
        }
        public Status(string message)
        {
            Item = default;
            Ok = false;
            Message = message;
        }
        public override string ToString()
        {
            return $"Status: {Ok} [{Item} {Message}]";
        }
    }
    public static class StatusExtensions
    {
        public static IStatus<T> AsStatusOk<T>(this T item)
        {
            return new Status<T>(item);
        }
        public static IStatus<T> AsStatusError<T>(this string message)
        {
            return new Status<T>(message);
        }
    }

}
