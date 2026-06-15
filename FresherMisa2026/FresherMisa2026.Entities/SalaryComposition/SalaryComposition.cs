using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Extensions;
using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.SalaryComposition
{
    /// <summary>
    /// Thành phần lương của đơn vị — bảng nghiệp vụ trung tâm
    /// Created By: ntdo (2026-06-02)
    /// </summary>
    [ConfigTable("pa_salary_composition", true, "Code", useSnakeCase: true)]
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

        /// <summary>Danh sách đơn vị áp dụng — lưu trong junction table pa_salary_composition_organization</summary>
        public List<Guid>? OrganizationIDs { get; set; }

        public string? OrganizationNames { get; set; }

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

        /// <summary>Phạm vi nhân viên khi ValueMode = AutoSum</summary>
        public SalaryAutoSumScope? ValueScope { get; set; }

        /// <summary>Cấp tổ chức khi ValueScope = OrgStructure (1=Cấp 1, 2=Cấp 2, ...)</summary>
        public byte? ValueScopeLevel { get; set; }

        /// <summary>FK tới TPL cần cộng tổng khi ValueMode = AutoSum</summary>
        public Guid? SumSourceCompositionID { get; set; }

        /// <summary>Công thức định mức — mức trần của khoản lương</summary>
        public string? NormFormula { get; set; }

        /// <summary>Công thức phần chịu thuế TNCN — chỉ dùng khi TaxType = PartiallyExempt (3)</summary>
        public string? TaxableFormula { get; set; }

        /// <summary>Công thức phần miễn thuế TNCN — chỉ dùng khi TaxType = PartiallyExempt (3)</summary>
        public string? ExemptFormula { get; set; }

        /// <summary>Ghi nhớ công thức nào được tự suy — dùng khi update để re-derive đúng khi ValueFormula thay đổi</summary>
        public TaxFormulaSource TaxFormulaSource { get; set; } = TaxFormulaSource.None;

        /// <summary>BR-09: Cho phép vượt định mức</summary>
        public bool AllowExceedNorm { get; set; } = false;

        public string? Description { get; set; }

        public bool ShowOnPayslip { get; set; } = true;

        public bool HideWhenZero { get; set; } = false;

        public SalaryCompositionSource Source { get; set; } = SalaryCompositionSource.Custom;

        /// <summary>BR-07: Ngừng theo dõi thay vì xóa khi không còn dùng</summary>
        public SalaryCompositionStatus Status { get; set; } = SalaryCompositionStatus.Active;

        /// <summary>
        /// Không lưu DB — FE truyền true khi người dùng xác nhận lưu dù công thức có TPL ngừng theo dõi.
        /// SP bỏ qua param thừa nên không ảnh hưởng.
        /// </summary>
        public bool IsSkipUnfollowedComposition { get; set; } = false;

        /// <summary>
        /// Không lưu DB — FE truyền true khi người dùng chọn "vẫn thêm mới bình thường"
        /// dù mã trùng với TPL hệ thống chưa kế thừa.
        /// </summary>
        public bool IsSkipSystemCodeCheck { get; set; } = false;

        /// <summary>
        /// Snapshot danh sách ID field bị khóa, copy từ TPL hệ thống tại thời điểm kế thừa.
        /// Lưu dạng JSON array of int (ví dụ "[1,2,5]"). Tham chiếu <see cref="LockableField"/>.
        /// Null/rỗng khi Source = Custom (không bị khóa field nào ngoài Code).
        /// </summary>
        public string? LockedFields { get; set; }

        #endregion
    }
}
