using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// The Damage tab shared by Detonate and 6 of the SpawnAbilityFamily types (Windmill/Smite/
    /// Rain/Meteor/Trap/Root/LogRain), each passing in its own <see cref="DamageData"/> instance.
    /// Every consumer shows the full, uncurated field set this round (no per-type trimming yet).
    ///
    /// Builds its rows directly as children of the <c>parent</c> passed to <see cref="Build"/> -
    /// that GameObject is expected to be a tab-content root the caller already owns and shows/hides
    /// itself (e.g. a type form's own General/Damage tab switcher); this class only owns its rows'
    /// content, never its own visibility.
    /// </summary>
    internal class DamageTabForm
    {
        private GeneralDamageTabSwitcher m_tabs;
        private DamageData m_data;

        /// <summary>
        /// The defaults every reset button on this tab writes back to - retained from whatever
        /// <see cref="DamageData"/> instance <see cref="Build"/> was first given. Every caller
        /// already constructs a fresh, never-persisted instance there purely to render the initial
        /// placeholder rows before <see cref="Populate"/> overwrites them with the live value, so
        /// that same instance doubles as the reset source for free - as long as the caller passes
        /// its *actual* type-specific defaults there (see DetonateForm, whose DetonateData.damageData
        /// field initializer tunes this away from DamageData's own bare defaults).
        /// </summary>
        private DamageData m_defaults;

        /// <summary>Y the last row ended at - lets the caller size a scroll container if needed.</summary>
        public float ContentBottomY { get; private set; }

        /// <summary>Builds this tab's rows. Call again via <see cref="Populate"/> to resync from a (possibly different) DamageData instance.</summary>
        public void Build(GeneralDamageTabSwitcher tabs, DamageData damageData)
        {
            m_tabs = tabs;
            m_data = damageData;
            m_defaults = damageData;
            RebuildRows();
        }

        /// <summary>
        /// Re-reads every row from <paramref name="damageData"/> - safe to call repeatedly (e.g.
        /// every time the caller's Damage tab is (re)selected), mirroring the write-on-change/
        /// re-read-on-entry contract <c>RedeemWizard.PopulateStep3Fields</c> already uses.
        /// </summary>
        public void Populate(DamageData damageData)
        {
            m_data = damageData;
            RebuildRows();
        }

        // Full clear+rebuild rather than in-place reflow - needed because
        // basedOnMaxHealthAndArmor gates whether the two percentage rows exist at all, and this
        // project's convention (see Step2RowLayout's consumers) is to rebuild a form's content on
        // any gating change rather than invent an in-place row-reflow mechanism. Safe to destroy
        // the toggle mid-callback since Unity's GameObject.Destroy is deferred to end of frame.
        private void RebuildRows()
        {
            GuiHelper.ClearContainer(m_tabs.DamageRoot);
            var layout = new Step2RowLayout(m_tabs.DamageRoot, m_tabs.ContentTopY);

            layout.FloatRow("Blunt damage", null, m_data.blunt ?? 0f, v => m_data.blunt = v, m_defaults.blunt ?? 0f);
            layout.FloatRow("Chop damage", "This damage is only applied to trees and structures.", m_data.chop ?? 0f, v => m_data.chop = v, m_defaults.chop ?? 0f);
            layout.FloatRow("Fire damage", null, m_data.fire ?? 0f, v => m_data.fire = v, m_defaults.fire ?? 0f);
            layout.FloatRow("Frost damage", null, m_data.frost ?? 0f, v => m_data.frost = v, m_defaults.frost ?? 0f);
            layout.FloatRow("Lightning damage", null, m_data.lightning ?? 0f, v => m_data.lightning = v, m_defaults.lightning ?? 0f);
            layout.FloatRow("Pickaxe damage", "This damage is only applied to minable objects, like stone, ore etc. (StoneGolem too).", m_data.pickaxe ?? 0f, v => m_data.pickaxe = v, m_defaults.pickaxe ?? 0f);
            layout.FloatRow("Pierce damage", null, m_data.pierce ?? 0f, v => m_data.pierce = v, m_defaults.pierce ?? 0f);
            layout.FloatRow("Poison damage", null, m_data.poison ?? 0f, v => m_data.poison = v, m_defaults.poison ?? 0f);
            layout.FloatRow("Slash damage", null, m_data.slash ?? 0f, v => m_data.slash = v, m_defaults.slash ?? 0f);
            layout.FloatRow("Spirit damage", "This damage is only applied to undead.", m_data.spirit ?? 0f, v => m_data.spirit = v, m_defaults.spirit ?? 0f);

            layout.ToggleRow(
                "Scale from target's max health & armor",
                "If enabled, damage is calculated from the target's max health and armor instead of the fixed amounts above (Chop/Pickaxe still apply as typed).",
                m_data.basedOnMaxHealthAndArmor,
                v =>
                {
                    m_data.basedOnMaxHealthAndArmor = v;
                    RebuildRows();
                },
                m_defaults.basedOnMaxHealthAndArmor);

            if (m_data.basedOnMaxHealthAndArmor)
            {
                layout.FloatRow("Armor percentage", "Percentage of the target's armor to use for damage calculation.", m_data.armorPercentage, v => m_data.armorPercentage = v, m_defaults.armorPercentage);
                layout.FloatRow("Max health percentage", "Percentage of the target's max health to use for damage calculation.", m_data.maxHealthPercentage, v => m_data.maxHealthPercentage = v, m_defaults.maxHealthPercentage);
            }

            layout.ToggleRow("Damage bosses", null, m_data.damageBosses, v => m_data.damageBosses = v, m_defaults.damageBosses);
            layout.ToggleRow("Damage ships", null, m_data.damageShips, v => m_data.damageShips = v, m_defaults.damageShips);
            layout.ToggleRow("Damage structures", null, m_data.damageStructures, v => m_data.damageStructures = v, m_defaults.damageStructures);

            ContentBottomY = layout.CurrentY;
            m_tabs.SetDamageContentHeight(ContentBottomY);
        }
    }
}
