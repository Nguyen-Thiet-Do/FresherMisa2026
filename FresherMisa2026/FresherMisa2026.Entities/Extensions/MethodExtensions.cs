using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FresherMisa2026.Entities.Extensions
{

    public static class MethodExtensions
    {
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
        /// Lấy tên trường hiển thị
        /// </summary>
        /// <returns></returns>
        public static string GetColumnDisplayName(this Type type, string name)
        {
            var obj = type.GetProperty(name).GetCustomAttributes(typeof(DisplayAttribute),
                                               false).Cast<DisplayAttribute>().SingleOrDefault();
            if (obj == null) return name;

            return obj.Name;
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
