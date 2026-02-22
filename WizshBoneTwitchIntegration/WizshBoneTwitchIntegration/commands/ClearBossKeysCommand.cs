using Jotunn.Entities;
using System;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ClearBossKeysCommand : ConsoleCommand
    {
        public override string Name => "ClearBossKeys";

        public override string Help => "Remove all global boss keys";

        public override void Run(string[] args)
        {
            try
            {
                ZoneSystem.instance.RemoveGlobalKey(GlobalKeyType.DefeatedEikthyr);
                ZoneSystem.instance.RemoveGlobalKey(GlobalKeyType.DefeatedElder);
                ZoneSystem.instance.RemoveGlobalKey(GlobalKeyType.DefeatedBonemass);
                ZoneSystem.instance.RemoveGlobalKey(GlobalKeyType.DefeatedModer);
                ZoneSystem.instance.RemoveGlobalKey(GlobalKeyType.DefeatedYagluth);
                ZoneSystem.instance.RemoveGlobalKey(GlobalKeyType.DefeatedQueen);
                ZoneSystem.instance.RemoveGlobalKey(GlobalKeyType.DefeatedFader);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError(error);
            }
        }
    }
}
