namespace Nine.Application
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
#if iOS
    using System.Threading;
    using Foundation;
    using UIKit;
#endif

    public class ClientInfoProvider : IClientInfoProvider
    {
#if NETFX_CORE
        public PlatformName Platform => PlatformName.WindowsStore;

        public Task<ClientInfo> GetAsync()
        {
            var result = new ClientInfo();
            result.OperatingSystem = PlatformName.WindowsStore;
            result.DeviceUniqueId = GetDeviceDeviceUniqueId();
            return Task.FromResult(result);
        }

        private static string GetDeviceDeviceUniqueId() => null;

        public async Task<string> GetPushNotificationChannelAsync()
        {
            var channel = await Windows.Networking.PushNotifications.PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            return channel?.Uri;
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

        public Task<string> GetPushNotificationChannelAsync() => Task.FromResult<string>(null);
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

        public Task<string> GetPushNotificationChannelAsync() => Task.FromResult<string>(null);
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

        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
        private readonly TaskCompletionSource<string> _getPushNotificationChannelTcs = new TaskCompletionSource<string>();
        
        public Task<string> GetPushNotificationChannelAsync()
        {
            _syncContext.Post(x =>
                {
                    // http://developer.xamarin.com/guides/cross-platform/application_fundamentals/notifications/ios/remote_notifications_in_ios/
                    if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                    {
                        var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                                       UIUserNotificationType.Alert |  UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                                       new NSSet());
            
                        UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                        UIApplication.SharedApplication.RegisterForRemoteNotifications();
                    }
                    else
                    {
                        UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert |  UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                        UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
                    }
                }, null);

            return _getPushNotificationChannelTcs.Task;
        }
        
        public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            var DeviceToken = deviceToken.Description;
            if (!string.IsNullOrWhiteSpace(DeviceToken))
            {
                DeviceToken = DeviceToken.Replace("<", "").Replace(">", "").Replace(" ", "");
            }

            _getPushNotificationChannelTcs?.TrySetResult(DeviceToken);
        }

        public void FailedToRegisterForRemoteNotifications (UIApplication application , NSError error)
        {
            _getPushNotificationChannelTcs?.TrySetResult(null);
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

        public Task<string> GetPushNotificationChannelAsync() => Task.FromResult<string>(null);
#else
        public PlatformName Platform => PlatformName.Portable;

        public ClientInfo GetClientInfo() => new ClientInfo { OperatingSystem = PlatformName.Portable };

        public Task<string> GetPushNotificationChannelAsync() => Task.FromResult<string>(null);
#endif
#endif
    }
}
