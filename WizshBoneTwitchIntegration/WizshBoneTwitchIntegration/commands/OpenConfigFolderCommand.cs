using Jotunn.Entities;
using System;
using System.Diagnostics;
using System.IO;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class OpenConfigFolderCommand : ConsoleCommand
    {
        public override string Name => "WBTIOpenConfigFolder";

        public override string Help => "Opens the location of the mod's configs in explorer";

        public override void Run(string[] args)
        {
            if (args.Length > 0)
                return;

            using Process fileopener = new Process();

            string path = Path.Combine(Environment.CurrentDirectory, WizshBoneTwitchIntegration.customConfigPath);
            Jotunn.Logger.LogWarning(path);

            fileopener.StartInfo.FileName = path;
            fileopener.StartInfo.UseShellExecute = true;
            fileopener.StartInfo.Verb = "open";
            fileopener.Start();
        }
    }
}
