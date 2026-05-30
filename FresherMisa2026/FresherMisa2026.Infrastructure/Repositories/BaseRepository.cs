using Dapper;
using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.AdvancedFilter;
using FresherMisa2026.Entities.Department;
using FresherMisa2026.Entities.Exceptions;
using FresherMisa2026.Entities.Extensions;
using FresherMisa2026.Entities.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
                var rawKeyName = match.Groups[2].Value.Split('.').Last(); // "UQ_EmployeeCode"
                var columnName = rawKeyName.StartsWith("UQ_", StringComparison.OrdinalIgnoreCase)
                    ? rawKeyName[3..]
                    : rawKeyName; // "EmployeeCode"
                var fieldName = _modelType.GetColumnDisplayName(columnName); // "Mã nhân viên"
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
        protected string _keyName;
        public Type _modelType = null;
        private readonly CacheSettings _cacheSettings;

        protected IMemoryCache _cache;
        protected ILogger<BaseRepository<TEntity>> _logger;

        //Constructor
        public BaseRepository(IConfiguration configuration, IMemoryCache cache, ILogger<BaseRepository<TEntity>> logger, IOptions<CacheSettings> cacheSettings)
        {
            _configuration = configuration;
            _cache = cache;
            _logger = logger;
            _cacheSettings = cacheSettings.Value;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")!;
            _modelType = typeof(TEntity);
            _tableName = _modelType.GetTableName();
            _keyName = _modelType.GetKeyName();
        }
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

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.ExpirationMinutes));
            _logger.LogInformation("[TRUY VẤN DB] GetEntitiesAsync - Bảng: {Table} | Lấy được {Count} bản ghi trong {ElapsedMs}ms | Đã lưu cache {ExpirationMinutes} phút",
                _tableName, result.Count(), sw.ElapsedMilliseconds, _cacheSettings.ExpirationMinutes);
            return result;
        }

        /// <summary>
        /// Lấy tất cả theo command text
        /// </summary>
        /// <returns></returns>
        /// CREATED BY: DVHAI (11/07/2021)
        protected virtual async Task<IEnumerable<TEntity>> GetEntitiesUsingCommandTextAsync()
        {
            var query = new StringBuilder($"select * from {_tableName}");
            int whereCount = 0;

            if (_modelType.GetHasDeletedColumn())
            {
                whereCount++;
                query.Append($" where IsDeleted = FALSE");
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
                    _cache.Set(cacheKey, found, TimeSpan.FromMinutes(_cacheSettings.ExpirationMinutes));
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

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.ExpirationMinutes));
            _logger.LogInformation("[TRUY VẤN DB] GetEntityByIDAsync - Bảng: {Table} | ID: {Id} | Lấy dữ liệu trong {ElapsedMs}ms | Đã lưu cache {ExpirationMinutes} phút",
                _tableName, entityId, sw.ElapsedMilliseconds, _cacheSettings.ExpirationMinutes);
            return result;
        }

        /// <summary>
        /// Lấy bản ghi theo id dùng command text
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual async Task<TEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var query = new StringBuilder($"select * from {_tableName}");
            int whereCount = 0;

            Func<StringBuilder, bool> AppendWhere = (query) => { query.Append(whereCount == 0 ? " WHERE " : " AND "); return true; };

            var primaryKey = _keyName;

            if (primaryKey != null)
            {
                AppendWhere(query);
                query.Append($"{primaryKey} = @Id");
                whereCount++;
            }

            if (_modelType.GetHasDeletedColumn())
            {
                AppendWhere(query);
                query.Append("IsDeleted = FALSE");
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
                    dynamicParams.Add($"@v_{_keyName}", entityId);

                    //2. Kết nối tới CSDL:
                    rowAffects = await connection.ExecuteAsync($"Proc_Delete{_tableName}ById", param: dynamicParams, transaction: transaction, commandType: CommandType.StoredProcedure);

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
                    dynamicParams.Add($"@v_{_keyName}", id);

                    totalRowAffects += await connection.ExecuteAsync(
                        $"Proc_Delete{_tableName}ById",
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
                    rowAffects = await connection.ExecuteAsync($"Proc_Insert{_tableName}", param: parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

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
                    rowAffects = await connection.ExecuteAsync($"Proc_Update{_tableName}", param: parameters, transaction: transaction, commandType: CommandType.StoredProcedure);

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

            string store = string.Format("Proc_{0}_FilterPaging", _tableName);
            var parameters = new DynamicParameters();
            parameters.Add("@v_pageIndex", pageIndex);
            parameters.Add("@v_pageSize", pageSize);
            parameters.Add("@v_search", search);
            parameters.Add("@v_sort", sort);
            parameters.Add("@v_searchFields", JsonSerializer.Serialize(searchFields));

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
                var propertyName = property.Name;
                var propertyValue = property.GetValue(entity);
                var propertyType = property.PropertyType;

                if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                    parameters.Add($"@v_{propertyName}", propertyValue, DbType.String);
                else
                    parameters.Add($"@v_{propertyName}", propertyValue);
            }

            return parameters;
        }

        #region Advanced Filter

        /// <summary>
        /// Approach 1: C# tự build câu SQL động — field names whitelist qua reflection, values luôn parameterized.
        /// Endpoint: POST /api/{entity}/AdvancedFilter
        /// </summary>
        public virtual async Task<(long Total, IEnumerable<TEntity> Data)> GetAdvancedFilterPagingAsync(AdvancedFilterRequest request)
        {
            var filters = request.Filters ?? new List<FilterCondition>();
            var parameters = new DynamicParameters();

            // mandatory conditions — luôn AND (IsDeleted...)
            var mandatoryParts = new List<string>();
            if (_modelType.GetHasDeletedColumn())
                mandatoryParts.Add("IsDeleted = FALSE");

            // user conditions — nối theo Logic (And/Or)
            var userParts = new List<string>();
            BuildFilterConditions(filters, parameters, userParts);

            var whereSection = BuildWhereSection(mandatoryParts, userParts, request.Logic);

            var orderBy = BuildSortSql(request.Sort);
            var pageIndex = Math.Max(1, request.PageIndex);
            var pageSize = Math.Max(1, request.PageSize);
            var offset = (pageIndex - 1) * pageSize;

            parameters.Add("@_limit", pageSize);
            parameters.Add("@_offset", offset);

            var dataSql = $"SELECT * FROM `{_tableName}` {whereSection} {orderBy} LIMIT @_limit OFFSET @_offset";
            var countSql = $"SELECT COUNT(*) FROM `{_tableName}` {whereSection}";

            using var connection = CreateConnection();
            await connection.OpenAsync();

            var data = await connection.QueryAsync<TEntity>(dataSql, parameters, commandType: CommandType.Text);
            var total = await connection.ExecuteScalarAsync<long>(countSql, parameters, commandType: CommandType.Text);

            return (total, data.ToList());
        }

        /// <summary>
        /// Ghép mandatory (luôn AND) và user conditions (theo logic) thành WHERE clause.
        /// Kết quả: WHERE mandatory1 AND mandatory2 AND (user1 OR/AND user2 ...)
        /// </summary>
        protected static string BuildWhereSection(List<string> mandatoryParts, List<string> userParts, FilterLogic logic)
        {
            var parts = new List<string>(mandatoryParts);

            if (userParts.Count > 0)
            {
                var sep = logic == FilterLogic.Or ? " OR " : " AND ";
                var userBlock = userParts.Count == 1
                    ? userParts[0]
                    : $"({string.Join(sep, userParts)})";
                parts.Add(userBlock);
            }

            return parts.Count > 0 ? $"WHERE {string.Join(" AND ", parts)}" : string.Empty;
        }

        /// <summary>
        /// Approach 2: Truyền filters dưới dạng JSON vào stored procedure — SP tự build WHERE.
        /// C# vẫn validate field names trước khi gọi SP.
        /// Endpoint: POST /api/{entity}/AdvancedFilterProc
        /// </summary>
        public async Task<(long Total, IEnumerable<TEntity> Data)> GetAdvancedFilterPagingWithProcAsync(AdvancedFilterRequest request)
        {
            var filters = request.Filters ?? new List<FilterCondition>();
            ValidateFilterFields(filters);

            using var connection = CreateConnection();
            await connection.OpenAsync();

            var store = $"Proc_{_tableName}_AdvancedFilterPaging";
            var parameters = new DynamicParameters();
            parameters.Add("@v_pageIndex", Math.Max(1, request.PageIndex));
            parameters.Add("@v_pageSize", Math.Max(1, request.PageSize));
            parameters.Add("@v_sort", request.Sort ?? string.Empty);
            parameters.Add("@v_filters", JsonSerializer.Serialize(filters));
            parameters.Add("@v_logic", (int)request.Logic);

            using var reader = await connection.QueryMultipleAsync(
                new CommandDefinition(store, parameters, commandType: CommandType.StoredProcedure));

            var data = (await reader.ReadAsync<TEntity>()).ToList();
            var total = await reader.ReadFirstAsync<long>();

            return (total, data);
        }

        /// <summary>
        /// Validate tất cả field names trong filters phải tồn tại trên entity — dùng cho cả 2 approach.
        /// </summary>
        private void ValidateFilterFields(List<FilterCondition> filters)
        {
            var validProps = _modelType.GetProperties()
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var filter in filters)
            {
                if (!validProps.Contains(filter.Field))
                    throw new ArgumentException($"Trường '{filter.Field}' không tồn tại trên entity {_tableName}");
            }
        }

        /// <summary>
        /// Build ORDER BY từ sort string (ví dụ: "-Salary,+EmployeeName").
        /// Field names được validate qua reflection — không thể inject.
        /// </summary>
        protected string BuildSortSql(string? sort)
        {
            if (string.IsNullOrWhiteSpace(sort))
                return $"ORDER BY `{_keyName}` DESC";

            var validProps = _modelType.GetProperties()
                .ToDictionary(p => p.Name, p => p.Name, StringComparer.OrdinalIgnoreCase);

            var orderParts = sort
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(part =>
                {
                    var trimmed = part.Trim();
                    var isDesc = trimmed.StartsWith('-');
                    var fieldName = trimmed.TrimStart('+', '-').Trim();
                    return validProps.TryGetValue(fieldName, out var actual)
                        ? $"`{actual}` {(isDesc ? "DESC" : "ASC")}"
                        : null;
                })
                .Where(s => s != null)
                .ToList();

            return orderParts.Count > 0
                ? $"ORDER BY {string.Join(", ", orderParts)}"
                : $"ORDER BY `{_keyName}` DESC";
        }

        /// <summary>
        /// Build danh sách WHERE conditions từ filters.
        /// Field names: whitelist qua reflection → safe. Values: Dapper parameters → safe.
        /// </summary>
        protected void BuildFilterConditions(List<FilterCondition> filters, DynamicParameters parameters, List<string> whereParts)
        {
            var validProps = _modelType.GetProperties()
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];

                if (!validProps.TryGetValue(filter.Field, out var prop))
                    throw new ArgumentException($"Trường '{filter.Field}' không tồn tại trên entity {_tableName}");

                var col = $"`{prop.Name}`";
                var p = $"@fp{i}";
                var pTo = $"@fp{i}to";
                string? condition = null;

                switch (filter.Operator)
                {
                    case FilterOperator.Eq:
                        condition = $"{col} = {p}";
                        parameters.Add(p, ConvertValue(filter.Value, prop.PropertyType));
                        break;
                    case FilterOperator.Neq:
                        condition = $"{col} <> {p}";
                        parameters.Add(p, ConvertValue(filter.Value, prop.PropertyType));
                        break;
                    case FilterOperator.Contains:
                        condition = $"{col} LIKE CONCAT('%', {p}, '%')";
                        parameters.Add(p, filter.Value?.GetString());
                        break;
                    case FilterOperator.NotContains:
                        condition = $"{col} NOT LIKE CONCAT('%', {p}, '%')";
                        parameters.Add(p, filter.Value?.GetString());
                        break;
                    case FilterOperator.StartsWith:
                        condition = $"{col} LIKE CONCAT({p}, '%')";
                        parameters.Add(p, filter.Value?.GetString());
                        break;
                    case FilterOperator.EndsWith:
                        condition = $"{col} LIKE CONCAT('%', {p})";
                        parameters.Add(p, filter.Value?.GetString());
                        break;
                    case FilterOperator.Empty:
                        condition = $"({col} IS NULL OR {col} = '')";
                        break;
                    case FilterOperator.NotEmpty:
                        condition = $"({col} IS NOT NULL AND {col} <> '')";
                        break;
                    case FilterOperator.Gt:
                        condition = $"{col} > {p}";
                        parameters.Add(p, ConvertValue(filter.Value, prop.PropertyType));
                        break;
                    case FilterOperator.Lt:
                        condition = $"{col} < {p}";
                        parameters.Add(p, ConvertValue(filter.Value, prop.PropertyType));
                        break;
                    case FilterOperator.Gte:
                        condition = $"{col} >= {p}";
                        parameters.Add(p, ConvertValue(filter.Value, prop.PropertyType));
                        break;
                    case FilterOperator.Lte:
                        condition = $"{col} <= {p}";
                        parameters.Add(p, ConvertValue(filter.Value, prop.PropertyType));
                        break;
                    case FilterOperator.Between:
                        condition = $"{col} BETWEEN {p} AND {pTo}";
                        parameters.Add(p, ConvertValue(filter.Value, prop.PropertyType));
                        parameters.Add(pTo, ConvertValue(filter.ValueTo, prop.PropertyType));
                        break;
                    case FilterOperator.NotBetween:
                        condition = $"{col} NOT BETWEEN {p} AND {pTo}";
                        parameters.Add(p, ConvertValue(filter.Value, prop.PropertyType));
                        parameters.Add(pTo, ConvertValue(filter.ValueTo, prop.PropertyType));
                        break;
                    case FilterOperator.Today:
                        condition = $"DATE({col}) = CURDATE()";
                        break;
                    case FilterOperator.ThisWeek:
                        condition = $"{col} BETWEEN DATE_SUB(CURDATE(), INTERVAL WEEKDAY(CURDATE()) DAY) " +
                                    $"AND DATE_ADD(DATE_SUB(CURDATE(), INTERVAL WEEKDAY(CURDATE()) DAY), INTERVAL 6 DAY)";
                        break;
                    case FilterOperator.ThisMonth:
                        condition = $"{col} BETWEEN DATE_FORMAT(CURDATE(), '%Y-%m-01') AND LAST_DAY(CURDATE())";
                        break;
                    case FilterOperator.ThisYear:
                        condition = $"{col} BETWEEN DATE_FORMAT(CURDATE(), '%Y-01-01') AND DATE_FORMAT(CURDATE(), '%Y-12-31')";
                        break;
                    case FilterOperator.LastNDays:
                        var lastN = GetIntValue(filter.Value, 30);
                        condition = $"{col} >= DATE_SUB(CURDATE(), INTERVAL {lastN} DAY)";
                        break;
                    case FilterOperator.NextNDays:
                        var nextN = GetIntValue(filter.Value, 7);
                        condition = $"{col} BETWEEN CURDATE() AND DATE_ADD(CURDATE(), INTERVAL {nextN} DAY)";
                        break;
                    case FilterOperator.In:
                        var inParams = BuildMultiValueParams(filter.Values, prop.PropertyType, $"fpi{i}", parameters);
                        condition = inParams.Count > 0
                            ? $"{col} IN ({string.Join(", ", inParams)})"
                            : "1 = 0";
                        break;
                    case FilterOperator.NotIn:
                        var notInParams = BuildMultiValueParams(filter.Values, prop.PropertyType, $"fpni{i}", parameters);
                        condition = notInParams.Count > 0
                            ? $"{col} NOT IN ({string.Join(", ", notInParams)})"
                            : "1 = 1";
                        break;
                }

                if (condition != null)
                    whereParts.Add(condition);
            }
        }

        private List<string> BuildMultiValueParams(List<JsonElement>? values, Type targetType, string prefix, DynamicParameters parameters)
        {
            if (values == null || values.Count == 0) return new List<string>();

            var paramNames = new List<string>();
            for (int j = 0; j < values.Count; j++)
            {
                var key = $"@{prefix}_{j}";
                parameters.Add(key, ConvertValue(values[j], targetType));
                paramNames.Add(key);
            }
            return paramNames;
        }

        private static object? ConvertValue(JsonElement? element, Type targetType)
        {
            if (!element.HasValue || element.Value.ValueKind == JsonValueKind.Null) return null;

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            var kind = element.Value.ValueKind;

            if (underlying == typeof(Guid)) return element.Value.GetString();
            if (underlying == typeof(DateTime))
                return kind == JsonValueKind.String ? element.Value.GetDateTime() : (object?)null;
            if (underlying == typeof(decimal))
                return kind == JsonValueKind.Number ? element.Value.GetDecimal() : decimal.Parse(element.Value.GetString() ?? "0");
            if (underlying == typeof(double))
                return kind == JsonValueKind.Number ? element.Value.GetDouble() : double.Parse(element.Value.GetString() ?? "0");
            if (underlying == typeof(int))
                return kind == JsonValueKind.Number ? element.Value.GetInt32() : int.Parse(element.Value.GetString() ?? "0");
            if (underlying == typeof(long))
                return kind == JsonValueKind.Number ? element.Value.GetInt64() : long.Parse(element.Value.GetString() ?? "0");
            if (underlying == typeof(bool))
            {
                if (kind == JsonValueKind.True) return true;
                if (kind == JsonValueKind.False) return false;
                return bool.TryParse(element.Value.GetString(), out var b) ? b : false;
            }

            return element.Value.GetString();
        }

        private static int GetIntValue(JsonElement? element, int defaultValue = 0)
        {
            if (!element.HasValue || element.Value.ValueKind == JsonValueKind.Null) return defaultValue;
            return element.Value.ValueKind == JsonValueKind.Number
                ? element.Value.GetInt32()
                : defaultValue;
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
        public async Task<int> PatchFieldAsync(Guid entityId, string fieldName, object? value)
        {
            var sql = new StringBuilder($"UPDATE `{_tableName}` SET `{fieldName}` = @value WHERE `{_keyName}` = @id");

            if (_modelType.GetHasDeletedColumn())
                sql.Append(" AND IsDeleted = FALSE");

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
                var prop = _modelType.GetProperty(column);
                if (prop == null) continue;

                var value = prop.GetValue(entity);
                if (value == null) continue;

                var sql = excludeId.HasValue
                    ? $"SELECT COUNT(*) FROM {_tableName} WHERE {column} = @value AND {_keyName} != @excludeId"
                    : $"SELECT COUNT(*) FROM {_tableName} WHERE {column} = @value";

                var count = await connection.ExecuteScalarAsync<int>(sql, new { value, excludeId = excludeId?.ToString() });
                if (count > 0)
                {
                    var displayName = _modelType.GetColumnDisplayName(column);
                    throw new DuplicateEntityException($"{displayName} '{value}' đã tồn tại");
                }
            }
        }

        #endregion
    }
}
