using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.GridConfig.DTO
{
    /// <summary>
    /// Request batch upsert toàn bộ config cột của 1 user cho 1 lưới
    /// Created By: Nguyen Thiet Do (2026-05-28)
    /// </summary>
    public class GridConfigBatchRequest
    {
        [Required]
        public string UserID { get; set; } = string.Empty;

        [Required]
        public string GridCode { get; set; } = string.Empty;

        [Required]
        public List<GridConfigColumnDto> Columns { get; set; } = new();
    }
}
