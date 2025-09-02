using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movie_wed_api.Database;
using movie_wed_api.Models;

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

            var avg = ratings.Count > 0 ? ratings.Average(r => r.Score) : 0;

            return Ok(new { average = avg, total = ratings.Count, data = ratings });
        }

        // POST /api/ratings/movie/5
        [HttpPost("movie/{movieId}")]
        [Authorize]
        public async Task<IActionResult> RateMovie(int movieId, [FromBody] int score)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            if (score < 1 || score > 5)
                return BadRequest(new { message = "Score must be between 1 and 5" });

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound(new { message = "Movie not found" });

            var existing = await _context.Ratings
                .FirstOrDefaultAsync(r => r.MovieId == movieId && r.UserId == userId);

            if (existing != null)
            {
                existing.Score = score;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var rating = new Rating
                {
                    UserId = userId,
                    MovieId = movieId,
                    Score = score,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Ratings.Add(rating);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Rating saved" });
        }
    }
}
