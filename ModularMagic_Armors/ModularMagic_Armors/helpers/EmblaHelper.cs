using Jotunn;
using ModularMagic_Armors.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    /**
     * Retextures the vanilla Embla armor (red with dark gray ash, because the FrostWolf set uses the same models) and gives it its flame effects.
     * Both are done once when the prefabs become available and only when the Embla option is on. Vanilla prefab edits persist for the whole session,
     * so everything here checks what is already there before adding it.
     */
    internal class EmblaHelper
    {
        public const string HoodEffectName = "EmblaHood_Effects_MMA";
        public const string ShoulderLeftEffectName = "EmblaChest_Left_Effects_MMA";
        public const string ShoulderRightEffectName = "EmblaChest_Right_Effects_MMA";
        public const string SpineEffectName = "EmblaChest_Spine_Effects_MMA";

        public static void ApplyReskin(CustomPrefabs prefabs, CustomMaterials materials, CustomSprites sprites)
        {
            materials.EmblaArmor.FixReferences();
            materials.EmblaChest.FixReferences();
            materials.EmblaLegs.FixReferences();

            List<Material> armorMaterials = new List<Material>();
            armorMaterials.Add(materials.EmblaArmor);

            ItemDrop helmetItemDrop = prefabs.EmblaHelmetPrefab.GetComponent<ItemDrop>();
            ItemDrop chestItemDrop = prefabs.EmblaChestPrefab.GetComponent<ItemDrop>();
            ItemDrop legsItemDrop = prefabs.EmblaLegsPrefab.GetComponent<ItemDrop>();

            Transform helmet = prefabs.EmblaHelmetPrefab.transform.Find("attach_skin/AshlandsHood");
            Transform helmetFlat = prefabs.EmblaHelmetPrefab.transform.Find("hood");
            Transform chest = prefabs.EmblaChestPrefab.transform.Find("attach_skin/AshlandsMageChest");
            Transform chestFlat = prefabs.EmblaChestPrefab.transform.Find("model");
            Transform legs = prefabs.EmblaLegsPrefab.transform.Find("attach_skin/AshlandsMageLegs");
            Transform legsFlat = prefabs.EmblaLegsPrefab.transform.Find("log");

            SkinnedMeshRenderer helmetMesh = helmet.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer helmetFlatMesh = helmetFlat.gameObject.GetComponent<MeshRenderer>();
            SkinnedMeshRenderer chestMesh = chest.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer chestFlatMesh = chestFlat.gameObject.GetComponent<MeshRenderer>();
            SkinnedMeshRenderer legsMesh = legs.gameObject.GetComponent<SkinnedMeshRenderer>();
            MeshRenderer legsFlatMesh = legsFlat.gameObject.GetComponent<MeshRenderer>();

            List<Sprite> helmetSpriteList = new List<Sprite>();
            helmetSpriteList.Add(sprites.EmblaHood);
            helmetItemDrop.m_itemData.m_shared.m_icons = helmetSpriteList.ToArray();
            helmetMesh.materials = armorMaterials.ToArray();
            helmetFlatMesh.materials = armorMaterials.ToArray();

            List<Sprite> chestSpriteList = new List<Sprite>();
            chestSpriteList.Add(sprites.EmblaChest);
            chestItemDrop.m_itemData.m_shared.m_icons = chestSpriteList.ToArray();
            chestItemDrop.m_itemData.m_shared.m_armorMaterial = materials.EmblaChest;
            chestMesh.materials = armorMaterials.ToArray();
            chestFlatMesh.materials = armorMaterials.ToArray();

            List<Sprite> legsSpriteList = new List<Sprite>();
            legsSpriteList.Add(sprites.EmblaLegs);
            legsItemDrop.m_itemData.m_shared.m_icons = legsSpriteList.ToArray();
            legsItemDrop.m_itemData.m_shared.m_armorMaterial = materials.EmblaLegs;
            legsMesh.materials = armorMaterials.ToArray();
            legsFlatMesh.materials = armorMaterials.ToArray();
        }

        /**
         * Puts a copy of the flame effect in the attach_skin of the hood (one) and the chest (two, one per shoulder).
         * ArmorEffectSetup then lets the effects follow the bones, like the effects of the other sets.
         */
        public static void InjectEffects(CustomPrefabs prefabs)
        {
            if (prefabs.EmblaEffects == null)
            {
                Jotunn.Logger.LogWarning("[Embla] Effect prefab is missing, effects not added");
                return;
            }

            Inject(prefabs.EmblaHelmetPrefab, prefabs.EmblaEffects, HoodEffectName);
            Inject(prefabs.EmblaChestPrefab, prefabs.EmblaEffects, ShoulderLeftEffectName);
            Inject(prefabs.EmblaChestPrefab, prefabs.EmblaEffects, ShoulderRightEffectName);
            Inject(prefabs.EmblaChestPrefab, prefabs.EmblaEffects, SpineEffectName, 2f);

            ArmorEffectSetup.ApplyEmbla(prefabs);
        }

        // flareSize: multiplies the size of the flare. The particle systems scale by their own object only (not by the parent), so the scale of the effect does not change it
        private static void Inject(GameObject itemPrefab, GameObject effectTemplate, string effectName, float flareSize = 1f)
        {
            Transform attach = itemPrefab.transform.Find("attach_skin");

            if (attach == null)
            {
                Jotunn.Logger.LogWarning($"[Embla] '{itemPrefab.name}' has no attach_skin, effect '{effectName}' not added");
                return;
            }

            // Already there from an earlier run
            if (attach.Find(effectName) != null)
                return;

            // Unity does not allow instantiating directly under a parent that belongs to a prefab, so the copy is made first and moved after
            GameObject effect = Object.Instantiate(effectTemplate);
            effect.name = effectName;
            effect.transform.SetParent(attach, false);
            effect.FixReferences(true);

            // Only the flare of the template is shown, the other flame parts stay off
            Transform flare = effect.transform.Find("equiped/flare");

            if (flare != null)
            {
                flare.gameObject.SetActive(true);
                flare.localScale *= flareSize;
            }

            effect.SetActive(false);
        }
    }
}
