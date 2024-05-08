using System.Collections.Generic;

namespace ClimaVoice.Speech_Class
{
    internal class AppCommand
    {
        public string Name { get; set; }
        public string ExecutablePath { get; set; }
        public HashSet<string> Actions { get; set; }

        public AppCommand(string name, string executablePath, HashSet<string> actions)
        {
            Name = name;
            ExecutablePath = executablePath;
            Actions = actions;
        }
    }
}
