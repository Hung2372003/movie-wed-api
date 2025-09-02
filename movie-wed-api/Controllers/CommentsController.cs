using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movie_wed_api.Database;
using movie_wed_api.Models;

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

        // GET /api/comments/movie/5
        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetCommentsByMovie(int movieId)
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.MovieId == movieId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        // POST /api/comments/movie/5
        [HttpPost("movie/{movieId}")]
        [Authorize]
        public async Task<IActionResult> CreateComment(int movieId, [FromBody] string content)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound(new { message = "Movie not found" });

            var comment = new Comment
            {
                UserId = userId,
                MovieId = movieId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        // DELETE /api/comments/10
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            if (comment.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
