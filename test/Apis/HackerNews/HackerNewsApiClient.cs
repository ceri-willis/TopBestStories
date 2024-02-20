using System.Text.Json;
using TopHackerNewsStories.Helpers;

namespace TopHackerNewsStories.Apis.HackerNews
{
    public class HackerNewsApiClient : IHackerNewsApiClient
    {
        private readonly IHttpClientFactory _hcf;
        private readonly ILogger<IHackerNewsApiClient> _logger;
        public HackerNewsApiClient(IHttpClientFactory httpClientFactory, ILogger<IHackerNewsApiClient> logger)
        {
            _hcf = httpClientFactory;
            _logger = logger;
        }

        public async Task<IStatus<IEnumerable<int>>> GetBestStoriesAsync()
        {
            try
            {
                var bestStoriesUrl = "https://hacker-news.firebaseio.com/v0/beststories.json";

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, bestStoriesUrl);

                var httpResponseMessage = await _hcf.CreateClient().SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                    var bestStories = await JsonSerializer.DeserializeAsync<IEnumerable<int>>(contentStream);

                    return bestStories != null ? bestStories.AsStatusOk() : "failed to deserialize".AsStatusError<IEnumerable<int>>();

                }
                return $"error{httpResponseMessage.StatusCode}".AsStatusError<IEnumerable<int>>();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return $"GetBestStoriesAsync Failed :{ex.Message}".AsStatusError<IEnumerable<int>>();
            }

        }
        public async Task<IStatus<StoryDetailApiContract>> GetStoryDetailAsync(int storyId)
        {

            try
            {
                var storyDetailUrl = $"https://hacker-news.firebaseio.com/v0/item/{storyId}.json";

                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, storyDetailUrl);

                var httpResponseMessage = await _hcf.CreateClient().SendAsync(httpRequestMessage);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                    var storyDetail = await JsonSerializer.DeserializeAsync<StoryDetailApiContract>(contentStream);

                    return storyDetail != null ? storyDetail.AsStatusOk() : "failed to deserialize".AsStatusError<StoryDetailApiContract>();

                }
                return $"error{httpResponseMessage.StatusCode}".AsStatusError<StoryDetailApiContract>();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return $"GetStoryDetailAsync Failed :{ex.Message}".AsStatusError<StoryDetailApiContract>();
            }

        }
    }
}
