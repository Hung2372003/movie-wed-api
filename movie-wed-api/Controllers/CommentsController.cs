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
    public class CommentsController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public CommentsController(MovieDbContext context)
        {
            _context = context;
        }

        // GET /api/comments/movie/5?page=1&pageSize=10
        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetCommentsByMovie(int movieId, int page = 1, int pageSize = 10)
        {
            var query = _context.Comments
                .Include(c => c.User)
                .Where(c => c.MovieId == movieId)
                .OrderByDescending(c => c.CreatedAt);

            var total = await query.CountAsync();
            var comments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreatedAt,
                    c.UpdatedAt,
                    User = new { c.User.Id, c.User.Username }
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, data = comments });
        }

        // POST /api/comments/movie/5
        [HttpPost("movie/{movieId}")]
        [Authorize]
        public async Task<IActionResult> CreateComment(int movieId, CommentCreateDto dto)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound(new { message = "Movie not found" });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = new Comment
            {
                MovieId = movieId,
                UserId = userId,
                Content = dto.Content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                comment.Id,
                comment.Content,
                comment.CreatedAt,
                User = new { Id = userId, Username = User.Identity?.Name }
            });
        }

        // PUT /api/comments/10
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateComment(int id, CommentUpdateDto dto)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (comment.UserId != userId) return Forbid();

            comment.Content = dto.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        // DELETE /api/comments/10
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (comment.UserId != userId) return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
