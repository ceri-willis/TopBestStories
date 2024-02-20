using Microsoft.Extensions.Logging;
using Moq;
using TopHackerNewsStories.Apis.HackerNews;
using TopHackerNewsStories.Helpers;
using TopHackerNewsStories.Model;
using TopHackerNewsStories.Services;

namespace TopHackerNewsStories.Tests
{
    public class GetTopNStoriesServiceTests
    {
        private Mock<IGetCachedStoryDetailService> _mockGetCachedStoryDetailService;
        private Mock<IHackerNewsApiClient> _mockHackerNewsApiClientOk;
        private Mock<IHackerNewsApiClient> _mockHackerNewsApiClientErrored;
        private Mock<ILogger<GetTopNStoriesService>> _mockLogger;
        private IEnumerable<int> _bestStories = Enumerable.Range(1, 5);
        private StoryDetail CreateStoryDetail(int id)
        {
            return new StoryDetail($"{id}", "uri", 100, new DateTimeOffset(new DateTime(2030, 12, 31, 0, 0, 0)), "postedBy", "commentCount");
        }
        [SetUp]
        public void Setup()
        {

            _mockLogger = new Mock<ILogger<GetTopNStoriesService>>();
            _mockHackerNewsApiClientOk = new Mock<IHackerNewsApiClient>();
            _mockHackerNewsApiClientOk.Setup(x => x.GetBestStoriesAsync()).ReturnsAsync(_bestStories.AsStatusOk());

            _mockHackerNewsApiClientErrored = new Mock<IHackerNewsApiClient>();
            _mockHackerNewsApiClientErrored.Setup(x => x.GetBestStoriesAsync()).ReturnsAsync("Exception in beststories call".AsStatusError<IEnumerable<int>>());

            // get storyDetails succeed except for storyId 4  - return fail status
            _mockGetCachedStoryDetailService = new Mock<IGetCachedStoryDetailService>();
            _mockGetCachedStoryDetailService.Setup(s => s.GetStoryDetailAsync(It.Is<int>(s => s != 4))).ReturnsAsync((int id) => CreateStoryDetail(id).AsStatusOk());
            _mockGetCachedStoryDetailService.Setup(s => s.GetStoryDetailAsync(It.Is<int>(s => s == 4))).ReturnsAsync("error in storyDetails".AsStatusError<StoryDetail>());

        }

        [Test]
        public async Task GetTopN_CallsGetBestStoriesFromApi_Success()
        {
            var service = new GetTopNStoriesService(_mockHackerNewsApiClientOk.Object, _mockGetCachedStoryDetailService.Object, _mockLogger.Object);

            var top3StoriesStatus = await service.GetTopN(3);

            _mockHackerNewsApiClientOk.Verify(m => m.GetBestStoriesAsync(), Times.Once);


        }

        [Test]
        public async Task GetTopN_PassesHintOfCurrentBestStoriesToCache()
        {
            var service = new GetTopNStoriesService(_mockHackerNewsApiClientOk.Object, _mockGetCachedStoryDetailService.Object, _mockLogger.Object);

            var top3StoriesStatus = await service.GetTopN(3);

            _mockGetCachedStoryDetailService.Verify(m => m.Hint(It.Is<IEnumerable<int>>(actual => _bestStories.SequenceEqual(actual))), Times.Once);

        }

        [Test]
        public async Task GetTopN_GetsStoryDetailsFromCachedStoryDetailService()
        {
            var service = new GetTopNStoriesService(_mockHackerNewsApiClientOk.Object, _mockGetCachedStoryDetailService.Object, _mockLogger.Object);

            var top3StoriesStatus = await service.GetTopN(3);

            _mockGetCachedStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(i => i == 1)), Times.Once);
            _mockGetCachedStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(i => i == 2)), Times.Once);
            _mockGetCachedStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(i => i == 3)), Times.Once);

        }

        [Test]
        public async Task GetTopN_ReturnsStatusOk_AndCorrectStoryDetailsReturned()
        {
            var service = new GetTopNStoriesService(_mockHackerNewsApiClientOk.Object, _mockGetCachedStoryDetailService.Object, _mockLogger.Object);

            var top3StoriesStatus = await service.GetTopN(3);

            Assert.That(top3StoriesStatus.Ok, Is.True);
            Assert.That(top3StoriesStatus.Item, Is.Not.Null);

            Assert.That(top3StoriesStatus.Item.Count(), Is.EqualTo(3));
            var storyDetails = top3StoriesStatus.Item.ToList();
            Assert.That(storyDetails[0].title, Is.EqualTo("1"));
            Assert.That(storyDetails[1].title, Is.EqualTo("2"));
            Assert.That(storyDetails[2].title, Is.EqualTo("3"));

        }


        [Test]
        public async Task GetTopN_GetBestStoriesFails_ReturnsStatusError()
        {
            var service = new GetTopNStoriesService(_mockHackerNewsApiClientErrored.Object, _mockGetCachedStoryDetailService.Object, _mockLogger.Object);

            var top3StoriesStatus = await service.GetTopN(3);
            Assert.That(top3StoriesStatus.Ok, Is.False);
            Assert.That(top3StoriesStatus.Item, Is.Null);
            Assert.That(top3StoriesStatus.Message, Is.EqualTo("Failed to fetch overall HackerNews best stories list [Exception in beststories call]"));

        }

        [Test]
        public async Task GetTopN_GetStoryDetailFails_PassesErroredStoryIdToCache()
        {
            var service = new GetTopNStoriesService(_mockHackerNewsApiClientOk.Object, _mockGetCachedStoryDetailService.Object, _mockLogger.Object);

            var top5StoriesStatus = await service.GetTopN(5); // story 4 will fail 

            // Assert that [] of storyId = 4 sent to Cache for it to be removed
            _mockGetCachedStoryDetailService.Verify(m => m.RemoveErrored(It.Is<IEnumerable<int>>(actual => actual.SequenceEqual(new[] { 4 }))), Times.Once);
         
        }

        [Test]
        public async Task GetTopN_GetStoryDetailFails_DoesNotAttemptToGetNextStoryIdAfterFirstFail()
         {
            var service = new GetTopNStoriesService(_mockHackerNewsApiClientOk.Object, _mockGetCachedStoryDetailService.Object, _mockLogger.Object);

            var top5StoriesStatus = await service.GetTopN(5); // story 4 will fail - should not call for story 5 

            // Assert that only first 4 stories are attempted fetch -  does not attempt further story details after first fail eg. story id 5
            _mockGetCachedStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(i => i == 1)), Times.Once);
            _mockGetCachedStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(i => i == 2)), Times.Once);
            _mockGetCachedStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(i => i == 3)), Times.Once);
            _mockGetCachedStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(i => i == 4)), Times.Once);
            _mockGetCachedStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(i => i == 5)), Times.Never);

        }

        
         [Test]
         public async Task GetTopN_GetStoryDetailFails_ReturnStatusErrorWithStoryDetailsId()
         {
            var service = new GetTopNStoriesService(_mockHackerNewsApiClientOk.Object, _mockGetCachedStoryDetailService.Object, _mockLogger.Object);

            var top5StoriesStatus = await service.GetTopN(5); // story 4 will fail - should return Error Status

            Assert.That(top5StoriesStatus.Ok, Is.False);
            Assert.That(top5StoriesStatus.Item, Is.Null);
            Assert.That(top5StoriesStatus.Message, Is.EqualTo("Failed to fetch HackerNews story details : ids [4]"));

        }


    }
}
