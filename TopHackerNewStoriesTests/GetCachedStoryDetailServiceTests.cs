using Microsoft.Extensions.Logging;
using Moq;
using TopHackerNewsStories.Helpers;
using TopHackerNewsStories.Model;
using TopHackerNewsStories.Services;

namespace TopHackerNewsStories.Tests
{
    class GetCachedStoryDetailServiceTests
    {
        private Mock<IGetStoryDetailService> _mockGetStoryDetailService;
        private Mock<ILogger<IGetCachedStoryDetailService>> _mockLogger;
        private StoryDetail _storyDetail;

        private StoryDetail CreateStoryDetail(int id)
        {
            return new StoryDetail($"{id}", "uri", 100, new DateTimeOffset(new DateTime(2030, 12, 31, 0, 0, 0)), "postedBy", "commentCount");
        }
        [SetUp]
        public void Setup()
        {
            _mockGetStoryDetailService = new Mock<IGetStoryDetailService>();
            _mockLogger = new Mock<ILogger<IGetCachedStoryDetailService>>();           
            _mockGetStoryDetailService.Setup(s => s.GetStoryDetailAsync(It.IsAny<int>())).ReturnsAsync((int id) => CreateStoryDetail(id).AsStatusOk());
        }

        [Test]
        public async Task ServiceCallsGetStoryDetailServiceOnlyIfStoryNotAlreadyRequested()
        {
            var storyId = 1;
            var service = new GetCachedStoryDetailService(_mockGetStoryDetailService.Object, _mockLogger.Object);

            var storyDetailStatus = await service.GetStoryDetailAsync(storyId);

            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == storyId)), Times.Once);
            Assert.That(storyDetailStatus.Ok, Is.True);
            Assert.That(storyDetailStatus.Item, Is.Not.Null);
            Assert.That(storyDetailStatus.Item.title, Is.EqualTo(storyId.ToString()));

            var storyDetailStatus2 = await service.GetStoryDetailAsync(storyId);

            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == storyId)), Times.Once);// still only once
            Assert.That(storyDetailStatus2.Ok, Is.True);
            Assert.That(storyDetailStatus2.Item, Is.Not.Null);
            Assert.That(storyDetailStatus2.Item.title, Is.EqualTo(storyId.ToString()));

            var storyDetailStatus3 = await service.GetStoryDetailAsync(storyId);

            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == storyId)), Times.Once);// still only once
            Assert.That(storyDetailStatus3.Ok, Is.True);
            Assert.That(storyDetailStatus3.Item, Is.Not.Null);
            Assert.That(storyDetailStatus3.Item.title, Is.EqualTo(storyId.ToString()));
        }

        [Test]
        public async Task HintCauseCachCleanupOnlyAfterStaleCacheThreshold()
        {

            var service = new GetCachedStoryDetailService(_mockGetStoryDetailService.Object, _mockLogger.Object);

            var storyDetailStatus = await service.GetStoryDetailAsync(1);

            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 1)), Times.Once);


            var storyDetailStatus2 = await service.GetStoryDetailAsync(2);

            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 2)), Times.Once);


            var storyDetailStatus3 = await service.GetStoryDetailAsync(3);

            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 3)), Times.Once);


            service.Hint([2, 3]);

            await service.GetStoryDetailAsync(1); //in cache though not in current best stories

            service.Hint([2, 3]);

            await service.GetStoryDetailAsync(2);//in cache

            service.Hint([2, 3]);

            await service.GetStoryDetailAsync(3);//in cache


            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 1)), Times.Once);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 2)), Times.Once);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 3)), Times.Once);



            await service.GetStoryDetailAsync(4);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 4)), Times.Once);

            // now 2,4 are best stories , and 4 items in cache (1,2,3,4)
            service.Hint([2, 4]);
            // now Hint is that bestStories are [2,4],  there is now 1,2,3,4 in cache (4 items) , so more than third of cache is stale (ie. not in current best stories)
            // ( cache count is 4 > 3 where (3 = 1.5 * 2 ( 2 items in best stories))
            // so remove items not in bestStories ( 1,3)
            // - so  further calls to ids 1,3 should cause refetch on underlying service, calls to 2, 4 should not


            await service.GetStoryDetailAsync(1);
            await service.GetStoryDetailAsync(2);
            await service.GetStoryDetailAsync(3);
            await service.GetStoryDetailAsync(4);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 1)), Times.Exactly(2));
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 2)), Times.Once);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 3)), Times.Exactly(2));
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 4)), Times.Once);


        }

        [Test]
        public async Task RemoveErroredCauseCacheRemoval()
        {

            IGetCachedStoryDetailService service = new GetCachedStoryDetailService(_mockGetStoryDetailService.Object, _mockLogger.Object);

            var storyDetailStatus = await service.GetStoryDetailAsync(1);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 1)), Times.Once);


            var storyDetailStatus2 = await service.GetStoryDetailAsync(2);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 2)), Times.Once);


            var storyDetailStatus3 = await service.GetStoryDetailAsync(3);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 3)), Times.Once);         

            await service.GetStoryDetailAsync(1);          
            await service.GetStoryDetailAsync(2);    
            await service.GetStoryDetailAsync(3);

            //Assert all  1,2,3 in cache
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 1)), Times.Once);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 2)), Times.Once);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 3)), Times.Once);

            service.RemoveErrored([1, 3]);
            // next calls for 1,3 should call underlying

            await service.GetStoryDetailAsync(1);
            await service.GetStoryDetailAsync(2);
            await service.GetStoryDetailAsync(3);

            //Assert   1,3 were not in cache - causing extra call for each
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 1)), Times.Exactly(2));
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 2)), Times.Once);
            _mockGetStoryDetailService.Verify(m => m.GetStoryDetailAsync(It.Is<int>(id => id == 3)), Times.Exactly(2));
            


        }

    }
}
