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
        public bool RequiresConfirmation { get; set; } = true;
        public List<InactiveCodeInfo> InactiveCodes { get; set; } = new();
    }
}
