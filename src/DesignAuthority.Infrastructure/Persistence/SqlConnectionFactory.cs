using Microsoft.Data.SqlClient;

namespace DesignAuthority.Infrastructure.Persistence;

public sealed class SqlConnectionFactory(string connectionString)
{
    public async Task<SqlConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DesignAuthority must be configured outside source control.");

        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
