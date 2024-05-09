using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using ClimaVoice.API_Class;

namespace ClimaVoice.Speech_Class
{
    internal class SpeechRecognition
    {


        private static System.Speech.Synthesis.SpeechSynthesizer synthesizer;
        private static SpeechRecognizer msRecognizer;
        private static WeatherServices weatherServices;
        private static OpenAiService openAiService;
        private static bool commandProcessed = false;
        private static bool isCommandbeingProcessed = false;
        private static bool _muteInput = false;

        public void muteSpeech(bool muteInput)
        {
            _muteInput = muteInput;
        }
        public SpeechRecognition()
        {
            weatherServices = new WeatherServices();
            openAiService = new OpenAiService();
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
        public async Task Run()
        {
            Console.WriteLine("Initializing synthesizer...");
            synthesizer = new System.Speech.Synthesis.SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();

            Console.WriteLine("Setting up Cognitive Services speech recognition...");
            var apiKey = new APIKey().getAPIKey();
            var config = SpeechConfig.FromSubscription(apiKey, "eastus");
            msRecognizer = new SpeechRecognizer(config);

            msRecognizer.Recognized += MsRecognizer_Recognized;
            msRecognizer.Canceled += MsRecognizer_Canceled;

            await msRecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            Console.WriteLine("Speech recognition services are running...");

            Console.ReadLine(); // Keeps the console open until a newline is entered.

            Console.WriteLine("Stopping speech recognition services...");
            await msRecognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            Console.WriteLine("Speech recognition services stopped.");
        }

        private static async void MsRecognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                string command = e.Result.Text.ToLowerInvariant();
                Console.WriteLine($"Recognized Speech: {command}");

                //if (!command.Contains("eva"))
                //{
                //    Console.WriteLine("Wake word not found.");
                //    return;
                //}
                // Check for specific keywords and handle them
                if (!commandProcessed)
                {
                    if (command.Contains("wear"))
                    {
                        Console.WriteLine("Detected 'wear' in command");
                        string weatherData = await weatherServices.GetFormattedWeatherDataAsync(new APIKey().getCity());
                        string weatherSum = await openAiService.SummarizeWeatherAsyncWearable(weatherData);
                        await SynthesizeAudioAsync(weatherSum); commandProcessed = true;

                    }
                    else if (command.Contains("weather"))
                    {
                        Console.WriteLine("Detected 'weather' in command.");
                        commandProcessed = true;
                        string weatherData = await weatherServices.GetFormattedWeatherDataAsync(new APIKey().getCity());
                        string weatherSum = await openAiService.SummarizeWeatherAsync(weatherData);
                        await SynthesizeAudioAsync(weatherSum);

                    }
                    else
                    {
                        commandProcessed = false;
                        Console.WriteLine("Command does not contain 'wear' or 'weather', normal processing.");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Recognition failed. Reason: {e.Result.Reason}");
            }
            commandProcessed = false;
        }

        private static void MsRecognizer_Canceled(object sender, SpeechRecognitionCanceledEventArgs e)
        {
            Console.WriteLine($"Speech recognition canceled. Reason: {e.Reason}");
            if (e.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"Error Code: {e.ErrorCode}. Error Details: {e.ErrorDetails}");
            }
        }


        public static async Task SynthesizeAudioAsync(string text)
        {
            var apiKey = new APIKey().getAPIKey();
            var config = SpeechConfig.FromSubscription(apiKey, "eastus");
            config.SpeechSynthesisVoiceName = "en-GB-OliverNeural";

            using (var localSynthesizer = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(config))
            {
                var result = await localSynthesizer.SpeakTextAsync(text);
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Console.WriteLine($"Audio synthesis completed for text: {text}");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    Console.WriteLine($"Audio synthesis canceled: {cancellation.Reason}");
                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"Error: {cancellation.ErrorCode}, {cancellation.ErrorDetails}");
                    }
                }
            }
        }

    }
}
