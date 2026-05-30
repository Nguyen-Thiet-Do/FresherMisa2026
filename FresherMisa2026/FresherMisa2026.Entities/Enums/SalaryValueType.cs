namespace FresherMisa2026.Entities.Enums
{
    /// <summary>
    /// Kiểu giá trị của thành phần lương
    /// </summary>
    public enum SalaryValueType
    {
        /// <summary>Tiền tệ (VNĐ)</summary>
        Currency = 1,

        /// <summary>Số (nguyên hoặc thập phân)</summary>
        Number = 2,

        /// <summary>Phần trăm (%)</summary>
        Percentage = 3
    }
}
