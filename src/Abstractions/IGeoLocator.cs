namespace Nine.Application
{
    using System.Threading.Tasks;

    public class GeoLocation
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double Altitude { get; set; }
    }

    public interface IGeolocator
    {
        Task<GeoLocation> FindAsync();
    }
}
