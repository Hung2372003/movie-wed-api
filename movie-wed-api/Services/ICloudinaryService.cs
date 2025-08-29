namespace movie_wed_api.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadVideoAsync(IFormFile file);
        Task<string> UploadImageAsync(IFormFile file);
    }
}
