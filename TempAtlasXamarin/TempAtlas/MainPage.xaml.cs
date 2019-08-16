using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using TempAtlas;

namespace TempAtlas
{
    public partial class MainPage : ContentPage
    {
        private static Position mapPosition = new Position(43.08291577840266, -77.6772236820356);
        private static Map currentMap;
        private WeatherResponse currentResponse;
        private const string FAVORITE_KEY = "FORECAST_FAVORITES";
        private List<Favorite> favorites;

        public MainPage()
        {
            InitializeComponent();
            On<iOS>().SetUseSafeArea(true);
            currentMap = map;
            favorites = LoadFavorites();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (currentResponse == null)
            {
                mainStack.Children.Remove(forecastInfo); // this gets re-added when the first forecast is displayed

                // await location for map
                mapPosition = DependencyService.Get<ILocationService>().GetPosition();
                DependencyService.Get<ILocationService>().PositionUpdated += MainPage_PositionUpdated;

                AsyncUpdateMapAndForecast();
            }
            else if (currentResponse.coord.lat != mapPosition.Latitude || currentResponse.coord.lon != mapPosition.Longitude)
            {
                mapPosition = new Position(currentResponse.coord.lat, currentResponse.coord.lon);
                AsyncUpdateMapAndForecast();
            }
        }

        // Event Handlers

        public async void OnSearchPressed(object sender, EventArgs e)
        {
            currentResponse = (WeatherResponse)await WeatherAPI.sharedInstance.GetWeatherByCity(searchBar.Text);

            if (currentResponse != null)
            {
                Position cityPosition = new Position(currentResponse.coord.lat, currentResponse.coord.lon);
                UpdateMapPosition(cityPosition);
                UpdatePinPosition(cityPosition);
                UpdateForecast(currentResponse);
            }
        }

        public async void OnSwitchToggled(object sender, ToggledEventArgs e)
        {
            WeatherAPI.sharedInstance.units = e.Value ? WeatherAPI.Units.metric : WeatherAPI.Units.imperial;

            if (currentResponse != null)
            {
                currentResponse = await WeatherAPI.sharedInstance.GetWeatherByCoordinates(currentResponse.coord.lat, currentResponse.coord.lon);

                if (currentResponse != null)
                {
                    UpdateForecast(currentResponse);
                    UpdatePinPosition(map.Pins[0].Position);
                }
            }
        }

        public async void OnMapTapped(object sender, MapClickedEventArgs e)
        {
            Position tappedPosition = new Position(e.Point.Latitude, e.Point.Longitude);
            UpdateMapPosition(tappedPosition);
            currentResponse = await WeatherAPI.sharedInstance.GetWeatherByCoordinates(mapPosition.Latitude, mapPosition.Longitude);

            if (currentResponse != null)
            {
                UpdateForecast(currentResponse);
                UpdatePinPosition(tappedPosition);
            }
        }

        public void OnFavoriteClicked(object sender, EventArgs e)
        {
            Favorite newFavorite = new Favorite(currentResponse.name, currentResponse.coord);

            // remove if in the fav collection, otherwise add
            if (isFavorite(newFavorite.Coordinate))
            {
                int index = favorites.FindIndex(x => x.Coordinate == newFavorite.Coordinate);
                favorites.RemoveAt(index);
            }
            else
            {
                favorites.Add(newFavorite);
            }
            SaveFavorites();

            // update the button text after the modification
            favoriteButton.Text = isFavorite(newFavorite.Coordinate) ? "Remove Favorite" : "Add Favorite";
        }

        public void OnSearchBarFocused(object sender, EventArgs e)
        {
            searchBar.Text = ""; // reset bar to be empty
        }

        public async void OnSeeFavoritesClicked(object sender, EventArgs e)
        {
            var favoritesModal = new FavoritesModal(ref currentResponse, favorites);

            await Navigation.PushAsync(favoritesModal);
        }

        private void MainPage_PositionUpdated(object sender, PositionUpdatedArgs e)
        {
            if (mapPosition.Latitude != e.position.Latitude || mapPosition.Longitude != e.position.Longitude)
            {
                mapPosition = e.position;
                AsyncUpdateMapAndForecast();
            }
        }

        // Favorites Helpers

        private List<Favorite> LoadFavorites()
        {
            if (Xamarin.Forms.Application.Current.Properties.ContainsKey(FAVORITE_KEY))
            {
                string jsonFavorites = Xamarin.Forms.Application.Current.Properties[FAVORITE_KEY] as string;
                return JsonConvert.DeserializeObject<List<Favorite>>(jsonFavorites);
            }
            return new List<Favorite>();
        }

        private void SaveFavorites()
        {
            string jsonFavorites = JsonConvert.SerializeObject(favorites);
            Xamarin.Forms.Application.Current.Properties[FAVORITE_KEY] = jsonFavorites;
            Xamarin.Forms.Application.Current.SavePropertiesAsync();
        }

        private bool isFavorite(Coordinate coordinate)
        {
            return favorites.Find(x => x.Coordinate == coordinate) != null;
        }

        // Helpers

        private void UpdateForecast(WeatherResponse response)
        {
            if (!mainStack.Children.Contains(forecastInfo))
            {
                mainStack.Children.Insert(mainStack.Children.Count - 1, forecastInfo);
                forecastInfo.FadeTo(1);
            }

            // populate labels with data
            locationLabel.Text = response.name;
            conditionsLabel.Text = response.weather[0].main + ", " + response.main.temp + "°";
            highLabel.Text = response.main.temp_max + "°";
            lowLabel.Text = response.main.temp_min + "°";
            humidityLabel.Text = response.main.humidity + "%";
            windSpeedLabel.Text = response.wind.speed + (WeatherAPI.sharedInstance.units == WeatherAPI.Units.imperial ? "mph" : "m/s");
            cloudCoverLabel.Text = response.clouds.all + "%";

            favoriteButton.Text = isFavorite(response.coord) ? "Remove Favorite" : "Add Favorite";
        }

        private void UpdatePinPosition(Position position)
        {
            if (currentResponse != null)
            {
                map.Pins.Clear();
                Pin current = new Pin
                {
                    Label = currentResponse.main.temp.ToString() + "°",
                    Position = position
                };
                map.Pins.Add(current);
            }
        }

        private void AsyncUpdateMapAndForecast()
        {
            UpdateMapPosition(mapPosition);
            Device.BeginInvokeOnMainThread(async () =>
            {
                currentResponse = await WeatherAPI.sharedInstance.GetWeatherByCoordinates(mapPosition.Latitude, mapPosition.Longitude);

                if (currentResponse != null)
                {
                    UpdateForecast(currentResponse);
                    UpdatePinPosition(mapPosition);
                }
            });
        }

        public static void UpdateMapPosition(Position position)
        {
            mapPosition = position;
            MapSpan newSpan = MapSpan.FromCenterAndRadius(mapPosition, Distance.FromMiles(1));
            if (currentMap != null)
            {
                currentMap.MoveToRegion(newSpan);
            }
        }
    }
}
