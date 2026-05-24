using FresherMisa2026.Entities;
using FresherMisa2026.Entities.AdvancedFilter;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace FresherMisa2026.Application.Interfaces.Services
{
    public interface IBaseService<TEntity>
    {
        /// <summary>
        /// Approach 1: Advanced filter dùng Dynamic SQL trong C#
        /// </summary>
        Task<ServiceResponse> AdvancedFilterPagingAsync(AdvancedFilterRequest request);

        /// <summary>
        /// Approach 2: Advanced filter dùng Stored Procedure
        /// </summary>
        Task<ServiceResponse> AdvancedFilterPagingWithProcAsync(AdvancedFilterRequest request);

        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách bản ghi</returns>
        /// CREATED BY: DVHAI 11/07/2026
        Task<ServiceResponse> GetEntitiesAsync();

        /// <summary>
        /// Lấy bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi thông tin 1 bản ghi</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<ServiceResponse> GetEntityByIDAsync(Guid entityId);

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <returns>ServiceResponse</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<ServiceResponse> DeleteByIDAsync(Guid entityId);

        /// <summary>
        /// Xóa nhiều bản ghi trong một transaction — fail-fast: rollback toàn bộ nếu có 1 ID lỗi
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>ServiceResponse</returns>
        /// CREATED BY: DVHAI (19/05/2026)
        Task<ServiceResponse> DeleteManyAsync(List<Guid> ids);

        /// <summary>
        /// Xóa nhiều bản ghi — partial result: tiếp tục xóa dù có ID thất bại, trả về succeeded/failed
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>ServiceResponse chứa BulkDeleteResult</returns>
        /// CREATED BY: DVHAI (19/05/2026)
        Task<ServiceResponse> DeleteManyPartialAsync(List<Guid> ids);

        /// <summary>
        /// Thêm một thực thể
        /// </summary>
        /// <param name="entity">Thực thể cần thêm</param>
        /// <returns>ServiceResponse</returns>
        /// CREATED BY: DVHAI (11/07/2026)
        Task<ServiceResponse> InsertAsync(TEntity entity);

        /// <summary>
        /// Cập nhập thông tin bản ghi 
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>ServiceResponse</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        Task<ServiceResponse> UpdateAsync(Guid entityId, TEntity entity);

        /// <summary>
        /// Lấy danh sách thực thể paging
        /// </summary>
        /// <param name="pagingRequest">Thông tin phân trang</param>
        /// <returns>Danh sách thực thể phân trang</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<ServiceResponse> GetFilterPagingAsync(PagingRequest pagingRequest);

        /// <summary>
        /// Cập nhật một trường cụ thể — validate trường bảo mật trước khi ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="fieldName">Tên trường cần cập nhật</param>
        /// <param name="value">Giá trị mới dưới dạng JSON</param>
        /// <returns>ServiceResponse</returns>
        /// CREATED BY: NTDo (24/05/2026)
        Task<ServiceResponse> PatchFieldAsync(Guid entityId, string fieldName, JsonElement value);
    }
}