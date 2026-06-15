using FresherMisa2026.Entities.Enums;

namespace FresherMisa2026.Entities.SalaryComposition.DTO
{
    /// <summary>
    /// Thông tin một thành phần lương ngừng theo dõi được tham chiếu trong công thức
    /// </summary>
    public record InactiveCodeInfo(string Code, string Name);

    /// <summary>
    /// Payload trả về khi công thức chứa TPL ngừng theo dõi — yêu cầu người dùng xác nhận trước khi lưu
    /// </summary>
    public class FormulaConfirmationRequired
    {
        public ConfirmationType Type { get; set; } = ConfirmationType.UnfollowedComposition;
        public List<InactiveCodeInfo> InactiveCodes { get; set; } = new();
    }

    /// <summary>
    /// Payload trả về khi mã TPL mới trùng với TPL hệ thống chưa được kế thừa
    /// </summary>
    public class SystemCodeConflictConfirmation
    {
        public ConfirmationType Type { get; set; } = ConfirmationType.SystemCodeConflict;
        public string Code { get; set; } = string.Empty;
        public Guid SystemCompositionID { get; set; }
        public string SystemCompositionName { get; set; } = string.Empty;
    }
}
