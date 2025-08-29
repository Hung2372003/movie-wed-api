using Org.BouncyCastle.Tls;
using CG.Web.MegaApiClient;

namespace movie_wed_api.Services
{
    public class MegaService
    {
        private readonly string _email;
        private readonly string _password;
        private readonly MegaApiClient _client;

        public MegaService(IConfiguration config)
        {
            _email = config["Mega:Email"] ?? throw new ArgumentNullException("Mega:Email not found");
            _password = config["Mega:Password"] ?? throw new ArgumentNullException("Mega:Password not found");
            _client = new MegaApiClient();
        }

        public async Task<string> UploadFileAsync(string filePath)
        {
            await _client.LoginAsync(_email, _password);

            // Lấy node Root (thư mục gốc Mega)
            var root = _client.GetNodes().First(n => n.Type == NodeType.Root);

            // Upload
            using var stream = File.OpenRead(filePath);
            var node = await _client.UploadAsync(stream, Path.GetFileName(filePath), root);

            // Tạo link public
            var link = await _client.GetDownloadLinkAsync(node);

            await _client.LogoutAsync();
            return link.ToString();
        }
    }
}
