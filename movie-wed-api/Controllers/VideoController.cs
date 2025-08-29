using Microsoft.AspNetCore.Mvc;
using movie_wed_api.Services;

namespace movie_wed_api.Controllers
{

        [ApiController]
        [Route("api/[controller]")]
        public class VideoController : ControllerBase
        {
        private readonly MegaService _megaService;

        public VideoController(MegaService megaService)
        {
            _megaService = megaService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadVideo(IFormFile file, [FromQuery] string folderName = "Videos")
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var link = await _megaService.UploadFileAsync(filePath, folderName);
            System.IO.File.Delete(filePath);

            return Ok(new { FileName = file.FileName, MegaLink = link });
        }


        [HttpGet("stream")]
        public async Task<IActionResult> StreamVideo([FromQuery] string megaLink)
        {
            if (string.IsNullOrEmpty(megaLink))
                return BadRequest("Missing megaLink");

            var stream = await _megaService.DownloadFileAsync(megaLink);

            // enableRangeProcessing = true -> ASP.NET sẽ trả 206 Partial Content khi cần
            return File(stream, "video/mp4", enableRangeProcessing: true);
        }
        // Stream với Range
      
    }
}
