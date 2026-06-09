using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Exceptions;
using FresherMisa2026.Entities.Extensions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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

        private static readonly Regex _fieldNamePattern = new(@"^[A-Za-z0-9_]+$", RegexOptions.Compiled);

        // Các trường hệ thống từ BaseModel — không cho phép PATCH
        private static readonly HashSet<string> _securityFields = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(BaseModel.CreatedBy),
            nameof(BaseModel.CreateDate),
            nameof(BaseModel.ModifiedBy),
            nameof(BaseModel.ModifiedDate),
            nameof(BaseModel.State),
            nameof(BaseModel.IsDeleted),
        };
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

        protected static ServiceResponse CreateValidationErrorResponse(List<ValidationError> errors) => new()
        {
            IsSuccess = false,
            Code = (int)ResponseCode.BadRequest,
            DevMessage = "Validate thất bại",
            Data = errors
        };

        private static PropertyInfo[] GetCachedProperties(Type entityType)
        {
            return _cachedProperties.GetOrAdd(entityType, type => type.GetProperties());
        }

        /// <summary>
        /// Project danh sách entity sang danh sách dictionary chỉ chứa các property được yêu cầu.
        /// Tên property so sánh case-insensitive; property không tồn tại trên entity thì bị bỏ qua.
        /// </summary>
        protected static IEnumerable<IDictionary<string, object?>> ProjectColumns(
            IEnumerable<TEntity> data, IList<string> columns)
        {
            var props = GetCachedProperties(typeof(TEntity))
                .Where(p => columns.Any(c => string.Equals(c, p.Name, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            return data.Select(entity =>
            {
                IDictionary<string, object?> dict = new Dictionary<string, object?>(props.Length);
                foreach (var prop in props)
                    dict[prop.Name] = prop.GetValue(entity);
                return dict;
            });
        }

        /// <summary>Build PagingResponse chứa dữ liệu đã được project sang dictionary (khi Columns được chỉ định).</summary>
        protected static PagingResponse<IDictionary<string, object?>> BuildProjectedPaging(
            long total, int pageIndex, int pageSize,
            IEnumerable<IDictionary<string, object?>> data)
            => new()
            {
                Total       = total,
                PageSize    = pageSize,
                CurrentPage = pageIndex,
                PageCount   = (long)Math.Ceiling((double)total / pageSize),
                Data        = data.ToList()
            };

        /// <summary>
        /// Validate tên trường trong SearchFields và Filters — chỉ cho phép [A-Za-z0-9_].
        /// Trả về danh sách lỗi, rỗng nếu hợp lệ.
        /// </summary>
        protected static List<ValidationError> ValidateFilterFieldNames(
            IEnumerable<string>? searchFields,
            IEnumerable<FilterCondition>? filters)
        {
            var invalid = new HashSet<string>(StringComparer.Ordinal);

            if (searchFields != null)
                foreach (var f in searchFields)
                    if (!_fieldNamePattern.IsMatch(f))
                        invalid.Add(f);

            if (filters != null)
                foreach (var fc in filters)
                    if (!string.IsNullOrWhiteSpace(fc.Field) && !_fieldNamePattern.IsMatch(fc.Field))
                        invalid.Add(fc.Field);

            if (invalid.Count == 0) return new List<ValidationError>();

            return new List<ValidationError>
            {
                new ValidationError("Fields",
                    $"Tên trường không hợp lệ: {string.Join(", ", invalid)}. Chỉ được chứa chữ cái (A-Z, a-z), số (0-9) và dấu gạch dưới (_)")
            };
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
        /// Xóa nhiều bản ghi trong một transaction — fail-fast: rollback toàn bộ nếu có 1 ID lỗi
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>ServiceResponse</returns>
        /// CREATED BY: DVHAI (19/05/2026)
        public async Task<ServiceResponse> DeleteManyAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            var entitiesToDelete = new List<TEntity>();
            foreach (var id in ids)
            {
                if (id == Guid.Empty)
                    return CreateErrorResponse(ResponseCode.BadRequest, $"Id '{id}' không hợp lệ");

                var entity = await _baseRepository.GetEntityByIDAsync(id);
                if (entity == null)
                    return CreateErrorResponse(ResponseCode.NotFound, $"Không tìm thấy bản ghi với Id '{id}'");

                bool canDelete = await ValidateBeforeDeleteAsync(id);
                if (!canDelete)
                {
                    var msg = await GetDeleteValidationMessageAsync(id);
                    return CreateErrorResponse(ResponseCode.BadRequest, msg ?? "Không thể xóa bản ghi này");
                }

                entitiesToDelete.Add(entity);
            }

            int rowAffects = await _baseRepository.DeleteManyAsync(ids);

            if (rowAffects > 0)
            {
                foreach (var entity in entitiesToDelete)
                    AfterDelete(entity);
                return CreateSuccessResponse(rowAffects);
            }

            return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để xóa");
        }

        /// <summary>
        /// Xóa nhiều bản ghi — partial result: tiếp tục xóa dù có ID thất bại, trả về succeeded/failed
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>ServiceResponse chứa BulkDeleteResult</returns>
        /// CREATED BY: DVHAI (19/05/2026)
        public async Task<ServiceResponse> DeleteManyPartialAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách Id không được rỗng");

            var result = new BulkDeleteResult();

            foreach (var id in ids)
            {
                if (id == Guid.Empty)
                {
                    result.Failed.Add(new BulkDeleteFailedItem(id, "Id không hợp lệ"));
                    continue;
                }

                try
                {
                    var entity = await _baseRepository.GetEntityByIDAsync(id);
                    if (entity == null)
                    {
                        result.Failed.Add(new BulkDeleteFailedItem(id, "Không tìm thấy bản ghi"));
                        continue;
                    }

                    bool canDelete = await ValidateBeforeDeleteAsync(id);
                    if (!canDelete)
                    {
                        var msg = await GetDeleteValidationMessageAsync(id);
                        result.Failed.Add(new BulkDeleteFailedItem(id, msg ?? "Không thể xóa bản ghi này"));
                        continue;
                    }

                    int rows = await _baseRepository.DeleteAsync(id);
                    if (rows > 0)
                    {
                        AfterDelete(entity);
                        OnAfterDelete(id, rows);
                        result.Succeeded.Add(id);
                    }
                    else
                    {
                        result.Failed.Add(new BulkDeleteFailedItem(id, "Xóa thất bại"));
                    }
                }
                catch (Exception ex)
                {
                    result.Failed.Add(new BulkDeleteFailedItem(id, ex.Message));
                }
            }

            return CreateSuccessResponse(result);
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
        public virtual async Task<ServiceResponse> InsertAsync(TEntity entity)
        {
            entity.State = ModelSate.Add;
            entity.CreateDate = DateTime.Now;
            entity.CreatedBy = GetCurrentUser();

            //1. Validate tất cả các trường nếu được gắn thẻ
            var errors = Validate(entity);

            var insertValidationErrors = await ValidateBeforeInsertAsync(entity);
            errors.AddRange(insertValidationErrors);

            //2. Sử lí lỗi tương ứng
            if (errors.Count == 0)
            {
                try
                {
                    var result = await _baseRepository.InsertAsync(entity);
                    OnAfterInsert(entity, result);
                    return CreateSuccessResponse(result);
                }
                catch (DuplicateEntityException ex)
                {
                    return CreateValidationErrorResponse(new List<ValidationError>
                    {
                        new ValidationError(ex.ColumnName, ex.Message)
                    });
                }
            }

            return CreateValidationErrorResponse(errors);
        }

        /// <summary>
        /// Cập nhập thông tin bản ghi 
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>ServiceResponse chứa kết quả</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public virtual async Task<ServiceResponse> UpdateAsync(Guid entityId, TEntity entity)
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
            entity.ModifiedDate = DateTime.Now;
            entity.ModifiedBy = GetCurrentUser();

            //2. Validate tất cả các trường nếu được gắn thẻ
            var errors = Validate(entity);

            var updateValidationErrors = await ValidateBeforeUpdateAsync(entityId, entity);
            errors.AddRange(updateValidationErrors);
            
            if (errors.Count == 0)
            {
                try
                {
                    int rowAffects = await _baseRepository.UpdateAsync(entityId, entity);
                    if (rowAffects > 0)
                    {
                        OnAfterUpdate(entityId, entity, rowAffects);
                        return CreateSuccessResponse(rowAffects);
                    }
                    return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để cập nhật");
                }
                catch (DuplicateEntityException ex)
                {
                    return CreateValidationErrorResponse(new List<ValidationError>
                    {
                        new ValidationError(ex.ColumnName, ex.Message)
                    });
                }
            }

            //3. Validate fail - trả về BadRequest
            return CreateValidationErrorResponse(errors);
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

        /// <summary>
        /// Cập nhật một trường cụ thể — validate trường bảo mật trước khi ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="fieldName">Tên trường cần cập nhật</param>
        /// <param name="value">Giá trị mới dưới dạng JSON</param>
        /// <returns>ServiceResponse</returns>
        /// CREATED BY: NTDo (24/05/2026)
        public async Task<ServiceResponse> PatchFieldAsync(Guid entityId, string fieldName, JsonElement value)
        {
            if (entityId == Guid.Empty)
                return CreateErrorResponse(ResponseCode.BadRequest, "Id không hợp lệ");

            // 1. Tìm property theo tên (case-insensitive)
            var properties = GetCachedProperties(typeof(TEntity));
            var prop = properties.FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
            if (prop == null)
                return CreateErrorResponse(ResponseCode.BadRequest, $"Trường '{fieldName}' không tồn tại trên entity");

            // 2. Không cho phép cập nhật khóa chính
            var keyName = typeof(TEntity).GetKeyName();
            if (prop.Name.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                return CreateErrorResponse(ResponseCode.BadRequest, $"Không được phép cập nhật trường khóa chính '{prop.Name}'");

            // 3. Không cho phép cập nhật trường hệ thống của BaseModel
            if (_securityFields.Contains(prop.Name))
                return CreateErrorResponse(ResponseCode.BadRequest, $"Trường '{prop.Name}' là trường hệ thống, không được phép cập nhật");

            // 4. Không cho phép cập nhật trường gắn [NotPatchable]
            if (prop.IsDefined(typeof(NotPatchable), false))
            {
                var blockedDisplayName = typeof(TEntity).GetColumnDisplayName(prop.Name);
                return CreateErrorResponse(ResponseCode.BadRequest, $"Trường '{blockedDisplayName}' không được phép cập nhật vì lý do bảo mật");
            }

            // 5. Kiểm tra bản ghi tồn tại
            var existing = await _baseRepository.GetEntityByIDAsync(entityId);
            if (existing == null)
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi");

            // 6. Chuyển đổi giá trị JSON sang đúng kiểu của property
            object? convertedValue;
            try
            {
                convertedValue = ConvertJsonElementToType(value, prop.PropertyType);
            }
            catch
            {
                var displayName = typeof(TEntity).GetColumnDisplayName(prop.Name);
                return CreateErrorResponse(ResponseCode.BadRequest, $"Giá trị không hợp lệ cho trường '{displayName}'");
            }

            // 7. Validate [IRequired] nếu field có annotation đó
            if (prop.IsDefined(typeof(IRequired), false)
                && (convertedValue == null || string.IsNullOrEmpty(convertedValue.ToString())))
            {
                var displayName = typeof(TEntity).GetColumnDisplayName(prop.Name);
                return CreateValidationErrorResponse(new List<ValidationError>
                {
                    new(prop.Name, $"Trường {displayName} bắt buộc nhập")
                });
            }

            int rows = await _baseRepository.PatchFieldAsync(entityId, prop.Name, convertedValue);

            return rows > 0
                ? CreateSuccessResponse(rows)
                : CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để cập nhật");
        }

        /// <summary>Cập nhật nhiều trường cùng lúc trong một câu UPDATE duy nhất.</summary>
        public async Task<ServiceResponse> PatchFieldsAsync(Guid entityId, Dictionary<string, JsonElement> fields)
        {
            if (entityId == Guid.Empty)
                return CreateErrorResponse(ResponseCode.BadRequest, "Id không hợp lệ");
            if (fields == null || fields.Count == 0)
                return CreateErrorResponse(ResponseCode.BadRequest, "Danh sách fields không được rỗng");

            var properties = GetCachedProperties(typeof(TEntity));
            var keyName = typeof(TEntity).GetKeyName();
            var validated = new Dictionary<string, object?>(fields.Count);
            var errors = new List<ValidationError>();

            foreach (var (fieldName, jsonValue) in fields)
            {
                var prop = properties.FirstOrDefault(p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                if (prop == null)
                { errors.Add(new ValidationError(fieldName, $"Trường '{fieldName}' không tồn tại")); continue; }

                if (prop.Name.Equals(keyName, StringComparison.OrdinalIgnoreCase))
                { errors.Add(new ValidationError(fieldName, $"Không được phép cập nhật khóa chính '{prop.Name}'")); continue; }

                if (_securityFields.Contains(prop.Name))
                { errors.Add(new ValidationError(fieldName, $"Trường '{prop.Name}' là trường hệ thống, không được phép cập nhật")); continue; }

                if (prop.IsDefined(typeof(NotPatchable), false))
                {
                    var dn = typeof(TEntity).GetColumnDisplayName(prop.Name);
                    errors.Add(new ValidationError(fieldName, $"Trường '{dn}' không được phép cập nhật"));
                    continue;
                }

                object? converted;
                try { converted = ConvertJsonElementToType(jsonValue, prop.PropertyType); }
                catch
                {
                    var dn = typeof(TEntity).GetColumnDisplayName(prop.Name);
                    errors.Add(new ValidationError(fieldName, $"Giá trị không hợp lệ cho trường '{dn}'")); continue;
                }

                if (prop.IsDefined(typeof(IRequired), false)
                    && (converted == null || string.IsNullOrEmpty(converted.ToString())))
                {
                    var dn = typeof(TEntity).GetColumnDisplayName(prop.Name);
                    errors.Add(new ValidationError(prop.Name, $"Trường {dn} bắt buộc nhập")); continue;
                }

                validated[prop.Name] = converted;
            }

            if (errors.Count > 0) return CreateValidationErrorResponse(errors);

            var existing = await _baseRepository.GetEntityByIDAsync(entityId);
            if (existing == null)
                return CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi");

            var rows = await _baseRepository.PatchFieldsAsync(entityId, validated);
            return rows > 0
                ? CreateSuccessResponse(rows)
                : CreateErrorResponse(ResponseCode.NotFound, "Không tìm thấy bản ghi để cập nhật");
        }

        private static object? ConvertJsonElementToType(JsonElement element, Type targetType)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying == typeof(string)) return element.GetString();
            if (underlying == typeof(Guid)) return element.GetString();
            if (underlying == typeof(int)) return element.GetInt32();
            if (underlying == typeof(long)) return element.GetInt64();
            if (underlying == typeof(double)) return element.GetDouble();
            if (underlying == typeof(decimal)) return element.GetDecimal();
            if (underlying == typeof(bool)) return element.GetBoolean();
            if (underlying == typeof(DateTime)) return element.GetDateTime();
            if (underlying == typeof(byte)) return element.GetByte();
            if (underlying.IsEnum) return Enum.ToObject(underlying, element.GetInt32());

            return element.GetString();
        }
        #endregion

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

        /// <summary>
        /// Lấy user hiện tại — override khi có module đăng nhập để trả về username từ JWT claim.
        /// </summary>
        protected virtual string? GetCurrentUser() => null;

        #endregion
    }

    /// <summary>
    /// Lỗi validate
    /// </summary>
    /// <param name="Field">Tên trường</param>
    /// <param name="Message">Thông báo lỗi</param>
    public record ValidationError(string Field, string Message)
    {
        /// <summary>
        /// Vị trí các mã TPL không tồn tại trong công thức (FE dùng để highlight đỏ trong input).
        /// Chỉ có ý nghĩa với lỗi công thức; null/không xuất hiện cho mọi loại lỗi khác.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyList<MissingCodeInfo>? MissingCodes { get; init; }
    }
}
