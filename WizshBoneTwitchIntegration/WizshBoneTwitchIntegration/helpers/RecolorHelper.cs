using HarmonyLib;
using Jotunn;
using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class RecolorHelper
    {
        public static Dictionary<string, Color> specialViewers = new Dictionary<string, Color>()
        {
            // Subs
            {"beardedfeo", new Color(1, 0.412f, 0.706f)},
            {"cloudedrogue", new Color(0.384f, 0.004f, 0.004f)},
            {"daitjen", new Color(0.239f, 0.271f, 0.278f)},
            {"firgaardttv", new Color(0.6f, 0.443f, 0.224f)},
            {"karambiee", new Color(0.522f, 0.176f, 0.435f)},
            {"kimetsu159", new Color(0.686f, 1f, 0.004f)},
            {"lothren147", new Color(0.859f, 0.29f, 0.247f)},
            {"velmira12", new Color(0.192f, 0.604f, 0.141f)},

            // Dragonmancer123456789
            // Awesome ppl or gift subs
            {"1mmun1tyy", new Color(0.471f, 0.839f, 0.384f)},
            {"dragonmancer123456789", new Color(0.75f, 0f, 0f)},
            {"durdyjay", new Color(1f, 0f, 0.298f)},
            {"itzsjoeky", new Color(0.851f, 0.239f, 0.447f)},
            {"kassiethelittlepumpkin", new Color(1f, 0.271f, 0f)},
            {"phenazo", new Color(0.686f, 0.922f, 0.71f)},
            {"soma_af", new Color(1f, 0.733f, 0.875f)},
            {"waterfox1316", new Color(0.259f, 0.773f, 0.886f)},
            {"xxainty", new Color(0.3f, 0.3f, 0.3f)},
            {"yummyzebracakes", new Color(1f, 1f, 1f)},
            {"devwizsh", new Color(1f, 0.733f, 0.875f)},
        };

        public static Dictionary<string, List<RecolorCreatureData>> creatureList = new Dictionary<string, List<RecolorCreatureData>>()
        {
            { "Abomination", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Abomination") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorAbomination, emissive = true},
            }},
            { "Bjorn", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Asora") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = false, zebraMaterial = WizshBoneTwitchIntegration.Instance.materials.RecolorBjornZebra },
                new RecolorCreatureData("Visual/Plane") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = false },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = true },
                new RecolorCreatureData("Visual/Cube.002") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = true },
                new RecolorCreatureData("Visual/Sphere") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = true },
            }},
            { "Blob", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/blob/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlob, emissive = true, emissiveMultiplier = 1f },
                new RecolorCreatureData("Visual/Point light") { isLight = true },
                new RecolorCreatureData("Visual/particles/ooz") { isParticle = true },
                new RecolorCreatureData("Visual/particles/wetsplsh") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
                new RecolorCreatureData("Visual/particles/wetsplsh_local") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
            }},
            { "BlobElite", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/blob/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlob, emissive = true, emissiveMultiplier = 1f },
                new RecolorCreatureData("Visual/Point light") { isLight = true },
                new RecolorCreatureData("Visual/particles/ooz") { isParticle = true },
                new RecolorCreatureData("Visual/particles/wetsplsh") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
                new RecolorCreatureData("Visual/particles/wetsplsh_local") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
            }},
            { "Boar", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Poly Art Boar") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoar, emissive = true },
                new RecolorCreatureData("Visual/Fangs 001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true },
                new RecolorCreatureData("Visual/Fangs 002") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true },
                new RecolorCreatureData("Visual/Fangs 003") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true },
                new RecolorCreatureData("Visual/Fangs 004") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true },
                new RecolorCreatureData("Visual/Fangs 005") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true },
                new RecolorCreatureData("Visual/Fangs 006") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true },
            }},
            { "Draugr", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_draugr_base/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDraugr, emissive = true },
            }},
            { "Draugr_Ranged", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_draugr_base/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDraugrFem, emissive = true },
            }},
            { "Draugr_Elite", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_draugr_base/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDraugrElite, emissive = true },
            }},
            { "Ghost", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGhost, emissive = true },
                new RecolorCreatureData("Visual/LowerCloth") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGhost, emissive = true },
                new RecolorCreatureData("Visual/Point light") { isLight = true },
                new RecolorCreatureData("Visual/black_smoke") { isParticle = true },
                new RecolorCreatureData("Visual/black_smoke/SmallerSmoke") { isParticle = true },
                new RecolorCreatureData("Visual/Armature/Root/Hips/Spine1/Spine2/flare") { isParticle = true },
            }},
            { "Greyling", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true },
            }},
            { "Greydwarf", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true },
            }},
            { "Greydwarf_Shaman", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfShaman, emissive = true },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfShaman, emissive = true },
            }},
            { "Greydwarf_Elite", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true },
                new RecolorCreatureData("Visual/Armature.001/root/spine1/spine2/spine3/r_shoulder/r_arm1/r_arm2/r_hand/Rootsword/default") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfRootsword, emissive = true },
            }},
            { "Neck", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorNeck, emissive = true },
                new RecolorCreatureData("Visual/Lillies") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorNeck, emissive = true },
            }},
            { "Skeleton", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_skeleton_base/Skeleton") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSkeleton, emissive = true },
                new RecolorCreatureData("Visual/_skeleton_base/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_l") { disable = true },
                new RecolorCreatureData("Visual/_skeleton_base/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_r") { disable = true },
            }},
            { "Skeleton_Poison", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_skeleton_base/Skeleton") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSkeleton, emissive = true },
                new RecolorCreatureData("Visual/_skeleton_base/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_l") { disable = true },
                new RecolorCreatureData("Visual/_skeleton_base/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_r") { disable = true },
            }},
            { "Troll", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTroll, emissive = true },
                new RecolorCreatureData("Visual/Hair") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTroll, emissive = true },
            }},
            { "Wraith", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/wraith/wraith") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = true, zebraMaterial = WizshBoneTwitchIntegration.Instance.materials.RecolorWraithZebra },
                new RecolorCreatureData("Visual/wraith/chain") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = true },
                new RecolorCreatureData("Visual/wraith/RagPlanes") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = false },
                new RecolorCreatureData("Visual/wraith/Armature/root/spine1/spine2/spine3/neck/head/Point light") { isLight = true },
            }},
        };  

        public static void RecolorCreature(GameObject creature, string redeemerName)
        {
            Jotunn.Logger.LogWarning($"Found creature {creature.name} and {redeemerName} is very special!");
            Color color = GetColorByKey(redeemerName);
            List<RecolorCreatureData> list = GetRecolorCreatureDataByKey(creature.name.Replace("(Clone)", ""));

            foreach (RecolorCreatureData recolorCreatureData in list)
            {
                Transform transform = creature.transform.Find(recolorCreatureData.transformPath);

                if (transform == null)
                {
                    Jotunn.Logger.LogWarning($"Could not find transform to recolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                if (recolorCreatureData.disable)
                {
                    transform.gameObject.SetActive(false);
                    continue;
                }

                if (recolorCreatureData.isLight)
                {
                    Jotunn.Logger.LogWarning("Changing light...");
                    Light light = transform.GetComponent<Light>();
                    light.color = color;
                    continue;
                }

                if (recolorCreatureData.isParticle)
                {
                    Jotunn.Logger.LogWarning("Changing Particle...");
                    ParticleSystem particleSystem = transform.GetComponent<ParticleSystem>();

                    if (recolorCreatureData.material != null)
                    {
                        ParticleSystemRenderer particleRenderder = particleSystem.GetComponent<ParticleSystemRenderer>();
                        recolorCreatureData.material.FixReferences();
                        particleRenderder.materials = new Material[1] { recolorCreatureData.material };
                        Recolor(recolorCreatureData, color, particleRenderder.materials[0], recolorCreatureData.emissiveMultiplier);
                    }
                    else
                    {
                        ParticleSystem.MainModule main = particleSystem.main;
                        float h, s, v;

                        Color.RGBToHSV(color, out h, out s, out v);
                        Color newColor = Color.HSVToRGB(h, s, 0.2f);
                        main.startColor = newColor;
                    }

                    continue;
                }

                SkinnedMeshRenderer skinnedMeshRenderer = transform.gameObject.GetComponent<SkinnedMeshRenderer>();
                MeshRenderer meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();

                if (skinnedMeshRenderer == null && meshRenderer == null)
                {
                    Jotunn.Logger.LogWarning($"Could not find any mesh renderer to recolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                if (redeemerName == "yummyzebracakes" && recolorCreatureData.zebraMaterial != null)
                {
                    recolorCreatureData.zebraMaterial.FixReferences();

                    if (skinnedMeshRenderer)
                        skinnedMeshRenderer.materials = new Material[1] { recolorCreatureData.zebraMaterial };

                    else if (meshRenderer)
                        meshRenderer.materials = new Material[1] { recolorCreatureData.material };
                }
                else
                {
                    recolorCreatureData.material.FixReferences();

                    if (skinnedMeshRenderer)
                        skinnedMeshRenderer.materials = new Material[1] { recolorCreatureData.material };

                    else if (meshRenderer)
                        meshRenderer.materials = new Material[1] { recolorCreatureData.material };
                }

                // Get the material from the renderer as its now a copy
                Material mat = null;

                if (skinnedMeshRenderer)
                    mat = skinnedMeshRenderer.materials[0];

                else if (meshRenderer)
                    mat = meshRenderer.materials[0];

                if (mat == null)
                {
                    Jotunn.Logger.LogWarning($"Material is null for recolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                Transform visualTrans = creature.transform.Find("Visual");
                LevelEffects levelEffects = visualTrans.gameObject.GetComponent<LevelEffects>();

                if (levelEffects != null)
                    levelEffects.m_levelSetups.Clear();

                Recolor(recolorCreatureData, color, mat, recolorCreatureData.emissiveMultiplier);
            }
        }

        public static void Recolor(RecolorCreatureData recolorCreatureData, Color color, Material mat, float multiplier = 2f)
        {
            if (recolorCreatureData.emissive)
            {
                mat.SetColor("_EmissionColor", new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier));
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                mat.SetColor("_EmissiveColor", new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier));
                mat.SetColor("_FlowColor", color);
                mat.SetColor("_SSS", color);
            }

            mat.color = color;
        }

        public static Color GetColorByKey(string key)
        {
            return specialViewers.GetValueSafe(key.ToLower());
        }

        public static List<RecolorCreatureData> GetRecolorCreatureDataByKey(string key)
        {
            return creatureList.GetValueSafe(key);
        }

        public static bool IsCreatureInList(string prefabName)
        {
            return creatureList.ContainsKey(prefabName);
        }

        public static bool IsRedeemerSpecialViewer(string name)
        {
            return specialViewers.ContainsKey(name.ToLower());
        }
    }
}
