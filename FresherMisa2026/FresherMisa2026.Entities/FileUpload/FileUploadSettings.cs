namespace FresherMisa2026.Entities.FileUpload
{
    public class FileUploadSettings
    {
        /// <summary>
        /// Đường dẫn tuyệt đối đến wwwroot — set bởi Program.cs khi khởi động
        /// </summary>
        public string BasePath { get; set; } = string.Empty;
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
        public string[] AllowedExtensions { get; set; } = [".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"];
    }
}
