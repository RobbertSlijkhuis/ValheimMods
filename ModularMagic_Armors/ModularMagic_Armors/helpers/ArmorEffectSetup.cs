using ModularMagic_Armors.Components;
using ModularMagic_Armors.Models;
using ModularMagic_Armors.Types;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    /**
     * Knows which effect goes to which bone, when it is on and where it sits.
     * In Unity the effects only have to be children of the attach_skin (or attach) object of the armor piece, with their original names.
     * The prefab only gets an empty ArmorEffectAttach: values stored on the component are lost when the game copies the piece onto the wearer,
     * so the component looks its effects up here by name when it starts.
     * An effect name must have the same settings everywhere it is used (the eyes are on both the Wraith and the DarkWizard helmet).
     * Tweaking a position or a bone only needs a new DLL, not a new asset bundle.
     */
    internal class ArmorEffectSetup
    {
        public class Effect
        {
            public readonly string name;
            public readonly string bone;
            public readonly Vector3 position;
            public readonly Vector3? euler;
            public readonly Vector3 scale;
            public ArmorSetId gateSet;

            // bone = "" keeps the effect where it is and only switches it on/off (e.g. a child of another effect)
            // euler = null keeps the rotation the effect has in the prefab
            // gateSet = None is on as long as the piece is worn, otherwise only while the full set is worn
            public Effect(string name, string bone, Vector3 position, Vector3? euler, Vector3 scale, ArmorSetId gateSet = ArmorSetId.None)
            {
                this.name = name;
                this.bone = bone;
                this.position = position;
                this.euler = euler;
                this.scale = scale;
                this.gateSet = gateSet;
            }
        }

        private static readonly Dictionary<string, Effect> effectsByName = new Dictionary<string, Effect>();

        public static bool TryGetEffect(string effectName, out Effect effect)
        {
            return effectsByName.TryGetValue(effectName, out effect);
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

        // The Wraith effects are authored at scale 1.13 and the old code kept that size in the world when parenting them to the bones.
        // The bones of the player scale by 100 (the eyes show it: 0.02 authored became 0.0002), so the local scale was 1.13 / 100.
        // Rotation was never set in code (it came from how the game parented them), zero is a guess
        private static readonly Vector3 WraithEffectScale = new Vector3(0.0113f, 0.0113f, 0.0113f);

        // The hand bones do not have the scale of 100 the other bones have, so the hands use the authored scale
        private static readonly Vector3 WraithHandScale = new Vector3(1.13f, 1.13f, 1.13f);

        public static void ApplyWraith(CustomPrefabs prefabs)
        {
            Attach(prefabs.WraithHelmetPrefab, ArmorSetId.None,
                CreateEye("MMA_eye_left", new Vector3(-0.00087f, 0.00178f, -0.00035f), new Vector3(0f, 340.544f, 0f)),
                CreateEye("MMA_eye_right", new Vector3(-0.00087f, 0.00178f, 0.00044f), new Vector3(0f, 27.096f, 0f)),
                new Effect("MMA_wraith_head_effect", "Head", Vector3.zero, Vector3.zero, WraithEffectScale),
                new Effect("MMA_wraith_face_effect", "Head", new Vector3(-0.0015f, 0.001f, 0f), Vector3.zero, WraithEffectScale, ArmorSetId.Wraith));

            Attach(prefabs.WraithChestPrefab, ArmorSetId.None,
                new Effect("MMA_wraith_chest_effect", "Spine1", Vector3.zero, Vector3.zero, WraithEffectScale),
                new Effect("MMA_wraith_hand_left_effect", "LeftHand_Attach", Vector3.zero, Vector3.zero, WraithHandScale),
                new Effect("MMA_wraith_hand_right_effect", "RightHand_Attach", Vector3.zero, Vector3.zero, WraithHandScale));

            // Hides the body of the wearer while the full set is worn
            AddComponentOnce<SetBodyHider>(FindAttach(prefabs.WraithChestPrefab));

            Attach(prefabs.WraithLegsPrefab, ArmorSetId.None,
                new Effect("MMA_wraith_knee_left_effect", "LeftLeg", Vector3.zero, Vector3.zero, WraithEffectScale),
                new Effect("MMA_wraith_knee_right_effect", "RightLeg", Vector3.zero, Vector3.zero, WraithEffectScale));
        }

        public static void ApplyDarkWizard(CustomPrefabs prefabs)
        {
            Attach(prefabs.DarkWizardHelmetPrefab, ArmorSetId.None,
                CreateEye("MMA_eye_left", new Vector3(-0.00087f, 0.00178f, -0.00035f), new Vector3(0f, 340.544f, 0f)),
                CreateEye("MMA_eye_right", new Vector3(-0.00087f, 0.00178f, 0.00044f), new Vector3(0f, 27.096f, 0f)),
                CreateEyeFlames());

            Attach(prefabs.DarkWizardChestPrefab, ArmorSetId.DarkWizard,
                new Effect("MMA_darkwizard_shoulder_left_effect", "LeftArm", new Vector3(0f, 0f, -0.0001f), new Vector3(58f, 220f, 0f), new Vector3(0.5f, 0.5f, 0.5f)),
                new Effect("MMA_darkwizard_shoulder_right_effect", "RightArm", new Vector3(0f, 0f, -0.0001f), new Vector3(58f, 220f, 0f), new Vector3(0.5f, 0.5f, 0.5f)));
        }

        // The effects themselves are added by EmblaHelper, they are per piece (no set gate).
        // The old code kept the authored size (scale 1) in the world: the hood bone (Helmet_attach) does not scale, the shoulder bones scale by 100, so the shoulders get 1 / 100
        public static void ApplyEmbla(CustomPrefabs prefabs)
        {
            Attach(prefabs.EmblaHelmetPrefab, ArmorSetId.None,
                new Effect(EmblaHelper.HoodEffectName, "Helmet_attach", new Vector3(0f, -0.1f, -0.15f), Vector3.zero, Vector3.one));

            Vector3 shoulderScale = new Vector3(0.01f, 0.01f, 0.01f);

            Attach(prefabs.EmblaChestPrefab, ArmorSetId.None,
                new Effect(EmblaHelper.ShoulderLeftEffectName, "LeftArm", new Vector3(0f, -0.002f, -0.002f), Vector3.zero, shoulderScale),
                new Effect(EmblaHelper.ShoulderRightEffectName, "RightArm", new Vector3(0f, -0.002f, -0.002f), Vector3.zero, shoulderScale),
                new Effect(EmblaHelper.SpineEffectName, "Spine", new Vector3(0f, 0f, -0.0015f), Vector3.zero, shoulderScale));
        }

        // Same eyes on the Wraith and the DarkWizard helmet, only the material (set in Unity) differs
        private static Effect CreateEye(string name, Vector3 position, Vector3 euler)
        {
            return new Effect(name, "Head", position, euler, new Vector3(0.0002f, 0.0001f, 0.0003f));
        }

        // Child of each eye, only on with the full DarkWizard set. The Wraith eyes never have it on
        private static Effect CreateEyeFlames()
        {
            return new Effect("flames", "", Vector3.zero, null, Vector3.one, ArmorSetId.DarkWizard);
        }

        // Helmets only instantiate one of the two, chest and legs prefer attach_skin
        private static Transform FindAttach(GameObject itemPrefab)
        {
            if (itemPrefab == null)
                return null;

            Transform attach = itemPrefab.transform.Find("attach_skin");

            if (attach == null)
                attach = itemPrefab.transform.Find("attach");

            return attach;
        }

        private static void AddComponentOnce<T>(Transform attach) where T : Component
        {
            if (attach != null && attach.GetComponent<T>() == null)
                attach.gameObject.AddComponent<T>();
        }

        // defaultGate is used for the effects that do not name a gate themselves
        private static void Attach(GameObject itemPrefab, ArmorSetId defaultGate, params Effect[] effects)
        {
            if (itemPrefab == null)
            {
                Jotunn.Logger.LogWarning("[ArmorEffectSetup] Armor prefab is missing, effects not attached");
                return;
            }

            Transform attach = FindAttach(itemPrefab);

            if (attach == null)
            {
                Jotunn.Logger.LogWarning($"[ArmorEffectSetup] '{itemPrefab.name}' has no attach_skin or attach object, effects not attached");
                return;
            }

            AddComponentOnce<ArmorEffectAttach>(attach);

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

                if (effect.gateSet == ArmorSetId.None)
                    effect.gateSet = defaultGate;

                effectsByName[effect.name] = effect;
            }
        }
    }
}
