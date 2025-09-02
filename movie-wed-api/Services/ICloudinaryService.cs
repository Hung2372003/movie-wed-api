namespace movie_wed_api.Services
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file);
        Task<(string Url, string PublicId)> UploadVideoAsync(IFormFile file);
        Task DeleteFileAsync(string publicId);
    }
}
