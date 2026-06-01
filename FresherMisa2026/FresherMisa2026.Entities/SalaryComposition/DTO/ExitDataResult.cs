namespace FresherMisa2026.Entities.SalaryComposition.DTO
{
    /// <summary>
    /// Kết quả phân loại danh sách TPL khi kiểm tra trước khi xóa / ngừng theo dõi hàng loạt.
    /// </summary>
    public class ExitDataResult
    {
        /// <summary>TPL kế thừa từ hệ thống (Source = InheritedFromSystem) và không đang được tham chiếu trong công thức</summary>
        public List<SalaryComposition> DataSystem { get; set; } = new();

        /// <summary>TPL đang được tham chiếu trong công thức của TPL khác (kể cả TPL hệ thống)</summary>
        public List<SalaryComposition> DataExist { get; set; } = new();

        /// <summary>TPL không phải hệ thống và không đang được tham chiếu — an toàn để xóa / ngừng theo dõi</summary>
        public List<SalaryComposition> DataNotExist { get; set; } = new();

        public long Total { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public long PageCount { get; set; }
    }
}
