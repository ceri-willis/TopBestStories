namespace TopHackerNewsStories.Model
{
    public record class StoryDetail
    {
        public string? title { get; set; }
        public string? uri { get; set; }
        public int score { get; set; }
        public DateTimeOffset time { get; set; }
        public string? postedBy { get; set; }
        public string? commentCount { get; set; }
        public StoryDetail(string? title, string? uri, int score, DateTimeOffset time, string? postedBy, string? commentCount)
        {
            this.title = title;
            this.uri = uri;
            this.score = score;
            this.time = time;
            this.postedBy = postedBy;
            this.commentCount = commentCount;
        }
    }
}
