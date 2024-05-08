using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
                var response = await _httpClient.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    JObject weatherData = JObject.Parse(content);

                    // Converting JSON to simplified key:value string
                    string formattedData = "";
                    foreach (var pair in weatherData)
                    {
                        formattedData += $"{pair.Key}: {pair.Value}\n";
                    }

                    return formattedData;
                }
                else
                {
                    return "Failed to retrieve weather data.";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }




    }
}
