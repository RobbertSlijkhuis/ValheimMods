using System;
using UnityEngine;
using UnityEngine.UI;
using WizshBoneTwitchIntegration.Helpers;

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

        // ── layout constants (same derived-from-shell-size pattern as ProfilesTab.cs) ──────────
        private const float ContentWidth = WizshBoneShellGUI.PanelWidth - ShellSidebar.Width;
        private const float ContentMargin = 30f;
        private const float ScrollContentWidth = ContentWidth - 2f * ContentMargin - ScrollableList.ScrollbarWidth;

        private const float TitleY = -30f;
        private const float TitleWidth = 300f;
        private const float ScrollTopInset = 65f;

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
                    v => ProfileSettingsHelper.Current.enableRedeemsOnLogin = v),
                ("Auto resolve", "Whether the redeems are resolved automaticly",
                    () => ProfileSettingsHelper.Current.autoResolveRedeems,
                    v => ProfileSettingsHelper.Current.autoResolveRedeems = v));

            StringFieldRow("Redeem prefix", "Added in front of every redeem title. No trailing space needed, it's added automatically.",
                () => ProfileSettingsHelper.Current.redeemTitlePrefix,
                v => ProfileSettingsHelper.Current.redeemTitlePrefix = v);
        }

        private void BuildChattingSection()
        {
            SectionHeader("Chatting");

            BoolFieldRow(
                ("Enable in-game chatting feature", "Whether viewer chat messages are shown above creatures in-game",
                    () => ProfileSettingsHelper.Current.chattingEnabled,
                    v => ProfileSettingsHelper.Current.chattingEnabled = v),
                ("Ignore tames", "Excludes tamed creatures from the chat feature",
                    () => ProfileSettingsHelper.Current.chattingIgnoreTames,
                    v => ProfileSettingsHelper.Current.chattingIgnoreTames = v));

            StringFieldRow("Chatting black list", "Prevents bots or viewers from being chosen for the chat feature",
                () => ProfileSettingsHelper.Current.chattingBlackList,
                v => ProfileSettingsHelper.Current.chattingBlackList = v);

            IntFieldRow(
                ("Claim duration", "0 means permanent",
                    () => ProfileSettingsHelper.Current.chattingClaimDuration,
                    v => ProfileSettingsHelper.Current.chattingClaimDuration = v),
                ("Max chat balloons", "Max balloons shown at once",
                    () => ProfileSettingsHelper.Current.chattingMaxTalkers,
                    v => ProfileSettingsHelper.Current.chattingMaxTalkers = v));

            FloatFieldRow(
                ("Scan radius", "Meters around player to scan for targets",
                    () => ProfileSettingsHelper.Current.chattingRadius,
                    v => ProfileSettingsHelper.Current.chattingRadius = v),
                ("Scan interval", "Seconds between chat scans",
                    () => ProfileSettingsHelper.Current.chattingInterval,
                    v => ProfileSettingsHelper.Current.chattingInterval = v));
        }

        private void BuildCreaturesSection()
        {
            SectionHeader("Creatures");

            BoolFieldRow(
                ("Same faction", "Spawned creatures won't attack each other",
                    () => ProfileSettingsHelper.Current.creaturesSameFaction,
                    v => ProfileSettingsHelper.Current.creaturesSameFaction = v),
                ("Health/damage scaling", "Scale spawned creatures by biome tier",
                    () => ProfileSettingsHelper.Current.creaturesScaling,
                    v => ProfileSettingsHelper.Current.creaturesScaling = v));

            FloatFieldRow(
                ("Damage scaling per biome tier", "Multiplier added per biome tier",
                    () => ProfileSettingsHelper.Current.creaturesDamageScale,
                    v => ProfileSettingsHelper.Current.creaturesDamageScale = v),
                ("Health scaling per biome tier", "Multiplier added per biome tier",
                    () => ProfileSettingsHelper.Current.creaturesHealthScale,
                    v => ProfileSettingsHelper.Current.creaturesHealthScale = v));

            FloatFieldRow(
                ("Max creature amount", "Max creatures spawned by the mod at once",
                    () => ProfileSettingsHelper.Current.creaturesMaxAmount,
                    v => ProfileSettingsHelper.Current.creaturesMaxAmount = v),
                ("Max creature radius", "Meters from player creatures can spawn",
                    () => ProfileSettingsHelper.Current.creaturesMaxRadius,
                    v => ProfileSettingsHelper.Current.creaturesMaxRadius = v));

            FloatFieldRow(
                ("Friendly follow radius with emote", "Meters friendly creatures will follow when emoted at",
                    () => ProfileSettingsHelper.Current.creaturesFollowRadius,
                    v => ProfileSettingsHelper.Current.creaturesFollowRadius = v));
        }

        private void BuildIndestructibleSection()
        {
            SectionHeader("Indestructible");

            BoolFieldRow(
                ("Boats", "Makes boats indestructible",
                    () => ProfileSettingsHelper.Current.indestructibleBoats,
                    v => ProfileSettingsHelper.Current.indestructibleBoats = v),
                ("Chests", "Makes chests indestructible",
                    () => ProfileSettingsHelper.Current.indestructibleChests,
                    v => ProfileSettingsHelper.Current.indestructibleChests = v));

            BoolFieldRow(
                ("Portals", "Makes portals indestructible",
                    () => ProfileSettingsHelper.Current.indestructiblePortals,
                    v => ProfileSettingsHelper.Current.indestructiblePortals = v),
                ("Crops", "Makes crops indestructible",
                    () => ProfileSettingsHelper.Current.indestructibleVegetables,
                    v => ProfileSettingsHelper.Current.indestructibleVegetables = v));
        }

        private void BuildSafezonesSection()
        {
            SectionHeader("Safezones");

            StringFieldRow("Twitch Ward recipe", "Comma-separated item:amount pairs required to craft the ward",
                () => ProfileSettingsHelper.Current.wardRecipe,
                v => ProfileSettingsHelper.Current.wardRecipe = v);

            BoolFieldRow(
                ("Ward burns spawned creatures", "Damages mod-spawned creatures that enter the ward",
                    () => ProfileSettingsHelper.Current.wardBurnCreatures,
                    v => ProfileSettingsHelper.Current.wardBurnCreatures = v),
                ("Ward pushes out spawned creatures", "Knocks mod-spawned creatures back outside the ward",
                    () => ProfileSettingsHelper.Current.wardPushCreatures,
                    v => ProfileSettingsHelper.Current.wardPushCreatures = v));

            BoolFieldRow(
                ("Add safezone to traders", "Creates a safezone around trader NPCs",
                    () => ProfileSettingsHelper.Current.safezoneTraders,
                    v => ProfileSettingsHelper.Current.safezoneTraders = v),
                ("Add safezone to boats", "Only for player-built boats",
                    () => ProfileSettingsHelper.Current.safezoneBoats,
                    v => ProfileSettingsHelper.Current.safezoneBoats = v));
        }

        // ── generic row builders ────────────────────────────────────────────────────────────

        private void SectionHeader(string title)
        {
            Text header = GuiHelper.CreateTitle(title, m_scrollContent, new Vector2(0f, m_cursorY), width: ScrollContentWidth);
            GuiHelper.PivotToTop(header.rectTransform, m_cursorY);
            m_cursorY -= SectionHeaderHeight;
        }

        private void BoolFieldRow(params (string Title, string Description, Func<bool> Get, Action<bool> Set)[] fields)
        {
            GameObject row = CreateRow();
            ForEachCell(fields.Length, (x, width, i) =>
            {
                var f = fields[i];
                GameObject cell = CreateCell(row, x, width);
                GuiHelper.CreateCardTitle(cell, f.Title, width - 24f);
                GuiFieldBuilder.CreateBoolField(cell, new Vector2(0f, -56f), width - 24f, f.Get(), v => { f.Set(v); ProfileSettingsPersistHelper.Persist(); });
                GuiHelper.CreateCardDescription(cell, f.Description, width - 24f);
            });
        }

        private void IntFieldRow(params (string Title, string Description, Func<int> Get, Action<int> Set)[] fields)
        {
            GameObject row = CreateRow();
            ForEachCell(fields.Length, (x, width, i) =>
            {
                var f = fields[i];
                GameObject cell = CreateCell(row, x, width);
                GuiHelper.CreateCardTitle(cell, f.Title, width - 24f);
                GuiFieldBuilder.CreateIntField(cell, new Vector2(0f, -58f), width - 24f, f.Get(), v => { f.Set(v); ProfileSettingsPersistHelper.Persist(); });
                GuiHelper.CreateCardDescription(cell, f.Description, width - 24f);
            });
        }

        private void FloatFieldRow(params (string Title, string Description, Func<float> Get, Action<float> Set)[] fields)
        {
            GameObject row = CreateRow();
            ForEachCell(fields.Length, (x, width, i) =>
            {
                var f = fields[i];
                GameObject cell = CreateCell(row, x, width);
                GuiHelper.CreateCardTitle(cell, f.Title, width - 24f);
                GuiFieldBuilder.CreateFloatField(cell, new Vector2(0f, -58f), width - 24f, f.Get(), v => { f.Set(v); ProfileSettingsPersistHelper.Persist(); });
                GuiHelper.CreateCardDescription(cell, f.Description, width - 24f);
            });
        }

        private void StringFieldRow(string title, string description, Func<string> get, Action<string> set)
        {
            GameObject row = CreateRow();
            GameObject cell = CreateCell(row, 0f, ScrollContentWidth);
            GuiHelper.CreateCardTitle(cell, title, ScrollContentWidth - 24f);
            InputField input = GuiFieldBuilder.CreateInputField(cell, new Vector2(0f, -58f), ScrollContentWidth - 24f, get());
            input.onValueChanged.AddListener(v => { set(v); ProfileSettingsPersistHelper.Persist(); });
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
