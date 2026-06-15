using FresherMisa2026.Entities.AdvancedFilter;

namespace FresherMisa2026.Entities.SalaryCompositionSystem.DTO
{
    /// <summary>
    /// Request lọc nâng cao danh mục TPL hệ thống — hỗ trợ 3 phần: tìm kiếm, lọc điều kiện động và projection cột.
    /// </summary>
    /// <remarks>Created By: ntdo (2026-06-02)</remarks>
    public class SalaryCompositionSystemAdvancedFilterRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Sort { get; set; }
        public string? Search { get; set; }

        /// <summary>Danh sách trường search (OR). NULL = ['Code','Name']</summary>
        public List<string>? SearchFields { get; set; }

        public Guid? ComponentTypeID { get; set; }
        public List<FilterCondition>? Filters { get; set; }
        public FilterLogic FilterLogic { get; set; } = FilterLogic.And;

        /// <summary>Danh sách tên property muốn lấy. NULL hoặc rỗng = trả tất cả cột.</summary>
        public List<string>? Columns { get; set; }

        /// <summary>true = ẩn TPL đã được đơn vị kế thừa (mặc định). false = hiện tất cả.</summary>
        public bool ExcludeInherited { get; set; } = true;
    }
}
