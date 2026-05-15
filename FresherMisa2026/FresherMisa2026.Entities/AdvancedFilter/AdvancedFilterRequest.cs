namespace FresherMisa2026.Entities.AdvancedFilter
{
    public class AdvancedFilterRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        /// <summary>Sắp xếp — ví dụ: "-Salary,+EmployeeName"</summary>
        public string? Sort { get; set; }

        /// <summary>Danh sách điều kiện lọc — null hoặc rỗng thì trả toàn bộ có phân trang</summary>
        public List<FilterCondition>? Filters { get; set; }
    }
}
