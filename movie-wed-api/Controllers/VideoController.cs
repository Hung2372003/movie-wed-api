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
            public async Task<IActionResult> UploadVideo(IFormFile file)
            {
                if (file == null || file.Length == 0)
                    return BadRequest("No file uploaded.");

                // Lưu file tạm vào server
                var filePath = Path.Combine(Path.GetTempPath(), file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Upload lên Mega
                var link = await _megaService.UploadFileAsync(filePath);

                // Xóa file tạm
                System.IO.File.Delete(filePath);

                return Ok(new { FileName = file.FileName, MegaLink = link });
            }
        
    }
}
