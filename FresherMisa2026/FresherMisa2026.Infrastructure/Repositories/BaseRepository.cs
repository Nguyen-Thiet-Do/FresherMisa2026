using Dapper;
using FresherMisa2026.Application.Interfaces;
using FresherMisa2026.Entities;
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
        string _connectionString = string.Empty;
        IConfiguration _configuration;
        protected string _tableName;
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
        private async Task<IEnumerable<TEntity>> GetEntitiesUsingCommandTextAsync()
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
                var keyName = _modelType.GetKeyName();
                var keyProp = typeof(TEntity).GetProperty(keyName);
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
        private async Task<TEntity> GetEntitieByIdUsingCommandTextAsync(string id)
        {
            var query = new StringBuilder($"select * from {_tableName}");
            int whereCount = 0;

            Func<StringBuilder, bool> AppendWhere = (query) => { if (whereCount == 0) query.Append(" where "); return true; };

            var primaryKey = _modelType.GetKeyName();

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
                    //1. Lấy tên của khóa chính
                    var keyName = _modelType.GetKeyName();

                    var dynamicParams = new DynamicParameters();
                    dynamicParams.Add($"@v_{keyName}", entityId);

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
        /// Thêm bản ghi mới
        /// </summary>
        /// <param name="entity">Thông tin bản ghi</param>
        /// <returns>Số bản ghi thêm mới</returns>
        /// CREATED BY: DVHAI (11/07/2021)
        public async Task<int> InsertAsync(TEntity entity)
        {
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
            var keyName = _modelType.GetKeyName();
            var keyProperty = entity.GetType().GetProperty(keyName);

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
            var keyName = _modelType.GetKeyName();
            var keyProperty = entity.GetType().GetProperty(keyName);

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

        #endregion
    }
}
