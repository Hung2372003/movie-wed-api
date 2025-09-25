using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace movie_wed_api.Models
{
    public class Episode
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        public int EpisodeNumber { get; set; }

        [StringLength(255)]
        public string? Title { get; set; }

        [Required]
        public string VideoUrl { get; set; }
        public string? VideoPublicId { get; set; }

        public int? Duration { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [JsonIgnore]
        public Movie Movie { get; set; }
    }
}
