namespace TopHackerNewsStories.Apis.HackerNews
{
    public record class StoryDetailApiContract
    {
        public string? title { get; set; }
        public string? url { get; set; }
        public int score { get; set; }
        public long time { get; set; }
        public string? by { get; set; }
        public int commentCount { get; set; }
    }
}
