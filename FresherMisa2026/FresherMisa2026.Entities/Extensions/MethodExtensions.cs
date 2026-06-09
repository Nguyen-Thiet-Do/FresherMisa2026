using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FresherMisa2026.Entities.Extensions
{

    public static class MethodExtensions
    {
        /// <summary>
        /// Cache reverse-map snake_case column name → PropertyInfo cho mỗi entity type.
        /// Dùng cho Dapper CustomPropertyTypeMap và TranslateMySqlException.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _columnToPropertyCache = new();

        /// <summary>
        /// Lấy tên class
        /// </summary>
        /// <returns></returns>
        public static string GetTableName(this Type type)
        {
            var configTable = GetConfigTable(type);
            if (string.IsNullOrWhiteSpace(configTable.TableName))
            {
                throw new ArgumentException($"{type.Name} chưa khai báo [ConfigTable] hoặc TableName bị trống");
            }
            return configTable.TableName;
        }

        /// <summary>
        /// Lấy trường unique trong table
        /// </summary>
        /// <returns></returns>
        public static string GetUniqueColumns(this Type type)
        {
            var configTable = GetConfigTable(type);
            return configTable.UniqueColumns;
        }

        /// <summary>
        /// Entity có dùng convention snake_case cho DB column/SP param/SP name không?
        /// </summary>
        public static bool GetUseSnakeCase(this Type type)
            => GetConfigTable(type).UseSnakeCase;

        /// <summary>
        /// Lấy tên cột DB tương ứng với property C#. Snake_case nếu opt-in, PascalCase nếu không.
        /// </summary>
        public static string GetColumnName(this Type type, string propertyName)
            => GetConfigTable(type).UseSnakeCase ? Naming.ToSnakeCase(propertyName) : propertyName;

        /// <summary>
        /// Lấy tên cột khóa chính trong DB (snake_case nếu opt-in).
        /// </summary>
        public static string GetKeyColumn(this Type type)
            => type.GetColumnName(type.GetKeyName());

        /// <summary>
        /// Lấy tên cột "đã xóa mềm" trong DB ("is_deleted" hoặc "IsDeleted").
        /// </summary>
        public static string GetDeletedColumn(this Type type)
            => GetConfigTable(type).UseSnakeCase ? "is_deleted" : "IsDeleted";

        /// <summary>
        /// Tìm property tương ứng với tên cột DB (case-insensitive). Hoạt động với cả snake_case lẫn PascalCase column.
        /// Dùng cho Dapper CustomPropertyTypeMap (map kết quả SELECT) và TranslateMySqlException (parse constraint name).
        /// </summary>
        public static PropertyInfo? GetPropertyByColumnName(this Type type, string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) return null;
            var map = _columnToPropertyCache.GetOrAdd(type, BuildColumnToPropertyMap);
            return map.TryGetValue(columnName, out var prop) ? prop : null;
        }

        private static Dictionary<string, PropertyInfo> BuildColumnToPropertyMap(Type type)
        {
            var useSnake = GetConfigTable(type).UseSnakeCase;
            var map = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in type.GetProperties())
            {
                // PascalCase key luôn được index (fallback khi alias dùng PascalCase trực tiếp)
                map[prop.Name] = prop;
                if (useSnake)
                {
                    // snake_case key cho column name từ DB
                    map[Naming.ToSnakeCase(prop.Name)] = prop;
                }
            }
            return map;
        }

        /// <summary>
        /// Lấy tên trường hiển thị
        /// </summary>
        /// <returns></returns>
        public static string GetColumnDisplayName(this Type type, string name)
        {
            // Hỗ trợ cả property name (PascalCase) lẫn column name (snake_case).
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)
                       ?? type.GetPropertyByColumnName(name);

            if (prop == null) return name;

            var display = prop.GetCustomAttributes(typeof(DisplayAttribute), false)
                              .Cast<DisplayAttribute>()
                              .SingleOrDefault();
            return display?.Name ?? prop.Name;
        }

        /// <summary>
        /// Lấy trạng thái table có trường deleted không
        /// </summary>
        /// <returns></returns>
        public static bool GetHasDeletedColumn(this Type type)
        {
            var configTable = GetConfigTable(type);
            return configTable.HasDeletedColumn;
        }

        /// <summary>
        /// Lấy config table
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static ConfigTable GetConfigTable(this Type type)
        {
            var configTable = type.GetCustomAttributes(typeof(ConfigTable), true).FirstOrDefault() as ConfigTable;

            if (configTable == null)
            {
                configTable = new ConfigTable();
            }

            return configTable;
        }

        /// <summary>
        /// Lấy tên khóa chính
        /// </summary>
        /// <returns></returns>
        public static string GetKeyName(this Type type)
        {
            var propeties = type.GetProperties();
            var key = propeties.FirstOrDefault(f => f.IsDefined(typeof(KeyAttribute), true));

            if (key == null)
            {
                throw new ArgumentException($"{type.GetTableName()} Không có primarykey");
            }
            ;

            return key.Name;
        }
    }
}
