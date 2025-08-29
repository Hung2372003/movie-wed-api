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
    public class FavoritesController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public FavoritesController(MovieDbContext context)
        {
            _context = context;
        }

        // GET /api/favorites (lấy danh sách phim yêu thích của user)
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var favorites = await _context.Favorites
                .Include(f => f.Movie)
                .Where(f => f.UserId == userId)
                .Select(f => new
                {
                    f.Id,
                    Movie = new
                    {
                        f.Movie.Id,
                        f.Movie.Title,
                        f.Movie.PosterUrl,
                        f.Movie.ReleaseYear,
                        f.Movie.Type
                    },
                    f.CreatedAt
                })
                .ToListAsync();

            return Ok(favorites);
        }

        // POST /api/favorites
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToFavorites(FavoriteCreateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // kiểm tra phim có tồn tại không
            var movie = await _context.Movies.FindAsync(dto.MovieId);
            if (movie == null) return NotFound(new { message = "Movie not found" });

            // kiểm tra đã có chưa
            var exists = await _context.Favorites
                .AnyAsync(f => f.MovieId == dto.MovieId && f.UserId == userId);
            if (exists) return BadRequest(new { message = "Already in favorites" });

            var favorite = new Favorite
            {
                UserId = userId,
                MovieId = dto.MovieId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Added to favorites" });
        }

        // DELETE /api/favorites/{movieId}
        [HttpDelete("{movieId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromFavorites(int movieId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

            if (favorite == null) return NotFound(new { message = "Not in favorites" });

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Removed from favorites" });
        }
    }
}
