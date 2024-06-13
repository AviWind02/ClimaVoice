using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ClimaVoice.MediaControl_Class
{
    internal class FocusSpotify
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public static void BringSpotifyToFront()
        {
            // Get the process of Spotify
            Process[] processes = Process.GetProcessesByName("Spotify");
            if (processes.Length > 0)
            {
                // Get the main window handle of the Spotify process
                IntPtr hWnd = processes[0].MainWindowHandle;

                if (hWnd != IntPtr.Zero)
                {
                    // Bring the Spotify window to the foreground
                    SetForegroundWindow(hWnd);
                    Console.WriteLine("Spotify is now in focus.");
                }
                else
                {
                    Console.WriteLine("Spotify is running but no window found.");
                }
            }
            else
            {
                Console.WriteLine("Spotify is not running.");
            }
        }
    }
}
