using Jotunn.Entities;
using Jotunn.Managers;
using System.Collections.Generic;
using System.Linq;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class TestCommand : ConsoleCommand
    {
        public List<string> redeemNames = new List<string>();
        public override string Name => "testWBTI";

        public override string Help => "Test command for things";

        public override void Run(string[] args)
        {
            //if (args.Length == 0)
            //{
            //    foreach (string recipe in Player.m_localPlayer.m_knownRecipes)
            //    {
            //        Jotunn.Logger.LogWarning(recipe);
            //    }
            //    return;
            //}

            // Jotunn.Logger.LogWarning(Player.m_localPlayer.IsRecipeKnown(args[0]));

            // WizshBoneTwitchIntegration.Instance.SpawnSystemLogging();

            if (args.Length == 0)
            {
                Jotunn.Logger.LogWarning("Canceling test spawning...");
                Game.instance.CancelInvoke(nameof(SpawnRandomShit));
                return;
            }

            if (redeemNames.Count == 0)
            {
                redeemNames.Add("WBTI Random creature: Light");
                redeemNames.Add("WBTI Random creature: Medium");
                redeemNames.Add("WBTI Random creature: Heavy");
                redeemNames.Add("WBTI Random creature group");
                redeemNames.Add("WBTI Random creature group: BIOME");
                redeemNames.Add("WBTI Creatures: BOSSES!");
            }

            Jotunn.Logger.LogWarning("Start test spawning...");
            SpawnRandomShit();
        }

        private void SpawnRandomShit(int repeat = 5)
        {
            Jotunn.Logger.LogWarning("SpawnRandomShit");
            ConsoleCommand command = CommandManager.Instance.CustomCommands.First(item => item.Name == "UseTwitchRedeem");

            if (command == null)
            {
                Jotunn.Logger.LogWarning($"Could not find command!");
                return;
            }

            for (int i = 0; i < repeat; i++) {
                foreach (string name in redeemNames)
                {
                    command.Run(new string[] { name });
                }
            }
        }
    }
}
