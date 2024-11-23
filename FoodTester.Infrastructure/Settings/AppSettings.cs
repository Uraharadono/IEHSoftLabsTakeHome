namespace FoodTester.Infrastructure.Settings
{
    public interface IAppSettings
    {
        bool IsDebug { get; }
        string AdminEmail { get; }
    }

    public class AppSettings : IAppSettings
    {
        public bool IsDebug { get; set; }
        public string AdminEmail { get; set; }
        public ClientAppSettings ClientAppSettings { get; set; }
        public FolderSettings FolderSettings { get; set; }
        public RabbitMqSettings RabbitMqSettings { get; set; }
    }
}
