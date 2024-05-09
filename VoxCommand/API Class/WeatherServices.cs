using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using ClimaVoice.Speech_Class;

namespace ClimaVoice.API_Class
{
    internal class WeatherServices
    {
        private readonly string _apiKey = new APIKey().getAPIKey_OpenWeather(); // Replace with your actual API key
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<string> GetFormattedWeatherDataAsync(string city)
        {
            try
            {
                string apiUrl = $"http://api.openweathermap.org/data/2.5/weather?q={city},ON,CA&appid={_apiKey}&units=metric";
                Console.WriteLine($"Requesting data from API: {apiUrl}");
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("API response received successfully.");
                    Console.WriteLine(content);
                    JObject weatherData = JObject.Parse(content);
                    JObject formattedData = FormattedForAI(weatherData);
                    Console.WriteLine("Formatted data successfully.");
                    Console.WriteLine(formattedData.ToString());
                    return formattedData.ToString();
                }
                else
                {
                    Console.WriteLine("Failed to retrieve weather data.");
                    return "Failed to retrieve weather data.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving weather data: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        private JObject FormattedForAI(JObject weatherData)
        {
            Console.WriteLine("Formatting data for AI.");
            var formattedJson = new JObject
            {
                ["Location"] = weatherData["name"],
                ["Temperature"] = weatherData["main"]["temp"],
                ["Feels Like Temperature"] = weatherData["main"]["feels_like"],
                ["Weather Conditions"] = weatherData["weather"][0]["description"],
                ["Visibility"] = weatherData["visibility"],
                ["Humidity"] = weatherData["main"]["humidity"],
                ["Wind Speed"] = weatherData["wind"]["speed"],
                ["Wind Direction"] = ConvertDegreesToDirection((double)weatherData["wind"]["deg"])
            };

            Console.WriteLine("Data formatted successfully.");
            return formattedJson;
        }

        private string ConvertDegreesToDirection(double degrees)
        {
            Console.WriteLine($"Converting wind direction from degrees: {degrees}");
            string[] directions = new string[] { "North", "North-Northeast", "Northeast", "East-Northeast", "East", "East-Southeast", "Southeast", "South-Southeast", "South", "South-Southwest", "Southwest", "West-Southwest", "West", "West-Northwest", "Northwest", "North-Northwest" };
            int index = (int)((degrees + 11.25) / 22.5);
            string direction = directions[index % 16];
            Console.WriteLine($"Converted wind direction: {direction}");
            return direction;
        }
    }
}
