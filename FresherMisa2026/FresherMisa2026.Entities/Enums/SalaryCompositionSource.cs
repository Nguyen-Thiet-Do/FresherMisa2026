namespace FresherMisa2026.Entities.Enums
{
    /// <summary>
    /// Nguồn gốc của thành phần lương đơn vị
    /// </summary>
    public enum SalaryCompositionSource
    {
        /// <summary>Tự thêm mới hoàn toàn</summary>
        Custom = 1,

        /// <summary>Kế thừa từ danh mục hệ thống</summary>
        InheritedFromSystem = 2
    }
}
