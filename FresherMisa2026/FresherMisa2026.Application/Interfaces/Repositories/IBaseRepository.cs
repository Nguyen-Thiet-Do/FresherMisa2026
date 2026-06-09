using FresherMisa2026.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FresherMisa2026.Application.Interfaces
{
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
        /// CREATED BY: DVHAI (07/07/2026)
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
        /// CREATED BY: DVHAI (07/07/2026)
        Task<IEnumerable<BaseModel>> GetEntitiesAsync();

        /// <summary>
        /// Lấy bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi tìm thấy hoặc null</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<TEntity> GetEntityByIDAsync(Guid entityId);

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<int> DeleteAsync(Guid entityId);

        /// <summary>
        /// Xóa nhiều bản ghi trong một transaction
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// CREATED BY: DVHAI (19/05/2026)
        Task<int> DeleteManyAsync(List<Guid> ids);

        /// <summary>
        /// Thêm bản ghi
        /// </summary>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi thêm mới</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<int> InsertAsync(TEntity entity);

        /// <summary>
        /// Cập nhập thông tin bản ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        Task<int> UpdateAsync(Guid entityId, TEntity entity);

        /// <summary>
        /// Cập nhật một trường cụ thể của bản ghi (PATCH single field)
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="fieldName">Tên cột trong DB (đã validate qua reflection)</param>
        /// <param name="value">Giá trị mới</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CREATED BY: NTDo (24/05/2026)
        Task<int> PatchFieldAsync(Guid entityId, string fieldName, object? value);

        /// <summary>Cập nhật nhiều trường trong một câu UPDATE duy nhất.</summary>
        Task<int> PatchFieldsAsync(Guid entityId, IReadOnlyDictionary<string, object?> fields);
    }
}
