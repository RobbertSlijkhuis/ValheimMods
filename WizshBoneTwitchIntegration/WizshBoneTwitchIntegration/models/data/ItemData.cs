using WizshBoneTwitchIntegration.GuiOld;

namespace WizshBoneTwitchIntegration.Models
{
    internal class ItemData : CloneableData
    {
        // Upper bound for quality, enforced both in the editor and when spawning - a hand-edited or
        // synced profile.yaml could otherwise hand out absurdly strong items.
        public const int MaxQuality = 10;

        public string prefabName;
        public int amount = 1;
        public int quality = 1;

        // Read-only legacy key: "amount" used to be called "stackSize". It is only ever deserialized
        // (null is never written back) so ProfileMigrationHelper can carry it over to amount.
        [EditorHidden] public int? stackSize;

        public ItemData() { }
    }
}
