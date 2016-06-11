namespace Nine.Application
{
    using System;
    using System.Threading.Tasks;
    using Android.Content;
    using Android.Locations;
    using Android.OS;

    public class Geolocator : IGeolocator
    {
        private readonly GeoLocationProvider _provider;

        public Geolocator(Context context) : this(() => context) { }
        public Geolocator(Func<Context> contextFactory)
        {
            _provider = new GeoLocationProvider(contextFactory);
        }

        public async Task<GeoLocation> FindAsync()
        {
            var point = await _provider.FindAsync();
            if (point == null) return null;
            return new GeoLocation { Altitude = point.Altitude, Latitude = point.Latitude, Longitude = point.Longitude };
        }

        class GeoLocationProvider : Java.Lang.Object, ILocationListener
        {
            private readonly LocationManager _location;
            private TaskCompletionSource<Location> _completion;

            public GeoLocationProvider(Func<Context> contextFactory)
            {
                _location = contextFactory().GetSystemService(Context.LocationService) as LocationManager;
            }

            public Task<Location> FindAsync()
            {
                if (_location == null) return Task.FromResult<Location>(null);

                var lastKnownGps = _location.GetLastKnownLocation(LocationManager.GpsProvider);
                var lastKnownNet = _location.GetLastKnownLocation(LocationManager.NetworkProvider);

                var validSince = DateTime.UtcNow.AddHours(-8);
                if (lastKnownGps != null && ToDateTime(lastKnownGps.Time) > validSince) return Task.FromResult(lastKnownGps);
                if (lastKnownNet != null && ToDateTime(lastKnownNet.Time) > validSince) return Task.FromResult(lastKnownNet);

                _completion = new TaskCompletionSource<Location>();
                _location.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, this);
                return _completion.Task;
            }

            public void OnLocationChanged(Location point)
            {
                _location.RemoveUpdates(this);
                _completion.TrySetResult(point);
            }

            public void OnProviderDisabled(string provider)
            {

            }

            public void OnProviderEnabled(string provider)
            {

            }

            public void OnStatusChanged(string provider, Availability status, Bundle extras)
            {

            }

            private static DateTime unixTimeStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            public static DateTime ToDateTime(long unixTimestamp)
            {
                // http://stackoverflow.com/questions/4964634/how-to-convert-long-type-datetime-to-datetime-with-correct-time-zone            
                return unixTimeStart.AddMilliseconds(unixTimestamp);
            }
        }
    }
}
