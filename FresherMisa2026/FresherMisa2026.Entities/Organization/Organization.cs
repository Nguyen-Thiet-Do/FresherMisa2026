using FresherMisa2026.Entities.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.Organization
{
    /// <summary>
    /// Đơn vị công tác / cơ cấu tổ chức
    /// Created By: ntdo (2026-06-01)
    /// </summary>
    [ConfigTable("pa_organization", true, "Code", useSnakeCase: true)]
    public class Organization : BaseModel
    {
        #region Declare

        [Key]
        public Guid OrganizationID { get; set; }

        [IRequired]
        [Display(Name = "Mã đơn vị")]
        public string Code { get; set; } = string.Empty;

        [IRequired]
        [Display(Name = "Tên đơn vị")]
        public string Name { get; set; } = string.Empty;

        /// <summary>ID đơn vị cha (null nếu là gốc)</summary>
        public Guid? ParentID { get; set; }

        /// <summary>Materialized path, ví dụ: /CTY/KD/</summary>
        public string? Path { get; set; }

        public int? SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        #endregion
    }
}
