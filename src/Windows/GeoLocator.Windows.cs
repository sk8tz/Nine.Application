namespace Nine.Application
{
    using System.Threading.Tasks;

    partial class Geolocator
    {
        public static Task<GeoLocation> FindAsync()
        {
            return Task.FromResult<GeoLocation>(null);
        }
    }
}
