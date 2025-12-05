using System.Collections.Generic;
using System.IO;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Data
{
    internal class Redeems
    {
        public List<RedeemEntry> list = new List<RedeemEntry>();

        public Redeems()
        {
            if (FileExists())
            {
                list = YAMLHelper.ReadRedeemsConfig();
            }
            else
            {
                Jotunn.Logger.LogError("Could not find redeems configuration");
            }
        }

        public bool Reload()
        {
            if (!FileExists())
            {
                Jotunn.Logger.LogError("Could not find redeems configuration");
                return false;
            }

            list = YAMLHelper.ReadRedeemsConfig();
            return true;
        }

        private bool FileExists()
        {
            return File.Exists(WizshBoneTwitchIntegration.redeemsConfigPath);
        }
    }
}
