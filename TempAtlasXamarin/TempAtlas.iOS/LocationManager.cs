using System;
using CoreLocation;
using TempAtlas.iOS;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

[assembly: Dependency(typeof(LocationManager))]
namespace TempAtlas.iOS
{
    public class LocationManager : ILocationService
    {
        CLLocationManager locationManager;

        public LocationManager()
        {
            locationManager = new CLLocationManager();
            locationManager.RequestWhenInUseAuthorization();
            locationManager.DesiredAccuracy = 100;
            locationManager.AuthorizationChanged += LocationManager_AuthorizationChanged;
            locationManager.LocationsUpdated += LocationManager_LocationsUpdated;
        }

        private void LocationManager_LocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            CLLocation current = e.Locations[0];
            Position position = new Position(current.Coordinate.Latitude, current.Coordinate.Longitude);
            PositionUpdatedArgs positionArgs = new PositionUpdatedArgs();
            positionArgs.position = position;
            OnPositionUpdated(positionArgs);
        }

        private void LocationManager_AuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e)
        {
            if (isAuthorized(e.Status))
            {
                locationManager.StartUpdatingLocation();
            }    
        }

        private bool isAuthorized(CLAuthorizationStatus status)
        {
            return status == CLAuthorizationStatus.Authorized || status == CLAuthorizationStatus.AuthorizedAlways || status == CLAuthorizationStatus.AuthorizedWhenInUse;
        }

        public Position GetPosition()
        {
            Position defaultPos = new Position(43.08291577840266, -77.6772236820356);
            if (CLLocationManager.LocationServicesEnabled && isAuthorized(CLLocationManager.Status))
            {
                CLLocation current = locationManager.Location;
                return current != null ? new Position(current.Coordinate.Latitude, current.Coordinate.Longitude) : defaultPos;
            }
            return defaultPos;
        }

        protected virtual void OnPositionUpdated(PositionUpdatedArgs e)
        {
            PositionUpdated?.Invoke(this, e);
        }

        public void StopUpdating()
        {
            locationManager.StopUpdatingLocation();
        }

        public event EventHandler<PositionUpdatedArgs> PositionUpdated;

    }
}
