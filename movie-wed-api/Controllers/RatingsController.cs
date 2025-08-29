using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movie_wed_api.Database;
using movie_wed_api.DTOs;
using movie_wed_api.Models;
using System.Security.Claims;

namespace movie_wed_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public RatingsController(MovieDbContext context)
        {
            _context = context;
        }

        // GET /api/ratings/movie/5
        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetRatingsByMovie(int movieId)
        {
            var ratings = await _context.Ratings
                .Where(r => r.MovieId == movieId)
                .ToListAsync();

            if (!ratings.Any())
                return Ok(new { average = 0, count = 0 });

            var avg = ratings.Average(r => r.Score);
            return Ok(new { average = avg, count = ratings.Count });
        }

        // POST /api/ratings/movie/5
        [HttpPost("movie/{movieId}")]
        [Authorize]
        public async Task<IActionResult> RateMovie(int movieId, RatingCreateDto dto)
        {
            if (dto.Score < 1 || dto.Score > 5)
                return BadRequest(new { message = "Score must be between 1 and 5" });

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound(new { message = "Movie not found" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Kiểm tra user đã rate chưa
            var existing = await _context.Ratings
                .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

            if (existing != null)
            {
                existing.Score = dto.Score;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var rating = new Rating
                {
                    MovieId = movieId,
                    UserId = userId,
                    Score = dto.Score,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Ratings.Add(rating);
            }

            await _context.SaveChangesAsync();

            // Trả về điểm trung bình mới
            var ratings = await _context.Ratings.Where(r => r.MovieId == movieId).ToListAsync();
            var avg = ratings.Average(r => r.Score);

            return Ok(new { average = avg, count = ratings.Count });
        }

        // DELETE /api/ratings/movie/5
        [HttpDelete("movie/{movieId}")]
        [Authorize]
        public async Task<IActionResult> DeleteRating(int movieId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var rating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

            if (rating == null) return NotFound();

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            // Tính lại average
            var ratings = await _context.Ratings.Where(r => r.MovieId == movieId).ToListAsync();
            var avg = ratings.Any() ? ratings.Average(r => r.Score) : 0;

            return Ok(new { average = avg, count = ratings.Count });
        }
    }
}
