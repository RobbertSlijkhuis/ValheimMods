using ModularMagic_Armors.Components;
using ModularMagic_Armors.Models;
using ModularMagic_Armors.Types;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    /**
     * Adds ArmorEffectAttach to the armor prefabs when the mod loads and tells it which effect goes to which bone, when it is on and where it sits.
     * In Unity the effects only have to be children of the attach_skin (or attach) object of the armor piece, with their original names.
     * Tweaking a position or a bone only needs a new DLL, not a new asset bundle.
     */
    internal class ArmorEffectSetup
    {
        private class Effect
        {
            public readonly string name;
            public readonly string bone;
            public readonly Vector3 position;
            public readonly Vector3? euler;
            public readonly Vector3 scale;

            // euler = null keeps the rotation the effect has in the prefab
            public Effect(string name, string bone, Vector3 position, Vector3? euler, Vector3 scale)
            {
                this.name = name;
                this.bone = bone;
                this.position = position;
                this.euler = euler;
                this.scale = scale;
            }
        }

        public static void ApplyShaman(CustomPrefabs prefabs)
        {
            Attach(prefabs.ShamanHelmetPrefab, ArmorSetId.Shaman,
                new Effect("MMA_shaman_head_effect", "Head", new Vector3(-0.0017f, -0.0008f, 0f), new Vector3(90f, 0f, 0f), new Vector3(0.4f, 0.4f, 0.4f)),
                new Effect("MMA_shaman_antler_left_effect", "Head", new Vector3(0f, 0.004f, 0f), Vector3.zero, new Vector3(0.0007f, 0.0007f, 0.0007f)),
                new Effect("MMA_shaman_antler_right_effect", "Head", new Vector3(0f, 0.004f, 0f), Vector3.zero, new Vector3(0.0007f, 0.0007f, 0.0007f)));

            Attach(prefabs.ShamanChestPrefab, ArmorSetId.Shaman,
                new Effect("MMA_shaman_wrist_left_effect", "LeftHand", new Vector3(-0.0012f, -0.001f, -0.00081f), Vector3.zero, new Vector3(0.001f, 0.001f, 0.001f)),
                new Effect("MMA_shaman_wrist_right_effect", "RightHand", new Vector3(-0.0012f, -0.001f, 0f), Vector3.zero, new Vector3(0.001f, 0.001f, 0.001f)));

            Attach(prefabs.ShamanLegsPrefab, ArmorSetId.Shaman,
                new Effect("MMA_shaman_knee_left_effect", "LeftLeg", new Vector3(-0.0012f, 0.003f, 0f), Vector3.zero, new Vector3(0.001f, 0.001f, 0.001f)),
                new Effect("MMA_shaman_knee_right_effect", "RightLeg", new Vector3(-0.0012f, 0.003f, 0f), Vector3.zero, new Vector3(0.001f, 0.001f, 0.001f)));
        }

        public static void ApplyFrostWolf(CustomPrefabs prefabs)
        {
            Attach(prefabs.FrostWolfHelmetPrefab, ArmorSetId.FrostWolf,
                new Effect("MMA_frostwolf_head_effect", "Head", new Vector3(0f, 0.0025f, 0f), null, Vector3.one));

            Attach(prefabs.FrostWolfChestPrefab, ArmorSetId.FrostWolf,
                new Effect("MMA_frostwolf_chest_effect", "Spine2", Vector3.zero, null, Vector3.one),
                new Effect("MMA_frostwolf_hand_left_effect", "LeftHand_Attach", Vector3.zero, null, Vector3.one),
                new Effect("MMA_frostwolf_hand_right_effect", "RightHand_Attach", Vector3.zero, null, Vector3.one));

            Attach(prefabs.FrostWolfLegsPrefab, ArmorSetId.FrostWolf,
                new Effect("MMA_frostwolf_knee_left_effect", "LeftLeg", Vector3.zero, null, Vector3.one),
                new Effect("MMA_frostwolf_knee_right_effect", "RightLeg", Vector3.zero, null, Vector3.one));
        }

        private static void Attach(GameObject itemPrefab, ArmorSetId gateSet, params Effect[] effects)
        {
            if (itemPrefab == null)
            {
                Jotunn.Logger.LogWarning("[ArmorEffectSetup] Armor prefab is missing, effects not attached");
                return;
            }

            // Helmets only instantiate one of the two, chest and legs prefer attach_skin
            Transform attach = itemPrefab.transform.Find("attach_skin");

            if (attach == null)
                attach = itemPrefab.transform.Find("attach");

            if (attach == null)
            {
                Jotunn.Logger.LogWarning($"[ArmorEffectSetup] '{itemPrefab.name}' has no attach_skin or attach object, effects not attached");
                return;
            }

            ArmorEffectAttach component = attach.GetComponent<ArmorEffectAttach>();

            if (component == null)
                component = attach.gameObject.AddComponent<ArmorEffectAttach>();

            component.bindings.Clear();

            foreach (Effect effect in effects)
            {
                Transform target = TransformHelper.FindChildDepthFirst(attach, effect.name);

                if (target == null)
                {
                    Jotunn.Logger.LogWarning($"[ArmorEffectSetup] Effect '{effect.name}' not found under '{attach.name}' of '{itemPrefab.name}'");
                    continue;
                }

                // The component switches it on when the piece is worn
                target.gameObject.SetActive(false);

                component.bindings.Add(new EffectBinding()
                {
                    target = target,
                    bone = effect.bone,
                    gateSet = gateSet,
                    overrideLocal = true,
                    localPosition = effect.position,
                    localEuler = effect.euler ?? target.localEulerAngles,
                    localScale = effect.scale,
                });
            }
        }
    }
}
