namespace TopHackerNewsStories.Services
{
    public interface IStoryDetailCacheManager
    {
        void Hint(IEnumerable<int> currentBestStories);
        void RemoveErrored(IEnumerable<int> erroredStoryIds);
    }
}
