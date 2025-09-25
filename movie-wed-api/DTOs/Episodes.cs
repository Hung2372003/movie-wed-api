namespace movie_wed_api.DTOs.Episodes
{
    public class EpisodeCreateDto
    {
        public int? Id { get; set; }
        public int EpisodeNumber { get; set; }
        public string? Title { get; set; }
        public string? VideoUrl { get; set; } = null!;

        public IFormFile? Video { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class EpisodeUpdateDto : EpisodeCreateDto { }
}
