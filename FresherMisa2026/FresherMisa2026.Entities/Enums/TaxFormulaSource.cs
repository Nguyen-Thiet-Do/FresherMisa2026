namespace FresherMisa2026.Entities.Enums;

/// <summary>
/// Ghi nhớ công thức nào được tự suy trong cặp TaxableFormula / ExemptFormula.
/// Dùng khi update để re-derive đúng khi ValueFormula thay đổi.
/// </summary>
public enum TaxFormulaSource
{
    None = 0,
    ExemptDerived = 1,
    TaxableDerived = 2
}
