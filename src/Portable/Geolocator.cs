namespace Nine.Application
{
    using System;
    using System.Threading.Tasks;

    public partial class Geolocator
    {
#if PCL
        public static Task<GeoLocation> FindAsync()
        {
            throw new NotSupportedException();
        }
#endif
    }
}
