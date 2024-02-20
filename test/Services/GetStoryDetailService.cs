using TopHackerNewsStories.Adapters;
using TopHackerNewsStories.Apis.HackerNews;
using TopHackerNewsStories.Helpers;
using TopHackerNewsStories.Model;

namespace TopHackerNewsStories.Services
{
    public class GetStoryDetailService : IGetStoryDetailService
    {
        private readonly IHackerNewsApiClient _client;
        private readonly IAdapter<StoryDetailApiContract, StoryDetail> _adapter;

        public GetStoryDetailService(IHackerNewsApiClient client, IAdapter<StoryDetailApiContract, StoryDetail> adapter)
        {
            _client = client;
            _adapter = adapter;
        }
        public async Task<IStatus<StoryDetail>> GetStoryDetailAsync(int storyId)
        {

            var storyDetailApiStatus = await _client.GetStoryDetailAsync(storyId);

            if (storyDetailApiStatus.Ok && storyDetailApiStatus.Item != null)
            {
                try
                {
                    return _adapter.Adapt(storyDetailApiStatus.Item).AsStatusOk(); ;
                }
                catch (Exception ex)
                {
                    return $"Failed to Adapt StoryDetail for id {storyId}. [{ex.Message}]".AsStatusError<StoryDetail>();
                }

            }
            else
            {
                return storyDetailApiStatus.Message.AsStatusError<StoryDetail>();
            }

        }
    }
}
