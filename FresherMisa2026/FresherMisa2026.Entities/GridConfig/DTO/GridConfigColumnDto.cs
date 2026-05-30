namespace FresherMisa2026.Entities.GridConfig.DTO
{
    /// <summary>
    /// Dữ liệu một cột trong request batch upsert grid config
    /// Created By: Nguyen Thiet Do (2026-05-28)
    /// </summary>
    public class GridConfigColumnDto
    {
        public string ColumnKey { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int OrderIndex { get; set; } = 0;

        public int? Width { get; set; }

        public bool IsPinned { get; set; } = false;

        public int? PinPosition { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}
