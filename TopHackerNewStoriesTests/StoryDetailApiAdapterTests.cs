using Moq;
using TopHackerNewsStories.Adapters;
using TopHackerNewsStories.Apis.HackerNews;
using TopHackerNewsStories.Model;

namespace TopHackerNewsStories.Tests
{
    public class StoryDetailApiAdapterTests
    {
        private IAdapter<StoryDetailApiContract, StoryDetail> _adapter = new StoryDetailApiAdapter();

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void AdaptsToCorrectStoryDetail()
        {
            var time = 1708348338;
            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(time);
            var storyDetailApiContract = new StoryDetailApiContract() { url = "a url", title = "a title", time = time, by = "user", score = 100};

            var storyDetail = new StoryDetail("a title", "a url", 100, dateTimeOffset, "user", "");

            var actualStoryDetail = _adapter.Adapt(storyDetailApiContract);

            Assert.That(actualStoryDetail, Is.EqualTo(storyDetail));

        }

        [Test]
        public void ThrowExceptionOnBadApiDetail()
        {
           
            //time is large negative number - no coversion to DateTime
            var storyDetailApiContract = new StoryDetailApiContract() { url = "a url", title = "a title", time = -100000000000, by = "user", score = 100 };

         
            Assert.Throws<ArgumentOutOfRangeException>(()=>_adapter.Adapt(storyDetailApiContract));

        }
    }
}
