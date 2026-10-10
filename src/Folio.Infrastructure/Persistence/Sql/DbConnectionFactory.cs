using Microsoft.Data.SqlClient;

namespace Folio.Infrastructure.Persistence.Sql;

public sealed class DbConnectionFactory(string connectionString)
{
    public string ConnectionString { get; } = connectionString;

    public SqlConnection CreateConnection() => new(ConnectionString);
}
