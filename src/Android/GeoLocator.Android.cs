namespace Nine.Application
{
    using System;
    using System.Threading.Tasks;
    using Android.Content;
    using Android.Locations;
    using Android.OS;

    partial class Geolocator
    {
        private readonly GeoLocationProvider provider;

        public Geolocator(Context context) : this(() => context) { }
        public Geolocator(Func<Context> contextFactory)
        {
            this.provider = new GeoLocationProvider(contextFactory);
        }

        public async Task<GeoLocation> FindAsync()
        {
            var point = await provider.FindAsync();
            if (point == null) return null;
            return new GeoLocation { Altitude = point.Altitude, Latitude = point.Latitude, Longitude = point.Longitude };
        }

        class GeoLocationProvider : Java.Lang.Object, ILocationListener
        {
            private readonly LocationManager location;
            private TaskCompletionSource<Location> completion;

            public GeoLocationProvider(Func<Context> contextFactory)
            {
                this.location = contextFactory().GetSystemService(Context.LocationService) as LocationManager;
            }

            public Task<Location> FindAsync()
            {
                if (location == null) return Task.FromResult<Location>(null);

                var lastKnownGps = location.GetLastKnownLocation(LocationManager.GpsProvider);
                var lastKnownNet = location.GetLastKnownLocation(LocationManager.NetworkProvider);

                var validSince = DateTime.UtcNow.AddHours(-8);
                if (lastKnownGps != null && ToDateTime(lastKnownGps.Time) > validSince) return Task.FromResult(lastKnownGps);
                if (lastKnownNet != null && ToDateTime(lastKnownNet.Time) > validSince) return Task.FromResult(lastKnownNet);

                completion = new TaskCompletionSource<Location>();
                location.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, this);
                return completion.Task;
            }

            public void OnLocationChanged(Location point)
            {
                location.RemoveUpdates(this);
                completion.SetResult(point);
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
