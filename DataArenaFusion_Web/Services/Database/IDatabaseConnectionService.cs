using System.Data.Common;
using DataArenaFusion.Models;
using System.Data;

namespace DataArenaFusion.Services.Database
{
    public interface IDatabaseConnectionService
    {
        DatabaseProvider Provider { get; }
        string DisplayName { get; }
        int DefaultPort { get; }
        DbConnection CreateConnection(DatabaseConnectionSettings settings);
        string BuildConnectionString(DatabaseConnectionSettings settings);
        bool TryTestConnection(DatabaseConnectionSettings settings, out string mensaje);
        string MigrarDatos(DatabaseConnectionSettings settings, DataTable tabla);
    }
}
