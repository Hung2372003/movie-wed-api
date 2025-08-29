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

        public async Task<string> UploadFileAsync(string filePath, string folderName)
        {
            await _client.LoginAsync(_email, _password);

            var nodes = _client.GetNodes();
            var folder = nodes.FirstOrDefault(n => n.Type == NodeType.Directory && n.Name == folderName);

            // Nếu folder chưa có thì tạo
            if (folder == null)
            {
                var root = nodes.First(n => n.Type == NodeType.Root);
                folder = await _client.CreateFolderAsync(folderName, root);
            }

            using var stream = File.OpenRead(filePath);
            var node = await _client.UploadAsync(stream, Path.GetFileName(filePath), folder);

            var link = await _client.GetDownloadLinkAsync(node);
            await _client.LogoutAsync();

            return link.ToString();
        }


        // Tải file về dạng stream từ link Mega
        public async Task<Stream> DownloadFileAsync(string megaLink)
        {
            await _client.LoginAsync(_email, _password);

            var uri = new Uri(megaLink);
            var node = await _client.GetNodeFromLinkAsync(uri);

            // Lấy stream trực tiếp
            var stream = await _client.DownloadAsync(node);

            await _client.LogoutAsync();
            return stream;
        }

        public async Task<(Stream Stream, string FileName)> DownloadFileWithNameAsync(string megaLink)
        {
            await _client.LoginAsync(_email, _password);

            var uri = new Uri(megaLink);
            var node = await _client.GetNodeFromLinkAsync(uri);

            var stream = await _client.DownloadAsync(node);
            var fileName = node.Name;

            await _client.LogoutAsync();

            return (stream, fileName);
        }

        public async Task LoginAsync()
        {
            if (_client.IsLoggedIn) return;
            await _client.LoginAsync(_email, _password);
        }

        public async Task<(Uri DirectLink, string FileName)> GetDirectLinkAsync(string megaLink)
        {
            var uri = new Uri(megaLink);
            var node = await _client.GetNodeFromLinkAsync(uri);

            var directLink = await _client.GetDownloadLinkAsync(node);
            var fileName = node.Name;

            return (directLink, fileName);
        }
    }
}
