namespace FresherMisa2026.Entities.Enums
{
    /// <summary>
    /// Loại thuế TNCN của thành phần lương (chỉ áp dụng khi Nature = Income)
    /// </summary>
    public enum SalaryTaxType
    {
        /// <summary>Chịu thuế TNCN</summary>
        Taxable = 1,

        /// <summary>Miễn toàn phần</summary>
        FullyExempt = 2,

        /// <summary>Miễn một phần (theo chứng từ thực tế)</summary>
        PartiallyExempt = 3
    }
}
