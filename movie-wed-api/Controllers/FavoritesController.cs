using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movie_wed_api.Database;
using movie_wed_api.Models;
using System.Security.Claims;

namespace movie_wed_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public FavoritesController(MovieDbContext context)
        {
            _context = context;
        }

        // GET /api/favorites
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Movie)
                .Select(f => new
                {
                    f.Id,
                    f.UserId,
                    f.MovieId,
                    f.CreatedAt,
                    Movie = new
                    {
                        f.Movie.Id,
                        f.Movie.Title,
                        f.Movie.PosterUrl,
                        f.Movie.ReleaseYear,
                        f.Movie.Description,
                        f.Movie.Ratings
                    }
                })
                .ToListAsync();

            return Ok(favorites);
        }


        // POST /api/favorites/movie/5
        [HttpPost("movie/{movieId}")]
        [Authorize]
        public async Task<IActionResult> AddFavorite(int movieId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound(new { message = "Movie not found" });

            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

            if (existing != null)
                return BadRequest(new { message = "Already in favorites" });

            var favorite = new Favorite
            {
                UserId = userId,
                MovieId = movieId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(favorite);
        }

        // DELETE /api/favorites/movie/5
        [HttpDelete("movie/{movieId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFavorite(int movieId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

            if (favorite == null) return NotFound();

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
