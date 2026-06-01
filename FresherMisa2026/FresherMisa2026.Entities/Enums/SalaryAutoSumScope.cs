namespace FresherMisa2026.Entities.Enums
{
    /// <summary>
    /// Phạm vi nhân viên được cộng tổng khi ValueMode = AutoSum
    /// </summary>
    public enum SalaryAutoSumScope
    {
        /// <summary>Trong cùng đơn vị công tác</summary>
        SameWorkUnit = 1,

        /// <summary>Dưới quyền (cấp dưới trực tiếp và gián tiếp)</summary>
        Subordinates = 2,

        /// <summary>Thuộc cơ cấu tổ chức — kết hợp với ValueScopeLevel để xác định cấp</summary>
        OrgStructure = 3
    }
}
