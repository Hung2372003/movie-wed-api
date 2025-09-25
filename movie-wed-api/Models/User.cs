using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace movie_wed_api.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = null!; // 🔹 username đăng nhập
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = "User";
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? AvatarPublicId { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
