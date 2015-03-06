namespace Nine.Application
{
    using System.Threading.Tasks;

    partial class Geolocator
    {
        public Task<GeoLocation> FindAsync()
        {
            return Task.FromResult<GeoLocation>(null);
        }
    }
}
