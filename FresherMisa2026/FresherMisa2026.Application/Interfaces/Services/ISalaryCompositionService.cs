using FresherMisa2026.Entities;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.SalaryComposition;
using FresherMisa2026.Entities.SalaryComposition.DTO;
using System.Collections.Generic;

namespace FresherMisa2026.Application.Interfaces.Services
{
    public interface ISalaryCompositionService : IBaseService<SalaryComposition>
    {
        /// <summary>
        /// Lọc thành phần lương theo nhiều điều kiện có phân trang
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        Task<ServiceResponse> FilterAsync(SalaryCompositionFilterRequest request);

        /// <summary>
        /// Chuyển TPL hệ thống sang TPL đơn vị (Source = InheritedFromSystem)
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        Task<ServiceResponse> InheritFromSystemAsync(Guid systemCompositionId, List<Guid>? organizationIds);

        /// <summary>
        /// Chuyển nhiều TPL hệ thống sang TPL đơn vị — partial result
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        Task<ServiceResponse> InheritFromSystemBatchAsync(InheritFromSystemBatchRequest request);

        /// <summary>
        /// Lấy danh sách gợi ý (Code, Name, Description) cho ô nhập công thức — chỉ TPL đang theo dõi
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        Task<ServiceResponse> GetSuggestionsAsync(string? search);

        /// <summary>
        /// Chuyển trạng thái theo dõi của TPL (Active ↔ Inactive)
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-27)
        Task<ServiceResponse> SetStatusAsync(Guid id, SalaryCompositionStatus status);

        /// <summary>
        /// Chuyển trạng thái nhiều TPL cùng lúc — partial result
        /// </summary>
        /// Created By: Nguyen Thiet Do (2026-05-28)
        Task<ServiceResponse> SetStatusBulkAsync(List<Guid> ids, SalaryCompositionStatus status);


        /// <summary>Lọc nâng cao 4 phần qua stored procedure</summary>
        Task<ServiceResponse> AdvancedFilterWithProcAsync(SalaryCompositionAdvancedFilterRequest request);

        /// <summary>
        /// Phân loại danh sách TPL thành 3 nhóm trước khi xóa / ngừng theo dõi hàng loạt:
        /// DataSystem (hệ thống, không dùng trong công thức),
        /// DataExist (đang được tham chiếu trong công thức),
        /// DataNotExist (không hệ thống, không tham chiếu — an toàn).
        /// Có phân trang theo danh sách đầu vào.
        /// </summary>
        Task<ServiceResponse> ExitDataAsync(ExitDataRequest request);
    }
}
