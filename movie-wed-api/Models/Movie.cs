using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace movie_wed_api.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? PosterPublicId { get; set; }   // 👈 thêm
        public string? TrailerUrl { get; set; }
        public string? TrailerPublicId { get; set; }  // 👈 thêm
        public int? ReleaseYear { get; set; }
        public string? Country { get; set; }
        public string? Director { get; set; }
        public int? Duration { get; set; }
        public string Type { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    }
}
