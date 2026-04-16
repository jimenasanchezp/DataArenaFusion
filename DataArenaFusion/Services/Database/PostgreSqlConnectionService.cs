using System.Data.Common;
using DataArenaFusion.Models;
using Npgsql;

namespace DataArenaFusion.Services.Database
{
    public sealed class PostgreSqlConnectionService : IDatabaseConnectionService
    {
        public DatabaseProvider Provider => DatabaseProvider.PostgreSql;
        public string DisplayName => "PostgreSQL";
        public int DefaultPort => 5432;

        public string BuildConnectionString(DatabaseConnectionSettings settings)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = settings.Host,
                Port = settings.Port > 0 ? settings.Port : DefaultPort,
                Database = settings.Database,
                Username = settings.Username,
                Password = settings.Password,
                SslMode = settings.UseSsl ? SslMode.Require : SslMode.Disable,
                Timeout = 15
            };

            return builder.ConnectionString;
        }

        public DbConnection CreateConnection(DatabaseConnectionSettings settings)
        {
            return new NpgsqlConnection(BuildConnectionString(settings));
        }

        public bool TryTestConnection(DatabaseConnectionSettings settings, out string mensaje)
        {
            try
            {
                using var connection = CreateConnection(settings);
                connection.Open();
                mensaje = "Conexion a PostgreSQL exitosa.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = ex.Message;
                return false;
            }
        }
    }
}
