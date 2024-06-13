using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using ClimaVoice.API_Class;
using System.Linq;
using System.Timers;
using ClimaVoice.MediaControl_Class;
using VoxCommand.Other_Class;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using OpenQA.Selenium.DevTools.V121.Debugger;

namespace ClimaVoice.Speech_Class
{
    internal class SpeechRecognition
    {


        private static System.Speech.Synthesis.SpeechSynthesizer synthesizer;
        private static SpeechRecognizer msRecognizer;
        private static WeatherServices weatherServices;
        private static OpenAiService openAiService;

        private static bool _muteInput = false;

        private static bool isCommandbeingProcessed = false;
        private static bool commandProcessed = false;
        private static bool commandProcessing = false;
        private static bool commandDelay = false;

        private static Timer listeningTimer;
        private const int ListeningPeriod = 10000; // 10 seconds


        public void muteSpeech(bool muteInput)
        {
            _muteInput = muteInput;
        }
        public SpeechRecognition()
        {
            openAiService = new OpenAiService();
            weatherServices = new WeatherServices(openAiService);
        }

        public SpeechRecognition(string V)
        {

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
            var apiKey = HiddenKeys.APIKeyAzureSpeech();
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

                if (!command.Contains("epsilon") && !commandDelay)
                {
                    Console.WriteLine("Wake word not found.");
                    return;
                }

                if (!commandProcessed)
                {

                    string commandSum = await openAiService.QuestionAsync(command);
                    await SynthesizeAudioAsync(commandSum);
                    switch (ExtractTag(commandSum, false))
                    {
                        case "%media": PlaybackControl.AdjustMediaBasedOnCommand(ExtractTag(commandSum)); break;
                        case "%vol": VolumeControl.AdjustVolumeBasedOnCommand(ExtractTag(commandSum)); break;
                        case "%weather": weatherServices.GiveDataToGPTBasedOnCommand(commandSum); break;
                    }
                    

                    commandProcessed = true;

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
        public static string ExtractTag(string text, bool fullTag = true)
        {
            
            string fullTagPattern = @"%\w+(\.\w+)*";// Regular expression to match tags like %tag, %tag.Tag, or %tag.Tag.SubTag
            string firstTagPattern = @"%\w+"; // Regular expression to match only the first part of the tag like %tag
            string pattern = fullTag ? fullTagPattern : firstTagPattern;
            
            // Match the first occurrence of the pattern
            Match match = Regex.Match(text, pattern);
            Console.WriteLine($"Extracted key: {match.Value}");
            return match.Success ? match.Value : text;
        }

        public static async Task SynthesizeAudioAsync(string text)
        {
            // Regular expression to match tags like %tag, %tag.subtag, or %tag.subtag.childsubtag
            string pattern = @"%\w+(\.\w+){0,2}";

            // Replace the tagged items with an empty string
            string cleanedText = Regex.Replace(text, pattern, string.Empty);

            var apiKey = HiddenKeys.APIKeyAzureSpeech();
            var config = SpeechConfig.FromSubscription(apiKey, "eastus");
            config.SpeechSynthesisVoiceName = "en-US-AndrewNeural";

            using (var localSynthesizer = new Microsoft.CognitiveServices.Speech.SpeechSynthesizer(config))
            {
                var result = await localSynthesizer.SpeakTextAsync(cleanedText);
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    Console.WriteLine($"Audio synthesis completed for text: {cleanedText}");
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
