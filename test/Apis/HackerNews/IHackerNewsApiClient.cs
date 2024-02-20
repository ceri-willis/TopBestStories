using TopHackerNewsStories.Helpers;

namespace TopHackerNewsStories.Apis.HackerNews
{
    public interface IHackerNewsApiClient
    {
        Task<IStatus<IEnumerable<int>>> GetBestStoriesAsync();
        Task<IStatus<StoryDetailApiContract>> GetStoryDetailAsync(int storyId);
    }
}
