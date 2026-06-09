namespace FresherMisa2026.Entities.SalaryComposition.DTO
{
    /// <summary>
    /// TPL đang tham chiếu Code của một TPL trong công thức của nó
    /// </summary>
    public record ReferencingCompositionInfo(Guid SalaryCompositionID, string Code, string Name);

    /// <summary>
    /// Thông tin gọn của một TPL trong kết quả exit-data
    /// </summary>
    public class ExitDataItem
    {
        public Guid SalaryCompositionID { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Source { get; set; }
        public int Status { get; set; }

        /// <summary>TPL đang dùng Code này trong công thức — chỉ có giá trị ở DataExist</summary>
        public List<ReferencingCompositionInfo> ReferencedBy { get; set; } = new();
    }

    /// <summary>
    /// Kết quả phân loại danh sách TPL khi kiểm tra trước khi xóa / ngừng theo dõi hàng loạt.
    /// Ưu tiên: DataSystem > DataExist > DataNotExist
    /// </summary>
    public class ExitDataResult
    {
        /// <summary>TPL kế thừa từ hệ thống (Source = 2) — không thể xóa, có thể ngừng theo dõi</summary>
        public List<ExitDataItem> DataSystem { get; set; } = new();

        /// <summary>TPL tự tạo đang được tham chiếu trong công thức — không thể xóa, ngừng theo dõi sẽ ảnh hưởng công thức</summary>
        public List<ExitDataItem> DataExist { get; set; } = new();

        /// <summary>TPL an toàn — có thể xóa hoặc ngừng theo dõi</summary>
        public List<ExitDataItem> DataNotExist { get; set; } = new();

        public long Total { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public long PageCount { get; set; }
    }
}
