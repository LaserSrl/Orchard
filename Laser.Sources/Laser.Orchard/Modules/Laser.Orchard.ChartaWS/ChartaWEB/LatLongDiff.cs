using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChartaWEB
{
    public abstract class LatLongDiff
    {

        public static double MToLat(double dyMeters, double aLat)
        {
            double rLat = aLat * Math.PI / 180;

            double m = 111132.09 * rLat - 566.05 * Math.Cos(2 * rLat) + 1.2 * Math.Cos(4 * rLat);
            double dLat = dyMeters / m;

            return dLat;
        }

        public static double MToLon(double dxMeters, double aLon)
        {
            double rLon = aLon * Math.PI / 180;

            double m = 111415.13 * Math.Cos(rLon) - 94.55 * Math.Cos(3 * rLon);
            double dLon = dxMeters / m;

            return dLon;
        }
    }
}