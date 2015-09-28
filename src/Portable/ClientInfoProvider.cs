namespace Nine.Application
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class ClientInfoProvider : IClientInfoProvider
    {
#if NETFX_CORE
        public PlatformName Platform => PlatformName.WindowsStore;

        public async Task<ClientInfo> GetAsync()
        {
            var result = new ClientInfo();
            result.OperatingSystem = PlatformName.WindowsStore;
            result.DeviceUniqueId = GetDeviceDeviceUniqueId();
            result.DeviceName = await TryGetRootDeviceInfoAsync("System.Devices.ModelDeviceName").ConfigureAwait(false);
            result.DeviceManufacturer = await TryGetRootDeviceInfoAsync("System.Devices.DeviceManufacturer").ConfigureAwait(false);
            return result;
        }

        private static string GetDeviceDeviceUniqueId() => null;

        private static async Task<string> TryGetRootDeviceInfoAsync(string propertyKey)
        {
            try
            {
                // http://stackoverflow.com/questions/18599589/getting-os-platform-and-device-information-on-windows-8
                object result;
                var pnp = await Windows.Devices.Enumeration.Pnp.PnpObject.CreateFromIdAsync(
                    Windows.Devices.Enumeration.Pnp.PnpObjectType.DeviceContainer, "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}", new[] { propertyKey });
                return pnp != null && pnp.Properties.TryGetValue(propertyKey, out result) && result != null ? result.ToString() : "";
            }
            catch
            {
                return null;
            }
        }
#else
        public Task<ClientInfo> GetAsync()
        {
            try
            {
                return Task.FromResult(GetClientInfo());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }
        }

#if WINDOWS_PHONE
        public PlatformName Platform => PlatformName.WindowsPhone;

        public ClientInfo GetClientInfo()
        {
            return new ClientInfo
            {
                OperatingSystem = PlatformName.WindowsPhone,
                OperatingSystemVersion = Environment.OSVersion.Version.ToString(),
                DeviceUniqueId = GetDeviceDeviceUniqueId(),
                DeviceName = Microsoft.Phone.Info.DeviceStatus.DeviceName,
                DeviceManufacturer = Microsoft.Phone.Info.DeviceStatus.DeviceManufacturer,
            };
        }

        private string GetDeviceDeviceUniqueId()
        {
            // For Windows Phone OS 7.1 apps running on Windows Phone 8 devices, 
            // the DeviceDeviceUniqueId value is unique per device. 
            object value;
            return Microsoft.Phone.Info.DeviceExtendedProperties.TryGetValue("DeviceDeviceUniqueId", out value) && value is byte[] ? Convert.ToBase64String((byte[])value) : "";
        }
#elif ANDROID
        public PlatformName Platform => PlatformName.Android;

        private readonly Android.Content.Context context;

        public ClientInfoProvider(Android.Content.Context context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            this.context = context;
        }

        public ClientInfo GetClientInfo()
        {
            var result = new ClientInfo
            {
                OperatingSystem = PlatformName.Android,
                OperatingSystemVersion = Android.OS.Build.VERSION.Release,
                DeviceName = Android.OS.Build.Model,
                DeviceManufacturer = Android.OS.Build.Manufacturer,
            };

            if (context != null)
            {
                result.DeviceUniqueId = Android.Provider.Settings.Secure.GetString(
                    context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
            }
            return result;
        }
#elif iOS
        public PlatformName Platform => PlatformName.iOS;

        public ClientInfo GetClientInfo()
        {
            return new ClientInfo
            {
                OperatingSystem = PlatformName.iOS,
                OperatingSystemVersion = UIKit.UIDevice.CurrentDevice.SystemVersion,
                DeviceUniqueId = UIKit.UIDevice.CurrentDevice.IdentifierForVendor.AsString(),
                DeviceName = UIKit.UIDevice.CurrentDevice.SystemName,
                DeviceManufacturer = "Apple",
            };
        }
#elif WINDOWS
        public PlatformName Platform => PlatformName.Windows;

        public ClientInfo GetClientInfo()
        {
            return new ClientInfo
            {
                OperatingSystem = PlatformName.Windows,
                OperatingSystemVersion = Environment.OSVersion.VersionString,
            };
        }
#else
        public PlatformName Platform => PlatformName.Portable;

        public ClientInfo GetClientInfo()
        {
            return new ClientInfo { OperatingSystem = PlatformName.Portable };
        }
#endif
#endif
    }
}
