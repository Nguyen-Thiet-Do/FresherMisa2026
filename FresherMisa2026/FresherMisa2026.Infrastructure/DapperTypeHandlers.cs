using Dapper;
using System.Data;

namespace FresherMisa2026.Infrastructure
{
    /// <summary>
    /// Cho phép Dapper parse CHAR(36) string → Guid.
    /// Cần thiết khi Guid đến từ computed column (COALESCE, expression) thay vì direct column.
    /// </summary>
    public class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
    {
        public override void SetValue(IDbDataParameter parameter, Guid value)
            => parameter.Value = value.ToString();

        public override Guid Parse(object value)
            => Guid.Parse(value.ToString()!);
    }
}
