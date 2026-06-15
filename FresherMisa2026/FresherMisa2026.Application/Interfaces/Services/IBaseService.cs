using FresherMisa2026.Entities;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace FresherMisa2026.Application.Interfaces.Services
{
    /// <summary>
    /// Interface service dùng chung
    /// Created By: ntdo (09/04/2026)
    /// </summary>
    public interface IBaseService<TEntity>
    {
        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách bản ghi</returns>
        /// Created By: ntdo (09/04/2026)
        Task<ServiceResponse> GetEntitiesAsync();

        /// <summary>
        /// Lấy bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi thông tin 1 bản ghi</returns>
        /// Created By: ntdo (09/04/2026)
        Task<ServiceResponse> GetEntityByIDAsync(Guid entityId);

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <returns>ServiceResponse</returns>
        /// Created By: ntdo (09/04/2026)
        Task<ServiceResponse> DeleteByIDAsync(Guid entityId);

        /// <summary>
        /// Xóa nhiều bản ghi trong một transaction — fail-fast: rollback toàn bộ nếu có 1 ID lỗi
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>ServiceResponse</returns>
        /// Created By: ntdo (19/04/2026)
        Task<ServiceResponse> DeleteManyAsync(List<Guid> ids);

        /// <summary>
        /// Xóa nhiều bản ghi — partial result: tiếp tục xóa dù có ID thất bại, trả về succeeded/failed
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>ServiceResponse chứa BulkDeleteResult</returns>
        /// Created By: ntdo (19/04/2026)
        Task<ServiceResponse> DeleteManyPartialAsync(List<Guid> ids);

        /// <summary>
        /// Thêm một thực thể
        /// </summary>
        /// <param name="entity">Thực thể cần thêm</param>
        /// <returns>ServiceResponse</returns>
        /// Created By: ntdo (09/04/2026)
        Task<ServiceResponse> InsertAsync(TEntity entity);  

        /// <summary>
        /// Cập nhập thông tin bản ghi 
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>ServiceResponse</returns>
        /// Created By: ntdo (09/04/2026)
        Task<ServiceResponse> UpdateAsync(Guid entityId, TEntity entity);

        /// <summary>
        /// Lấy danh sách thực thể paging
        /// </summary>
        /// <param name="pagingRequest">Thông tin phân trang</param>
        /// <returns>Danh sách thực thể phân trang</returns>
        /// Created By: ntdo (09/04/2026)
        Task<ServiceResponse> GetFilterPagingAsync(PagingRequest pagingRequest);

        /// <summary>
        /// Cập nhật một trường cụ thể — validate trường bảo mật trước khi ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="fieldName">Tên trường cần cập nhật</param>
        /// <param name="value">Giá trị mới dưới dạng JSON</param>
        /// <returns>ServiceResponse</returns>
        /// Created By: ntdo (24/04/2026)
        Task<ServiceResponse> PatchFieldAsync(Guid entityId, string fieldName, JsonElement value);

        /// <summary>Cập nhật nhiều trường cùng lúc — validate từng trường trước khi ghi.</summary>
        /// Created By: ntdo (24/04/2026)
        Task<ServiceResponse> PatchFieldsAsync(Guid entityId, Dictionary<string, JsonElement> fields);
    }
}