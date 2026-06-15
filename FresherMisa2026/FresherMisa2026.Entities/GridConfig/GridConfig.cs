using FresherMisa2026.Entities.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.GridConfig
{
    /// <summary>
    /// Cấu hình cột bảng theo từng người dùng (ghim, ẩn/hiện, độ rộng, thứ tự)
    /// Created By: ntdo (2026-06-03)
    /// </summary>
    [ConfigTable("pa_grid_config", false, "", useSnakeCase: true)]
    public class GridConfig : BaseModel
    {
        #region Declare

        [Key]
        public Guid GridConfigID { get; set; }

        [IRequired]
        [Display(Name = "ID người dùng")]
        public string UserID { get; set; } = string.Empty;

        /// <summary>Mã định danh lưới, ví dụ: SALARY_COMPOSITION_LIST</summary>
        [IRequired]
        [Display(Name = "Mã lưới")]
        public string GridCode { get; set; } = string.Empty;

        /// <summary>Tên cột, ví dụ: code, name, status</summary>
        [IRequired]
        [Display(Name = "Tên cột")]
        public string ColumnKey { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int OrderIndex { get; set; } = 0;

        /// <summary>Độ rộng cột (px)</summary>
        public int? Width { get; set; }

        public bool IsPinned { get; set; } = false;

        /// <summary>1 = ghim trái, 2 = ghim phải</summary>
        public int? PinPosition { get; set; }

        public bool IsVisible { get; set; } = true;

        #endregion
    }
}
