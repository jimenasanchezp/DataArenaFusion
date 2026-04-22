namespace DataArenaFusion.Core.Models
{
    public sealed class DatabaseConnectionSettings
    {
        public DatabaseProvider Provider { get; set; }
        public string Host { get; set; } = "localhost";
        public int Port { get; set; }
        public string Database { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; }

        public DatabaseConnectionSettings Clone()
        {
            return new DatabaseConnectionSettings
            {
                Provider = Provider,
                Host = Host,
                Port = Port,
                Database = Database,
                Username = Username,
                Password = Password,
                UseSsl = UseSsl
            };
        }
    }
}
