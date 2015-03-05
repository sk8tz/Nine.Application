namespace Nine.Application
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CoreLocation;

    partial class Geolocator
    {
        private static readonly CLLocationManager location = new CLLocationManager();
        private static TaskCompletionSource<GeoLocation> completion;

        public static Task<GeoLocation> FindAsync()
        {
            completion = new TaskCompletionSource<GeoLocation>();

            EventHandler<CLLocationsUpdatedEventArgs> handler = null;
            handler = (sender, e) =>
            {
                var point = e.Locations.LastOrDefault();
                if (point == null)
                {
                    completion.SetResult(null);
                }
                else
                {
                    completion.SetResult(new GeoLocation
                    {
                        Altitude = point.Altitude,
                        Longitude = point.Coordinate.Longitude,
                        Latitude = point.Coordinate.Latitude
                    });
                }
                location.LocationsUpdated -= handler;
                location.StopUpdatingLocation();
            };

            location.LocationsUpdated += handler;
            location.StartUpdatingLocation();
            return completion.Task;
        }
    }
}
