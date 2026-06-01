using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace FresherMisa2026.Infrastructure.TypeHandlers
{
    public class GuidListTypeHandler : SqlMapper.TypeHandler<List<Guid>>
    {
        public override List<Guid> Parse(object value)
        {
            if (value is null || value == DBNull.Value) return new List<Guid>();
            var s = value.ToString();
            if (string.IsNullOrEmpty(s)) return new List<Guid>();
            return s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Guid.Parse(x.Trim()))
                    .ToList();
        }

        public override void SetValue(IDbDataParameter parameter, List<Guid> value)
        {
            parameter.Value = value is null ? DBNull.Value : (object)string.Join(",", value);
        }
    }
}
