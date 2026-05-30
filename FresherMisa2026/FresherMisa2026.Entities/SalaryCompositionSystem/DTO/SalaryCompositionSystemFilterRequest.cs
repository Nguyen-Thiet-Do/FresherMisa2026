using FresherMisa2026.Entities.Enums;

namespace FresherMisa2026.Entities.SalaryCompositionSystem.DTO
{
    /// <summary>
    /// Request lọc danh sách thành phần lương hệ thống
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class SalaryCompositionSystemFilterRequest
    {
        public string? Search { get; set; }

        public Guid? ComponentTypeID { get; set; }

        public SalaryNature? Nature { get; set; }

        public int PageSize { get; set; } = 10;

        public int PageIndex { get; set; } = 1;
    }
}
