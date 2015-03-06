namespace Nine.Application
{
    using System;
    using System.Threading.Tasks;
    using Windows.Devices.Geolocation;

    partial class Geolocator
    {
        private readonly Windows.Devices.Geolocation.Geolocator locator = new Windows.Devices.Geolocation.Geolocator();

        public async Task<GeoLocation> FindAsync()
        {
            if (locator.LocationStatus == PositionStatus.Disabled || locator.LocationStatus == PositionStatus.NotAvailable) return null;

            var location = await locator.GetGeopositionAsync();

            return new GeoLocation
            {
                Altitude = location.Coordinate.Altitude ?? 0.0,
                Latitude = location.Coordinate.Latitude,
                Longitude = location.Coordinate.Longitude,
            };
        }
    }
}
