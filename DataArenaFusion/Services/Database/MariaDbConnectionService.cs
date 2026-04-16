using System.Data.Common;
using DataArenaFusion.Models;
using MySqlConnector;

namespace DataArenaFusion.Services.Database
{
    public sealed class MariaDbConnectionService : IDatabaseConnectionService
    {
        public DatabaseProvider Provider => DatabaseProvider.MariaDb;
        public string DisplayName => "MariaDB";
        public int DefaultPort => 3306;

        public string BuildConnectionString(DatabaseConnectionSettings settings)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = settings.Host,
                Port = (uint)(settings.Port > 0 ? settings.Port : DefaultPort),
                Database = settings.Database,
                UserID = settings.Username,
                Password = settings.Password,
                SslMode = settings.UseSsl ? MySqlSslMode.Required : MySqlSslMode.None,
                AllowUserVariables = true,
                ConnectionTimeout = 15
            };

            return builder.ConnectionString;
        }

        public DbConnection CreateConnection(DatabaseConnectionSettings settings)
        {
            return new MySqlConnection(BuildConnectionString(settings));
        }

        public bool TryTestConnection(DatabaseConnectionSettings settings, out string mensaje)
        {
            try
            {
                using var connection = CreateConnection(settings);
                connection.Open();
                mensaje = "Conexion a MariaDB exitosa.";
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
