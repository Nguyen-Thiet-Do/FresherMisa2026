using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Entities.SalaryComposition.DTO;

namespace FresherMisa2026.Application.Interfaces.Repositories
{
    /// <summary>
    /// Interface repository cho SalaryComposition
    /// Created By: ntdo (2026-06-05)
    /// </summary>
    public interface ISalaryCompositionRepository : IBaseRepository<SalaryComposition>
    {
        /// <summary>Lọc nâng cao 4 phần qua stored procedure</summary>
        /// Created By: ntdo (2026-06-05)
        Task<(IEnumerable<SalaryComposition> Data, long Total)> AdvancedFilterWithProcAsync(SalaryCompositionAdvancedFilterRequest request);

        /// <summary>Đồng bộ danh sách đơn vị áp dụng (junction table) cho 1 TPL — xóa cũ rồi insert batch mới.</summary>
        /// Created By: ntdo (2026-06-05)
        Task<int> UpdateOrganizationIDsAsync(Guid compositionId, List<Guid> organizationIds);

        /// <summary>Kiểm tra có TPL kế thừa từ hệ thống với Code cho trước chưa.</summary>
        /// Created By: ntdo (2026-06-05)
        Task<bool> ExistsInheritedByCodeAsync(string code);

        /// <summary>
        /// Lấy Code, Name, Status của các TPL khớp với danh sách code cho trước.
        /// Dùng để validate công thức mà không fetch toàn bảng.
        /// </summary>
        /// Created By: ntdo (2026-06-05)
        Task<IEnumerable<(string Code, string Name, SalaryCompositionStatus Status)>>
            GetCodeInfoByCodesAsync(IEnumerable<string> codes);

/// <summary>
        /// Kiểm tra mã TPL có đang được tham chiếu trong công thức của bất kỳ TPL nào khác không.
        /// Dùng MySQL REGEXP word-boundary thay vì fetch toàn bảng.
        /// </summary>
        /// Created By: ntdo (2026-06-05)
        Task<bool> IsCodeReferencedInFormulasAsync(string code, Guid excludeId);
    }
}
