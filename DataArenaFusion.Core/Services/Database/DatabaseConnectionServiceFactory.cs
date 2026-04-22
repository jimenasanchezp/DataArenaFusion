using DataArenaFusion.Core.Models;

namespace DataArenaFusion.Core.Services.Database
{
    public static class DatabaseConnectionServiceFactory
    {
        public static IDatabaseConnectionService Create(DatabaseProvider provider)
        {
            return provider switch
            {
                DatabaseProvider.MariaDb => new MariaDbConnectionService(),
                DatabaseProvider.PostgreSql => new PostgreSqlConnectionService(),
                _ => throw new NotSupportedException($"Proveedor no soportado: {provider}")
            };
        }
    }
}
