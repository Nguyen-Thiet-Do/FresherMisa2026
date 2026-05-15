using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Extensions;
using System.Collections.Concurrent;
using System.Reflection;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service dùng chung
    /// </summary>
    /// <typeparam name="TEntity">Loại thực thể</typeparam>
    /// CREATED BY: DVHAI (11/07/2026)
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : BaseModel
    {
        #region Declare
        protected readonly IBaseRepository<TEntity> _baseRepository;
        private readonly string _tableName;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _cachedProperties = new();
        private const string SearchFieldSeparator = ";";
        #endregion

        #region Constructer
        public BaseService(IBaseRepository<TEntity> baseRepository)
        {
            _baseRepository = baseRepository;
            _tableName = typeof(TEntity).GetTableName().ToLowerInvariant();
        }
        #endregion

        #region Protected Helpers - có thể override trong derived class
        protected static ServiceResponse CreateSuccessResponse(object? data = null) => new()
        {
            IsSuccess = true,
            Code = (int)ResponseCode.Success,
            Data = data
        };

        protected static ServiceResponse CreateErrorResponse(ResponseCode code, string devMessage, string? userMessage = null) => new()
        {
            IsSuccess = false,
            Code = (int)code,
            DevMessage = devMessage,
            Data = userMessage,
            UserMessage = userMessage
        };

        private static PropertyInfo[] GetCachedProperties(Type entityType)
        {
            return _cachedProperties.GetOrAdd(entityType, type => type.GetProperties());
        }
        #endregion

        #region Methods
        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách bản ghi</returns>
        /// CREATED BY: DVHAI 11/07/2026
        public async Task<ServiceResponse> GetEntitiesAsync()
        {
            var entities = await _baseRepository.GetEntitiesAsync();
            return CreateSuccessResponse(entities.Cast<TEntity>().ToList());
        }

        /// <summary>
        /// Lấy bản ghi theo Id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi duy nhất</returns>
        /// CREATED BY: DVHAI (11/07/2026)
        public async Task<ServiceResponse> GetEntityByIDAsync(Guid entityId)
        {
            if (entityId == Guid.Empty)
            {
                return CreateErrorResponse(ResponseCode.BadRequest, "Id không hợp lệ");
            }

            var entity = await _baseRepository.GetEntityByIDAsync(entityId);
            return entity != null 
                ? CreateSuccessResponse(entity) 
                : CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi");
        }

        /// <summary>
        /// Xóa bản ghi
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Số dòng bị xóa</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        public async Task<ServiceResponse> DeleteByIDAsync(Guid entityId)
        {
            if (entityId == Guid.Empty)
            {
                return CreateErrorResponse(ResponseCode.BadRequest, "Id không hợp lệ");
            }

            var existingEntity = await _baseRepository.GetEntityByIDAsync(entityId);
            if (existingEntity == null)
            {
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để xóa");
            }

            //1. Validate xóa
            bool canDelete = await ValidateBeforeDeleteAsync(entityId);
            if (!canDelete)
            {
                var deleteValidationMessage = await GetDeleteValidationMessageAsync(entityId);
                return CreateErrorResponse(ResponseCode.BadRequest, deleteValidationMessage ?? "Không thể xóa bản ghi này");
            }
            
            //2. Thực hiện xóa
            int rowAffects = await _baseRepository.DeleteAsync(entityId);
            
            if (rowAffects > 0)
            {
                AfterDelete(existingEntity);
                OnAfterDelete(entityId, rowAffects);
                return CreateSuccessResponse(rowAffects);
            }

            return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để xóa");
        }

        /// <summary>
        /// Validate tất cả
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// <returns>Danh sách lỗi validate</returns>
        /// CREATED BY: DVHAI (07/07/2021)
        private List<ValidationError> Validate(TEntity entity)
        {
            var errors = new List<ValidationError>();
            var properties = GetCachedProperties(entity.GetType());

            foreach (var property in properties)
            {
                //1.1 Kiểm tra xem có attribute cần phải validate không
                if (property.IsDefined(typeof(IRequired), false))
                {
                    var error = ValidateRequired(entity, property);
                    if (error != null)
                    {
                        errors.Add(error);
                    }
                }
            }

            //2. Validate tùy chỉnh từng màn hình
            var customErrors = ValidateCustom(entity);
            errors.AddRange(customErrors);

            return errors;
        }

        /// <summary>
        /// Validate bắt buộc nhập
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// <param name="propertyInfo">Thuộc tính của thực thể</param>
        /// <returns>Lỗi validate hoặc null nếu hợp lệ</returns>
        /// CREATED BY: DVHAI (07/07/2021)
        private ValidationError? ValidateRequired(TEntity entity, PropertyInfo propertyInfo)
        {
            //1. Tên trường
            var propertyName = propertyInfo.Name;

            //2. Giá trị
            var propertyValue = propertyInfo.GetValue(entity);

            //3. Tên hiển thị
            var propertyDisplayName = typeof(TEntity).GetColumnDisplayName(propertyName);

            if (propertyValue == null || string.IsNullOrEmpty(propertyValue.ToString()))
            {
                return new ValidationError(propertyName, $"Trường {propertyDisplayName} bắt buộc nhập");
            }

            return null;
        }

        /// <summary>
        /// Validate từng màn hình
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// <returns>Danh sách lỗi tùy chỉnh</returns>
        /// CREATED BY: DVHAI (07/07/2021)
        protected virtual List<ValidationError> ValidateCustom(TEntity entity)
        {
            return new List<ValidationError>();
        }


        /// <summary>
        /// Thêm một thực thể
        /// </summary>
        /// <param name="entity">Thực thể cần thêm</param>
        /// <returns>ServiceResponse chứa kết quả</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<ServiceResponse> InsertAsync(TEntity entity)
        {
            entity.State = ModelSate.Add;

            //1. Validate tất cả các trường nếu được gắn thẻ
            var errors = Validate(entity);

            var insertValidationErrors = await ValidateBeforeInsertAsync(entity);
            errors.AddRange(insertValidationErrors);

            //2. Sử lí lỗi tương ứng
            if (errors.Count == 0)
            {
                var result = await _baseRepository.InsertAsync(entity);
                OnAfterInsert(entity, result);
                return CreateSuccessResponse(result);
            }

            return CreateErrorResponse(
                ResponseCode.BadRequest, 
                "Validate thất bại", 
                string.Join("; ", errors.Select(e => e.Message))
            );
        }

        /// <summary>
        /// Cập nhập thông tin bản ghi 
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>ServiceResponse chứa kết quả</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<ServiceResponse> UpdateAsync(Guid entityId, TEntity entity)
        {
            if (entityId == Guid.Empty)
            {
                return CreateErrorResponse(ResponseCode.BadRequest, "Id không hợp lệ");
            }

            var existingEntity = await _baseRepository.GetEntityByIDAsync(entityId);
            if (existingEntity == null)
            {
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để cập nhật");
            }

            //1. Trạng thái
            entity.State = ModelSate.Update;

            //2. Validate tất cả các trường nếu được gắn thẻ
            var errors = Validate(entity);

            var updateValidationErrors = await ValidateBeforeUpdateAsync(entityId, entity);
            errors.AddRange(updateValidationErrors);
            
            if (errors.Count == 0)
            {
                int rowAffects = await _baseRepository.UpdateAsync(entityId, entity);
                if (rowAffects > 0)
                {
                    OnAfterUpdate(entityId, entity, rowAffects);
                    return CreateSuccessResponse(rowAffects);
                }
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để cập nhật");
            }

            //3. Validate fail - trả về BadRequest
            return CreateErrorResponse(
                ResponseCode.BadRequest,
                "Validate thất bại",
                string.Join("; ", errors.Select(e => e.Message))
            );
        }

        /// <summary>
        /// Lấy danh sách thực thể paging
        /// </summary>
        /// <param name="pagingRequest">Thông tin phân trang</param>
        /// <returns>Danh sách thực thể phân trang</returns>
        /// CREATED BY: DVHAI (07/07/2026)
        public async Task<ServiceResponse> GetFilterPagingAsync(PagingRequest pagingRequest)
        {
            var fields = string.IsNullOrEmpty(pagingRequest.SearchFields)
                ? new List<string>()
                : pagingRequest.SearchFields.Split(SearchFieldSeparator, StringSplitOptions.RemoveEmptyEntries).ToList();

            var (total, data) = await _baseRepository.GetFilterPagingAsync(
                pagingRequest.PageSize, 
                pagingRequest.PageIndex, 
                pagingRequest.Search,
                fields, 
                pagingRequest.Sort
            );

            var response = new PagingResponse<TEntity>
            {

                Total = total,
                PageSize = pagingRequest.PageSize,
                CurrentPage = pagingRequest.PageIndex,
                PageCount = (long)Math.Ceiling((double)total / pagingRequest.PageSize),
                Data = data.ToList()
            };

            return CreateSuccessResponse(response);
        }
        #endregion

        /// <summary>
        /// Approach 1: Advanced filter paging — Dynamic SQL trong C#
        /// </summary>
        public async Task<ServiceResponse> AdvancedFilterPagingAsync(AdvancedFilterRequest request)
        {
            var (total, data) = await _baseRepository.GetAdvancedFilterPagingAsync(request);
            return CreateSuccessResponse(BuildPagingResponse(total, request.PageIndex, request.PageSize, data));
        }

        /// <summary>
        /// Approach 2: Advanced filter paging — Stored Procedure nhận JSON
        /// </summary>
        public async Task<ServiceResponse> AdvancedFilterPagingWithProcAsync(AdvancedFilterRequest request)
        {
            var (total, data) = await _baseRepository.GetAdvancedFilterPagingWithProcAsync(request);
            return CreateSuccessResponse(BuildPagingResponse(total, request.PageIndex, request.PageSize, data));
        }

        private static PagingResponse<TEntity> BuildPagingResponse(long total, int pageIndex, int pageSize, IEnumerable<TEntity> data)
        {
            return new PagingResponse<TEntity>
            {
                Total = total,
                PageSize = pageSize,
                CurrentPage = pageIndex,
                PageCount = (long)Math.Ceiling((double)total / pageSize),
                Data = data.ToList()
            };
        }

        #region Virtual method - Lifecycle hooks
        /// <summary>
        /// Sau khi thêm mới thành công — override để xử lý side effect (audit log, notification...)
        /// </summary>
        protected virtual void OnAfterInsert(TEntity entity, int result) { }

        /// <summary>
        /// Sau khi cập nhật thành công — override để xử lý side effect (audit log, cache...)
        /// </summary>
        protected virtual void OnAfterUpdate(Guid entityId, TEntity entity, int result) { }

        /// <summary>
        /// Sau khi xóa thành công — override để xử lý side effect (audit log...)
        /// </summary>
        protected virtual void OnAfterDelete(Guid entityId, int result) { }

        #endregion

        #region Virtual method - Override methods
        /// <summary>
        /// Xóa thành công — override để xử lý cleanup (ví dụ: xóa file)
        /// </summary>
        protected virtual void AfterDelete(TEntity entity)
        {
        }

        /// <summary>
        /// Trước khi xóa
        /// </summary>
        /// <param name="entityId">Id bản ghi cần xóa</param>
        /// <returns>Có thể xóa hay không</returns>
        protected virtual Task<bool> ValidateBeforeDeleteAsync(Guid entityId)
        {
            return Task.FromResult(true);
        }

        protected virtual Task<string?> GetDeleteValidationMessageAsync(Guid entityId)
        {
            return Task.FromResult<string?>(null);
        }

        protected virtual Task<List<ValidationError>> ValidateBeforeInsertAsync(TEntity entity)
        {
            return Task.FromResult(new List<ValidationError>());
        }

        protected virtual Task<List<ValidationError>> ValidateBeforeUpdateAsync(Guid entityId, TEntity entity)
        {
            return Task.FromResult(new List<ValidationError>());
        }
        #endregion
    }

    /// <summary>
    /// Lỗi validate
    /// </summary>
    /// <param name="Field">Tên trường</param>
    /// <param name="Message">Thông báo lỗi</param>
    public record ValidationError(string Field, string Message);
}
