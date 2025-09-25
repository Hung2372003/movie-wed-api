namespace movie_wed_api.DTOs
{
    public class UserDto
    {
    }
    public class UserUpdateDto
    {
        public string? Email { get; set; }
        public string? Country { get; set; }
        public string? State { get; set; }
        public string? FullName { get; set; }
        public string? Password { get; set; }
        public IFormFile? Avatar { get; set; }
    }
}
