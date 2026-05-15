using System.Text.Json;
using System.Text.Json.Serialization;

namespace FresherMisa2026.Entities.AdvancedFilter
{
    public class FilterCondition
    {
        /// <summary>Tên property của entity (ví dụ: "EmployeeName", "Salary")</summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>Loại so sánh</summary>
        public FilterOperator Operator { get; set; }

        /// <summary>Giá trị so sánh — dùng cho hầu hết operators; với LastNDays/NextNDays truyền số nguyên</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonElement? Value { get; set; }

        /// <summary>Giá trị thứ hai — chỉ dùng cho Between và NotBetween</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonElement? ValueTo { get; set; }

        /// <summary>Danh sách giá trị — chỉ dùng cho In và NotIn</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<JsonElement>? Values { get; set; }
    }
}
