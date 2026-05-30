namespace FresherMisa2026.Entities.Enums
{
    /// <summary>
    /// Tính chất của thành phần lương
    /// </summary>
    public enum SalaryNature
    {
        /// <summary>Thu nhập (có thể chịu hoặc không chịu thuế TNCN)</summary>
        Income = 1,

        /// <summary>Khấu trừ (có thể được giảm trừ khi tính thuế TNCN)</summary>
        Deduction = 2,

        /// <summary>Thông tin (căn cứ tính lương: số người phụ thuộc, KPI%...)</summary>
        Information = 3,

        /// <summary>Khác</summary>
        Other = 4
    }
}
