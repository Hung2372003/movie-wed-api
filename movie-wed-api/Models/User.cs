using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace movie_wed_api.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } = "User"; // "User" | "Admin"

        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
}
