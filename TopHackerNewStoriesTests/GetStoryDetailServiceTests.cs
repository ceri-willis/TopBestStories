using Moq;
using TopHackerNewsStories.Adapters;
using TopHackerNewsStories.Apis.HackerNews;
using TopHackerNewsStories.Helpers;
using TopHackerNewsStories.Model;
using TopHackerNewsStories.Services;

namespace TopHackerNewsStories.Tests
{
    public class GetStoryDetailServiceTests
    {
        private Mock<IHackerNewsApiClient> _mockApiClient;
        private Mock<IAdapter<StoryDetailApiContract, StoryDetail>> _mockAdapter;
        private Mock<IAdapter<StoryDetailApiContract, StoryDetail>> _mockAdapterThrowsException;
        private StoryDetailApiContract _storyDetailApiContract;
        private StoryDetail _storyDetail;   
        [SetUp]
        public void Setup()
        {
            _mockApiClient = new Mock<IHackerNewsApiClient>();
            _mockAdapter = new Mock<IAdapter<StoryDetailApiContract, StoryDetail>>();

            _storyDetailApiContract = new StoryDetailApiContract() { url = "url", title = "title",time= 12345678,by="by", };

            _storyDetail = new StoryDetail("title", "uri", 100, new DateTimeOffset(new DateTime(2030,12,31,0,0,0)),"postedBy","commentCount");

            _mockApiClient.Setup(s => s.GetStoryDetailAsync(It.Is<int>(s=>s==1))).ReturnsAsync(_storyDetailApiContract.AsStatusOk());
            _mockApiClient.Setup(s => s.GetStoryDetailAsync(It.Is<int>(s=>s==2))).ReturnsAsync("error".AsStatusError<StoryDetailApiContract>());
            _mockAdapter.Setup(s => s.Adapt(It.IsAny<StoryDetailApiContract>())).Returns(_storyDetail);

            _mockAdapterThrowsException = new Mock<IAdapter<StoryDetailApiContract, StoryDetail>>();
            _mockAdapterThrowsException.Setup(s => s.Adapt(It.IsAny<StoryDetailApiContract>())).Throws(new Exception("adapter exception"));
        }

        [Test]
        public async Task ServiceCallsApiWithStoryIdAndAdaptsResultfromApiContractToModel()
        {
            var storyId = 1;
            var service = new GetStoryDetailService(_mockApiClient.Object, _mockAdapter.Object);
           
            var storyDetailStatus = await service.GetStoryDetailAsync(storyId);

            _mockApiClient.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == storyId)));
            _mockAdapter.Verify(m => m.Adapt(It.Is<StoryDetailApiContract>(sdac => sdac == _storyDetailApiContract)),Times.Once);
            Assert.That(storyDetailStatus.Ok, Is.True);
            Assert.That(storyDetailStatus.Item, Is.EqualTo(_storyDetail));
        }

        [Test]
        public async Task ApiFailStatusReflectedInStoryDetailStatusMessage()
        {
            var storyId = 2;
            var service = new GetStoryDetailService(_mockApiClient.Object, _mockAdapter.Object);

            var storyDetailStatus = await service.GetStoryDetailAsync(storyId);

            _mockApiClient.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == storyId)));
            _mockAdapter.Verify(m => m.Adapt(It.IsAny<StoryDetailApiContract>()),Times.Never);
            Assert.That(storyDetailStatus.Ok, Is.False);
            Assert.That(storyDetailStatus.Item, Is.Null);
            Assert.That(storyDetailStatus.Message, Is.EqualTo("error"));
        }

        [Test]
        public async Task AdapterThownExceptionCaughtAndReflectedInStoryDetailStatusMessage()
        {
            var storyId = 1; //story will fetch ok but adapter will throw exception
            var service = new GetStoryDetailService(_mockApiClient.Object, _mockAdapterThrowsException.Object);

            var storyDetailStatus = await service.GetStoryDetailAsync(storyId);

            _mockApiClient.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == storyId)));
            _mockAdapterThrowsException.Verify(m => m.Adapt(It.IsAny<StoryDetailApiContract>()), Times.Once);
            Assert.That(storyDetailStatus.Ok, Is.False);
            Assert.That(storyDetailStatus.Item, Is.Null);
            Assert.That(storyDetailStatus.Message, Is.EqualTo("Failed to Adapt StoryDetail for id 1. [adapter exception]"));
        }
    }
}