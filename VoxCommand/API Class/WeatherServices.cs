using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;
using ClimaVoice.Speech_Class;
using OpenQA.Selenium;
using System.Linq;

namespace ClimaVoice.API_Class
{
    internal class WeatherServices
    {
        private readonly string _apiKey = HiddenKeys.APIKeyOpenWeather(); // Replace with your actual API key
        private readonly HttpClient _httpClient = new HttpClient();
        private static OpenAiService openAiService;

        public WeatherServices(OpenAiService _openAiService)
        {
            openAiService = _openAiService;
        }
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

        private JObject FormattedForAI_Forcast(JObject weatherData)
        {
            Console.WriteLine("Formatting data for AI.");

            if (weatherData == null)
            {
                Console.WriteLine("Weather data is null. Aborting formatting.");
                return null;
            }

            var cityName = weatherData["city"]?["name"];
            var weatherList = weatherData["list"];

            if (cityName == null || weatherList == null || !weatherList.Any())
            {
                Console.WriteLine("Critical weather data is missing. Aborting formatting.");
                return null;
            }

            var formattedJson = new JObject
            {
                ["Location"] = cityName,
                ["Today's Weather"] = new JObject
                {
                    ["Temperature"] = weatherList[0]?["main"]?["temp"],
                    ["Feels Like Temperature"] = weatherList[0]?["main"]?["feels_like"],
                    ["Description"] = weatherList[0]?["weather"]?[0]?["description"],
                    ["Visibility"] = weatherList[0]?["visibility"],
                    ["Humidity"] = weatherList[0]?["main"]?["humidity"],
                    ["Wind Speed"] = weatherList[0]?["wind"]?["speed"]
                },
                ["Next Days"] = new JArray(
                    weatherList.Take(5).Select(day => new JObject
                    {
                        ["Date"] = day?["dt_txt"],
                        ["Temperature"] = day?["main"]?["temp"],
                        ["Description"] = day?["weather"]?[0]?["description"]
                    }).Where(day => day["Date"] != null && day["Temperature"] != null && day["Description"] != null)
                )
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

        public async void GiveDataToGPTBasedOnCommand(string command)
        {
            if (command.Contains("%weather"))
            {
                string weatherdata = await GetFormattedWeatherDataAsync(HiddenKeys.MyCity());
                string datawithcommand = $"Provide weather information based on the provided data, {command}: {weatherdata}";
                Console.WriteLine($"Input: {datawithcommand}");
                string SST = await openAiService.QuestionAsync(datawithcommand);
                await SpeechRecognition.SynthesizeAudioAsync(SST);
            }
        }
    }
}
