using System;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Gui.Tabs
{
    /// <summary>
    /// Settings tab content - built directly against RedesignUI.dc.html's structure: a title +
    /// one scrollable body (mirrors GUI_OLD/tabs/RulesTab.cs's shape) holding 5 sections in a
    /// fixed order (Redeems, Chatting, Creatures, Indestructible, Safezones), each a header
    /// followed by 1- or 2-column field cards. Every field writes straight through
    /// ProfileSettingsHelper.Current + <see cref="ProfileSettingsPersistHelper.Persist"/> on
    /// change, same as GUI_OLD's per-field RulesSettingsViewHelper.Persist() calls - no Save
    /// button. Hand-wired per field via <see cref="GuiFieldBuilder"/>'s generic widgets rather
    /// than GUI_OLD/editors/ObjectEditor's reflection dispatcher, which stays deferred to
    /// whichever round edits RedeemData/CreatureData (see the round-2 "field widget factory"
    /// decision). Fields the mockup doesn't show (chattingCullingRange, wardPushForce, the
    /// whole HUD section, allowRedeemsOnBoats) are intentionally omitted, matching the mockup.
    /// </summary>
    internal class SettingsTab : IShellTabView
    {
        private GameObject m_root;
        private GameObject m_scrollContent;
        private float m_cursorY;

        // Fresh, never-persisted instance used only to read each field's original default (for
        // the reset button next to text fields) without duplicating the literals from
        // ProfileSettingsData.cs here.
        private static readonly ProfileSettingsData Defaults = new ProfileSettingsData();

        // ── layout constants (same derived-from-shell-size pattern as ProfilesTab.cs) ──────────
        private const float ContentWidth = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;
        private const float ContentMargin = 30f;
        private const float ScrollContentWidth = ContentWidth - 2f * ContentMargin - ScrollableList.ScrollbarWidth;

        private const float TitleY = -30f;
        private const float TitleWidth = 300f;
        private const float DescriptionY = -50f;
        private const float ScrollTopInset = 72f;

        private const float SectionHeaderHeight = 35f;
        private const float SectionGap = 10f;
        private const float RowHeight = 120f;
        private const float RowSpacing = 10f;
        // The mockup's paired fields (e.g. "Enable redeems on login" + "Auto resolve") are two flex
        // columns inside ONE bordered card, not two separate cards with a gap between them - so the
        // two cells here sit flush against each other rather than being spaced apart.
        private const float ColGap = 0f;

        // GuiHelper.CreateTitle's text (used for section headers) doesn't pin a top pivot the way
        // CreateRow/CreateCell explicitly do - Jotunn's CreateText centers it - so a header placed
        // flush at the content's own top edge (y = 0) pokes its upper half above the viewport mask
        // and gets clipped when scrolled all the way up. This starting inset keeps the first
        // section header's top edge at/below that boundary, mirroring ProfilesTab's ListTopPadding.
        private const float ListTopPadding = 15f;

        public GameObject Create(GameObject parent)
        {
            m_root = UIContainer.Create(parent, "SettingsTab");

            float leftEdgeX = -(ContentWidth / 2f) + ContentMargin;
            GuiHelper.CreateTitle("Settings", m_root, new Vector2(leftEdgeX + TitleWidth / 2f, TitleY), width: TitleWidth);
            GuiHelper.CreateTabDescription(
                "Settings are specific to each profile.",
                m_root, new Vector2(0f, DescriptionY), width: ContentWidth - 2f * ContentMargin);

            m_scrollContent = ScrollableList.CreateStretched(
                m_root, "SettingsScroll",
                offsetMin: new Vector2(ContentMargin, ContentMargin),
                offsetMax: new Vector2(-ContentMargin, -ScrollTopInset),
                autoHideScrollbar: true);

            Rebuild();

            return m_root;
        }

        public void Refresh()
        {
            Rebuild();
        }

        private void Rebuild()
        {
            GuiHelper.ClearContainer(m_scrollContent);
            m_cursorY = -ListTopPadding;

            BuildRedeemsSection();
            BuildChattingSection();
            BuildCreaturesSection();
            BuildIndestructibleSection();
            BuildSafezonesSection();

            ScrollableList.SetContentHeight(m_scrollContent, Mathf.Abs(m_cursorY));
        }

        // ── sections ─────────────────────────────────────────────────────────────────────────

        private void BuildRedeemsSection()
        {
            SectionHeader("Redeems");

            BoolFieldRow(
                ("Enable redeems on login", "Automatically turns redeems on when you connect",
                    () => ProfileSettingsHelper.Current.enableRedeemsOnLogin,
                    v => ProfileSettingsHelper.Current.enableRedeemsOnLogin = v,
                    Defaults.enableRedeemsOnLogin),
                ("Auto resolve", "Whether the redeems are resolved automaticly",
                    () => ProfileSettingsHelper.Current.autoResolveRedeems,
                    v => ProfileSettingsHelper.Current.autoResolveRedeems = v,
                    Defaults.autoResolveRedeems));

            StringFieldRow("Redeem prefix", "Added in front of every redeem title. No trailing space needed, it's added automatically. Max 6 characters.",
                () => ProfileSettingsHelper.Current.redeemTitlePrefix,
                v => ProfileSettingsHelper.Current.redeemTitlePrefix = v,
                Defaults.redeemTitlePrefix,
                maxLength: 6);
        }

        private void BuildChattingSection()
        {
            SectionHeader("Chatting");

            BoolFieldRow(
                ("Enable in-game chatting feature", "Whether viewer chat messages are shown above creatures in-game",
                    () => ProfileSettingsHelper.Current.chattingEnabled,
                    v => ProfileSettingsHelper.Current.chattingEnabled = v,
                    Defaults.chattingEnabled),
                ("Free-for-all claiming", "First viewer to type !claim gets the creature, instead of only the selected user",
                    () => ProfileSettingsHelper.Current.chattingClaimFreeForAll,
                    v => ProfileSettingsHelper.Current.chattingClaimFreeForAll = v,
                    Defaults.chattingClaimFreeForAll));

            StringFieldRow("Chatting black list", "Prevents bots or viewers from being chosen for the chat feature",
                () => ProfileSettingsHelper.Current.chattingBlackList,
                v => ProfileSettingsHelper.Current.chattingBlackList = v,
                Defaults.chattingBlackList,
                isValid: TwitchChatting.IsUserBlacklistValid);

            IntFieldRow(
                ("Claim duration", "0 means permanent",
                    () => ProfileSettingsHelper.Current.chattingClaimDuration,
                    v => ProfileSettingsHelper.Current.chattingClaimDuration = v,
                    Defaults.chattingClaimDuration),
                ("Max chat balloons", "Max balloons shown at once",
                    () => ProfileSettingsHelper.Current.chattingMaxTalkers,
                    v => ProfileSettingsHelper.Current.chattingMaxTalkers = v,
                    Defaults.chattingMaxTalkers));

            FloatFieldRow(
                ("Scan radius", "Meters around player to scan for targets",
                    () => ProfileSettingsHelper.Current.chattingRadius,
                    v => ProfileSettingsHelper.Current.chattingRadius = v,
                    Defaults.chattingRadius),
                ("Scan interval", "Seconds between chat scans",
                    () => ProfileSettingsHelper.Current.chattingInterval,
                    v => ProfileSettingsHelper.Current.chattingInterval = v,
                    Defaults.chattingInterval));
        }

        private void BuildCreaturesSection()
        {
            SectionHeader("Creatures");

            BoolFieldRow(
                ("Same faction", "Spawned creatures won't attack each other",
                    () => ProfileSettingsHelper.Current.creaturesSameFaction,
                    v => ProfileSettingsHelper.Current.creaturesSameFaction = v,
                    Defaults.creaturesSameFaction),
                ("Health/damage scaling", "Scale spawned creatures by biome tier",
                    () => ProfileSettingsHelper.Current.creaturesScaling,
                    v => ProfileSettingsHelper.Current.creaturesScaling = v,
                    Defaults.creaturesScaling));

            FloatFieldRow(
                ("Damage scaling per biome tier", "Multiplier added per biome tier",
                    () => ProfileSettingsHelper.Current.creaturesDamageScale,
                    v => ProfileSettingsHelper.Current.creaturesDamageScale = v,
                    Defaults.creaturesDamageScale),
                ("Health scaling per biome tier", "Multiplier added per biome tier",
                    () => ProfileSettingsHelper.Current.creaturesHealthScale,
                    v => ProfileSettingsHelper.Current.creaturesHealthScale = v,
                    Defaults.creaturesHealthScale));

            FloatFieldRow(
                ("Max creature amount", "Max creatures spawned by the mod at once",
                    () => ProfileSettingsHelper.Current.creaturesMaxAmount,
                    v => ProfileSettingsHelper.Current.creaturesMaxAmount = v,
                    Defaults.creaturesMaxAmount),
                ("Max creature radius", "Meters from player creatures can spawn",
                    () => ProfileSettingsHelper.Current.creaturesMaxRadius,
                    v => ProfileSettingsHelper.Current.creaturesMaxRadius = v,
                    Defaults.creaturesMaxRadius));

            FloatFieldRow(
                ("Friendly follow radius with emote", "Meters friendly creatures will follow when emoted at",
                    () => ProfileSettingsHelper.Current.creaturesFollowRadius,
                    v => ProfileSettingsHelper.Current.creaturesFollowRadius = v,
                    Defaults.creaturesFollowRadius));
        }

        private void BuildIndestructibleSection()
        {
            SectionHeader("Indestructible");

            BoolFieldRow(
                ("Boats", "Makes boats indestructible",
                    () => ProfileSettingsHelper.Current.indestructibleBoats,
                    v => ProfileSettingsHelper.Current.indestructibleBoats = v,
                    Defaults.indestructibleBoats),
                ("Chests", "Makes chests indestructible",
                    () => ProfileSettingsHelper.Current.indestructibleChests,
                    v => ProfileSettingsHelper.Current.indestructibleChests = v,
                    Defaults.indestructibleChests));

            BoolFieldRow(
                ("Portals", "Makes portals indestructible",
                    () => ProfileSettingsHelper.Current.indestructiblePortals,
                    v => ProfileSettingsHelper.Current.indestructiblePortals = v,
                    Defaults.indestructiblePortals),
                ("Crops", "Makes crops indestructible",
                    () => ProfileSettingsHelper.Current.indestructibleVegetables,
                    v => ProfileSettingsHelper.Current.indestructibleVegetables = v,
                    Defaults.indestructibleVegetables));
        }

        private void BuildSafezonesSection()
        {
            SectionHeader("Safezones");

            StringFieldRow("Twitch Ward recipe", "Comma-separated item:amount pairs required to craft the ward",
                () => ProfileSettingsHelper.Current.wardRecipe,
                v => ProfileSettingsHelper.Current.wardRecipe = v,
                Defaults.wardRecipe,
                isValid: v => RecipeHelper.GetAsPieceRequirementArray(v, null, null) != null);

            BoolFieldRow(
                ("Ward burns spawned creatures", "Damages mod-spawned creatures that enter the ward",
                    () => ProfileSettingsHelper.Current.wardBurnCreatures,
                    v => ProfileSettingsHelper.Current.wardBurnCreatures = v,
                    Defaults.wardBurnCreatures),
                ("Ward pushes out spawned creatures", "Knocks mod-spawned creatures back outside the ward",
                    () => ProfileSettingsHelper.Current.wardPushCreatures,
                    v => ProfileSettingsHelper.Current.wardPushCreatures = v,
                    Defaults.wardPushCreatures));

            BoolFieldRow(
                ("Add safezone to traders", "Creates a safezone around trader NPCs",
                    () => ProfileSettingsHelper.Current.safezoneTraders,
                    v => ProfileSettingsHelper.Current.safezoneTraders = v,
                    Defaults.safezoneTraders),
                ("Add safezone to boats", "Only for player-built boats",
                    () => ProfileSettingsHelper.Current.safezoneBoats,
                    v => ProfileSettingsHelper.Current.safezoneBoats = v,
                    Defaults.safezoneBoats));
        }

        // ── generic row builders ────────────────────────────────────────────────────────────

        private void SectionHeader(string title)
        {
            Text header = GuiHelper.CreateTitle(title, m_scrollContent, new Vector2(0f, m_cursorY), width: ScrollContentWidth);
            GuiHelper.PivotToTop(header.rectTransform, m_cursorY);
            m_cursorY -= SectionHeaderHeight;
        }

        private void BoolFieldRow(params (string Title, string Description, Func<bool> Get, Action<bool> Set, bool Default)[] fields)
        {
            GameObject row = CreateRow();
            ForEachCell(fields.Length, (x, width, i) =>
            {
                var f = fields[i];
                GameObject cell = CreateCell(row, x, width);
                GuiHelper.CreateCardTitle(cell, f.Title, width - 24f);
                // The toggle stays its usual fixed left-aligned width (see CreateBoolField) - only
                // the reset button's position comes from FieldAndResetLayout here.
                (_, _, float resetCenterX) = GuiHelper.FieldAndResetLayout(width);
                Toggle toggle = GuiFieldBuilder.CreateBoolField(cell, new Vector2(0f, -56f), width - 24f, f.Get(), v => { f.Set(v); ProfileSettingsPersistHelper.Persist(); });
                // Toggle.isOn is a no-op when already equal to the target value, so this only fires
                // the listener above (and persists) when a reset actually changes anything.
                GuiHelper.CreateResetButton(cell, resetCenterX, -58f, () => toggle.isOn = f.Default);
                GuiHelper.CreateCardDescription(cell, f.Description, width - 24f);
            });
        }

        private void IntFieldRow(params (string Title, string Description, Func<int> Get, Action<int> Set, int Default)[] fields)
        {
            GameObject row = CreateRow();
            ForEachCell(fields.Length, (x, width, i) =>
            {
                var f = fields[i];
                GameObject cell = CreateCell(row, x, width);
                GuiHelper.CreateCardTitle(cell, f.Title, width - 24f);
                (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(width);
                InputField input = GuiFieldBuilder.CreateIntField(cell, new Vector2(fieldCenterX, -58f), fieldWidth, f.Get(), v => { f.Set(v); ProfileSettingsPersistHelper.Persist(); });
                GuiHelper.CreateResetButton(cell, resetCenterX, -58f, () => input.text = f.Default.ToString());
                GuiHelper.CreateCardDescription(cell, f.Description, width - 24f);
            });
        }

        private void FloatFieldRow(params (string Title, string Description, Func<float> Get, Action<float> Set, float Default)[] fields)
        {
            GameObject row = CreateRow();
            ForEachCell(fields.Length, (x, width, i) =>
            {
                var f = fields[i];
                GameObject cell = CreateCell(row, x, width);
                GuiHelper.CreateCardTitle(cell, f.Title, width - 24f);
                (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(width);
                InputField input = GuiFieldBuilder.CreateFloatField(cell, new Vector2(fieldCenterX, -58f), fieldWidth, f.Get(), v => { f.Set(v); ProfileSettingsPersistHelper.Persist(); });
                GuiHelper.CreateResetButton(cell, resetCenterX, -58f, () => input.text = f.Default.ToString("G"));
                GuiHelper.CreateCardDescription(cell, f.Description, width - 24f);
            });
        }

        private static readonly Color InvalidFieldColor = Color.red;

        private void StringFieldRow(string title, string description, Func<string> get, Action<string> set, string defaultValue, int maxLength = 0, Func<string, bool> isValid = null)
        {
            GameObject row = CreateRow();
            GameObject cell = CreateCell(row, 0f, ScrollContentWidth);
            GuiHelper.CreateCardTitle(cell, title, ScrollContentWidth - 24f);

            (float fieldWidth, float fieldCenterX, float resetCenterX) = GuiHelper.FieldAndResetLayout(ScrollContentWidth);

            InputField input = GuiFieldBuilder.CreateInputField(cell, new Vector2(fieldCenterX, -58f), fieldWidth, get(), maxLength: maxLength);
            Color normalColor = input.textComponent.color;
            input.onValueChanged.AddListener(v =>
            {
                set(v);
                ProfileSettingsPersistHelper.Persist();
                if (isValid != null)
                    input.textComponent.color = isValid(v) ? normalColor : InvalidFieldColor;
            });

            // Setting InputField.text (rather than calling set()/Persist() directly) reuses the
            // onValueChanged listener above, so a reset gets the exact same persist + validity-color
            // behavior as typing the default value in by hand.
            GuiHelper.CreateResetButton(cell, resetCenterX, -58f, () => input.text = defaultValue);

            GuiHelper.CreateCardDescription(cell, description, ScrollContentWidth - 24f);
        }

        /// <summary>
        /// Splits a row into 1 or 2 equal-width cells and invokes <paramref name="build"/> once
        /// per cell with that cell's center-x, width, and index - shared by every *FieldRow method
        /// above so the 1-vs-2-column math lives in one place.
        /// </summary>
        private static void ForEachCell(int count, Action<float, float, int> build)
        {
            if (count == 1)
            {
                build(0f, ScrollContentWidth, 0);
                return;
            }

            float cellWidth = (ScrollContentWidth - ColGap) / 2f;
            build(-cellWidth / 2f - ColGap / 2f, cellWidth, 0);
            build(cellWidth / 2f + ColGap / 2f, cellWidth, 1);
        }

        private GameObject CreateRow()
        {
            GameObject row = new GameObject("Row");
            row.transform.SetParent(m_scrollContent.transform, false);

            RectTransform rt = row.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(ScrollContentWidth, RowHeight);
            rt.anchoredPosition = new Vector2(0f, m_cursorY);

            m_cursorY -= RowHeight + RowSpacing;
            return row;
        }

        private static GameObject CreateCell(GameObject row, float x, float width)
        {
            return GuiHelper.CreateCard(row, new Vector2(x, 0f), width, RowHeight);
        }
    }
}
