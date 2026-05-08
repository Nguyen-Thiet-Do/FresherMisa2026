using FresherMisa2026.Entities.FileUpload;

namespace FresherMisa2026.Application.Interfaces.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Lưu file lên server, trả về URL tương đối để truy cập qua HTTP
        /// </summary>
        Task<string> SaveFileAsync(FileUploadRequest request, string subFolder);

        /// <summary>
        /// Xóa file khỏi server theo URL tương đối
        /// </summary>
        void DeleteFile(string? relativeUrl);
    }
}
