namespace Nine.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    public class AppBundle : IAppBundle
    {
#if WINDOWS_PHONE || NETFX_CORE
        public async Task<Stream> Get(string filename)
        {
            try
            {
                var location = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var file = await location.GetFileAsync(filename.Replace("/", "\\"));
                if (file == null) return null;
                return await file.OpenStreamForReadAsync();
            }
            catch
            {
                return null;
            }
        }
#elif ANDROID
        private readonly Android.Content.Context context;
        private readonly Type resourceType;
        private readonly Lazy<Dictionary<string, Type>> classMap;
        private readonly string defaultResourceType;
        private static readonly char[] pathSeperators = new[] { '/', '\\' };

        public AppBundle(Android.Content.Context context, Type resourceType, string defaultResourceType = "raw")
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            this.context = context;
            this.defaultResourceType = defaultResourceType;
            this.classMap = new Lazy<Dictionary<string, Type>>(CreateClassMap);
        }

        private Dictionary<string, Type> CreateClassMap()
        {
            return resourceType.GetNestedTypes().ToDictionary(type => type.Name, StringComparer.OrdinalIgnoreCase);
        }

        public Task<Stream> Get(string filename)
        {
            var index = filename.IndexOfAny(pathSeperators);
            var className = index < 0 ? defaultResourceType : filename.Substring(0, index);
            var id = GetResourceIdentifier(className, Path.GetFileNameWithoutExtension(filename));

            return Task.FromResult(id != null ? context.Resources.OpenRawResource(id.Value) : null);
        }

        private int? GetResourceIdentifier(string className, string memberName)
        {
            Type classType;
            if (!classMap.Value.TryGetValue(className, out classType))
            {
                return null;
            }

            var field = classType.GetTypeInfo().DeclaredFields.FirstOrDefault(
                f => f.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase));

            if (field == null)
            {
                return null;
            }

            return (int)field.GetValue(null);
        }

#elif iOS
        public Task<Stream> Get(string filename)
        {
            throw new NotSupportedException();
        }
#elif WINDOWS
        public Task<Stream> Get(string filename)
        {
            throw new NotSupportedException();
        }
#else
        public Task<Stream> Get(string filename)
        {
            throw new NotSupportedException();
        }
#endif
    }
}
