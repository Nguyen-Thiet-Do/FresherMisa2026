using System.Text.Json.Serialization;

namespace FresherMisa2026.Entities;

/// <summary>Vị trí của một mã không tồn tại trong công thức (FE dùng để highlight).</summary>
public record MissingCodeInfo(string Code, int Position, int Length);

/// <summary>Lỗi validate một trường.</summary>
public record ValidationError(string Field, string Message)
{
    /// <summary>
    /// Vị trí các mã TPL không tồn tại trong công thức (FE dùng để highlight đỏ trong input).
    /// Chỉ có ý nghĩa với lỗi công thức; null/không xuất hiện cho mọi loại lỗi khác.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<MissingCodeInfo>? MissingCodes { get; init; }
}
