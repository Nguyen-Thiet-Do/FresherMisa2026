using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.SalaryComposition
{
    /// <summary>
    /// Thành phần lương của đơn vị — bảng nghiệp vụ trung tâm
    /// Created By: Nguyen Thiet Do (2026-05-26)
    /// </summary>
    [ConfigTable("pa_salary_composition", true, "Code")]
    public class SalaryComposition : BaseModel
    {
        #region Declare

        [Key]
        public Guid SalaryCompositionID { get; set; }

        /// <summary>BR-01: Mã không được sửa sau khi lưu</summary>
        [IRequired]
        [Display(Name = "Mã thành phần lương")]
        [NotPatchable]
        public string Code { get; set; } = string.Empty;

        [IRequired]
        [Display(Name = "Tên thành phần lương")]
        public string Name { get; set; } = string.Empty;

        public Guid? OrganizationID { get; set; }

        public string? OrganizationName { get; set; }

        [IRequired]
        [Display(Name = "Loại thành phần")]
        public Guid ComponentTypeID { get; set; }

        public string? ComponentTypeName { get; set; }

        /// <summary>Null nếu tự tạo hoàn toàn, có giá trị nếu kế thừa từ hệ thống</summary>
        public Guid? SystemCompositionID { get; set; }

        [IRequired]
        [Display(Name = "Tính chất")]
        public SalaryNature Nature { get; set; }

        /// <summary>BR-05: Chỉ có giá trị khi Nature = Income</summary>
        public SalaryTaxType? TaxType { get; set; }

        /// <summary>Giá trị khoản này được khấu trừ khỏi thu nhập tính thuế TNCN</summary>
        public bool TaxDeductible { get; set; } = false;

        public SalaryValueType ValueType { get; set; } = SalaryValueType.Currency;

        public SalaryValueMode ValueMode { get; set; } = SalaryValueMode.AutoSum;

        /// <summary>Công thức tính giá trị (lưu nguyên văn chuỗi)</summary>
        public string? ValueFormula { get; set; }

        public int? ValueScope { get; set; }

        /// <summary>Công thức định mức — mức trần của khoản lương</summary>
        public string? NormFormula { get; set; }

        /// <summary>BR-09: Cho phép vượt định mức</summary>
        public bool AllowExceedNorm { get; set; } = false;

        public string? Description { get; set; }

        public bool ShowOnPayslip { get; set; } = true;

        public bool HideWhenZero { get; set; } = false;

        public SalaryCompositionSource Source { get; set; } = SalaryCompositionSource.Custom;

        /// <summary>BR-07: Ngừng theo dõi thay vì xóa khi không còn dùng</summary>
        public SalaryCompositionStatus Status { get; set; } = SalaryCompositionStatus.Active;

        #endregion
    }
}
