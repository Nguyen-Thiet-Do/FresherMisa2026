namespace FresherMisa2026.Entities.Extensions
{
    /// <summary>
    /// Đánh dấu property không được phép cập nhật qua API PATCH single field.
    /// Dùng cho các trường nhạy cảm/bảo mật (ví dụ: PasswordHash, SecurityStamp).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NotPatchable : Attribute { }
}
