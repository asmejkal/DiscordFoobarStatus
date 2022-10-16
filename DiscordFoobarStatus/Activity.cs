namespace DiscordFoobarStatus
{
    public class Activity
    {
        public string? State { get; set; }
        public string? Details { get; set; }

        public long? StartTimestamp { get; set; }
        public long? EndTimestamp { get; set; }

        public string? Thumbnail { get; set; }
        public string? ThumbnailText { get; set; }
    }
}
