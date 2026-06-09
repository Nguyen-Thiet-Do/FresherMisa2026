using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FresherMisa2026.Entities.Extensions
{
    /// <summary>
    /// Convention chuyển PascalCase ↔ snake_case cho tên cột/SP/param của DB.
    /// </summary>
    /// <remarks>Created by: ntdo — 2026-06-05</remarks>
    public static class Naming
    {
        private static readonly ConcurrentDictionary<string, string> _snakeCache = new();

        // Pre-pass: normalize acronym dạng "ID", "URL", "HTTP" → "Id", "Url", "Http"
        // để regex chính không tách "I" và "D" thành "i_d".
        // Match: 2+ chữ hoa liên tiếp, không kèm chữ thường trước nó (boundary của acronym)
        private static readonly Regex AcronymPattern = new(
            @"([A-Z]{2,})(?=[A-Z][a-z]|s?$|s?[A-Z][a-z]|$)",
            RegexOptions.Compiled);

        // Main pass: chèn _ giữa chữ thường/số và chữ hoa
        private static readonly Regex CamelBoundary = new(
            @"(?<=[a-z0-9])(?=[A-Z])",
            RegexOptions.Compiled);

        /// <summary>
        /// Chuyển PascalCase/camelCase → snake_case (lowercase + underscore).
        /// Xử lý acronym: "OrganizationID" → "organization_id", "HTTPResponse" → "http_response".
        /// </summary>
        public static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return _snakeCache.GetOrAdd(input, src =>
            {
                // Pre-pass: "ID" → "Id", "IDs" → "Ids", "HTTPResponse" → "HttpResponse"
                var normalized = AcronymPattern.Replace(src, m =>
                {
                    var acro = m.Value;
                    return acro[0] + acro.Substring(1).ToLowerInvariant();
                });

                // Main pass: chèn _ tại ranh giới camel
                var snake = CamelBoundary.Replace(normalized, "_");
                return snake.ToLowerInvariant();
            });
        }
    }
}
