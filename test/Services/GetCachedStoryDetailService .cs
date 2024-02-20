using System.Collections.Concurrent;
using TopHackerNewsStories.Helpers;
using TopHackerNewsStories.Model;

namespace TopHackerNewsStories.Services
{
    public class GetCachedStoryDetailService : IGetCachedStoryDetailService
    {
        private readonly IGetStoryDetailService _getStoryDetail;
        private readonly ConcurrentDictionary<int, Task<IStatus<StoryDetail>>> _storiesCache = new ConcurrentDictionary<int, Task<IStatus<StoryDetail>>>();
        private readonly ILogger<IGetCachedStoryDetailService> _logger;

        public GetCachedStoryDetailService(IGetStoryDetailService getStoryDetail, ILogger<IGetCachedStoryDetailService> logger)
        {
            _getStoryDetail = getStoryDetail;
            _logger = logger;
        }

        public async Task<IStatus<StoryDetail>> GetStoryDetailAsync(int storyId)
        {
            return await _storiesCache.GetOrAdd(storyId, _getStoryDetail.GetStoryDetailAsync);
        }

        public void Hint(IEnumerable<int> currentBestStories)
        {
            // if guaranteed that over 1/3 in cache are stale stories (they are no longer in best stories list and will never be requested)
            // then prune cache by removing all for these ids that are no longer in the overall best stories
            if (_storiesCache.Count > currentBestStories.Count() * 1.5)
            {
                RemoveStoriesById(_storiesCache.Keys.Except(currentBestStories));
            }
        }

        public void RemoveErrored(IEnumerable<int> erroredStoryIds)
        {
            RemoveStoriesById(erroredStoryIds, true);
        }

        private void RemoveStoriesById(IEnumerable<int> storiesToRemove, bool errored = false)
        {
            var asList = storiesToRemove.ToList();
            _logger.LogInformation($"Removing stories from Cache : count {asList.Count} : reason {(errored ? "Errored" : "Stale")}");
            asList.ForEach(staleStoryId => _storiesCache.TryRemove(staleStoryId, out Task<IStatus<StoryDetail>>? ignore));
        }
    }
}
