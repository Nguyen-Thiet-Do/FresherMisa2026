using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities.FileUpload;
using Microsoft.Extensions.Options;

namespace FresherMisa2026.Infrastructure.Services
{
    public class FileService : IFileService
    {
        #region Declare
        private readonly FileUploadSettings _settings;
        #endregion

        #region Constructer
        public FileService(IOptions<FileUploadSettings> options)
        {
            _settings = options.Value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Lưu file lên server, trả về URL tương đối có thể truy cập qua HTTP
        /// </summary>
        /// Created By: Nguyen Thiet Do (09/05/2026)
        public async Task<string> SaveFileAsync(FileUploadRequest request, string subFolder)
        {
            ValidateFile(request);

            var targetFolder = Path.Combine(_settings.BasePath, "uploads", subFolder);
            Directory.CreateDirectory(targetFolder);

            var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(targetFolder, uniqueFileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await request.FileStream.CopyToAsync(stream);

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }

        /// <summary>
        /// Xóa file khỏi server theo URL tương đối
        /// </summary>
        /// Created By: Nguyen Thiet Do (09/05/2026)
        public void DeleteFile(string? relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;

            var relativePath = relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_settings.BasePath, relativePath);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        private void ValidateFile(FileUploadRequest request)
        {
            if (request.FileSize == 0)
                throw new ArgumentException("File không được rỗng");

            if (request.FileSize > _settings.MaxFileSizeBytes)
                throw new ArgumentException($"File vượt quá kích thước tối đa {_settings.MaxFileSizeBytes / 1024 / 1024}MB");

            var ext = Path.GetExtension(request.FileName).ToLowerInvariant();
            if (!_settings.AllowedExtensions.Contains(ext))
                throw new ArgumentException($"Định dạng không hỗ trợ. Chỉ chấp nhận: {string.Join(", ", _settings.AllowedExtensions)}");
        }
        #endregion
    }
}
