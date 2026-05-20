using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using static UnityEngine.ParticleSystem;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class RecolorHelper
    {
        public static List<ViewerEntry> m_viewers = ExtraConfigHelper.ReadViewersConfig();
        public static Dictionary<string, List<RecolorCreatureData>> creatureList = new Dictionary<string, List<RecolorCreatureData>>()
        {
            { "Abomination", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Abomination") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorAbomination, emissive = true},
            }},
            { "Bat", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/CaveBat/Bat") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBat, emissive = true},
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
            { "Draugr_Elite", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_draugr_base/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDraugrElite, emissive = true },
            }},
            { "Draugr_Ranged", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_draugr_base/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDraugrFem, emissive = true },
            }},
            { "Fenring", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFenring, emissive = true },
            }},
            { "Fenring_Cultist", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorCultist, emissive = true },
                new RecolorCreatureData("Visual/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorCultistCape, materialIndex = 1, emissive = true },
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
            { "Greydwarf_Elite", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true },
                new RecolorCreatureData("Visual/Armature.001/root/spine1/spine2/spine3/r_shoulder/r_arm1/r_arm2/r_hand/Rootsword/default") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfRootsword, emissive = true },
            }},
            { "Greydwarf_Shaman", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfShaman, emissive = true },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfShaman, emissive = true },
            }},
            { "Hatchling", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Hatchling_mountain/Hatchling") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorHatchling, emissive = true },
                new RecolorCreatureData("Visual/Hatchling_mountain/Hatchling.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorHatchling, emissive = true },
                new RecolorCreatureData("Visual/Hatchling_mountain/Horns") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorHatchling, emissive = true },
                new RecolorCreatureData("Visual/Hatchling_mountain/eyes") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorHatchling, emissive = true },
            }},
            { "Leech", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorLeech, emissive = true },
            }},
            { "Lox", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/offset/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorLox, emissive = true },
                new RecolorCreatureData("Visual/offset/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorLox, materialIndex = 1, emissive = true },
                new RecolorCreatureData("Visual/offset/Furr1") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorLox, emissive = true },
            }},
            { "Neck", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorNeck, emissive = true },
                new RecolorCreatureData("Visual/Lillies") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorNeck, emissive = true },
            }},
            { "Serpent", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Serpant") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSerpent, emissive = true },
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
            { "StoneGolem", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorStoneGolem, emissive = true },
                new RecolorCreatureData("Visual/attach_skin(Clone)/Cube.000") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorStoneGolemClubs, emissive = true },
                new RecolorCreatureData("Visual/attach_skin(Clone)/Mountain_Golem_Spike_Arm/Cube.003") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorStoneGolem, emissive = true },
                new RecolorCreatureData("Visual/attach_skin(Clone)/Mountain_Golem_dualSledge/Cube.002") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorStoneGolemClubs, emissive = true },
            }},
            { "Surtling", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/model/Kakari") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSurtling, emissive = true, emissiveMultiplier = 10f },
                new RecolorCreatureData("Point light") { isLight = true },
                new RecolorCreatureData("Visual/flames") { disable = true },
                new RecolorCreatureData("Visual/flames_world") { disable = true },
                new RecolorCreatureData("Visual/flames_world_Purple") { enable = true, isSurtling = true, isParticle = true, particleAlpha = 1f},
                new RecolorCreatureData("Visual/flames_world_Purple/Purple Sparks") { enable = true, isParticle = true, particleAlpha = 1f },
            }},
            { "Troll", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTroll, emissive = true },
                new RecolorCreatureData("Visual/Hair") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTroll, emissive = true },
            }},
            { "Ulv", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Ulv/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFenring, emissive = true },
            }},
            { "Wolf", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/WolfSmooth/M") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWolf, emissive = true },
            }},
            { "Wraith", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/wraith/wraith") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = true, zebraMaterial = WizshBoneTwitchIntegration.Instance.materials.RecolorWraithZebra },
                new RecolorCreatureData("Visual/wraith/chain") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = true },
                new RecolorCreatureData("Visual/wraith/RagPlanes") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = false },
                new RecolorCreatureData("Visual/wraith/Armature/root/spine1/spine2/spine3/neck/head/Point light") { isLight = true },
            }},
        };

        public static IEnumerator RecolorCreatureAfterDelay(string redeemerName, GameObject creature, float delay)
        {
            yield return new WaitForSeconds(delay);

            RecolorCreature(redeemerName, creature);
        }

        public static void RecolorCreature(string redeemerName, GameObject creature, string colorOverride = null)
        {
            ViewerEntry viewerEntry = GetViewer(redeemerName);

            Color resolvedColor;

            if (colorOverride != null && ColorUtility.TryParseHtmlString(colorOverride, out Color parsedOverride))
            {
                resolvedColor = parsedOverride;
            }
            else if (viewerEntry != null)
            {
                viewerEntry.Init();
                resolvedColor = viewerEntry.parsedColor1;
            }
            else
            {
                return;
            }

            List<RecolorCreatureData> list = GetRecolorCreatureDataByKey(creature.name);

            foreach (RecolorCreatureData recolorCreatureData in list)
            {
                Transform transform = creature.transform.Find(recolorCreatureData.transformPath);

                if (transform == null)
                {
                    //Jotunn.Logger.LogWarning($"Could not find transform to recolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                if (recolorCreatureData.disable)
                {
                    transform.gameObject.SetActive(false);
                    continue;
                }

                if (recolorCreatureData.enable)
                {
                    transform.gameObject.SetActive(true);
                }

                if (recolorCreatureData.isLight)
                {
                    //Jotunn.Logger.LogWarning("Changing light...");
                    Light light = transform.gameObject.GetComponent<Light>();
                    light.color = viewerEntry.parsedColor1;
                    continue;
                }

                if (recolorCreatureData.isParticle)
                {
                    //Jotunn.Logger.LogWarning("Changing Particle...");
                    ParticleSystem particleSystem = transform.gameObject.GetComponent<ParticleSystem>();

                    if (recolorCreatureData.material != null)
                    {
                        ParticleSystemRenderer particleRenderder = particleSystem.gameObject.GetComponent<ParticleSystemRenderer>();
                        recolorCreatureData.material.FixReferences();
                        particleRenderder.materials = new Material[1] { recolorCreatureData.material };
                        Recolor(recolorCreatureData, viewerEntry.parsedColor1, particleRenderder.materials[0], recolorCreatureData.emissiveMultiplier);
                    }
                    else
                    {
                        ParticleSystem.MainModule main = particleSystem.main;
                        float h, s, v;

                        Color.RGBToHSV(viewerEntry.parsedColor1, out h, out s, out v);
                        Color newColor = Color.HSVToRGB(h, s, recolorCreatureData.particleAlpha);
                        main.startColor = newColor;
                    }

                    if (recolorCreatureData.isSurtling)
                    {
                        CustomDataModule customData = particleSystem.customData;
                        //Jotunn.Logger.LogWarning("Color1: " + customData.GetMode(ParticleSystemCustomData.Custom1));
                        //Jotunn.Logger.LogWarning("Color2: " + customData.GetMode(ParticleSystemCustomData.Custom2));
                        MinMaxGradient gradient1 = customData.GetColor(ParticleSystemCustomData.Custom1);
                        MinMaxGradient gradient2 = customData.GetColor(ParticleSystemCustomData.Custom2);

                        GradientColorKey[] keys1 = gradient1.gradient.colorKeys;
                        for (int i = 0; i < keys1.Length; i++)
                        {
                            float decrease = 1f - (0.15f * i);
                            keys1[i].color = viewerEntry.parsedColor1 * decrease;
                        }

                        GradientColorKey[] keys2 = gradient2.gradient.colorKeys;
                        for (int i = 0; i < keys2.Length; i++)
                        {
                            float decrease = 1f - (0.15f * i);
                            keys2[i].color = viewerEntry.parsedColor1 * decrease;
                        }

                        gradient1.gradient.colorKeys = keys1;
                        gradient2.gradient.colorKeys = keys2;
                    }

                    continue;
                }

                SkinnedMeshRenderer skinnedMeshRenderer = transform.gameObject.GetComponent<SkinnedMeshRenderer>();
                MeshRenderer meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();

                if (skinnedMeshRenderer == null && meshRenderer == null)
                {
                    //Jotunn.Logger.LogWarning($"Could not find any mesh renderer to recolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                if (redeemerName == "yummyzebracakes" && recolorCreatureData.zebraMaterial != null)
                {
                    SetupMaterials(skinnedMeshRenderer, meshRenderer, recolorCreatureData.zebraMaterial, recolorCreatureData.materialIndex);
                }
                else
                {
                    SetupMaterials(skinnedMeshRenderer, meshRenderer, recolorCreatureData.material, recolorCreatureData.materialIndex);
                }

                // Get the material from the renderer as its now a copy
                Material mat = null;

                if (skinnedMeshRenderer)
                    mat = skinnedMeshRenderer.materials[recolorCreatureData.materialIndex];

                else if (meshRenderer)
                    mat = meshRenderer.materials[recolorCreatureData.materialIndex];

                if (mat == null)
                {
                    //Jotunn.Logger.LogWarning($"Material is null for recolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                Transform visualTrans = creature.transform.Find("Visual");
                LevelEffects levelEffects = visualTrans.gameObject.GetComponent<LevelEffects>();

                if (levelEffects != null)
                    levelEffects.m_levelSetups.Clear();

                Recolor(recolorCreatureData, resolvedColor, mat, recolorCreatureData.emissiveMultiplier);
            }
        }

        public static void UnColorCreature(GameObject creature)
        {
            //Jotunn.Logger.LogWarning($"Uncolor Creature: {creature.name}");
            List<RecolorCreatureData> list = GetRecolorCreatureDataByKey(creature.name);
            GameObject original = PrefabManager.Instance.GetPrefab(creature.name.Replace("(Clone)", ""));
            //Jotunn.Logger.LogWarning("Original found? " + original != null);

            if (original == null)
            {
                //Jotunn.Logger.LogWarning("Could not find original to uncolor!");
                return;
            }

            foreach (RecolorCreatureData recolorCreatureData in list)
            {
                Transform transform = creature.transform.Find(recolorCreatureData.transformPath);
                Transform transformOriginal = original.transform.Find(recolorCreatureData.transformPath);

                if (transform == null || transformOriginal == null)
                {
                    //Jotunn.Logger.LogWarning($"Could not find transform to uncolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                if (recolorCreatureData.disable)
                {
                    transform.gameObject.SetActive(true);
                    continue;
                }

                if (recolorCreatureData.enable)
                {
                    transform.gameObject.SetActive(false);
                }

                if (recolorCreatureData.isLight)
                {
                    //Jotunn.Logger.LogWarning("Changing light...");
                    Light light = transform.gameObject.GetComponent<Light>();
                    Light lightOrignal = transformOriginal.gameObject.GetComponent<Light>();
                    light.color = lightOrignal.color;
                    continue;
                }

                if (recolorCreatureData.isParticle)
                {
                    //Jotunn.Logger.LogWarning("Changing Particle...");
                    ParticleSystem particleSystem = transform.gameObject.GetComponent<ParticleSystem>();
                    ParticleSystem particleSystemOriginal = transformOriginal.gameObject.GetComponent<ParticleSystem>();

                    if (recolorCreatureData.material != null)
                    {
                        ParticleSystemRenderer particleRenderder = particleSystem.gameObject.GetComponent<ParticleSystemRenderer>();
                        ParticleSystemRenderer particleRenderderOriginal = particleSystemOriginal.gameObject.GetComponent<ParticleSystemRenderer>();
                        particleRenderder.materials = new Material[1] { particleRenderderOriginal.material };
                    }
                    else
                    {
                        ParticleSystem.MainModule main = particleSystem.main;
                        ParticleSystem.MainModule mainOriginal = particleSystemOriginal.main;
                        main.startColor = mainOriginal.startColor;
                    }

                    continue;
                }

                SkinnedMeshRenderer skinnedMeshRenderer = transform.gameObject.GetComponent<SkinnedMeshRenderer>();
                MeshRenderer meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();

                if (skinnedMeshRenderer == null && meshRenderer == null)
                {
                    //Jotunn.Logger.LogWarning($"Could not find any mesh renderer to uncolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                SkinnedMeshRenderer skinnedMeshRendererOriginal = transformOriginal.gameObject.GetComponent<SkinnedMeshRenderer>();
                MeshRenderer meshRendererOriginal = transformOriginal.gameObject.GetComponent<MeshRenderer>();

                if (skinnedMeshRendererOriginal == null && meshRendererOriginal == null)
                {
                    //Jotunn.Logger.LogWarning($"Could not find any original mesh renderer to uncolor creature: {recolorCreatureData.transformPath}");
                    continue;
                }

                if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.materials = new Material[1] { skinnedMeshRendererOriginal.materials[0] };
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.materials = new Material[1] { meshRendererOriginal.materials[0] };
                }

                Transform visualTrans = creature.transform.Find("Visual");
                LevelEffects levelEffects = visualTrans.gameObject.GetComponent<LevelEffects>();

                if (levelEffects != null)
                {
                    Transform visualTransOriginal = original.transform.Find("Visual");
                    LevelEffects levelEffectsOriginal = visualTransOriginal.gameObject.GetComponent<LevelEffects>();

                    if (levelEffectsOriginal != null)
                        levelEffects.m_levelSetups = levelEffectsOriginal.m_levelSetups;
                }
            }
        }

        public static void SetupMaterials(SkinnedMeshRenderer skinnedMeshRenderer, MeshRenderer meshRenderer, Material material, int index)
        {
            List<Material> materials = new List<Material>();
            material.FixReferences();

            if (skinnedMeshRenderer != null)
            {
                materials = skinnedMeshRenderer.materials.ToList();
                materials[index] = material;
                skinnedMeshRenderer.materials = materials.ToArray();
            }
            else if (meshRenderer != null)
            {
                materials = meshRenderer.materials.ToList();
                materials[index] = material;
                meshRenderer.materials = materials.ToArray();
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

        public static void ReloadViewersConfig()
        {
            m_viewers = ExtraConfigHelper.ReadViewersConfig();
        }

        public static bool CanRecolorCreature(string name, string creature)
        {
            return IsRedeemerSpecialViewer(name) && IsCreatureInList(creature);
        }

        public static bool IsRedeemerSpecialViewer(string name)
        {
            return m_viewers.Find(item => item.name == name.ToLower()) != null;
        }

        public static bool IsCreatureInList(string prefabName)
        {
            return creatureList.ContainsKey(prefabName.Replace("(Clone)", ""));
        }

        public static List<RecolorCreatureData> GetRecolorCreatureDataByKey(string key)
        {
            return creatureList.GetValueSafe(key.Replace("(Clone)", ""));
        }

        public static ViewerEntry GetViewer(string name)
        {
            return m_viewers.Find(item => item.name == name.ToLower());
        }
    }
}
