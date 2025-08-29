namespace movie_wed_api.DTOs.Episodes
{
    public class EpisodeCreateDto
    {
        public int EpisodeNumber { get; set; }
        public string? Title { get; set; }
        public string VideoUrl { get; set; } = null!;
    }

    public class EpisodeUpdateDto : EpisodeCreateDto { }
}
