using System.Collections.Generic;

namespace FresherMisa2026.Entities.Enums
{
    /// <summary>
    /// Mã ổn định của các field có thể bị khóa khi TPL kế thừa hệ thống.
    /// ID là số nguyên cố định lưu xuống DB (JSON array trên cột LockedFields).
    /// KHÔNG được tái sử dụng / đổi giá trị — chỉ thêm mới ở cuối.
    /// Code luôn bị khóa cứng nên không có trong enum này.
    /// </summary>
    /// <remarks>Created by: ntdo — 05/06/2026</remarks>
    public enum LockableField
    {
        ComponentTypeID         = 1,
        Nature                  = 2,
        TaxType                 = 3,
        TaxDeductible           = 4,
        ValueType               = 5,
        ValueMode               = 6,
        ValueFormula            = 7,
        ValueScope              = 8,
        ValueScopeLevel         = 9,
        SumSourceCompositionID  = 10,
        NormFormula             = 11,
        TaxableFormula          = 12,
        ExemptFormula           = 13,
        AllowExceedNorm         = 14,
        HideWhenZero            = 15,
        Name                    = 16,
        Description             = 17,
        ShowOnPayslip           = 18,
        OrganizationIDs         = 19,
        Status                  = 20,
    }

    /// <summary>
    /// Metadata của 1 LockableField — gắn liền tên hiển thị (tiếng Việt) dùng cho error message.
    /// Đây là single source of truth: thêm field khóa mới chỉ cần khai báo ở đây.
    /// </summary>
    public sealed record LockableFieldInfo(LockableField Id, string DisplayName);

    /// <summary>
    /// Registry tra cứu metadata của LockableField theo Id (int).
    /// </summary>
    public static class LockableFieldRegistry
    {
        public static readonly IReadOnlyDictionary<int, LockableFieldInfo> ById =
            new Dictionary<int, LockableFieldInfo>
            {
                [(int)LockableField.ComponentTypeID]        = new(LockableField.ComponentTypeID,        "Loại thành phần"),
                [(int)LockableField.Nature]                 = new(LockableField.Nature,                 "Tính chất"),
                [(int)LockableField.TaxType]                = new(LockableField.TaxType,                "Loại thuế TNCN"),
                [(int)LockableField.TaxDeductible]          = new(LockableField.TaxDeductible,          "Giảm trừ thuế"),
                [(int)LockableField.ValueType]              = new(LockableField.ValueType,              "Kiểu giá trị"),
                [(int)LockableField.ValueMode]              = new(LockableField.ValueMode,              "Chế độ tính"),
                [(int)LockableField.ValueFormula]           = new(LockableField.ValueFormula,           "Công thức giá trị"),
                [(int)LockableField.ValueScope]             = new(LockableField.ValueScope,             "Phạm vi cộng tổng"),
                [(int)LockableField.ValueScopeLevel]        = new(LockableField.ValueScopeLevel,        "Cấp phạm vi"),
                [(int)LockableField.SumSourceCompositionID] = new(LockableField.SumSourceCompositionID, "TPL nguồn AutoSum"),
                [(int)LockableField.NormFormula]            = new(LockableField.NormFormula,            "Công thức định mức"),
                [(int)LockableField.TaxableFormula]         = new(LockableField.TaxableFormula,         "Công thức phần chịu thuế"),
                [(int)LockableField.ExemptFormula]          = new(LockableField.ExemptFormula,          "Công thức phần miễn thuế"),
                [(int)LockableField.AllowExceedNorm]        = new(LockableField.AllowExceedNorm,        "Cho phép vượt định mức"),
                [(int)LockableField.HideWhenZero]           = new(LockableField.HideWhenZero,           "Ẩn khi bằng 0"),
                [(int)LockableField.Name]                   = new(LockableField.Name,                   "Tên thành phần"),
                [(int)LockableField.Description]            = new(LockableField.Description,            "Mô tả"),
                [(int)LockableField.ShowOnPayslip]          = new(LockableField.ShowOnPayslip,          "Hiển thị trên phiếu lương"),
                [(int)LockableField.OrganizationIDs]        = new(LockableField.OrganizationIDs,        "Đơn vị áp dụng"),
                [(int)LockableField.Status]                 = new(LockableField.Status,                 "Trạng thái"),
            };

        public static string GetDisplayName(int id)
            => ById.TryGetValue(id, out var info) ? info.DisplayName : $"Field#{id}";
    }
}
