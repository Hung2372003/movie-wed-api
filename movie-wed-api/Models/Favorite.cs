using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace movie_wed_api.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [JsonIgnore]
        public Movie Movie { get; set; }
        public User User { get; set; }
    }
}
