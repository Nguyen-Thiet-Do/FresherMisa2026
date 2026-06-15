using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Application.Interfaces.Repositories;
using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Enums;
using FresherMisa2026.Entities.Exceptions;
using FresherMisa2026.Entities.Extensions;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FresherMisa2026.Application.Services
{
    /// <summary>
    /// Service dùng chung
    /// </summary>
    /// <typeparam name="TEntity">Loại thực thể</typeparam>
    /// Created By: ntdo (2026-04-08)
    public class BaseService<TEntity> : IBaseService<TEntity> where TEntity : BaseModel
    {
        #region Declare
        protected readonly IBaseRepository<TEntity> _baseRepository;
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly string _tableName;
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _cachedProperties = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo> _cachedKeyProps = new();
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
        public BaseService(IBaseRepository<TEntity> baseRepository, IAuditLogRepository auditLogRepository)
        {
            _baseRepository = baseRepository;
            _auditLogRepository = auditLogRepository;
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

        /// <summary>Cache kết quả GetProperties() theo Type để tránh gọi reflection lặp lại mỗi request.</summary>
        /// Created By: ntdo (2026-04-08)
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
        /// Validate tên trường trong SearchFields và Filters — chỉ cho phép [A-Za-z0-9_] và phải là property tồn tại trên entity.
        /// Trả về danh sách lỗi, rỗng nếu hợp lệ.
        /// </summary>
        protected static List<ValidationError> ValidateFilterFieldNames(
            IEnumerable<string>? searchFields,
            IEnumerable<FilterCondition>? filters,
            Type entityType)
        {
            var props = GetCachedProperties(entityType);
            var invalid = new HashSet<string>(StringComparer.Ordinal);

            void Check(string? f)
            {
                if (string.IsNullOrWhiteSpace(f)) return;
                if (!_fieldNamePattern.IsMatch(f) ||
                    !props.Any(p => string.Equals(p.Name, f, StringComparison.OrdinalIgnoreCase)))
                    invalid.Add(f);
            }

            if (searchFields != null)
                foreach (var f in searchFields) Check(f);

            if (filters != null)
                foreach (var fc in filters) Check(fc.Field);

            if (invalid.Count == 0) return new List<ValidationError>();

            return new List<ValidationError>
            {
                new ValidationError("Fields",
                    $"Tên trường không hợp lệ: {string.Join(", ", invalid)}. Chỉ chấp nhận tên thuộc tính của entity (A-Z, a-z, 0-9, _).")
            };
        }

        /// <summary>
        /// Validate Sort: split theo ',', bỏ prefix '-'/'+', mỗi token phải khớp regex và là property tồn tại trên entity.
        /// Chặn SQLi qua tên cột vì SP build ORDER BY bằng PREPARE/EXECUTE.
        /// </summary>
        protected static List<ValidationError> ValidateSortFieldNames(string? sort, Type entityType)
        {
            if (string.IsNullOrWhiteSpace(sort)) return new List<ValidationError>();

            var props = GetCachedProperties(entityType);
            var invalid = new HashSet<string>(StringComparer.Ordinal);

            foreach (var raw in sort.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var token = raw.Trim();
                if (token.Length == 0) continue;
                if (token[0] == '-' || token[0] == '+') token = token[1..];

                if (!_fieldNamePattern.IsMatch(token) ||
                    !props.Any(p => string.Equals(p.Name, token, StringComparison.OrdinalIgnoreCase)))
                    invalid.Add(token);
            }

            if (invalid.Count == 0) return new List<ValidationError>();

            return new List<ValidationError>
            {
                new ValidationError("Sort",
                    $"Trường sắp xếp không hợp lệ: {string.Join(", ", invalid)}. Chỉ chấp nhận tên thuộc tính của entity (A-Z, a-z, 0-9, _).")
            };
        }
        #endregion

        #region Methods
        /// <summary>
        /// Lấy tất cả bản ghi
        /// </summary>
        /// <returns>Danh sách bản ghi</returns>
        /// Created By: ntdo (2026-04-08)
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
        /// Created By: ntdo (2026-04-08)
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
        /// Created By: ntdo (2026-04-08)
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
            bool canDelete = await ValidateBeforeDeleteAsync(entityId, existingEntity);
            if (!canDelete)
            {
                var deleteValidationMessage = await GetDeleteValidationMessageAsync(entityId, existingEntity);
                return CreateErrorResponse(ResponseCode.BadRequest, deleteValidationMessage ?? "Không thể xóa bản ghi này");
            }
            
            //2. Thực hiện xóa
            int rowAffects = await _baseRepository.DeleteAsync(entityId);
            
            if (rowAffects > 0)
            {
                await _auditLogRepository.LogAsync(entityId, _tableName, AuditAction.Delete, GetCurrentUser());
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
        /// Created By: ntdo (2026-04-08)
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

                bool canDelete = await ValidateBeforeDeleteAsync(id, entity);
                if (!canDelete)
                {
                    var msg = await GetDeleteValidationMessageAsync(id, entity);
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
        /// Created By: ntdo (2026-04-08)
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

                    bool canDelete = await ValidateBeforeDeleteAsync(id, entity);
                    if (!canDelete)
                    {
                        var msg = await GetDeleteValidationMessageAsync(id, entity);
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
        /// Created By: ntdo (2026-04-08)
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
        /// Created By: ntdo (2026-04-08)
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
        /// Created By: ntdo (2026-04-08)
        protected virtual List<ValidationError> ValidateCustom(TEntity entity)
        {
            return new List<ValidationError>();
        }


        /// <summary>
        /// Thêm một thực thể
        /// </summary>
        /// <param name="entity">Thực thể cần thêm</param>
        /// <returns>ServiceResponse chứa kết quả</returns>
        /// Created By: ntdo (2026-04-08)
        public virtual async Task<ServiceResponse> InsertAsync(TEntity entity)
        {
            entity.State = ModelSate.Add;
            entity.CreateDate = DateTime.Now;
            entity.CreatedBy = GetCurrentUser();

            //0. Check trùng Code/unique 
            var uniqueErrors = await _baseRepository.GetUniqueViolationsAsync(entity);
            if (uniqueErrors.Count > 0)
                return CreateValidationErrorResponse(uniqueErrors);

            //1. Validate [IRequired] + ValidateCustom
            var errors = Validate(entity);

            var insertValidationErrors = await ValidateBeforeInsertAsync(entity);
            errors.AddRange(insertValidationErrors);

            //2. Xử lý lỗi tương ứng
            if (errors.Count == 0)
            {
                try
                {
                    var result = await _baseRepository.InsertAsync(entity);
                    await _auditLogRepository.LogAsync(GetEntityPrimaryKey(entity), _tableName, AuditAction.Insert, GetCurrentUser());
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
        /// Created By: ntdo (2026-04-08)
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

            //0. Check trùng Code/unique trước tiên — bỏ qua chính bản ghi đang sửa
            var uniqueErrors = await _baseRepository.GetUniqueViolationsAsync(entity, entityId);
            if (uniqueErrors.Count > 0)
                return CreateValidationErrorResponse(uniqueErrors);

            //1. Trạng thái
            entity.State = ModelSate.Update;
            entity.ModifiedDate = DateTime.Now;
            entity.ModifiedBy = GetCurrentUser();

            //2. Validate [IRequired] + ValidateCustom
            var errors = Validate(entity);

            var updateValidationErrors = await ValidateBeforeUpdateAsync(entityId, entity, existingEntity);
            errors.AddRange(updateValidationErrors);
            
            if (errors.Count == 0)
            {
                try
                {
                    int rowAffects = await _baseRepository.UpdateAsync(entityId, entity);
                    if (rowAffects > 0)
                    {
                        await _auditLogRepository.LogAsync(entityId, _tableName, AuditAction.Update, GetCurrentUser());
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
        /// Created By: ntdo (2026-04-08)
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
        /// Created By: ntdo (2026-04-08)
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
        public virtual async Task<ServiceResponse> PatchFieldsAsync(Guid entityId, Dictionary<string, JsonElement> fields)
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

        /// <summary>
        /// Chuyển JsonElement sang CLR type đúng kiểu của property — xử lý Nullable và Enum.
        /// Ném exception nếu không convert được; caller bắt và trả BadRequest.
        /// </summary>
        /// Created By: ntdo (2026-04-08)
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

        /// Created By: ntdo (2026-04-08)
        private Guid GetEntityPrimaryKey(TEntity entity)
        {
            var prop = _cachedKeyProps.GetOrAdd(typeof(TEntity), t => t.GetProperty(t.GetKeyName())!);
            return (Guid)(prop.GetValue(entity) ?? Guid.Empty);
        }

        #region Virtual method - Lifecycle hooks
        /// <summary>
        /// Sau khi thêm mới thành công — override để xử lý side effect (notification...)
        /// </summary>
        protected virtual void OnAfterInsert(TEntity entity, int result) { }

        /// <summary>
        /// Sau khi cập nhật thành công — override để xử lý side effect (audit log...)
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
        /// <param name="existingEntity">Entity đã được fetch từ DB — dùng trực tiếp, không cần fetch lại</param>
        /// <returns>Có thể xóa hay không</returns>
        protected virtual Task<bool> ValidateBeforeDeleteAsync(Guid entityId, TEntity existingEntity)
        {
            return Task.FromResult(true);
        }

        /// <summary>Override để cung cấp thông báo nghiệp vụ khi block xóa. Base trả null (message mặc định dùng).</summary>
        /// <param name="existingEntity">Entity đã được fetch từ DB — dùng trực tiếp, không cần fetch lại</param>
        protected virtual Task<string?> GetDeleteValidationMessageAsync(Guid entityId, TEntity existingEntity)
        {
            return Task.FromResult<string?>(null);
        }

        /// <summary>Override để thêm validate async trước khi insert (ví dụ: lookup DB). Base trả danh sách rỗng.</summary>
        protected virtual Task<List<ValidationError>> ValidateBeforeInsertAsync(TEntity entity)
        {
            return Task.FromResult(new List<ValidationError>());
        }

        /// <summary>Override để thêm validate async trước khi update (ví dụ: BR-01, BR-10). Base trả danh sách rỗng.</summary>
        /// <param name="existingEntity">Entity hiện tại trong DB — dùng trực tiếp, không cần fetch lại</param>
        protected virtual Task<List<ValidationError>> ValidateBeforeUpdateAsync(Guid entityId, TEntity entity, TEntity existingEntity)
        {
            return Task.FromResult(new List<ValidationError>());
        }

        /// <summary>
        /// Lấy user hiện tại — override khi có module đăng nhập để trả về username từ JWT claim.
        /// </summary>
        protected virtual string? GetCurrentUser() => null;

        #endregion
    }

}
