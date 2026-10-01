using HarmonyLib;
using Jotunn;
using Jotunn.Managers;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;
using static UnityEngine.ParticleSystem;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class RecolorHelper
    {
        public static List<ViewerEntry> m_viewers = LoadInitialViewers();

        // Set once the very first load (successful or self-healed) completes, so
        // ReloadViewersConfig can tell "m_viewers already holds a real load" apart from
        // "nothing has been loaded yet" - see ReloadViewersConfig for why that distinction
        // matters.
        private static bool m_hasLoadedOnce;

        private static List<ViewerEntry> LoadInitialViewers()
        {
            List<ViewerEntry> viewers = ExtraConfigHelper.ReadViewersConfig();
            m_hasLoadedOnce = true;
            return viewers;
        }

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
            { "Deathsquito", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeathsquito, emissive = true },
            }},
            { "Deer", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Deer 003") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeer, emissive = true },
                new RecolorCreatureData("Visual/CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 01") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeerAntlers, emissive = true },
                new RecolorCreatureData("Visual/CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 04") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeerAntlers, emissive = true },
                new RecolorCreatureData("Visual/CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 05") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeerAntlers, emissive = true },
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
            { "Gjall", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Gjall") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGjall, emissive = true },
                new RecolorCreatureData("Visual/Armature/Root/Hip/Effects") { isParticle = true },
                new RecolorCreatureData("Visual/Armature/Root/Hip/Point Light (1)") { isLight = true },
                new RecolorCreatureData("Visual/Armature/Root/Hip/Forehead/Point Light") { isLight = true },
            }},
            { "Goblin", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/goblin") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFuling, emissive = true },
                //new RecolorCreatureData() { isGear = true, material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingArmor, emissive = true },
            }},
            { "GoblinArcher", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/goblin") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFuling, emissive = true },
                //new RecolorCreatureData() { isGear = true, material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingArmor, emissive = true },
            }},
            { "GoblinBrute", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/GoblinBrute") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingBrute, emissive = true },
            }},
            { "GoblinShaman", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Shaman") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingShaman, emissive = true },
                //new RecolorCreatureData("Visual/Shaman") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingShamanCape, materialIndex = 1, emissive = true },
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
            { "Seeker", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Plane.004") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSeeker, emissive = true },
            }},
            { "SeekerBrute", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/SeekerBrute") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSeekerBrute, emissive = true },
            }},
            { "SeekerBrood", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Plane.004") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSeekerBrood, emissive = true },
                new RecolorCreatureData("Point Light") { isLight = true },
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
            { "Tick", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/FeastingMesh") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTick, emissive = true },
            }},
            { "Troll", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTroll, emissive = true },
                new RecolorCreatureData("Visual/Hair") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTroll, emissive = true },
            }},
            { "Ulv", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Ulv/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFenring, emissive = true },
            }},
            { "Unbjorn", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Asora") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = false, zebraMaterial = WizshBoneTwitchIntegration.Instance.materials.RecolorBjornZebra },
                new RecolorCreatureData("Visual/Plane") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = false },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true },
                new RecolorCreatureData("Visual/Cube.002") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true },
                new RecolorCreatureData("Visual/Cube.002 (1)") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true },
                new RecolorCreatureData("Visual/Sphere") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true },
                new RecolorCreatureData("Visual/Sphere (1)") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true },
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

        // Gear (helmet/chest/legs/shoulder) is instantiated at runtime by VisEquipment and has no
        // counterpart on the static prefab for UnColorGear to read an "original" material from -
        // cache each renderer's pre-swap materials here (keyed by instance ID rather than the
        // Renderer itself, to avoid UnityEngine.Object's overridden equality as a dictionary key).
        private static readonly Dictionary<int, Material[]> m_originalGearMaterials = new Dictionary<int, Material[]>();

        public static IEnumerator RecolorCreatureAfterDelay(string redeemerName, GameObject creature, float delay)
        {
            yield return new WaitForSeconds(delay);

            RecolorCreature(redeemerName, creature);
        }

        public static void RecolorCreature(string redeemerName, GameObject creature, string colorOverride = null, bool forceColor = false)
        {
            ViewerEntry viewerEntry = GetViewer(redeemerName);

            Color resolvedColor;

            if (forceColor && colorOverride != null && ColorUtility.TryParseHtmlString(colorOverride, out Color parsedForceOverride))
            {
                resolvedColor = parsedForceOverride;
            }
            else if (viewerEntry != null)
            {
                viewerEntry.Init();
                resolvedColor = viewerEntry.parsedColor1;
            }
            else if (colorOverride != null && ColorUtility.TryParseHtmlString(colorOverride, out Color parsedOverride))
            {
                resolvedColor = parsedOverride;
            }
            else
            {
                return;
            }

            List<RecolorCreatureData> list = GetRecolorCreatureDataByKey(creature.name);

            foreach (RecolorCreatureData recolorCreatureData in list)
            {
                if (recolorCreatureData.isGear)
                {
                    RecolorGear(creature, recolorCreatureData, resolvedColor);
                    continue;
                }

                Transform transform = creature.transform.Find(recolorCreatureData.transformPath);

                if (transform == null)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Could not find transform to recolor creature {creature.name}: {recolorCreatureData.transformPath}");
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
                    Light light = transform.gameObject.GetComponent<Light>();
                    light.color = resolvedColor;
                    continue;
                }

                if (recolorCreatureData.isParticle)
                {
                    ParticleSystem particleSystem = transform.gameObject.GetComponent<ParticleSystem>();

                    if (recolorCreatureData.material != null)
                    {
                        ParticleSystemRenderer particleRenderder = particleSystem.gameObject.GetComponent<ParticleSystemRenderer>();
                        recolorCreatureData.material.FixReferences();
                        particleRenderder.materials = new Material[1] { recolorCreatureData.material };
                        Recolor(recolorCreatureData, resolvedColor, particleRenderder.materials[0], recolorCreatureData.emissiveMultiplier);
                    }
                    else
                    {
                        ParticleSystem.MainModule main = particleSystem.main;
                        float h, s, v;

                        Color.RGBToHSV(resolvedColor, out h, out s, out v);
                        Color newColor = Color.HSVToRGB(h, s, recolorCreatureData.particleAlpha);
                        main.startColor = newColor;
                    }

                    if (recolorCreatureData.isSurtling)
                    {
                        CustomDataModule customData = particleSystem.customData;
                        MinMaxGradient gradient1 = customData.GetColor(ParticleSystemCustomData.Custom1);
                        MinMaxGradient gradient2 = customData.GetColor(ParticleSystemCustomData.Custom2);

                        GradientColorKey[] keys1 = gradient1.gradient.colorKeys;
                        for (int i = 0; i < keys1.Length; i++)
                        {
                            float decrease = 1f - (0.15f * i);
                            keys1[i].color = resolvedColor * decrease;
                        }

                        GradientColorKey[] keys2 = gradient2.gradient.colorKeys;
                        for (int i = 0; i < keys2.Length; i++)
                        {
                            float decrease = 1f - (0.15f * i);
                            keys2[i].color = resolvedColor * decrease;
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
                    continue;
                }

                Transform visualTrans = creature.transform.Find("Visual");
                LevelEffects levelEffects = visualTrans.gameObject.GetComponent<LevelEffects>();

                // Neutralize the hue/saturation/value/emissive shift each LevelSetup would otherwise
                // apply to m_mainRender on (re)level - that's vanilla's own star-tint, and it would
                // silently override the material tint Recolor() sets below whenever LevelEffects.Start()
                // (Unity's Start() order between sibling components on the same GameObject is
                // undefined) runs after this, or a level gets re-set later. Deliberately NOT clearing
                // the list outright: each LevelSetup's m_enableObject/m_baseEnableObject also drives a
                // real per-star mesh swap (e.g. Deer's bigger 1-/2-star antlers) - wiping the whole list
                // left LevelEffects.SetupLevelVisualization() with nothing to look up, so that swap
                // silently never ran and star deer ended up with their antlers stuck off.
                if (levelEffects != null)
                {
                    foreach (LevelEffects.LevelSetup levelSetup in levelEffects.m_levelSetups)
                    {
                        levelSetup.m_hue = 0f;
                        levelSetup.m_saturation = 0f;
                        levelSetup.m_value = 0f;
                        levelSetup.m_setEmissiveColor = false;
                    }
                }

                Recolor(recolorCreatureData, resolvedColor, mat, recolorCreatureData.emissiveMultiplier);
            }
        }

        public static void UnColorCreature(GameObject creature)
        {
            List<RecolorCreatureData> list = GetRecolorCreatureDataByKey(creature.name);
            GameObject original = PrefabManager.Instance.GetPrefab(creature.name.Replace("(Clone)", ""));

            if (original == null)
            {
                return;
            }

            foreach (RecolorCreatureData recolorCreatureData in list)
            {
                if (recolorCreatureData.isGear)
                {
                    UnColorGear(creature);
                    continue;
                }

                Transform transform = creature.transform.Find(recolorCreatureData.transformPath);
                Transform transformOriginal = original.transform.Find(recolorCreatureData.transformPath);

                if (transform == null || transformOriginal == null)
                {
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
                    Light light = transform.gameObject.GetComponent<Light>();
                    Light lightOrignal = transformOriginal.gameObject.GetComponent<Light>();
                    light.color = lightOrignal.color;
                    continue;
                }

                if (recolorCreatureData.isParticle)
                {
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
                    continue;
                }

                SkinnedMeshRenderer skinnedMeshRendererOriginal = transformOriginal.gameObject.GetComponent<SkinnedMeshRenderer>();
                MeshRenderer meshRendererOriginal = transformOriginal.gameObject.GetComponent<MeshRenderer>();

                if (skinnedMeshRendererOriginal == null && meshRendererOriginal == null)
                {
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

        private static List<GameObject> GetArmorGearInstances(VisEquipment visEquipment)
        {
            List<GameObject> instances = new List<GameObject>();

            if (visEquipment.m_helmetItemInstance != null)
                instances.Add(visEquipment.m_helmetItemInstance);
            if (visEquipment.m_chestItemInstances != null)
                instances.AddRange(visEquipment.m_chestItemInstances);
            if (visEquipment.m_legItemInstances != null)
                instances.AddRange(visEquipment.m_legItemInstances);
            if (visEquipment.m_shoulderItemInstances != null)
                instances.AddRange(visEquipment.m_shoulderItemInstances);

            return instances;
        }

        private static void RecolorGear(GameObject creature, RecolorCreatureData recolorCreatureData, Color color)
        {
            VisEquipment visEquipment = creature.GetComponent<VisEquipment>();

            if (visEquipment == null)
                return;

            foreach (GameObject gearInstance in GetArmorGearInstances(visEquipment))
            {
                foreach (Renderer renderer in gearInstance.GetComponentsInChildren<Renderer>())
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                    MeshRenderer meshRenderer = renderer as MeshRenderer;

                    if (skinnedMeshRenderer == null && meshRenderer == null)
                        continue;

                    int instanceId = renderer.GetInstanceID();

                    if (!m_originalGearMaterials.ContainsKey(instanceId))
                        m_originalGearMaterials[instanceId] = renderer.materials; // renderer.materials getter already returns a fresh copy

                    int materialCount = m_originalGearMaterials[instanceId].Length;

                    for (int i = 0; i < materialCount; i++)
                    {
                        SetupMaterials(skinnedMeshRenderer, meshRenderer, recolorCreatureData.material, i);
                        Material mat = skinnedMeshRenderer != null ? skinnedMeshRenderer.materials[i] : meshRenderer.materials[i];
                        Recolor(recolorCreatureData, color, mat, recolorCreatureData.emissiveMultiplier);
                    }
                }
            }
        }

        private static void UnColorGear(GameObject creature)
        {
            VisEquipment visEquipment = creature.GetComponent<VisEquipment>();

            if (visEquipment == null)
                return;

            foreach (GameObject gearInstance in GetArmorGearInstances(visEquipment))
            {
                foreach (Renderer renderer in gearInstance.GetComponentsInChildren<Renderer>())
                {
                    int instanceId = renderer.GetInstanceID();

                    if (m_originalGearMaterials.TryGetValue(instanceId, out Material[] originalMaterials))
                    {
                        renderer.materials = originalMaterials;
                        m_originalGearMaterials.Remove(instanceId);
                    }
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

        // Rocky (the buildable stone with a face) is a Pet piece, not a creature - no Character, no
        // "Visual" child, and no entry in creatureList - so it can't use the per-prefab transform
        // table above. Detected by component instead of prefab name so it keeps working whichever
        // prefab variant carries Pet.
        public static bool IsPetRock(GameObject obj)
        {
            return obj != null && obj.GetComponent<Pet>() != null;
        }

        public static bool CanRecolorPetRock(string name, GameObject obj)
        {
            return IsPetRock(obj) && IsRedeemerSpecialViewer(name);
        }

        // Suffix on the per-instance tinted material copies, so a re-tint/un-tint can tell its own
        // copies apart from the shared prefab materials (which must never be destroyed or modified).
        private const string PetRockTintSuffix = " (WBTI tint)";

        // How far (0-1) the viewer's color is blended toward white before tinting a Rocky. Raise for
        // brighter/paler, lower for a more saturated/darker result.
        private const float PetRockBrighten = 0.35f;

        // Strength of the emission layer (tint color x main texture x this). 0 = no glow.
        private const float PetRockEmission = 1.4f;

        // A Rocky's whole mesh is a single material slot whose material IS the face - Pet.SetFace /
        // MaterialVariation swap that slot between a list of shared per-face materials. Tinting the
        // slot's current material would be undone by the next face change (and tinting the shared
        // asset would recolor every Rocky in the world), so instead this replaces every entry in THIS
        // instance's MaterialVariation list with a tinted copy: whatever face is picked from then on
        // is already tinted, with no per-frame re-tinting needed. The current face is swapped in
        // directly. Any other (non-face) material slot on the rock is tinted in place. Renderers under
        // an ItemStand attach point are skipped, so an item displayed on the Rocky isn't tinted.
        public static void RecolorPetRock(string redeemerName, GameObject rock)
        {
            ViewerEntry viewerEntry = GetViewer(redeemerName);

            if (viewerEntry == null)
                return;

            viewerEntry.Init();
            // The Standard shader multiplies this into the rock's grey albedo texture, so the raw viewer
            // color ends up noticeably darker than it looks in the viewers list - blend it toward white.
            Color color = Color.Lerp(viewerEntry.parsedColor1, Color.white, PetRockBrighten);
            GameObject original = PrefabManager.Instance.GetPrefab(rock.name.Replace("(Clone)", ""));
            int tinted = 0;

            foreach (MaterialVariation variation in rock.GetComponentsInChildren<MaterialVariation>(true))
            {
                MaterialVariation originalVariation = GetOriginalPetRockVariation(original, rock, variation);

                for (int i = 0; i < variation.m_materials.Count; i++)
                {
                    MaterialVariation.MaterialEntry entry = variation.m_materials[i];

                    // Always copy from the prefab's untouched material so re-claiming never stacks tints.
                    Material source = originalVariation != null && i < originalVariation.m_materials.Count
                        ? originalVariation.m_materials[i].m_material
                        : entry.m_material;

                    if (source == null || !source.HasProperty("_Color"))
                        continue;

                    DestroyPetRockTintCopy(entry.m_material);

                    Material copy = new Material(source) { name = source.name + PetRockTintSuffix };
                    ApplyPetRockTint(copy, color);
                    entry.m_material = copy;
                    tinted++;
                }

                ApplyPetRockCurrentFace(variation);
            }

            foreach (Renderer renderer in GetPetRockRenderers(rock))
            {
                Material[] materials = renderer.materials;

                for (int i = 0; i < materials.Length; i++)
                {
                    if (IsPetRockFaceSlot(renderer, i) || materials[i] == null || !materials[i].HasProperty("_Color"))
                        continue;

                    ApplyPetRockTint(materials[i], color);
                    tinted++;
                }
            }

            if (tinted == 0)
                Jotunn.Logger.LogWarning($"[WBTI] Rocky recolor for {redeemerName} found nothing to tint on {rock.name}");
        }

        public static void UnColorPetRock(GameObject rock)
        {
            GameObject original = PrefabManager.Instance.GetPrefab(rock.name.Replace("(Clone)", ""));

            if (original == null)
                return;

            foreach (MaterialVariation variation in rock.GetComponentsInChildren<MaterialVariation>(true))
            {
                MaterialVariation originalVariation = GetOriginalPetRockVariation(original, rock, variation);

                if (originalVariation == null)
                    continue;

                for (int i = 0; i < variation.m_materials.Count && i < originalVariation.m_materials.Count; i++)
                {
                    DestroyPetRockTintCopy(variation.m_materials[i].m_material);
                    variation.m_materials[i].m_material = originalVariation.m_materials[i].m_material;
                }

                ApplyPetRockCurrentFace(variation);
            }

            foreach (Renderer renderer in GetPetRockRenderers(rock))
            {
                string path = GetRelativePath(rock.transform, renderer.transform);
                Transform originalTransform = path == "" ? original.transform : original.transform.Find(path);
                Renderer originalRenderer = originalTransform != null ? originalTransform.GetComponent<Renderer>() : null;

                if (originalRenderer == null)
                    continue;

                Material[] materials = renderer.materials;
                Material[] originalMaterials = originalRenderer.sharedMaterials;

                for (int i = 0; i < materials.Length && i < originalMaterials.Length; i++)
                {
                    if (IsPetRockFaceSlot(renderer, i) || materials[i] == null || originalMaterials[i] == null)
                        continue;

                    if (materials[i].HasProperty("_Color") && originalMaterials[i].HasProperty("_Color"))
                        materials[i].color = originalMaterials[i].color;
                }
            }
        }

        // Albedo tint plus an emission layer that reuses the material's own main texture (same
        // pattern, so the face detail still reads) - light-colored areas glow in the viewer's color,
        // fully dark pixels (eyes/mouth) emit nothing.
        private static void ApplyPetRockTint(Material material, Color color)
        {
            material.color = color;

            Texture mainTexture = material.mainTexture;

            if (mainTexture == null || !material.HasProperty("_EmissionColor"))
                return;

            material.SetTexture("_EmissionMap", mainTexture);
            material.SetColor("_EmissionColor", color * PetRockEmission);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

        // MaterialVariation.UpdateMaterial is private and only re-runs for a face change - after
        // swapping the list entries, put the face currently shown into the renderer slot directly
        // (sharedMaterials, so no extra material instances are created). GetMaterial() is -1 until
        // MaterialVariation has picked a face; its own CheckMaterial then uses the swapped list.
        private static void ApplyPetRockCurrentFace(MaterialVariation variation)
        {
            int face = variation.GetMaterial();
            Renderer renderer = variation.GetComponent<Renderer>();

            if (renderer == null || face < 0 || face >= variation.m_materials.Count)
                return;

            Material[] materials = renderer.sharedMaterials;

            if (variation.m_materialIndex >= materials.Length)
                return;

            materials[variation.m_materialIndex] = variation.m_materials[face].m_material;
            renderer.sharedMaterials = materials;
        }

        private static MaterialVariation GetOriginalPetRockVariation(GameObject original, GameObject rock, MaterialVariation variation)
        {
            if (original == null)
                return null;

            string path = GetRelativePath(rock.transform, variation.transform);
            Transform originalTransform = path == "" ? original.transform : original.transform.Find(path);

            return originalTransform != null ? originalTransform.GetComponent<MaterialVariation>() : null;
        }

        private static void DestroyPetRockTintCopy(Material material)
        {
            if (material != null && material.name.EndsWith(PetRockTintSuffix))
                UnityEngine.Object.Destroy(material);
        }

        private static List<Renderer> GetPetRockRenderers(GameObject rock)
        {
            ItemStand itemStand = rock.GetComponent<ItemStand>();
            Transform attach = itemStand != null ? itemStand.m_attachOther : null;
            List<Renderer> renderers = new List<Renderer>();

            foreach (Renderer renderer in rock.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer))
                    continue;

                if (attach != null && renderer.transform.IsChildOf(attach))
                    continue;

                renderers.Add(renderer);
            }

            return renderers;
        }

        private static bool IsPetRockFaceSlot(Renderer renderer, int slot)
        {
            foreach (MaterialVariation variation in renderer.GetComponents<MaterialVariation>())
            {
                if (variation.m_materialIndex == slot)
                    return true;
            }

            return false;
        }

        // Path of 'child' relative to 'root' in Transform.Find format ("" if they're the same object).
        private static string GetRelativePath(Transform root, Transform child)
        {
            if (child == root)
                return "";

            string path = child.name;

            for (Transform parent = child.parent; parent != null && parent != root; parent = parent.parent)
                path = parent.name + "/" + path;

            return path;
        }

        public static void ReloadViewersConfig()
        {
            // The viewers file went missing after we already had it loaded once - the in-memory
            // list is more trustworthy than the embedded stock template, so re-save what's
            // actually loaded instead of silently reverting to defaults. ReadViewersConfig()'s
            // own self-heal (falling back to the stock template) only kicks in below for the
            // very first load, when there's nothing in memory yet to fall back to.
            if (m_hasLoadedOnce && !File.Exists(WizshBoneTwitchIntegration.viewersPath))
            {
                Jotunn.Logger.LogWarning($"[WBTI] Viewers file at '{WizshBoneTwitchIntegration.viewersPath}' is missing - restoring it from the currently loaded viewers instead of resetting to defaults.");
                ExtraConfigHelper.WriteViewersConfig(m_viewers);
                return;
            }

            m_viewers = ExtraConfigHelper.ReadViewersConfig();
            m_hasLoadedOnce = true;
        }

        public static bool CanRecolorCreature(string name, string creature, string colorOverride = null)
        {
            return IsCreatureInList(creature) && (IsRedeemerSpecialViewer(name) || colorOverride != null);
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
