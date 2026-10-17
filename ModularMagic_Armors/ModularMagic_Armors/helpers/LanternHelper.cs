using BepInEx.Configuration;
using Jotunn.Configs;
using Jotunn.Managers;
using ModularMagic_Armors.Components;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    /**
     * Knows the lanterns and whether they are lit.
     * The lit state is a bool on the ZDO of the wearer, so every client (and late joiners) sees it, and a missing key means lit.
     * Only the off state is stored in the player profile (Player.m_customData), because the ZDO of a player is gone after logging out.
     * The lantern prefab only gets an empty LanternEffect: values stored on the component are lost when the game copies the item onto the wearer,
     * so the component looks its lantern up here by the prefab hash of the worn utility item.
     */
    internal class LanternHelper
    {
        public const string ButtonName = "Lantern mode";

        private const string StateKey = "MMA_LanternOn";
        private static readonly int StateHash = StateKey.GetStableHashCode();

        public class Lantern
        {
            public readonly Material onMaterial;
            public readonly Material offMaterial;
            public readonly bool demister;

            public Lantern(Material onMaterial, Material offMaterial, bool demister)
            {
                this.onMaterial = onMaterial;
                this.offMaterial = offMaterial;
                this.demister = demister;
            }
        }

        private static readonly Dictionary<int, Lantern> lanterns = new Dictionary<int, Lantern>();
        private static ButtonConfig toggleButton;

        /**
         * Adds the component that lights the lantern on the wearer and remembers the lantern under its prefab name.
         * demister = false keeps the demister of the prefab switched off.
         */
        public static void Register(GameObject prefab, Material onMaterial, Material offMaterial, bool demister)
        {
            if (prefab == null)
            {
                Jotunn.Logger.LogWarning("[LanternHelper] Lantern prefab is missing, lantern not registered");
                return;
            }

            // Like armor pieces, only one of the two is instantiated on the wearer
            Transform attach = prefab.transform.Find("attach_skin");

            if (attach == null)
                attach = prefab.transform.Find("attach");

            if (attach == null)
            {
                Jotunn.Logger.LogWarning($"[LanternHelper] '{prefab.name}' has no attach_skin or attach object, lantern not registered");
                return;
            }

            if (attach.GetComponent<LanternEffect>() == null)
                attach.gameObject.AddComponent<LanternEffect>();

            if (offMaterial == null)
                Jotunn.Logger.LogWarning($"[LanternHelper] '{prefab.name}' has no Off material, the lantern will not change look when switched off");

            lanterns[prefab.name.GetStableHashCode()] = new Lantern(onMaterial, offMaterial, demister);
        }

        public static bool TryGetLantern(int prefabHash, out Lantern lantern)
        {
            return lanterns.TryGetValue(prefabHash, out lantern);
        }

        public static void InitInput(ConfigEntry<KeyboardShortcut> key)
        {
            toggleButton = new ButtonConfig
            {
                Name = ButtonName,
                ShortcutConfig = key,
            };

            InputManager.Instance.AddButton(ModularMagic_Armors.PluginGUID, toggleButton);
        }

        /**
         * No ZDO (e.g. the player preview in the main menu) is lit
         */
        public static bool IsLit(VisEquipment visEquipment)
        {
            ZNetView netView = visEquipment != null ? visEquipment.m_nview : null;

            if (netView == null || !netView.IsValid())
                return true;

            return netView.GetZDO().GetBool(StateHash, true);
        }

        public static void HandleToggleInput(Player player)
        {
            if (player == null || player != Player.m_localPlayer)
                return;

            // The name is read from the config: Jotunn renames the button to '<name>!<mod guid>' when it is added
            if (toggleButton == null || ZInput.instance == null || !ZInput.GetButtonDown(toggleButton.Name))
                return;

            if (!IsLanternEquipped(player) || !player.TakeInput())
                return;

            ZNetView netView = player.m_nview;

            if (netView == null || !netView.IsValid())
                return;

            ZDO zdo = netView.GetZDO();
            SetLit(player, zdo, !zdo.GetBool(StateHash, true));
        }

        /**
         * Puts the remembered state of the profile on the new player object
         */
        public static void RestoreState(Player player)
        {
            if (player != Player.m_localPlayer || player.m_nview == null || !player.m_nview.IsValid())
                return;

            if (player.m_customData.ContainsKey(StateKey))
                player.m_nview.GetZDO().Set(StateHash, false);
        }

        private static void SetLit(Player player, ZDO zdo, bool lit)
        {
            zdo.Set(StateHash, lit);

            if (lit)
                player.m_customData.Remove(StateKey);
            else
                player.m_customData[StateKey] = "off";
        }

        private static bool IsLanternEquipped(Player player)
        {
            ItemDrop.ItemData item = player.m_utilityItem;

            return item != null && item.m_dropPrefab != null && lanterns.ContainsKey(item.m_dropPrefab.name.GetStableHashCode());
        }
    }
}
