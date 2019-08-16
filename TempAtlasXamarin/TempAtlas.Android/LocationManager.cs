using System;
using TempAtlas.Droid;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Android;
using Android.App;
using Android.Content.PM;
using Android.Locations;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Runtime;

[assembly: Dependency(typeof(LocationManagerDroid))]
namespace TempAtlas.Droid
{
    public class LocationManagerDroid : Java.Lang.Object, ILocationService, ILocationListener
    {
        static LocationManager sLocationManager;
        static string sProvider;
        static Context sContext;
        static LocationManagerDroid sActive;
        public static int LOCATION_REQUEST_CODE = 404;

        public static void Init(Context context)
        {
            sContext = context;
            sLocationManager = (LocationManager)sContext.GetSystemService(Context.LocationService);
        }

        public static void StartLocation()
        {
            Criteria locationCriteria = new Criteria();
            locationCriteria.Accuracy = Accuracy.Coarse;
            locationCriteria.PowerRequirement = Power.Medium;
            sProvider = sLocationManager.GetBestProvider(locationCriteria, true);
            sLocationManager.RequestLocationUpdates(sProvider, 1000, 0, sActive);
            sActive.OnLocationChanged(sLocationManager.GetLastKnownLocation(sProvider));
        }

        public LocationManagerDroid()
        {
            if (sActive != null)
            {
                sLocationManager.RemoveUpdates(sActive);
            }
            sActive = this;

            if (ContextCompat.CheckSelfPermission(sContext, Manifest.Permission.AccessCoarseLocation) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions((Activity)sContext, new string[] { Manifest.Permission.AccessCoarseLocation }, LOCATION_REQUEST_CODE);
            }
            else
            {
                StartLocation();
            }
        }

        public event EventHandler<PositionUpdatedArgs> PositionUpdated;

        public Position GetPosition()
        {
            if (sLocationManager.IsLocationEnabled && sProvider != null)
            {
                Location last = sLocationManager.GetLastKnownLocation(sProvider);
                return new Position(last.Latitude, last.Longitude);
            }
            return new Position(43.08291577840266, -77.6772236820356);
        }

        public void StopUpdating()
        {
            sLocationManager.RemoveUpdates(sActive);
        }

        public void OnLocationChanged(Location location)
        {
            Position newPos = new Position(location.Latitude, location.Longitude);
            PositionUpdatedArgs args = new PositionUpdatedArgs();
            args.position = newPos;
            OnPositionUpdated(args);
        }

        public void OnProviderDisabled(string provider)
        {

        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }

        protected virtual void OnPositionUpdated(PositionUpdatedArgs e)
        {
            PositionUpdated?.Invoke(this, e);
        }
    }
}
