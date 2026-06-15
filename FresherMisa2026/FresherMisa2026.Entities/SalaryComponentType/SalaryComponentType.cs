using FresherMisa2026.Entities.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.SalaryComponentType
{
    /// <summary>
    /// Loại thành phần lương (Lương, Phụ cấp, Bảo hiểm, Khấu trừ...)
    /// Created By: ntdo (2026-06-01)
    /// </summary>
    [ConfigTable("pa_salary_component_type", false, "Code", useSnakeCase: true)]
    public class SalaryComponentType : BaseModel
    {
        #region Declare

        [Key]
        public Guid ComponentTypeID { get; set; }

        [IRequired]
        [Display(Name = "Mã loại thành phần")]
        public string Code { get; set; } = string.Empty;

        [IRequired]
        [Display(Name = "Tên loại thành phần")]
        public string Name { get; set; } = string.Empty;

        public int? SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        #endregion
    }
}
