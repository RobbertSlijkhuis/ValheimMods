using System.Collections.Generic;

namespace WizshBoneTwitchIntegration.Models
{
    internal class ModData
    {
        // On-disk format version, see ProfileMigrationHelper. Absent in files from before
        // versioning existed, which deserializes as 0 = legacy.
        public int version;
        public List<RedeemData> redeems;
        public List<CreatureGroupData> creatureGroups;
        public ProfileSettingsData settings;
    }
}
