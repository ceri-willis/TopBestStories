using TopHackerNewsStories.Apis.HackerNews;
using TopHackerNewsStories.Helpers;
using TopHackerNewsStories.Model;

namespace TopHackerNewsStories.Services
{
    public class GetTopNStoriesService
    {

        private readonly IHackerNewsApiClient _client;
        private readonly IGetStoryDetailService _getStoryDetails;
        private readonly IStoryDetailCacheManager _cacheManager;
        private readonly ILogger<GetTopNStoriesService> _logger;

        public GetTopNStoriesService(IHackerNewsApiClient client, IGetCachedStoryDetailService getCachedStoryDetails, ILogger<GetTopNStoriesService> logger)
        {
            _client = client;
            _getStoryDetails = getCachedStoryDetails;
            _cacheManager = getCachedStoryDetails;
            _logger = logger;
        }
        public async Task<IStatus<IEnumerable<StoryDetail>>> GetTopN(int n)
        {
            var bestStoriesStatus = await _client.GetBestStoriesAsync();
            if (bestStoriesStatus.Ok && bestStoriesStatus.Item != null)
            {
                var bestStories = bestStoriesStatus.Item;

                _cacheManager.Hint(bestStories); // Give cache a hint about the current best stories so it can remove stale story ids when it deems appropriate 

                var topStoryIds = bestStories.Take(n).ToList();

                var fetchedStoryDetails = new List<StoryDetail>();

                var erroredStoryDetailIds = new List<int>();//depending on whether we fail straight away on first bad story or attempt fetch of others (TBD)- use a list

                var storyIndex = 0;

                // use while/index lookup as we want to break out in first case of  error - cant with ForEach()
                while (storyIndex < topStoryIds.Count && erroredStoryDetailIds.Count == 0) 
                {
                    var storyId = topStoryIds[storyIndex++];

                    var storyDetailStatus = await _getStoryDetails.GetStoryDetailAsync(storyId);

                    if (storyDetailStatus.Ok && storyDetailStatus.Item != null)
                    {
                        fetchedStoryDetails.Add(storyDetailStatus.Item);
                    }
                    else
                    {
                        erroredStoryDetailIds.Add(storyId);
                    }
                }

                if (erroredStoryDetailIds.Count > 0)  // remove errored from cache to allow future request to retry afresh 
                {
                    _cacheManager.RemoveErrored(erroredStoryDetailIds);
                }

                return erroredStoryDetailIds.Count == 0 ? (fetchedStoryDetails as IEnumerable<StoryDetail>).AsStatusOk()
                    : $"Failed to fetch HackerNews story details : ids [{String.Join(',', erroredStoryDetailIds)}]".AsStatusError<IEnumerable<StoryDetail>>();
            }
            return $"Failed to fetch overall HackerNews best stories list [{bestStoriesStatus.Message}]".AsStatusError<IEnumerable<StoryDetail>>();
        }
    }
}
