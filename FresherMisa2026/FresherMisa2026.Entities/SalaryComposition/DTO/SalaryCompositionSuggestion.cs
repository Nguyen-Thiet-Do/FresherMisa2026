namespace FresherMisa2026.Entities.SalaryComposition.DTO
{
    /// <summary>
    /// Gợi ý thành phần lương cho ô nhập công thức
    /// Created By: Nguyen Thiet Do (2026-05-27)
    /// </summary>
    public class SalaryCompositionSuggestion
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
