namespace TopHackerNewsStories.Adapters
{
    public interface IAdapter<T, K>
    {
        T Adapt(K item);
        K Adapt(T item);
    }
}
