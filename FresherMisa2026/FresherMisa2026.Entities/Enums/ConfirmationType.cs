namespace FresherMisa2026.Entities.Enums
{
    /// <summary>Loại xác nhận cần người dùng phê duyệt trước khi lưu TPL.</summary>
    public enum ConfirmationType
    {
        /// <summary>Công thức chứa TPL ngừng theo dõi — yêu cầu xác nhận tiếp tục lưu.</summary>
        UnfollowedComposition = 1,

        /// <summary>Mã TPL mới trùng với TPL hệ thống chưa kế thừa — gợi ý kế thừa thay vì tạo mới.</summary>
        SystemCodeConflict = 2,
    }
}
