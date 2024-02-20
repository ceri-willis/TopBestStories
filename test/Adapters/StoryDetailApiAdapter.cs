using System;
using TopHackerNewsStories.Apis.HackerNews;
using TopHackerNewsStories.Model;

namespace TopHackerNewsStories.Adapters
{
    public class StoryDetailApiAdapter : IAdapter<StoryDetailApiContract, StoryDetail>
    {
        public StoryDetail Adapt(StoryDetailApiContract item)
        {
            // Adapts contract story to our version (as per specification)
            // url ->uri, by->postedBy , time(unix)->UTC         
            return new StoryDetail(item.title, item.url, item.score, DateTimeOffset.FromUnixTimeSeconds(item.time), item.by, string.Empty);
        }

        public StoryDetailApiContract Adapt(StoryDetail item)
        {
            throw new NotImplementedException();
        }
    }
}
