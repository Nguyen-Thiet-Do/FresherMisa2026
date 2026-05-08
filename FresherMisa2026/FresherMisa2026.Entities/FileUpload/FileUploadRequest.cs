namespace FresherMisa2026.Entities.FileUpload
{
    public class FileUploadRequest
    {
        public required Stream FileStream { get; init; }
        public required string FileName { get; init; }
        public long FileSize { get; init; }
    }
}
