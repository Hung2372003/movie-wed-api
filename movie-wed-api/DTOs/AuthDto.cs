namespace movie_wed_api.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginDto
    {
        public string Identifier { get; set; } = string.Empty; // username hoặc email
        public string Password { get; set; } = string.Empty;
    }
}
