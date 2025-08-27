using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace movie_wed_api.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }

        public int? ReleaseYear { get; set; }
        public string? Country { get; set; }
        public string? Director { get; set; }
        public int? Duration { get; set; } // phút

        [Required]
        public string Type { get; set; } // "Movie" | "Series"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Episode> Episodes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
}
