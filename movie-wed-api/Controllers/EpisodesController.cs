using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movie_wed_api.Database;
using movie_wed_api.DTOs.Episodes;
using movie_wed_api.Models;

namespace movie_wed_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EpisodesController : ControllerBase
    {
        private readonly MovieDbContext _context;

        public EpisodesController(MovieDbContext context)
        {
            _context = context;
        }

        // GET /api/episodes/movie/5
        [HttpGet("movie/{movieId}")]
        public async Task<IActionResult> GetEpisodesByMovie(int movieId)
        {
            var episodes = await _context.Episodes
                .Where(e => e.MovieId == movieId)
                .OrderBy(e => e.EpisodeNumber)
                .ToListAsync();

            return Ok(episodes);
        }

        // GET /api/episodes/10
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEpisode(int id)
        {
            var episode = await _context.Episodes
                .Include(e => e.Movie)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (episode == null) return NotFound();
            return Ok(episode);
        }

        // POST /api/episodes/movie/5
        [HttpPost("movie/{movieId}")]
        [Authorize] // 🔒 bạn có thể thay bằng [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEpisode(int movieId, EpisodeCreateDto dto)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null) return NotFound(new { message = "Movie not found" });

            var episode = new Episode
            {
                MovieId = movieId,
                EpisodeNumber = dto.EpisodeNumber,
                Title = dto.Title,
                VideoUrl = dto.VideoUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEpisode), new { id = episode.Id }, episode);
        }

        // PUT /api/episodes/10
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEpisode(int id, EpisodeUpdateDto dto)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();

            episode.EpisodeNumber = dto.EpisodeNumber;
            episode.Title = dto.Title;
            episode.VideoUrl = dto.VideoUrl;
            episode.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(episode);
        }

        // DELETE /api/episodes/10
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteEpisode(int id)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();

            _context.Episodes.Remove(episode);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
