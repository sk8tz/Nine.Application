namespace Nine.Application
{
    using System;
    using System.Collections;

    public class GeoLocation
    {
        public double Longitude;
        public double Latitude;
        public double Altitude;

        const int GeoHashBitCount = 6 * 5;
        const int ByteLength = 8;

        public byte[] GetGeoHash()
        {
            // Based on https://code.google.com/p/tambon/source/browse/trunk/AHGeo/GeoHash.cs
            // Based on http://code.google.com/p/geospatialweb/source/browse/trunk/geohash/src/Geohash.java
            var bits = new BitArray(GeoHashBitCount * 2);
            GeoHashEncode(Longitude, -180, 180, bits, 0);
            GeoHashEncode(Latitude, -90, 90, bits, 1);
            return BitArrayToByteArray(bits, 0, bits.Length);
        }

        public string GetGeoHashString()
        {
            return GetGeoHash().ToZBase32String();
        }

        /// <summary>
        /// Calculates the approximate distance between 2 geo points in meters.
        /// </summary>
        public double DistanceTo(GeoLocation geo)
        {
            return DistanceBetween(this, geo);
        }

        public override string ToString()
        {
            return string.Join(",", Longitude, Latitude, Altitude);
        }

        private static BitArray GeoHashEncode(double value, double floorValue, double ceilingValue, BitArray result, int index)
        {
            var floor = floorValue;
            var ceiling = ceilingValue;
            for (var i = index; i < result.Length; i += 2)
            {
                var middle = (floor + ceiling) / 2;
                if (value >= middle)
                {
                    result[i] = true;
                    floor = middle;
                }
                else
                {
                    result[i] = false;
                    ceiling = middle;
                }
            }
            return result;
        }

        /// <summary>
        /// https://utilities.codeplex.com/
        /// </summary>
        private static byte[] BitArrayToByteArray(BitArray bits, int startIndex, int count)
        {
            // Get the size of bytes needed to store all bytes
            int bytesize = count / ByteLength;

            // Any bit left over another byte is necessary
            if (count % ByteLength > 0)
                bytesize++;

            // For the result
            byte[] bytes = new byte[bytesize];

            // Must init to good value, all zero bit byte has value zero
            // Lowest significant bit has a place value of 1, each position to
            // to the left doubles the value
            byte value = 0;
            byte significance = 1;

            // Remember where in the input/output arrays
            int bytepos = 0;
            int bitpos = startIndex;

            while (bitpos - startIndex < count)
            {
                // If the bit is set add its value to the byte
                if (bits[bitpos])
                    value += significance;

                bitpos++;

                if (bitpos % ByteLength == 0)
                {
                    // A full byte has been processed, store it
                    // increase output buffer index and reset work values
                    bytes[bytepos] = value;
                    bytepos++;
                    value = 0;
                    significance = 1;
                }
                else
                {
                    // Another bit processed, next has doubled value
                    significance *= 2;
                }
            }
            return bytes;
        }

        private static double Radians(double x)
        {
            return x * Math.PI / 180;
        }

        /// <summary>
        /// Calculates the approximate distance between 2 geo points in meters.
        /// </summary>
        /// <returns>
        /// NaN if x or y is null.
        /// </returns>
        public static double DistanceBetween(GeoLocation x, GeoLocation y)
        {
            if (x == null || y == null) return double.NaN;

            // http://stackoverflow.com/questions/6544286/calculate-distance-of-two-geo-points-in-km-c-sharp
            var R = 6371.004 * 1000; // m

            var sLat1 = Math.Sin(Radians(x.Latitude));
            var sLat2 = Math.Sin(Radians(y.Latitude));
            var cLat1 = Math.Cos(Radians(x.Latitude));
            var cLat2 = Math.Cos(Radians(y.Latitude));
            var cLon = Math.Cos(Radians(x.Longitude) - Radians(y.Longitude));

            var cosD = sLat1 * sLat2 + cLat1 * cLat2 * cLon;

            var d = Math.Acos(cosD);

            return R * d;
        }
    }
}
