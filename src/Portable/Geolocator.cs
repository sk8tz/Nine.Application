namespace Nine.Application
{
    using System;
    using System.Threading.Tasks;

    public interface IGeolocator
    {
        Task<GeoLocation> FindAsync();
    }

    public partial class Geolocator : IGeolocator
    {
#if PCL
        public Task<GeoLocation> FindAsync()
        {
            throw new NotSupportedException();
        }
#endif
    }
}
