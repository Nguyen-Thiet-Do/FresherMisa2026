using Dapper;
using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Exceptions;
using FresherMisa2026.Entities.Extensions;
using ExtNaming = FresherMisa2026.Entities.Extensions.Naming;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace FresherMisa2026.Infrastructure.Repositories
{
    /// <summary>
    /// Base repository
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// Created By: dvhai (09/04/2026)
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseModel
    {
        private Exception TranslateMySqlException(MySqlException ex)
        {
            if (ex.Number == 1062)
            {
                var match = Regex.Match(ex.Message, @"Duplicate entry '(.+?)' for key '(.+?)'", RegexOptions.IgnoreCase);
                if (!match.Success)
                    return new DuplicateEntityException("Dữ liệu đã tồn tại trong hệ thống");

                var entryValue = match.Groups[1].Value;
                var rawKeyName = match.Groups[2].Value.Split('.').Last(); // "UQ_EmployeeCode" hoặc "uq_employee_code"
                var columnName = rawKeyName.StartsWith("UQ_", StringComparison.OrdinalIgnoreCase)
                    ? rawKeyName[3..]
                    : rawKeyName; // "EmployeeCode" hoặc "employee_code"
                var fieldName = _modelType.GetColumnDisplayName(columnName); // tự lookup được cả 2 case
                return new DuplicateEntityException($"{fieldName} '{entryValue}' đã tồn tại");
            }

            if (ex.Number == 1451)
                return new InvalidOperationException("Không thể xóa vì dữ liệu đang được sử dụng ở nơi khác");

            if (ex.Number == 1452)
                return new ArgumentException("Dữ liệu liên kết không tồn tại trong hệ thống");

            if (ex.SqlState == "45000")
            {
                return ex.Message.Contains("không tồn tại", StringComparison.OrdinalIgnoreCase)
                    ? new KeyNotFoundException(ex.Message)
                    : new InvalidOperationException(ex.Message);
            }

            return ex;
        }

        //Properties
        protected string _connectionString = string.Empty;
        IConfiguration _configuration;
        protected string _tableName;
        protected string _keyName;      // tên property C# của khóa chính (dùng cho reflection)
        protected string _keyColumn;    // tên cột DB của khóa chính (snake_case nếu opt-in)
        protected string _deletedColumn;// tên cột "đã xóa mềm" trong DB
        protected bool _useSnakeCase;
        public Type _modelType = null;
        private const int CacheExpirationMinutes = 5;

        protected IMemoryCache _cache;
        protected ILogger<BaseRepository<TEntity>> _logger;

        //Constructor
        public BaseRepository(IConfiguration configuration, IMemoryCache cache, ILogger<BaseRepository<TEntity>> logger)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            _modelType = typeof(TEntity);
            _tableName = _modelType.GetTableName();
            _keyName = _modelType.GetKeyName();
            _useSnakeCase = _modelType.GetUseSnakeCase();
            _keyColumn = _modelType.GetKeyColumn();
            _deletedColumn = _modelType.GetDeletedColumn();
        }

        // ── SP name templates: snake_case (proc_{table}_{action}) vs PascalCase (Proc_{Action}{Table}) ──
        private string SpInsert => _useSnakeCase ? $"proc_{_tableName}_insert"             : $"Proc_Insert{_tableName}";
        private string SpUpdate => _useSnakeCase ? $"proc_{_tableName}_update"             : $"Proc_Update{_tableName}";
        private string SpDeleteById => _useSnakeCase ? $"proc_{_tableName}_delete_by_id"   : $"Proc_Delete{_tableName}ById";
        private string SpFilterPaging => _useSnakeCase ? $"proc_{_tableName}_filter_paging" : $"Proc_{_tableName}_FilterPaging";
        protected MySqlConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString);
        }



        #region Method Get
        /// <summary>
        /// Lấy danh sách entity từ cache nếu có, nếu không có thì lấy từ database và lưu vào cache trong 5 phút
        /// </summary>
        /// <returns>Danh sách tất cả bản ghi</returns>
        /// Created By: dvhai (09/04/2026)
        public async Task<IEnumerable<BaseModel>> GetEntitiesAsync()
        {
            var cacheKey = $"{_tableName}_all";
            if (_cache.TryGetValue(cacheKey, out IEnumerable<TEntity> cached))
            {
                _logger.LogInformation("[CACHE TRÚNG] GetEntitiesAsync - Bảng: {Table} | Khóa: {Key} | Trả về {Count} bản ghi từ cache (bỏ qua truy vấn DB)",
                    _tableName, cacheKey, cached.Count());
                return cached;
            }

            _logger.LogInformation("[CACHE TRƯỢT] GetEntitiesAsync - Bảng: {Table} | Khóa: {Key} | Đang truy vấn cơ sở dữ liệu...", _tableName, cacheKey);
            var sw = Stopwatch.StartNew();
            var result = await GetEntitiesUsingCommandTextAsync();
            sw.Stop();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
            _logger.LogInformation("[TRUY VẤN DB] GetEntitiesAsync - Bảng: {Table} | Lấy được {Count} bản ghi trong {ElapsedMs}ms | Đã lưu cache {ExpirationMinutes} phút",
                _tableName, result.Count(), sw.ElapsedMilliseconds, CacheExpirationMinutes);
            return result;
        }

        /// <summary>
        /// Lấy tất cả theo command text
        /// </summary>
        /// <returns></returns>
        /// CREATED BY: DVHAI (11/07/2021)
        protected virtual async Task<IEnumerable<TEntity>> GetEntitiesUsingCommandTextAsync()
        {
            var query = new StringBuilder($"select * from `{_tableName}`");
            int whereCount = 0;

            if (_modelType.GetHasDeletedColumn())
            {
                whereCount++;
                query.Append($" where `{_deletedColumn}` = FALSE");
            }
            using var connection = CreateConnection();
            await connection.OpenAsync();

            var entities = await connection.QueryAsync<TEntity>(query.ToString(), commandType: CommandType.Text);

            return entities.ToList();
        }

        /// <summary>
        /// Lấy bản ghi theo id từ cache nếu có, nếu không có thì lấy từ database và lưu vào cache trong 5 phút
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Bản ghi tìm thấy hoặc null</returns>
        /// CREATED BY: DVHAI (07/07/2021)
        public async Task<TEntity> GetEntityByIDAsync(Guid entityId)
        {
            var cacheKey = $"{_tableName}_{entityId}";

            // 1. Tìm trong cache riêng lẻ theo ID
            if (_cache.TryGetValue(cacheKey, out TEntity cached))
            {
                _logger.LogInformation("[CACHE TRÚNG] GetEntityByIDAsync - Bảng: {Table} | ID: {Id} | Trả về từ cache ID (bỏ qua truy vấn DB)",
                    _tableName, entityId);
                return cached;
            }

            // 2. Tìm trong cache danh sách toàn bộ nếu có
            var allCacheKey = $"{_tableName}_all";
            if (_cache.TryGetValue(allCacheKey, out IEnumerable<TEntity> allCached))
            {
                var keyProp = typeof(TEntity).GetProperty(_keyName);
                var found = allCached.FirstOrDefault(e => keyProp?.GetValue(e) is Guid id && id == entityId);
                if (found != null)
                {
                    _cache.Set(cacheKey, found, TimeSpan.FromMinutes(CacheExpirationMinutes));
                    _logger.LogInformation("[CACHE TRÚNG - DANH SÁCH] GetEntityByIDAsync - Bảng: {Table} | ID: {Id} | Tìm thấy trong cache danh sách, không cần query DB",
                        _tableName, entityId);
                    return found;
                }
            }

            // 3. Không có trong cache → query DB
            _logger.LogInformation("[CACHE TRƯỢT] GetEntityByIDAsync - Bảng: {Table} | ID: {Id} | Không có trong cache, đang truy vấn cơ sở dữ liệu...", _tableName, entityId);
            var sw = Stopwatch.StartNew();
            var result = await GetEntitieByIdUsingCommandTextAsync(entityId.ToString());
            sw.Stop();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheExpirationMinutes));
            _logger.LogInformation("[TRUY VẤN DB] GetEntityByIDAsync - Bảng: {Table} | ID: {Id} | Lấy dữ liệu trong {ElapsedMs}ms | Đã lưu cache {ExpirationMinutes} phút",
                _tableName, entityId, sw.ElapsedMilliseconds, CacheExpirationMinutes);
            return result;
        }

        /// <summary>
        /// Lấy bản ghi theo id dùng command text
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual async Task<TEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var query = new StringBuilder($"select * from `{_tableName}`");
            int whereCount = 0;

            Func<StringBuilder, bool> AppendWhere = (query) => { query.Append(whereCount == 0 ? " WHERE " : " AND "); return true; };

            if (!string.IsNullOrEmpty(_keyColumn))
            {
                AppendWhere(query);
                query.Append($"`{_keyColumn}` = @Id");
                whereCount++;
            }

            if (_modelType.GetHasDeletedColumn())
            {
                AppendWhere(query);
                query.Append($"`{_deletedColumn}` = FALSE");
                whereCount++;
            }
            using var connection = CreateConnection();
            await connection.OpenAsync();
            var entities = await connection.QueryFirstOrDefaultAsync<TEntity>(query.ToString(), new { Id = id }, commandType: CommandType.Text);

            return entities;
        }

        /// <summary>
        /// Xóa bản ghi theo id
        /// </summary>
        /// <param name="entityId">Id của bản ghi</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<int> DeleteAsync(Guid entityId)
        {
            var rowAffects = 0;
            using var connection = CreateConnection();
            await connection.OpenAsync();

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var dynamicParams = new DynamicParameters();
                    dynamicParams.Add($"@v_{_keyColumn}", entityId);

                    //2. Kết nối tới CSDL:
                    rowAffects = await connection.ExecuteAsync(SpDeleteById, param: dynamicParams, transaction: transaction, commandType: CommandType.StoredProcedure);

                    transaction.Commit();
                    _cache.Remove($"{_tableName}_all");
                    _cache.Remove($"{_tableName}_{entityId}");
                    _logger.LogInformation("[XÓA CACHE] DeleteAsync - Bảng: {Table} | ID: {Id} | Đã xóa cache: {Key1}, {Key2}",
                        _tableName, entityId, $"{_tableName}_all", $"{_tableName}_{entityId}");
                }
                catch (MySqlException ex)
                {
                    transaction.Rollback();
                    throw TranslateMySqlException(ex);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            //3. Trả về số bản ghi bị ảnh hưởng
            return rowAffects;
        }


        /// <summary>
        /// Xóa nhiều bản ghi trong một transaction
        /// </summary>
        /// <param name="ids">Danh sách Id cần xóa</param>
        /// <returns>Số bản ghi bị xóa</returns>
        /// CREATED BY: DVHAI (19/05/2026)
        public async Task<int> DeleteManyAsync(List<Guid> ids)
        {
            var totalRowAffects = 0;
            using var connection = CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();
            try
            {
                foreach (var id in ids)
                {
                    var dynamicParams = new DynamicParameters();
                    dynamicParams.Add($"@v_{_keyColumn}", id);

                    totalRowAffects += await connection.ExecuteAsync(
                        SpDeleteById,
                        param: dynamicParams,
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure);
                }

                transaction.Commit();
                _cache.Remove($"{_tableName}_all");
                foreach (var id in ids)
                    _cache.Remove($"{_tableName}_{id}");

                _logger.LogInformation("[XÓA CACHE] DeleteManyAsync - Bảng: {Table} | Đã xóa {Count} bản ghi", _tableName, ids.Count);
            }
            catch (MySqlException ex)
            {
                transaction.Rollback();
                throw TranslateMySqlException(ex);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return totalRowAffects;
        }

        /// <summary>
        /// Thêm bản ghi mới
        /// </summary>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi thêm mới</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<int> InsertAsync(TEntity entity)
        {
            await ValidateUniqueColumnsAsync(entity);
            var rowAffects = 0;
            using var connection = CreateConnection();
            await connection.OpenAsync();
            
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    EnsurePrimaryKeyForInsert(entity);

                    //1.Duyệt các thuộc tính trên bản ghi và tạo parameters
                    var parameters = MappingDbType(entity);

                    //2.Thực hiện thêm bản ghi
                    rowAffects = await connection.ExecuteAsync(SpInsert, param: parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

                    await OnAfterInsertInTransactionAsync(entity, connection, transaction);
                    transaction.Commit();
                    _cache.Remove($"{_tableName}_all");
                    _logger.LogInformation("[XÓA CACHE] InsertAsync - Bảng: {Table} | Đã xóa cache: {Key}",
                        _tableName, $"{_tableName}_all");
                }
                catch (MySqlException ex)
                {
                    transaction.Rollback();
                    throw TranslateMySqlException(ex);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            //3.Trả về số bản ghi thêm mới
            return rowAffects;
        }

        /// <summary>
        /// Cập nhật thông tin bản ghi
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<int> UpdateAsync(Guid entityId, TEntity entity)
        {
            await ValidateUniqueColumnsAsync(entity, entityId);
            var rowAffects = 0;
            using var connection = CreateConnection();
            await connection.OpenAsync();
            
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    //1. Ánh xạ giá trị id
                    SetPrimaryKeyValue(entity, entityId);

                    //2. Duyệt các thuộc tính trên customer và tạo parameters
                    var parameters = MappingDbType(entity);

                    //3. Kết nối tới CSDL:
                    rowAffects = await connection.ExecuteAsync(SpUpdate, param: parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

                    await OnAfterUpdateInTransactionAsync(entity, entityId, connection, transaction);
                    transaction.Commit();
                    _cache.Remove($"{_tableName}_all");
                    _cache.Remove($"{_tableName}_{entityId}");
                    _logger.LogInformation("[XÓA CACHE] UpdateAsync - Bảng: {Table} | ID: {Id} | Đã xóa cache: {Key1}, {Key2}",
                        _tableName, entityId, $"{_tableName}_all", $"{_tableName}_{entityId}");
                }
                catch (MySqlException ex)
                {
                    transaction.Rollback();
                    throw TranslateMySqlException(ex);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            //4. Trả về dữ liệu
            return rowAffects;
        }

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
        public async Task<(long Total,
            IEnumerable<TEntity> Data)> GetFilterPagingAsync(
            int pageSize,
            int pageIndex,
            string search,
            List<string> searchFields,
            string sort)
        {
            long total = 0;
            var data = Enumerable.Empty<TEntity>();

            using var connection = CreateConnection();
            await connection.OpenAsync();

            string store = SpFilterPaging;
            var parameters = new DynamicParameters();
            parameters.Add(_useSnakeCase ? "@v_page_index"    : "@v_pageIndex",    pageIndex);
            parameters.Add(_useSnakeCase ? "@v_page_size"     : "@v_pageSize",     pageSize);
            parameters.Add("@v_search", search);
            parameters.Add("@v_sort", sort);
            parameters.Add(_useSnakeCase ? "@v_search_fields" : "@v_searchFields", JsonSerializer.Serialize(searchFields));

            using var reader = await connection.QueryMultipleAsync(
                new CommandDefinition(store, parameters, commandType: CommandType.StoredProcedure));

            data = (await reader.ReadAsync<TEntity>()).ToList();
            total = await reader.ReadFirstAsync<long>();

            return (total, data);
        }

        private void EnsurePrimaryKeyForInsert(TEntity entity)
        {
            var keyProperty = entity.GetType().GetProperty(_keyName);

            if (keyProperty == null)
            {
                return;
            }

            if (keyProperty.PropertyType == typeof(Guid))
            {
                var currentValue = (Guid)(keyProperty.GetValue(entity) ?? Guid.Empty);
                if (currentValue == Guid.Empty)
                {
                    keyProperty.SetValue(entity, Guid.NewGuid());
                }
            }
            else if (keyProperty.PropertyType == typeof(Guid?))
            {
                var currentValue = (Guid?)keyProperty.GetValue(entity);
                if (!currentValue.HasValue || currentValue.Value == Guid.Empty)
                {
                    keyProperty.SetValue(entity, Guid.NewGuid());
                }
            }
        }

        private void SetPrimaryKeyValue(TEntity entity, Guid entityId)
        {
            var keyProperty = entity.GetType().GetProperty(_keyName);

            if (keyProperty == null)
            {
                return;
            }

            if (keyProperty.PropertyType == typeof(Guid) || keyProperty.PropertyType == typeof(Guid?))
            {
                keyProperty.SetValue(entity, entityId);
            }
        }

        /// <summary>
        /// Ánh xạ các thuộc tính sang kiểu dynamic
        /// </summary>
        /// <param name="entity">Thực thể</param>
        /// <returns>Dan sách các biến động</returns>
            private DynamicParameters MappingDbType(TEntity entity)
        {
            var parameters = new DynamicParameters();
            var properties = entity.GetType().GetProperties();

            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;

                // Skip collection types — not mappable as scalar SQL params
                if (propertyType != typeof(string) &&
                    typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType))
                    continue;

                var paramName = _modelType.GetColumnName(property.Name);
                var propertyValue = property.GetValue(entity);

                if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                    parameters.Add($"@v_{paramName}", propertyValue, DbType.String);
                else
                    parameters.Add($"@v_{paramName}", propertyValue);
            }

            return parameters;
        }

        /// <summary>Hook gọi trong transaction sau khi Insert SP — override để xử lý bảng phụ</summary>
        protected virtual Task OnAfterInsertInTransactionAsync(TEntity entity, IDbConnection connection, IDbTransaction transaction)
            => Task.CompletedTask;

        /// <summary>Hook gọi trong transaction sau khi Update SP — override để xử lý bảng phụ</summary>
        protected virtual Task OnAfterUpdateInTransactionAsync(TEntity entity, Guid entityId, IDbConnection connection, IDbTransaction transaction)
            => Task.CompletedTask;

        #region Advanced Filter

        /// <summary>
        /// Chuẩn hóa danh sách filter trước khi serialize JSON gửi sang SP:
        /// chuyển <c>Field</c> từ property name PascalCase sang snake_case nếu entity opt-in,
        /// để SP có thể nhúng trực tiếp vào câu lệnh <c>sc.`{field}`</c>.
        /// Khi không opt-in trả về nguyên list ban đầu.
        /// </summary>
        protected IEnumerable<object> ProjectFiltersForSp(IEnumerable<FilterCondition> filters)
        {
            if (!_useSnakeCase)
                return filters.Cast<object>();

            return filters.Select(f => (object)new
            {
                field = ExtNaming.ToSnakeCase(f.Field),
                @operator = f.Operator.ToString(),
                value = f.Value,
                valueTo = f.ValueTo,
                values = f.Values,
            });
        }

        #endregion

        /// <summary>
        /// Cập nhật một trường cụ thể bằng inline SQL.
        /// fieldName đã được validate qua reflection ở tầng Service — không thể inject.
        /// </summary>
        /// <param name="entityId">Id bản ghi</param>
        /// <param name="fieldName">Tên cột trong DB (prop.Name từ reflection)</param>
        /// <param name="value">Giá trị mới đã được convert đúng kiểu</param>
        /// <returns>Số bản ghi bị ảnh hưởng</returns>
        /// CREATED BY: NTDo (24/05/2026)
        public async Task<int> PatchFieldsAsync(Guid entityId, IReadOnlyDictionary<string, object?> fields)
        {
            var setClauses = new List<string>(fields.Count);
            var parameters = new DynamicParameters();
            parameters.Add("@id", entityId.ToString());

            int i = 0;
            foreach (var (fieldName, value) in fields)
            {
                var columnName = _modelType.GetColumnName(fieldName);
                var paramName = $"@v_{i++}";
                setClauses.Add($"`{columnName}` = {paramName}");
                parameters.Add(paramName, value);
            }

            var sql = new StringBuilder(
                $"UPDATE `{_tableName}` SET {string.Join(", ", setClauses)} WHERE `{_keyColumn}` = @id");
            if (_modelType.GetHasDeletedColumn())
                sql.Append($" AND `{_deletedColumn}` = FALSE");

            using var connection = CreateConnection();
            await connection.OpenAsync();

            int rows;
            try
            {
                rows = await connection.ExecuteAsync(sql.ToString(), parameters, commandType: CommandType.Text);
            }
            catch (MySqlException ex)
            {
                throw TranslateMySqlException(ex);
            }

            if (rows > 0)
            {
                _cache.Remove($"{_tableName}_all");
                _cache.Remove($"{_tableName}_{entityId}");
            }

            return rows;
        }

        public async Task<int> PatchFieldAsync(Guid entityId, string fieldName, object? value)
        {
            // fieldName từ Service luôn là C# property name → convert sang DB column name nếu opt-in
            var columnName = _modelType.GetColumnName(fieldName);
            var sql = new StringBuilder($"UPDATE `{_tableName}` SET `{columnName}` = @value WHERE `{_keyColumn}` = @id");

            if (_modelType.GetHasDeletedColumn())
                sql.Append($" AND `{_deletedColumn}` = FALSE");

            using var connection = CreateConnection();
            await connection.OpenAsync();

            int rows;
            try
            {
                rows = await connection.ExecuteAsync(sql.ToString(),
                    new { value, id = entityId.ToString() },
                    commandType: CommandType.Text);
            }
            catch (MySqlException ex)
            {
                throw TranslateMySqlException(ex);
            }

            if (rows > 0)
            {
                _cache.Remove($"{_tableName}_all");
                _cache.Remove($"{_tableName}_{entityId}");
                _logger.LogInformation("[XÓA CACHE] PatchFieldAsync - Bảng: {Table} | ID: {Id} | Trường: {Field}",
                    _tableName, entityId, fieldName);
            }

            return rows;
        }

        private async Task ValidateUniqueColumnsAsync(TEntity entity, Guid? excludeId = null)
        {
            var uniqueColumnsRaw = _modelType.GetUniqueColumns();
            if (string.IsNullOrWhiteSpace(uniqueColumnsRaw)) return;

            var uniqueColumns = uniqueColumnsRaw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c));

            using var connection = CreateConnection();
            await connection.OpenAsync();

            foreach (var column in uniqueColumns)
            {
                // UniqueColumns trong [ConfigTable] là property name (PascalCase). DB column = snake_case nếu opt-in.
                var prop = _modelType.GetProperty(column);
                if (prop == null) continue;

                var value = prop.GetValue(entity);
                if (value == null) continue;

                var dbColumn = _modelType.GetColumnName(column);
                var sql = excludeId.HasValue
                    ? $"SELECT COUNT(*) FROM `{_tableName}` WHERE `{dbColumn}` = @value AND `{_keyColumn}` != @excludeId"
                    : $"SELECT COUNT(*) FROM `{_tableName}` WHERE `{dbColumn}` = @value";

                var count = await connection.ExecuteScalarAsync<int>(sql, new { value, excludeId = excludeId?.ToString() });
                if (count > 0)
                {
                    var displayName = _modelType.GetColumnDisplayName(column);
                    throw new DuplicateEntityException($"{displayName} '{value}' đã tồn tại", column);
                }
            }
        }

        #endregion
    }
}
