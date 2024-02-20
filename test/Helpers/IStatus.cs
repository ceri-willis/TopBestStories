namespace TopHackerNewsStories.Helpers
{
    public interface IStatus<T>
    {
        T? Item { get; }
        bool Ok { get; }
        string Message { get; }
    }
}
