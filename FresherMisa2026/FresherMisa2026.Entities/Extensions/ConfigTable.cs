using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Entities.Extensions
{
    public class ConfigTable : Attribute
    {
        public bool HasDeletedColumn { get; set; } = false;

        public string UniqueColumns { get; set; } = string.Empty;

        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// True → DB column/SP param/SP name dùng snake_case (vd `OrganizationID` → `organization_id`).
        /// False (mặc định) → giữ PascalCase như cũ.
        /// </summary>
        public bool UseSnakeCase { get; set; } = false;

        public ConfigTable(string tableName = "", bool hasDeletedColumn = false, string uniqueColumns = "", bool useSnakeCase = false)
        {
            TableName = tableName;

            HasDeletedColumn = hasDeletedColumn;

            UniqueColumns = uniqueColumns;

            UseSnakeCase = useSnakeCase;
        }
    }
}
