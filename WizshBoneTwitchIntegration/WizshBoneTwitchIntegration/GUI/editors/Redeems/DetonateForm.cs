using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Gui
{
    /// <summary>
    /// Step-2 form for Detonate. General tab: announce/damageTerrain/radius/Type + a Values widget
    /// whose shape depends on the live Type selection - Fish has no widget (locked to ["Fish"]),
    /// Creature/CreatureSpawned/Piece each get a SearchableChecklist off the matching
    /// <see cref="RedeemPrefabCatalog"/> list. Switching Type clears the current Values selection.
    /// Damage tab over <c>detonateData.damageData</c>, shown in full (the reference/richest case
    /// every SpawnAbilityFamily Damage-tab consumer trims down from).
    /// </summary>
    internal class DetonateForm : IRedeemStep2Form
    {
        private RedeemData m_working;
        private readonly GeneralDamageTabSwitcher m_tabs = new GeneralDamageTabSwitcher();
        private readonly DamageTabForm m_damageTab = new DamageTabForm();

        private InputField m_announceMessage;
        private Toggle m_damageTerrain;
        private InputField m_radius;
        private SearchableDropdown m_type;

        private GameObject m_valuesRoot;
        private float m_valuesFieldY;

        public void Build(GameObject parent)
        {
            m_tabs.Build(parent);
            var layout = new Step2RowLayout(m_tabs.GeneralRoot, m_tabs.ContentTopY);

            m_announceMessage = layout.TextRow("Announcement message", "Shown on screen when triggered. {{user}} is replaced with the redeemer's name.",
                "", "Optional announcement", v => m_working.detonateData.announceMessage = v, defaultValue: "");
            m_damageTerrain = layout.ToggleRow("Damage terrain", "Whether the explosion is allowed to dig/scorch terrain. Disabling avoids the heightmap edit, which helps performance on large detonations.",
                true, v => m_working.detonateData.damageTerrain = v, defaultValue: true);
            m_radius = layout.FloatRow("Radius", "How far around the target to search for objects to detonate, in meters.",
                20f, v => m_working.detonateData.radius = v, defaultValue: 20f);
            m_type = layout.DropdownRow("Type", "The type of objects to detonate.",
                GetTypeOptions(), DetonateType.Fish, OnTypeChanged, defaultValue: DetonateType.Fish, showSearch: false);

            m_valuesFieldY = layout.LabelOnlyRow("Values", "Which objects of the chosen type will detonate.");
            m_valuesRoot = UIContainer.Create(m_tabs.GeneralRoot, "Values", startActive: true);

            m_tabs.SetGeneralContentHeight(layout.CurrentY);

            // DetonateData.damageData's own field initializer tunes several defaults away from
            // DamageData's bare ones (blunt/chop/fire/pickaxe/basedOnMaxHealthAndArmor) - reading a
            // fresh top-level instance here, rather than bare `new DamageData()`, is what makes
            // DamageTabForm's reset buttons target Detonate's real baseline instead of zeros.
            m_damageTab.Build(m_tabs, new DetonateData().damageData);
        }

        private static List<DropdownOption> GetTypeOptions()
        {
            return new List<DropdownOption>
            {
                new DropdownOption(DetonateType.Creature, "Creature"),
                new DropdownOption(DetonateType.CreatureSpawned, "Creature (spawned by a redeem)"),
                new DropdownOption(DetonateType.Fish, "Fish"),
                new DropdownOption(DetonateType.Piece, "Piece"),
            };
        }

        private void OnTypeChanged(string newType)
        {
            m_working.detonateData.type = newType;
            m_working.detonateData.values = new List<string>();
            RebuildValuesWidget();
        }

        // Rebuilds the Values row's widget for the current type - Fish is locked (no widget,
        // values forced to ["Fish"]); Creature/CreatureSpawned/Piece each get a SearchableChecklist
        // off the matching live catalog. Called both on Type change (values already cleared by
        // OnTypeChanged above) and on Populate (shows the redeem's actual saved values).
        private void RebuildValuesWidget()
        {
            GuiHelper.ClearContainer(m_valuesRoot);
            DetonateData data = m_working.detonateData;

            if (data.type == DetonateType.Fish)
            {
                data.values = new List<string> { "Fish" };
                return;
            }

            List<DropdownOption> options = data.type == DetonateType.Piece
                ? RedeemPrefabCatalog.PlaceablePieces
                : RedeemPrefabCatalog.CreaturePrefabs;

            List<string> savedValues = data.values ?? new List<string>();
            foreach (string v in savedValues)
                options = RedeemPrefabCatalog.EnsureIncludesCurrentValue(options, v);

            var checklist = new SearchableChecklist();
            checklist.Build(m_valuesRoot, new Vector2(0f, m_valuesFieldY), RedeemWizard.Step2FieldWidth - ScrollableList.ScrollbarWidth, GuiFieldBuilder.FieldHeight, options, savedValues);
            checklist.OnSelectionChanged += v => m_working.detonateData.values = v;
        }

        public void Populate(RedeemData working)
        {
            m_working = working;
            DetonateData data = working.detonateData;

            m_announceMessage.text = data.announceMessage ?? "";
            m_damageTerrain.isOn = data.damageTerrain;
            m_radius.text = data.radius.ToString("G");
            m_type.SetOptions(GetTypeOptions(), data.type);

            RebuildValuesWidget();

            // Legacy-profile guard: a redeem saved before this field existed. Falls back to
            // DetonateData's own tuned baseline (see the Build-time comment above), not bare
            // DamageData zeros, so an old profile's Detonate damage matches what a newly-created
            // one would show.
            if (data.damageData == null)
                data.damageData = new DetonateData().damageData;
            m_damageTab.Populate(data.damageData);
        }
    }
}
