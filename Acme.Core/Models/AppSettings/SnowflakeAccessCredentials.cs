namespace Acme.Core.Models.AppSettings
{
    public class SnowflakeAccessCredentials
    {
        public string Schema { get; set; }

        public string Database { get; set; }

        public string Warehouse { get; set; }

        public string Role { get; set; }

        public string Account { get; set; }

        public string Host { get; set; }
    }
}