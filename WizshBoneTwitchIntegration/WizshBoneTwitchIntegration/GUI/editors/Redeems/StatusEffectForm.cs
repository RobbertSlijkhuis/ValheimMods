using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for StatusEffect - full multi-entry list support via
    /// <see cref="EntryListEditor{TEntry}"/>. The <c>playerScale</c> sub-fields are only shown when
    /// Name is exactly "PlayerShrink"/"PlayerGrow", matching <c>StatusEffectEntry</c>'s own
    /// <c>[EditorVisibleWhen]</c> attribute; the underlying data is preserved (not reset) when Name
    /// is switched away and back, per an explicit decision on this exact scenario.
    /// </summary>
    internal class StatusEffectForm : IRedeemStep2Form
    {
        private readonly EntryListEditor<StatusEffectEntry> m_list = new EntryListEditor<StatusEffectEntry>();

        public void Build(GameObject parent)
        {
            m_list.Build(parent, RedeemWizard.BodyTopY, ItemLabel, BuildEntryFields);
        }

        private static string ItemLabel(StatusEffectEntry entry)
        {
            return string.IsNullOrEmpty(entry.name) ? "New status effect" : entry.name;
        }

        private static bool IsPlayerScaleEffect(string name)
        {
            return name == "PlayerShrink" || name == "PlayerGrow";
        }

        // Name gates both the Duration tooltip's wording and whether the 7 playerScale fields show
        // at all - re-runs this whole entry form on Name change (also re-reads Duration, since
        // OnNameChanged's real-effect TTL auto-fill needs to show immediately).
        private void BuildEntryFields(GameObject cardRoot, StatusEffectEntry entry)
        {
            GuiHelper.ClearContainer(cardRoot);
            var layout = new Step2RowLayout(cardRoot, m_list.CardContentTopY, m_list.CardContentWidth);

            List<DropdownOption> nameOptions = RedeemPrefabCatalog.EnsureIncludesCurrentValue(RedeemPrefabCatalog.StatusEffects, entry.name);
            layout.DropdownRow("Name", "The status effect to apply.", nameOptions, entry.name, v =>
            {
                entry.name = v;
                entry.duration = RedeemPrefabCatalog.LookupStatusEffectTTL(v);
                BuildEntryFields(cardRoot, entry);
            },
            defaultValue: null);

            bool isPlayerScale = IsPlayerScaleEffect(entry.name);
            layout.FloatRow("Duration",
                isPlayerScale
                    ? "How long the scale/speed change lasts, in seconds. Both -1 and 0 fall back to a fixed 30s for this effect, not the general -1=default/0=infinite convention."
                    : "Duration in seconds. -1 = the effect's own default duration, 0 = infinite.",
                entry.duration, v => entry.duration = v, defaultValue: -1f);
            layout.ToggleRow("Persists through death", "Whether this status effect survives the player dying.",
                entry.persistsThroughDeath, v => entry.persistsThroughDeath = v, defaultValue: false);
            layout.ToggleRow("Renew", "Whether re-applying this redeem while already active resets its duration.",
                entry.renew, v => entry.renew = v, defaultValue: false);

            if (isPlayerScale)
            {
                SE_PlayerScaleData scale = entry.playerScale ?? (entry.playerScale = new SE_PlayerScaleData());
                layout.FloatRow("Scale delta", "Amount to change the player's scale by (stacks across repeated redemptions).",
                    scale.scaleDelta, v => scale.scaleDelta = v, defaultValue: 0.3f);
                layout.FloatRow("Scale min", "Minimum scale the player can be reduced to.",
                    scale.scaleMin, v => scale.scaleMin = v, defaultValue: 0.4f);
                layout.FloatRow("Scale max", "Maximum scale the player can be increased to.",
                    scale.scaleMax, v => scale.scaleMax = v, defaultValue: 4f);
                layout.FloatRow("Scale duration", "Duration of the scale change's visual transition, in seconds.",
                    scale.scaleDuration, v => scale.scaleDuration = v, defaultValue: 0.5f);
                layout.FloatRow("Speed multiplier delta", "Amount to change the player's speed/jump multiplier by (stacks across repeated redemptions).",
                    scale.speedMultiplierDelta, v => scale.speedMultiplierDelta = v, defaultValue: 0.125f);
                layout.FloatRow("Speed multiplier min", "Minimum speed/jump multiplier the player can be reduced to.",
                    scale.speedMultiplierMin, v => scale.speedMultiplierMin = v, defaultValue: 0.75f);
                layout.FloatRow("Speed multiplier max", "Maximum speed/jump multiplier the player can be increased to.",
                    scale.speedMultiplierMax, v => scale.speedMultiplierMax = v, defaultValue: 2.25f);
            }
        }

        public void Populate(RedeemData working)
        {
            StatusEffectData data = working.statusEffectData;
            m_list.Populate(data.list, () => data.random, v => data.random = v);
        }
    }
}
