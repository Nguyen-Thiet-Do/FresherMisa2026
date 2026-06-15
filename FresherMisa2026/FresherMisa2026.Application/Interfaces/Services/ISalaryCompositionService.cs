using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using System.Collections.Generic;

namespace FresherMisa2026.Application.Interfaces.Services
{
    /// <summary>
    /// Interface service cho SalaryComposition
    /// Created By: ntdo (2026-06-06)
    /// </summary>
    public interface ISalaryCompositionService : IBaseService<SalaryComposition>
    {
/// <summary>
        /// Chuyển TPL hệ thống sang TPL đơn vị (Source = InheritedFromSystem)
        /// </summary>
        /// Created By: ntdo (2026-06-06)
        Task<ServiceResponse> InheritFromSystemAsync(Guid systemCompositionId, List<Guid>? organizationIds);

        /// <summary>
        /// Chuyển nhiều TPL hệ thống sang TPL đơn vị — partial result
        /// </summary>
        /// Created By: ntdo (2026-06-06)
        Task<ServiceResponse> InheritFromSystemBatchAsync(InheritFromSystemBatchRequest request);

/// <summary>
        /// Chuyển trạng thái theo dõi của TPL (Active ↔ Inactive)
        /// </summary>
        /// Created By: ntdo (2026-06-06)
        Task<ServiceResponse> SetStatusAsync(Guid id, SalaryCompositionStatus status);

        /// <summary>
        /// Chuyển trạng thái nhiều TPL cùng lúc — partial result
        /// </summary>
        /// Created By: ntdo (2026-06-06)
        Task<ServiceResponse> SetStatusBulkAsync(List<Guid> ids, SalaryCompositionStatus status);


        /// <summary>Lọc nâng cao 4 phần qua stored procedure</summary>
        /// Created By: ntdo (2026-06-06)
        Task<ServiceResponse> AdvancedFilterWithProcAsync(SalaryCompositionAdvancedFilterRequest request);

        /// <summary>
        /// Phân loại danh sách TPL thành 3 nhóm trước khi xóa / ngừng theo dõi hàng loạt:
        /// DataSystem (hệ thống, không dùng trong công thức),
        /// DataExist (đang được tham chiếu trong công thức),
        /// DataNotExist (không hệ thống, không tham chiếu — an toàn).
        /// Có phân trang theo danh sách đầu vào.
        /// </summary>
        /// Created By: ntdo (2026-06-06)
        Task<ServiceResponse> ExitDataAsync(ExitDataRequest request);
    }
}
