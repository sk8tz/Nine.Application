namespace Nine.Application
{
    using System.Threading.Tasks;

    public enum PlatformName
    {
        None,
        Windows,
        WindowsPhone,
        WindowsStore,
        Android,
        iOS,
        Mac,
        Portable,
    }
    
    public class ClientInfo
    {
        public PlatformName OperatingSystem { get; set; }
        public string OperatingSystemVersion { get; set; }
        public string ClientVersion { get; set; }
        public string DeviceUniqueId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceManufacturer { get; set; }
    }

    public interface IClientInfoProvider
    {
        PlatformName Platform { get; }

        Task<ClientInfo> GetAsync();

        Task<string> GetPushNotificationChannelAsync();
    }
}
