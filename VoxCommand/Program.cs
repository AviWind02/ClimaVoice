using System;
using System.Threading.Tasks;
using ClimaVoice.Speech_Class;

namespace ClimaVoice
{
    internal static class Program
    {
       [STAThread]
        static async Task Main(string[] args)
        {
  
            Console.WriteLine("Start");
            var speechTask = new SpeechRecognition().Run(); // Start the speech recognition task
            await speechTask; // wait for the speech task to complete after the form is closed
        
        }

  
    }
}
