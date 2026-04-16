using System.Data;
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
            // Paso 1: verificar servidor y credenciales (conectando a 'postgres')
            try
            {
                using var adminConnection = new NpgsqlConnection(BuildAdminConnectionString(settings));
                adminConnection.Open();
            }
            catch (Exception ex)
            {
                mensaje = $"No se pudo conectar al servidor '{settings.Host}:{settings.Port}'.\n{ex.Message}";
                return false;
            }

            // Paso 2: verificar si la base de datos existe
            try
            {
                using var adminConnection = new NpgsqlConnection(BuildAdminConnectionString(settings));
                adminConnection.Open();

                using var cmd = adminConnection.CreateCommand();
                cmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @db;";
                var param = cmd.CreateParameter();
                param.ParameterName = "@db";
                param.Value = settings.Database;
                cmd.Parameters.Add(param);

                var existe = cmd.ExecuteScalar() != null;
                mensaje = existe
                    ? $"Conexion exitosa. Base de datos '{settings.Database}' encontrada en PostgreSQL."
                    : $"Servidor OK, pero '{settings.Database}' no existe aun. Se creara automaticamente al migrar.";
                return true;
            }
            catch (Exception ex)
            {
                mensaje = $"Servidor accesible, pero error al verificar la base de datos.\n{ex.Message}";
                return false;
            }
        }

        public string MigrarDatos(DatabaseConnectionSettings settings, DataTable tabla)
        {
            if (tabla.Rows.Count == 0)
            {
                return "No hay datos para migrar.";
            }

            if (!tabla.Columns.Contains("Fuente"))
            {
                return "La tabla no contiene la columna Fuente para separar los archivos.";
            }

            EnsureDatabaseExists(settings);

            using var connection = CreateConnection(settings);
            connection.Open();

            var existingTables = LoadExistingTables(connection);
            var columnas = tabla.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .Where(c => !string.Equals(c, "Fuente", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var grupos = DatabaseMigrationHelper.GroupBySource(tabla, "Fuente").ToList();
            var tablasCreadas = 0;
            var filasInsertadas = 0;

            foreach (var grupo in grupos)
            {
                var baseTableName = DatabaseMigrationHelper.NormalizeIdentifier(Path.GetFileNameWithoutExtension(grupo.Key), "datos");
                var tableName = DatabaseMigrationHelper.EnsureUniqueName(existingTables, baseTableName);
                existingTables.Add(tableName);

                CrearTabla(connection, tableName, columnas);
                tablasCreadas++;

                using var transaction = connection.BeginTransaction();
                using var cmd = connection.CreateCommand();
                cmd.Transaction = transaction;

                var nombresParametros = new List<string>();
                for (int i = 0; i < columnas.Count; i++)
                {
                    var nombreParametro = $"@p{i}";
                    nombresParametros.Add(nombreParametro);

                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = nombreParametro;
                    cmd.Parameters.Add(parameter);
                }

                var columnasSql = string.Join(", ", columnas.Select(QuoteIdentifier));
                var parametrosSql = string.Join(", ", nombresParametros);
                cmd.CommandText = $"INSERT INTO {QuoteIdentifier(tableName)} ({columnasSql}) VALUES ({parametrosSql});";
                cmd.Prepare();

                foreach (var fila in grupo)
                {
                    for (int i = 0; i < columnas.Count; i++)
                    {
                        cmd.Parameters[i].Value = fila[columnas[i]]?.ToString() ?? string.Empty;
                    }
                    cmd.ExecuteNonQuery();
                    filasInsertadas++;
                }

                transaction.Commit();
            }

            return $"PostgreSQL listo. Se crearon {tablasCreadas} tablas e insertaron {filasInsertadas} filas.";
        }

        private void EnsureDatabaseExists(DatabaseConnectionSettings settings)
        {
            using var adminConnection = new NpgsqlConnection(BuildAdminConnectionString(settings));
            adminConnection.Open();

            using var existsCmd = adminConnection.CreateCommand();
            existsCmd.CommandText = "SELECT 1 FROM pg_database WHERE datname = @db;";
            var existsParam = existsCmd.CreateParameter();
            existsParam.ParameterName = "@db";
            existsParam.Value = settings.Database;
            existsCmd.Parameters.Add(existsParam);

            var exists = existsCmd.ExecuteScalar() != null;
            if (exists)
            {
                return;
            }

            using var createCmd = adminConnection.CreateCommand();
            createCmd.CommandText = $"CREATE DATABASE {QuoteIdentifier(settings.Database)};";
            createCmd.ExecuteNonQuery();
        }

        private string BuildAdminConnectionString(DatabaseConnectionSettings settings)
        {
            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = settings.Host,
                Port = settings.Port > 0 ? settings.Port : DefaultPort,
                Database = "postgres",
                Username = settings.Username,
                Password = settings.Password,
                SslMode = settings.UseSsl ? SslMode.Require : SslMode.Disable,
                Timeout = 15
            };

            return builder.ConnectionString;
        }

        private static List<string> LoadExistingTables(DbConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public';";

            var tables = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }

            return tables;
        }

        private void CrearTabla(DbConnection connection, string tableName, IReadOnlyList<string> columnas)
        {
            using var cmd = connection.CreateCommand();
            var definiciones = columnas.Select(columna => $"{QuoteIdentifier(columna)} TEXT").ToList();
            cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {QuoteIdentifier(tableName)} ({string.Join(", ", definiciones)});";
            cmd.ExecuteNonQuery();
        }



        private static string QuoteIdentifier(string value)
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
    }
}
