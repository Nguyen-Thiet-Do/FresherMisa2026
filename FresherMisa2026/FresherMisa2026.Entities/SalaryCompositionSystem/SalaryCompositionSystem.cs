using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.SalaryCompositionSystem
{
    /// <summary>
    /// Danh mục thành phần lương chuẩn của hệ thống (chỉ đọc với người dùng)
    /// Created By: Nguyen Thiet Do (2026-05-26)
    /// </summary>
    [ConfigTable("pa_salary_composition_system", false, "Code", useSnakeCase: true)]
    public class SalaryCompositionSystem : BaseModel
    {
        #region Declare

        [Key]
        public Guid SystemCompositionID { get; set; }

        [IRequired]
        [Display(Name = "Mã thành phần hệ thống")]
        public string Code { get; set; } = string.Empty;

        [IRequired]
        [Display(Name = "Tên thành phần hệ thống")]
        public string Name { get; set; } = string.Empty;

        public Guid? ComponentTypeID { get; set; }

        public string? ComponentTypeName { get; set; }

        public Guid? OrganizationID { get; set; }

        public string? OrganizationName { get; set; }

        [IRequired]
        [Display(Name = "Tính chất")]
        public SalaryNature Nature { get; set; }

        /// <summary>Loại thuế TNCN — chỉ có giá trị khi Nature = Income (BR-05)</summary>
        public SalaryTaxType? TaxType { get; set; }

        public bool TaxDeductible { get; set; } = false;

        public SalaryValueType ValueType { get; set; } = SalaryValueType.Currency;

        public string? ValueFormula { get; set; }

        public decimal? DefaultValue { get; set; }

        /// <summary>Công thức định mức (mức trần)</summary>
        public string? NormFormula { get; set; }

        /// <summary>Công thức phần chịu thuế TNCN — chỉ dùng khi TaxType = PartiallyExempt (3)</summary>
        public string? TaxableFormula { get; set; }

        /// <summary>Công thức phần miễn thuế TNCN — chỉ dùng khi TaxType = PartiallyExempt (3)</summary>
        public string? ExemptFormula { get; set; }

        public string? Description { get; set; }

        public bool ShowOnPayslip { get; set; } = true;

        /// <summary>
        /// Danh sách ID field bị khóa khi TPL được kế thừa (JSON array of int, ví dụ "[1,2,5]").
        /// ID tham chiếu <see cref="LockableField"/>. Null hoặc rỗng = chỉ Code bị khóa cứng.
        /// </summary>
        public string? LockedFields { get; set; }

        #endregion
    }
}
