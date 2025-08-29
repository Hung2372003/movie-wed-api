using Microsoft.AspNetCore.Mvc;
using movie_wed_api.Services;

namespace movie_wed_api.Controllers
{

        [ApiController]
        [Route("api/[controller]")]
        public class VideoController : ControllerBase
        {
        private readonly MegaService _megaService;
        private readonly CloudinaryService _cloudinaryService;

        public VideoController(MegaService megaService, CloudinaryService cloudinaryService)
        {
            _megaService = megaService;
            _cloudinaryService = cloudinaryService;
        }


        [HttpPost("upload-video")]
        public async Task<IActionResult> UploadVideo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var url = await _cloudinaryService.UploadVideoAsync(file);
            return Ok(new { Url = url });
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var url = await _cloudinaryService.UploadImageAsync(file);
            return Ok(new { Url = url });
        }




    }
}
