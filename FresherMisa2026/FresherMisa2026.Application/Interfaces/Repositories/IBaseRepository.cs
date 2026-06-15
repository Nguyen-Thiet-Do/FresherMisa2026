using FresherMisa2026.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Interfaces
{
    /// <summary>
    /// Interface repository dùng chung
    /// Created By: ntdo (2026-04-07)
    /// </summary>
    public interface IBaseRepository<TEntity>
    {
        /// <summary>
        /// Lấy danh sách thực thể paging
        /// </summary>
        /// <param name="pageSize">Số bản ghi mỗi trang</param>
        /// <param name="pageIndex">Chỉ số trang</param>
        /// <param name="search">Từ khóa tìm kiếm</param>
        /// <param name="searchFields">Danh sách trường tìm kiếm</param>
        /// <param name="sort">Sắp xếp theo</param>
        /// <returns>Tổng số bản ghi và danh sách dữ liệu</returns>
        /// Created By: ntdo (2026-04-07)
        Task<(long Total,
            IEnumerable<TEntity> Data)> GetFilterPagingAsync(
            int pageSize, 
            int pageIndex, 
            string search, 
            List<string> searchFields, 
            string sort);

        /// <summary>
        /// Lấy danh sách thực thể
        /// </summary>
        /// <returns>Danh sách tất cả bản ghi</returns>
        /// Created By: ntdo (2026-04-07)
        Task<IEnumerable<BaseModel>> GetEntitiesAsync();

        /// <summary>
        /// Lấy bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi tìm thấy hoặc null</returns>
        /// Created By: ntdo (2026-04-07)
        Task<TEntity> GetEntityByIDAsync(Guid entityId);

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// Created By: ntdo (2026-04-08)
        Task<int> DeleteAsync(Guid entityId);

        /// <summary>
        /// Xóa nhiều bản ghi trong một transaction
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// Created By: ntdo (2026-04-08)
        Task<int> DeleteManyAsync(List<Guid> ids);

        /// <summary>
        /// Thêm bản ghi
        /// </summary>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi thêm mới</returns>
        /// Created By: ntdo (2026-04-08)
        Task<int> InsertAsync(TEntity entity);

        /// <summary>
        /// Cập nhập thông tin bản ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// Created By: ntdo (2026-04-09)
        Task<int> UpdateAsync(Guid entityId, TEntity entity);

        /// <summary>
        /// Cập nhật một trường cụ thể của bản ghi (PATCH single field)
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="fieldName">Tên cột trong DB (đã validate qua reflection)</param>
        /// <param name="value">Giá trị mới</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// Created By: ntdo (2026-04-09)
        Task<int> PatchFieldAsync(Guid entityId, string fieldName, object? value);

        /// <summary>Cập nhật nhiều trường trong một câu UPDATE duy nhất.</summary>
        /// Created By: ntdo (2026-04-09)
        Task<int> PatchFieldsAsync(Guid entityId, IReadOnlyDictionary<string, object?> fields);

        /// <summary>
        /// Kiểm tra các cột unique khai báo trong [ConfigTable] và trả về danh sách lỗi trùng.
        /// Không throw — dùng để Service layer check sớm trước các validate khác.
        /// </summary>
        /// <param name="entity">Thực thể cần kiểm tra</param>
        /// <param name="excludeId">Id của bản ghi đang cập nhật (bỏ qua chính nó khi Update)</param>
        /// Created By: ntdo (2026-04-09)
        Task<List<ValidationError>> GetUniqueViolationsAsync(TEntity entity, Guid? excludeId = null);
    }
}
