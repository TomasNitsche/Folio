using System.Data;
using Dapper;

namespace Folio.Infrastructure.Persistence.Sql;

internal static class DapperTypeHandlers
{
    private static bool _registered;
    private static readonly object Sync = new();

    public static void Register()
    {
        if (_registered) return;

        lock (Sync)
        {
            if (_registered) return;

            SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
            SqlMapper.AddTypeHandler(new NullableDateOnlyTypeHandler());
            _registered = true;
        }
    }

    private sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly Parse(object value) => value switch
        {
            DateTime dt => DateOnly.FromDateTime(dt),
            string s => DateOnly.Parse(s),
            _ => throw new DataException($"Cannot convert {value.GetType()} to DateOnly.")
        };
    }

    private sealed class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly? value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.HasValue
                ? value.Value.ToDateTime(TimeOnly.MinValue)
                : DBNull.Value;
        }

        public override DateOnly? Parse(object value) => value switch
        {
            null => null,
            DBNull => null,
            DateTime dt => DateOnly.FromDateTime(dt),
            string s => DateOnly.Parse(s),
            _ => throw new DataException($"Cannot convert {value.GetType()} to DateOnly?.")
        };
    }
}
