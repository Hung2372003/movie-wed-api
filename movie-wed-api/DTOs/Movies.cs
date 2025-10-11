namespace movie_wed_api.DTOs.Movies
{
    public class MovieCreateDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Country { get; set; }
        public string? Director { get; set; }
        public int? Duration { get; set; }
        public string Type { get; set; } = null!;

        public IFormFile? Poster { get; set; }
        public IFormFile? Trailer { get; set; }
    }

    public class MovieUpdateDto : MovieCreateDto {
    
    }
    public class MovieSearchRequest
    {
        public string? MovieName { get; set; }
        public List<string>? Genres { get; set; }   // tương ứng Type
        public string? Rating { get; set; }         // "0-3"
        public int? ReleaseYearFrom { get; set; }
        public int? ReleaseYearTo { get; set; }

        // 🧭 Phân trang
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
