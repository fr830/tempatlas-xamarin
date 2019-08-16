using System;
using Xamarin.Forms.GoogleMaps;

namespace TempAtlas
{
    public interface ILocationService
    {
        Position GetPosition();
        void StopUpdating();
        event EventHandler<PositionUpdatedArgs> PositionUpdated;   
    }

    public class PositionUpdatedArgs : EventArgs
    {
        public Position position { get; set; }
    }
}
