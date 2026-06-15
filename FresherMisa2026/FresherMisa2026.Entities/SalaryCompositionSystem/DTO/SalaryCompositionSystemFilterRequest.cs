using FresherMisa2026.Entities.Enums;

namespace FresherMisa2026.Entities.SalaryCompositionSystem.DTO
{
    /// <summary>
    /// Request lọc danh sách thành phần lương hệ thống
    /// Created By: ntdo (2026-06-02)
    /// </summary>
    public class SalaryCompositionSystemFilterRequest
    {
        public string? Search { get; set; }

        public Guid? ComponentTypeID { get; set; }

        public SalaryNature? Nature { get; set; }

        public int PageSize { get; set; } = 10;

        public int PageIndex { get; set; } = 1;

        /// <summary>true = ẩn TPL đã được đơn vị kế thừa (mặc định). false = hiện tất cả.</summary>
        public bool ExcludeInherited { get; set; } = true;
    }
}
