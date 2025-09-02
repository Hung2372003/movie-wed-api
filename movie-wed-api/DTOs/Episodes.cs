namespace movie_wed_api.DTOs.Episodes
{
    public class EpisodeCreateDto
    {
        public int EpisodeNumber { get; set; }
        public string? Title { get; set; }
        public string VideoUrl { get; set; } = null!;

        public IFormFile? Video { get; set; }
    }

    public class EpisodeUpdateDto : EpisodeCreateDto { }
}
