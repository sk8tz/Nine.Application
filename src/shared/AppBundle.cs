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
        public async Task<Stream> Open(string filename)
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
        private readonly Android.Content.Context _context;
        private readonly Lazy<Dictionary<string, Type>> _classMap;
        private readonly string _defaultResourceType;
        private readonly Type _resourceType;
        private static readonly char[] PathSeperators = new[] { '/', '\\' };

        public AppBundle(Android.Content.Context context, Type resourceType, string defaultResourceType = "raw")
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            _context = context;
            _defaultResourceType = defaultResourceType;
            _resourceType = resourceType;
            _classMap = new Lazy<Dictionary<string, Type>>(CreateClassMap);
        }

        private Dictionary<string, Type> CreateClassMap()
        {
            return _resourceType.GetNestedTypes().ToDictionary(type => type.Name, StringComparer.OrdinalIgnoreCase);
        }

        public Task<Stream> Open(string filename)
        {
            var index = filename.IndexOfAny(PathSeperators);
            var className = index < 0 ? _defaultResourceType : filename.Substring(0, index);
            var id = GetResourceIdentifier(className, Path.GetFileNameWithoutExtension(filename));

            return Task.FromResult(id != null ? _context.Resources.OpenRawResource(id.Value) : null);
        }

        private int? GetResourceIdentifier(string className, string memberName)
        {
            Type classType;
            if (!_classMap.Value.TryGetValue(className, out classType))
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
        public Task<Stream> Open(string filename)
        {
            throw new NotSupportedException();
        }
#elif WINDOWS
        public Task<Stream> Open(string filename)
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
