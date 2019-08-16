using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace TempAtlas
{
    public partial class FavoritesModal : ContentPage
    {
        public List<Favorite> favoritesList;
        public WeatherResponse currentResponse;

        public FavoritesModal(ref WeatherResponse activeResponse, List<Favorite> currentFavorites = null)
        {
            InitializeComponent();
            On<iOS>().SetUseSafeArea(true);
            favoritesList = currentFavorites ?? new List<Favorite>();
            currentResponse = activeResponse ?? new WeatherResponse();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var source = new List<Favorite>();
            foreach (var fav in favoritesList)
            {
                source.Add(new Favorite
                {
                    Name = fav.Name,
                    Coordinate = fav.Coordinate,
                    FormatCoord = fav.FormatCoord
                });
            }

            favListView.ItemsSource = source;

            emptyLabel.IsVisible = source.Count < 1;
            favListView.IsVisible = !emptyLabel.IsVisible;
        }

        public async void OnFavoriteLocationClicked(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                var tappedFav = (Favorite)e.SelectedItem;
                currentResponse.coord = tappedFav.Coordinate; // resets the current coordinate, causes MainPage to update the map and forecast
                await Navigation.PopAsync();
            }
        }
    }
}
