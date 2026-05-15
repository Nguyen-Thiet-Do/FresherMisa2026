using System.Text.Json.Serialization;

namespace FresherMisa2026.Entities.AdvancedFilter
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FilterOperator
    {
        // String
        Eq, Neq, Contains, NotContains, StartsWith, EndsWith, Empty, NotEmpty,
        // Number / Date
        Gt, Lt, Gte, Lte, Between, NotBetween,
        // Date presets — backend tự tính, FE không cần tính ngày
        Today, ThisWeek, ThisMonth, ThisYear, LastNDays, NextNDays,
        // Multi-value
        In, NotIn
    }
}
