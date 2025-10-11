using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movie_wed_api.Database;
using movie_wed_api.DTOs.Movies;
using movie_wed_api.Models;
using movie_wed_api.Services;

namespace movie_wed_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public MoviesController(MovieDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET /api/movies?search=...&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetMovies(
          string? search, string? type, string? country, int? year,
          int page = 1, int pageSize = 10)
            {
                var query = _context.Movies.AsQueryable();

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(m => m.Title.Contains(search));

                if (!string.IsNullOrEmpty(type))
                    query = query.Where(m => m.Type == type);

                if (!string.IsNullOrEmpty(country))
                    query = query.Where(m => m.Country == country);

                if (year.HasValue)
                    query = query.Where(m => m.ReleaseYear == year);

                var total = await query.CountAsync();

                var movies = await query
                    .OrderByDescending(m => m.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        m.PosterUrl,
                        m.ReleaseYear,
                        m.Country,
                        m.Director,
                        m.Duration,
                        m.Type,
                        m.TrailerUrl,
                        m.TrailerPublicId,
                        m.CreatedAt,
                        AverageRating = m.Ratings.Any()
                            ? m.Ratings.Average(r => r.Score)
                            : 10,  // nếu chưa có rating
                        RatingsCount = m.Ratings.Count
                    })
                    .ToListAsync();

            return Ok(new { total, page, pageSize, data = movies });
        }


        // GET /api/movies/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovie(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Episodes)
                .Include(m => m.Comments)
                .Include(m => m.Ratings)
                .FirstOrDefaultAsync(m => m.Id == id);


            if (movie == null) return NotFound();

            return Ok(movie);
        }

        // POST /api/movies (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMovie([FromForm] MovieCreateDto dto)
        {
            var movie = new Movie
            {
                Title = dto.Title,
                Description = dto.Description,
                ReleaseYear = dto.ReleaseYear,
                Country = dto.Country,
                Director = dto.Director,
                Duration = dto.Duration,
                Type = dto.Type,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Poster != null)
            {
                var poster = await _cloudinaryService.UploadImageAsync(dto.Poster);
                movie.PosterUrl = poster.Url;
                movie.PosterPublicId = poster.PublicId;
            }

            if (dto.Trailer != null)
            {
                var trailer = await _cloudinaryService.UploadVideoAsync(dto.Trailer);
                movie.TrailerUrl = trailer.Url;
                movie.TrailerPublicId = trailer.PublicId;
            }

            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
        }

        // PUT /api/movies/5 (Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMovie(int id, [FromForm] MovieUpdateDto dto)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            movie.Title = dto.Title;
            movie.Description = dto.Description;
            movie.ReleaseYear = dto.ReleaseYear;
            movie.Country = dto.Country;
            movie.Director = dto.Director;
            movie.Duration = dto.Duration;
            movie.Type = dto.Type;
            movie.UpdatedAt = DateTime.UtcNow;

            if (dto.Poster != null)
            {
                if (!string.IsNullOrEmpty(movie.PosterPublicId))
                    await _cloudinaryService.DeleteFileAsync(movie.PosterPublicId);

                var poster = await _cloudinaryService.UploadImageAsync(dto.Poster);
                movie.PosterUrl = poster.Url;
                movie.PosterPublicId = poster.PublicId;
            }

            if (dto.Trailer != null)
            {
                if (!string.IsNullOrEmpty(movie.TrailerPublicId))
                    await _cloudinaryService.DeleteFileAsync(movie.TrailerPublicId);

                var trailer = await _cloudinaryService.UploadVideoAsync(dto.Trailer);
                movie.TrailerUrl = trailer.Url;
                movie.TrailerPublicId = trailer.PublicId;
            }

            await _context.SaveChangesAsync();
            return Ok(movie);
        }

        // DELETE /api/movies/5 (Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return NotFound();

            if (!string.IsNullOrEmpty(movie.PosterPublicId))
                await _cloudinaryService.DeleteFileAsync(movie.PosterPublicId);

            if (!string.IsNullOrEmpty(movie.TrailerPublicId))
                await _cloudinaryService.DeleteFileAsync(movie.TrailerPublicId);

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpPost("search")]
        public async Task<IActionResult> SearchMovies([FromBody] MovieSearchRequest request)
        {
            if(request.ReleaseYearFrom == 0)
            {
                request.ReleaseYearFrom = 2000;
            }
            if (request.ReleaseYearTo == 0)
            {
                request.ReleaseYearFrom = 2070;
            }


            var query = _context.Movies
                .Include(m => m.Episodes)
                .Include(m => m.Comments)
                .Include(m => m.Ratings)
                .Include(m => m.Favorites)
                .AsQueryable();

            // 🔍 Lọc theo tên phim
            if (!string.IsNullOrWhiteSpace(request.MovieName))
            {
                query = query.Where(m => m.Title.Contains(request.MovieName));
            }

            // 🎭 Lọc theo thể loại (Type)
            if (request.Genres != null && request.Genres.Any())
            {
                query = query.Where(m => request.Genres.Contains(m.Type));
            }

            // ⭐ Lọc theo rating
            if (!string.IsNullOrEmpty(request.Rating))
            {
                var parts = request.Rating.Split('-');
                if (parts.Length == 2 &&
                    double.TryParse(parts[0], out var minRating) &&
                    double.TryParse(parts[1], out var maxRating))
                {
                    query = query.Where(m =>
                        m.Ratings.Any() &&
                        m.Ratings.Average(r => r.Score) >= minRating &&
                        m.Ratings.Average(r => r.Score) <= maxRating
                    );
                }
            }

            // 📅 Lọc theo năm
            if (request.ReleaseYearFrom.HasValue)
                query = query.Where(m => m.ReleaseYear >= request.ReleaseYearFrom.Value);
            if (request.ReleaseYearTo.HasValue)
                query = query.Where(m => m.ReleaseYear <= request.ReleaseYearTo.Value);

            // 🧮 Tổng số phim trước khi phân trang
            var totalCount = await query.CountAsync();

            // 📄 Áp dụng phân trang
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

            var skip = (page - 1) * pageSize;
            var movies = await query
     .OrderByDescending(m => m.CreatedAt)
     .Skip(skip)
     .Take(pageSize) // ✅ dùng biến pageSize an toàn
     .Select(m => new
     {
         m.Id,
         m.Title,
         m.Description,
         m.PosterUrl,
         m.PosterPublicId,
         m.TrailerUrl,
         m.TrailerPublicId,
         m.ReleaseYear,
         m.Country,
         m.Director,
         m.Duration,
         m.Type,
         m.CreatedAt,
         m.UpdatedAt,
         AverageRating = m.Ratings.Any() ? m.Ratings.Average(r => r.Score) : 0,
         Episodes = m.Episodes.Select(e => new
         {
             e.Id,
             e.MovieId,
             e.Title,
             e.EpisodeNumber,
             e.VideoUrl,
             e.VideoPublicId,
             e.Duration,
             e.CreatedAt,
             e.UpdatedAt
         }),
         Comments = m.Comments.Select(c => new
         {
             c.Id,
             c.MovieId,
             c.UserId,
             c.Content,
             c.CreatedAt
         }),
         Ratings = m.Ratings.Select(r => new
         {
             r.Id,
             r.MovieId,
             r.UserId,
             r.Score
         }),
         Favorites = m.Favorites.Select(f => new
         {
             f.Id,
             f.MovieId,
             f.UserId
         })
     })
     .ToListAsync();


            // ✅ Trả về đúng format client yêu cầu
            var response = new
            {
                total = totalCount,
                page = request.Page,
                pageSize = request.PageSize,
                data = movies
            };

            return Ok(response);
        }
    }
}
