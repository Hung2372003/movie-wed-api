using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using movie_wed_api.Database;
using movie_wed_api.DTOs.Episodes;
using movie_wed_api.Models;
using movie_wed_api.Services;

namespace movie_wed_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EpisodesController : ControllerBase
    {
        private readonly MovieDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public EpisodesController(MovieDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        [HttpGet("movie/all")]
        public async Task<IActionResult> GetEpisodesAllMovie()
        {
            var episodes = await _context.Episodes         
                .OrderBy(e => e.EpisodeNumber)
                .ToListAsync();

            return Ok(episodes);
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateEpisode(int movieId, [FromForm] EpisodeCreateDto dto)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
                return NotFound(new { message = "Movie not found" });

            // Nếu không truyền EpisodeNumber thì tự động lấy số lớn nhất + 1
            int episodeNumber =  dto.EpisodeNumber > 0 
                ? dto.EpisodeNumber
                : await _context.Episodes
                    .Where(e => e.MovieId == movieId)
                    .Select(e => e.EpisodeNumber)
                    .DefaultIfEmpty(0)
                    .MaxAsync() + 1;

            // Check trùng trước khi thêm
            bool exists = await _context.Episodes
                .AnyAsync(e => e.MovieId == movieId && e.EpisodeNumber == episodeNumber);

            if (exists)
            {
                return Conflict(new { message = $"Episode {episodeNumber} already exists for this movie." });
            }

            var episode = new Episode
            {
                MovieId = movieId,
                EpisodeNumber = episodeNumber,
                Title = dto.Title,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Video != null)
            {
                var uploadResult = await _cloudinaryService.UploadVideoAsync(dto.Video);
                episode.VideoUrl = uploadResult.Url;
                episode.VideoPublicId = uploadResult.PublicId;
            }

            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEpisode), new { id = episode.Id }, new
            {
                Id = episode.Id,
                MovieId = episode.MovieId,
                EpisodeNumber = episode.EpisodeNumber,
                Title = episode.Title,
                VideoUrl = episode.VideoUrl,
                CreatedAt = episode.CreatedAt
            });
        }


        // PUT /api/episodes/10
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateEpisode(int id, [FromForm] EpisodeUpdateDto dto)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();

            episode.EpisodeNumber = dto.EpisodeNumber;
            episode.Title = dto.Title;
            episode.UpdatedAt = DateTime.UtcNow;

            if (dto.Video != null)
            {
                // xóa video cũ
                if (!string.IsNullOrEmpty(episode.VideoPublicId))
                    await _cloudinaryService.DeleteFileAsync(episode.VideoPublicId);

                var uploadResult = await _cloudinaryService.UploadVideoAsync(dto.Video);
                episode.VideoUrl = uploadResult.Url;
                episode.VideoPublicId = uploadResult.PublicId;
            }

            await _context.SaveChangesAsync();
            return Ok(episode);
        }

        // DELETE /api/episodes/10
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEpisode(int id)
        {
            var episode = await _context.Episodes.FindAsync(id);
            if (episode == null) return NotFound();

            if (!string.IsNullOrEmpty(episode.VideoPublicId))
                await _cloudinaryService.DeleteFileAsync(episode.VideoPublicId);

            _context.Episodes.Remove(episode);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
