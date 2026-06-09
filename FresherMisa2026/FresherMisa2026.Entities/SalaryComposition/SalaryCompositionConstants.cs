namespace FresherMisa2026.Entities.SalaryComposition
{
    /// <summary>
    /// Hằng số nghiệp vụ cho module Thành phần lương.
    /// </summary>
    /// <remarks>Created by: ntdo — 03/06/2026</remarks>
    public static class SalaryCompositionConstants
    {
        /// <summary>Độ dài tối đa của trường Code (đồng bộ với VARCHAR(255) trong DB).</summary>
        public const int MaxCodeLength = 255;

        /// <summary>Độ dài tối đa của trường Name (đồng bộ với VARCHAR(255) trong DB).</summary>
        public const int MaxNameLength = 255;

        /// <summary>Pattern hợp lệ cho Code TPL: chữ, số và dấu gạch dưới.</summary>
        public const string CodePattern = @"^[A-Za-z0-9_]+$";

        /// <summary>UserID đặc biệt đại diện cho cấu hình mặc định của hệ thống — không cho ghi đè.</summary>
        public const string SystemUserId = "SYSTEM";

        /// <summary>Mã định danh connection string cho database tiền lương.</summary>
        public const string ConnectionName = "SalaryConnection";
    }
}
