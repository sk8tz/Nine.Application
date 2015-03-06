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

            try
            {
                var location = await locator.GetGeopositionAsync();

                return new GeoLocation
                {
                    Altitude = location.Coordinate.Point.Position.Altitude,
                    Latitude = location.Coordinate.Point.Position.Latitude,
                    Longitude = location.Coordinate.Point.Position.Longitude,
                };
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }
    }
}
