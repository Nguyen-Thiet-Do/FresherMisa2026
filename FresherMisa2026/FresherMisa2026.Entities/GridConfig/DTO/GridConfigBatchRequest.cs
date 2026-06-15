using System.ComponentModel.DataAnnotations;

namespace FresherMisa2026.Entities.GridConfig.DTO
{
    /// <summary>
    /// Request batch upsert toàn bộ config cột của 1 user cho 1 lưới
    /// Created By: ntdo (2026-06-03)
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
