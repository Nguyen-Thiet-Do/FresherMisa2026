using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Enums;

namespace FresherMisa2026.Entities.SalaryComposition.DTO
{
    public class SalaryCompositionAdvancedFilterRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Sort { get; set; }

        /// <summary>Phần 1 — từ khóa tìm kiếm</summary>
        public string? Search { get; set; }

        /// <summary>Phần 1 — danh sách trường search (OR). NULL = ['Code','Name']</summary>
        public List<string>? SearchFields { get; set; }

        /// <summary>Phần 2 — lọc theo trạng thái</summary>
        public SalaryCompositionStatus? Status { get; set; }

        /// <summary>Phần 3 — lọc theo danh sách đơn vị áp dụng</summary>
        public List<Guid>? OrganizationIDs { get; set; }

        /// <summary>Phần 4 — lọc nâng cao các trường (Contains, Between, In...)</summary>
        public List<FilterCondition>? Filters { get; set; }

        /// <summary>Logic nối các conditions ở Phần 4 — And (mặc định) hoặc Or</summary>
        public FilterLogic FilterLogic { get; set; } = FilterLogic.And;

        /// <summary>Danh sách tên property muốn lấy. NULL hoặc rỗng = trả tất cả cột.</summary>
        public List<string>? Columns { get; set; }
    }
}
