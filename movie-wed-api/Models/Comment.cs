using System.ComponentModel.DataAnnotations;

namespace movie_wed_api.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Movie Movie { get; set; }
        public User User { get; set; }
    }
}
