using System;
using AudioSwitcher.AudioApi.CoreAudio;
using System.Text.RegularExpressions;
using ClimaVoice.Speech_Class;

namespace VoxCommand.Other_Class
{
    internal class VolumeControl
    {
        private CoreAudioDevice defaultPlaybackDevice;


        public VolumeControl()
        {
            var controller = new CoreAudioController();
            defaultPlaybackDevice = controller.DefaultPlaybackDevice; 
            Console.WriteLine("Default playback device initialized.");
        }

        // Sets the volume level to a specific percentage.
        public void SetVolume(double volume)
        {
            // Validate the input volume range.
            if (volume < 0 || volume > 100)
            {
                Console.WriteLine($"Attempted to set volume outside valid range: {volume}%. Volume must be between 0 and 100.");
                return;
            }

            // Set the volume asynchronously and log the action.
            defaultPlaybackDevice.SetVolumeAsync(volume).Wait();
            Console.WriteLine($"Volume set to: {volume}%.");
        }

        // Gets the current volume level from the default playback device.
        public double GetVolume()
        {
            double volume = defaultPlaybackDevice.Volume;
            Console.WriteLine($"Current volume: {volume}%.");
            return volume;
        }

        // Decreases the volume by 10 units.
        public void LowerVolume()
        {
            double currentVolume = GetVolume();
            SetVolume(currentVolume - 10);
        }

        // Increases the volume by 10 units.
        public void HigherVolume()
        {
            double currentVolume = GetVolume(); 
            SetVolume(currentVolume + 10);
        }

        // Mutes or unmutes the volume based on the boolean parameter.
        public void MuteVolume(bool mute)
        {
            defaultPlaybackDevice.SetMuteAsync(mute).Wait(); 
            Console.WriteLine($"Mute state set to: {mute}.");
        }
        //%vol.down.epsilon
        public static void AdjustVolumeBasedOnCommand(string command)
        {
            if (command.Contains("%vol"))
            {
                VolumeControl volumeControl = new VolumeControl();

                // Extract the first number from the command
                var match = Regex.Match(command, @"\d+");
                int volumeChangePercentage = match.Success ? int.Parse(match.Value) : 10; // Default to 10 if no number found

                // Retrieve current volume
                double currentVolume = volumeControl.GetVolume();

                // Calculate new volume based on the command
                double newVolume = 0;
                if (command.Contains(".increase") || command.Contains(".up"))
                {
                    newVolume = currentVolume + (currentVolume * (volumeChangePercentage / 100.0));
                    newVolume = Math.Min(newVolume, 100); // Ensure the volume does not exceed 100%
                }
                else if (command.Contains(".decrease") || command.Contains(".down"))
                {
                    newVolume = currentVolume - (currentVolume * (volumeChangePercentage / 100.0));
                    newVolume = Math.Max(newVolume, 0); // Ensure the volume does not go below 0%
                }
                else if (command.Contains(".set"))
                {
                    newVolume = volumeChangePercentage;
                }
                else if (command.Contains(".mute") || command.Contains(".up"))
                {
                    if (volumeControl.defaultPlaybackDevice.IsMuted)
                    {
                        return;
                    }

                    volumeControl.MuteVolume(true);
                }
                else if (command.Contains(".unmute"))
                {
                    if (!volumeControl.defaultPlaybackDevice.IsMuted)
                    {
                        return;
                    }
                    volumeControl.MuteVolume(false);
                }
                else
                {
                    Console.WriteLine("No action (increase/decrease) specified in the command.");
                }

                // Set the new volume
                volumeControl.SetVolume(newVolume);
            }
        }
    }
}
