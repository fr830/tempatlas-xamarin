using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace TempAtlas
{
    public sealed class WeatherAPI
    {
        private WeatherAPI() { }

        public static WeatherAPI sharedInstance
        {
            get { return Nested.instance; }
        }

        private class Nested
        {
            static Nested() { }
            internal static readonly WeatherAPI instance = new WeatherAPI();
        }

        public enum Units
        {
            imperial,
            metric
        }

        public Units units = Units.imperial;

        private readonly string baseApiUrl = "https://api.openweathermap.org/data/2.5/weather?";
        private readonly string appId = "&APPID=YOUR_APP_ID_HERE";
        
        public async Task<WeatherResponse> GetWeatherByCity(string city)
        {
            string encodedCity = Uri.EscapeDataString(city);
            string uri = baseApiUrl + "q=" + encodedCity + "&units=" + units.ToString() + appId;
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Method = "GET";
            EndLocationUpdates();

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    if ((int)response.StatusCode < 200 || (int)response.StatusCode > 399)
                    {
                        return null;
                    }
                    string data = await reader.ReadToEndAsync();
                    var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(data);
                    return weatherResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("City Request Error: " + ex.Message);
                return null;
            }
        }

        public async Task<WeatherResponse> GetWeatherByCoordinates(double lat, double lon)
        {
            string uri = baseApiUrl + "lat=" + lat.ToString() + "&lon=" + lon.ToString() + "&units=" + units.ToString() + appId;
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Method = "GET";
            EndLocationUpdates();

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    if ((int)response.StatusCode < 200 || (int)response.StatusCode > 399)
                    {
                        return null;
                    }
                    string data = await reader.ReadToEndAsync();
                    var weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(data);
                    return weatherResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Coordinate Request Error: " + ex.Message);
                return null;
            }
        }

        private void EndLocationUpdates()
        {
            DependencyService.Get<ILocationService>().StopUpdating();
        }
    }

    public class WeatherResponse
    {
        public Coordinate coord;
        public WeatherInfo[] weather;
        public string name;
        public WeatherData main;
        public Wind wind;
        public Clouds clouds;
    }

    public struct WeatherInfo
    {
        public int id;
        public string main;
        public string description;
        public string icon;
    }

    public struct WeatherData
    {
        public double temp;
        public double temp_min;
        public double temp_max;
        public int humidity;
    }

    public struct Coordinate
    {
        public double lat;
        public double lon;

        public static bool operator ==(Coordinate lhs, Coordinate rhs)
        {
            return lhs.lat == rhs.lat && lhs.lon == rhs.lon;
        }

        public static bool operator !=(Coordinate lhs, Coordinate rhs)
        {
            return !(rhs == lhs);
        }
    }

    public struct Wind
    {
        public double speed;
    }

    public struct Clouds
    {
        public double all;
    }

    public class Favorite
    {
        public string Name { get; set; }
        public Coordinate Coordinate { get; set; }
        public string FormatCoord { get; set; }

        public Favorite()
        {
            Name = "";
            Coordinate = new Coordinate();
            FormatCoord = "";
        }

        public Favorite(string place, Coordinate coord)
        {
            Name = place;
            Coordinate = coord;
            FormatCoord = string.Format("{0}, {1}", coord.lat, coord.lon);
        }
    }
}
