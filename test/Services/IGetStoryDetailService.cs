using TopHackerNewsStories.Helpers;
using TopHackerNewsStories.Model;

namespace TopHackerNewsStories.Services
{
    public interface IGetStoryDetailService
    {
        Task<IStatus<StoryDetail>> GetStoryDetailAsync(int storyId);
    }
}
