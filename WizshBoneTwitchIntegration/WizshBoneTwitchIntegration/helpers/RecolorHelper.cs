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

        // Every skeleton variant recolors the same way, so they all share this one list (it is only
        // ever read). Declared above creatureList: static fields initialize in file order.
        private static readonly List<RecolorCreatureData> SkeletonRecolor = new List<RecolorCreatureData>()
        {
            new RecolorCreatureData("Visual/_skeleton_base/Skeleton") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSkeleton, emissive = true },
            new RecolorCreatureData("Visual/_skeleton_base/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_l") { disable = true },
            new RecolorCreatureData("Visual/_skeleton_base/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_r") { disable = true },
        };

        // Both bat variants recolor the same way and share this list (it is only ever read).
        private static readonly List<RecolorCreatureData> BatRecolor = new List<RecolorCreatureData>()
        {
            new RecolorCreatureData("Visual/CaveBat/Bat") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBat, emissive = true },
        };

        // Blob and BlobElite recolor the same way and share this list (it is only ever read).
        private static readonly List<RecolorCreatureData> BlobRecolor = new List<RecolorCreatureData>()
        {
            new RecolorCreatureData("Visual/blob/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlob, emissive = true, emissiveMultiplier = 1f },
            new RecolorCreatureData("Visual/Point light") { isLight = true },
            new RecolorCreatureData("Visual/particles/ooz") { isParticle = true },
            new RecolorCreatureData("Visual/particles/wetsplsh") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
            new RecolorCreatureData("Visual/particles/wetsplsh_local") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
        };

        // Goblin and GoblinArcher recolor the same way and share this list (it is only ever read).
        private static readonly List<RecolorCreatureData> GoblinRecolor = new List<RecolorCreatureData>()
        {
            new RecolorCreatureData("Visual/goblin") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFuling, emissive = true },
            //new RecolorCreatureData() { isGear = true, material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingArmor, emissive = true },
        };

        public static Dictionary<string, List<RecolorCreatureData>> creatureList = new Dictionary<string, List<RecolorCreatureData>>()
        {
            { "Abomination", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Abomination") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorAbomination, emissive = true, ragdollPath = "Abomination/Abomination"},
            }},
            { "Bat", BatRecolor },
            { "Bat_Swamp", BatRecolor },
            { "Bjorn", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Asora") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = false, zebraMaterial = WizshBoneTwitchIntegration.Instance.materials.RecolorBjornZebra, ragdollPath = "Asora" },
                new RecolorCreatureData("Visual/Plane") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = false, ragdollPath = "Plane" },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = true, ragdollPath = "Cube.001" },
                new RecolorCreatureData("Visual/Cube.002") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = true, ragdollPath = "Cube.002" },
                new RecolorCreatureData("Visual/Sphere") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBjorn, emissive = true, ragdollPath = "Sphere" },
            }},
            { "Blob", BlobRecolor },
            { "BlobElite", BlobRecolor },
            { "Boar", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Poly Art Boar") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoar, emissive = true, ragdollPath = "Poly Art Boar" },
                new RecolorCreatureData("Visual/Fangs 001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true, ragdollPath = "Fangs 001" },
                new RecolorCreatureData("Visual/Fangs 002") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true, ragdollPath = "Fangs 002" },
                new RecolorCreatureData("Visual/Fangs 003") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true, ragdollPath = "Fangs 003" },
                new RecolorCreatureData("Visual/Fangs 004") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true, ragdollPath = "Fangs 004" },
                new RecolorCreatureData("Visual/Fangs 005") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true, ragdollPath = "Fangs 005" },
                new RecolorCreatureData("Visual/Fangs 006") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBoarTusk, emissive = true, ragdollPath = "Fangs 006" },
            }},
            { "Bonemass", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/model/bonemass") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBonemassBones },
                new RecolorCreatureData("Visual/model/bonemass") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBonemass, materialIndex = 1, emissive = true, emissiveMultiplier = 1.5f },
                new RecolorCreatureData("Visual/model/heads") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBonemassBones },
                new RecolorCreatureData("Visual/Point light") { isLight = true },
                new RecolorCreatureData("Visual/particles/ooz") { isParticle = true },
                new RecolorCreatureData("Visual/particles/wetsplsh") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
                new RecolorCreatureData("Visual/particles/wetsplsh_local") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
            }},
            { "Deathsquito", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeathsquito, emissive = true },
            }},
            { "Deer", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Deer 003") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeer, emissive = true, ragdollPath = "CG/Deer 003" },
                new RecolorCreatureData("Visual/CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 01") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeerAntlers, emissive = true, ragdollPath = "CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 01" },
                new RecolorCreatureData("Visual/CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 04") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeerAntlers, emissive = true, ragdollPath = "CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 04" },
                new RecolorCreatureData("Visual/CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 05") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDeerAntlers, emissive = true, ragdollPath = "CG/Pelvis/Spine/Spine1/Spine2/Neck/Neck1/Head/Antlers 05" },
            }},
            { "Dragon", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/DragonQueen") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorModer, emissive = true },
            }},
            { "Draugr", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_draugr_base/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDraugr, emissive = true, ragdollPath = "Visual/_playertest_serk_base (2)/Cube.001" },
            }},
            { "Draugr_Elite", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_draugr_base/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDraugrElite, emissive = true, ragdollPath = "Visual/_playertest_serk_base (2)/Cube.001" },
            }},
            { "Draugr_Ranged", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/_draugr_base/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorDraugrFem, emissive = true, ragdollPath = "Visual/_playertest_serk_base (2)/Cube.001" },
            }},
            { "Eikthyr", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Deer") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorEikthyr, emissive = true, ragdollPath = "Visual/Deer" },
                new RecolorCreatureData("Visual/chains") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorEikthyr, emissive = true, ragdollPath = "Visual/chains" },
                new RecolorCreatureData("Visual/extrafur") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorEikthyrHair, emissive = true, ragdollPath = "Visual/extrafur" },
                new RecolorCreatureData("Visual/mane") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorEikthyr, emissive = true, ragdollPath = "Visual/mane" },
            }},
            { "Fenring", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFenring, emissive = true, ragdollPath = "Fenring.001" },
            }},
            { "Fenring_Cultist", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorCultist, emissive = true, ragdollPath = "Fenring.001" },
                new RecolorCreatureData("Visual/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorCultistCape, materialIndex = 1, emissive = true, ragdollPath = "Fenring.001" },
            }},
            { "gd_king", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/gd_king") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGDKing, emissive = true },
                new RecolorCreatureData("Visual/Armature/root/spine1/spine2/spine3/head/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGDKingEye, emissive = true },
                new RecolorCreatureData("Visual/Armature/root/spine1/spine2/spine3/head/Cube (2)") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGDKingEye, emissive = true },
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
            { "Goblin", GoblinRecolor },
            { "GoblinArcher", GoblinRecolor },
            { "GoblinBrute", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/GoblinBrute") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingBrute, emissive = true, ragdollPath = "Visual/GoblinBrute" },
            }},
            { "GoblinShaman", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Shaman") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingShaman, emissive = true, ragdollPath = "Visual (1)/Shaman" },
                //new RecolorCreatureData("Visual/Shaman") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFulingShamanCape, materialIndex = 1, emissive = true },
            }},
            { "GoblinKing", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Yagluth") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorYagluth, emissive = true },
                new RecolorCreatureData("Visual/Yagluth") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorYagluth, materialIndex = 1, emissive = true },
                new RecolorCreatureData("Visual/Yagluth_Hair") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorYagluthHair, emissive = true },
                new RecolorCreatureData("Visual/Armature/Root/Root2/Hip/Spine1/Spine2/Spine3/Point light") { isLight = true },
                new RecolorCreatureData("Visual/Armature/Root/Root2/Hip/Spine1/Spine2/Spine3/Particle System (2)") { isParticle = true },
                new RecolorCreatureData("Visual/Armature/Root/Root2/Hip/Spine1/Spine2/Spine3/Neck/Bone.007/Point light (2)") { isLight = true },
                new RecolorCreatureData("Visual/Armature/Root/Root2/Hip/Spine1/Spine2/Spine3/Neck/Bone.007/Particle System") { isParticle = true },
                new RecolorCreatureData("Visual/Armature/Root/Root2/Hip/Spine1/Spine2/Spine3/Neck/Bone.007/Particle System (1)") { isParticle = true },
            }},
            { "Greyling", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true, ragdollPath = "Cube" },
            }},
            { "Greydwarf", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true, ragdollPath = "Cube" },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true, ragdollPath = "Cube.001" },
            }},
            { "Greydwarf_Elite", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true, ragdollPath = "Cube" },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarf, emissive = true, ragdollPath = "Cube.001" },
                new RecolorCreatureData("Visual/Armature.001/root/spine1/spine2/spine3/r_shoulder/r_arm1/r_arm2/r_hand/Rootsword/default") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfRootsword, emissive = true },
            }},
            { "Greydwarf_Shaman", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfShaman, emissive = true, ragdollPath = "Visual/Cube" },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorGreydwarfShaman, emissive = true, ragdollPath = "Visual/Cube.001" },
            }},
            { "Hatchling", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Hatchling_mountain/Hatchling") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorHatchling, emissive = true, ragdollPath = "Hatchling_mountain/Hatchling" },
                new RecolorCreatureData("Visual/Hatchling_mountain/Hatchling.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorHatchling, emissive = true, ragdollPath = "Hatchling_mountain/Hatchling.001" },
                new RecolorCreatureData("Visual/Hatchling_mountain/Horns") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorHatchling, emissive = true, ragdollPath = "Hatchling_mountain/Horns" },
                new RecolorCreatureData("Visual/Hatchling_mountain/eyes") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorHatchling, emissive = true, ragdollPath = "Hatchling_mountain/eyes" },
            }},
            { "Leech", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorLeech, emissive = true },
            }},
            { "Lox", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/offset/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorLox, emissive = true, ragdollPath = "Visual/Body" },
                new RecolorCreatureData("Visual/offset/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorLox, materialIndex = 1, emissive = true, ragdollPath = "Visual/Body" },
                new RecolorCreatureData("Visual/offset/Furr1") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorLox, emissive = true, ragdollPath = "Visual/Furr1" },
            }},
            { "Neck", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorNeck, emissive = true, ragdollPath = "Body" },
                new RecolorCreatureData("Visual/Lillies") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorNeck, emissive = true, ragdollPath = "Lillies" },
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
            { "SeekerQueen", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorQueen, emissive = true },
                new RecolorCreatureData("Visual/Plane") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorQueen, emissive = true },
            }},
            { "Serpent", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Serpant") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorSerpent, emissive = true },
            }},
            { "Skeleton", SkeletonRecolor },
            { "Skeleton_Friendly", SkeletonRecolor },
            { "Skeleton_Meadows", SkeletonRecolor },
            { "Skeleton_Meadows_noarcher", SkeletonRecolor },
            { "Skeleton_Mountains", SkeletonRecolor },
            { "Skeleton_Mountains_noarcher", SkeletonRecolor },
            { "Skeleton_Poison", SkeletonRecolor },
            { "Skeleton_Swamps", SkeletonRecolor },
            { "Skeleton_Swamps_noarcher", SkeletonRecolor },
            { "StoneGolem", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorStoneGolem, emissive = true, ragdollPath = "Mountain_Golem/Cube.001" },
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
                new RecolorCreatureData("Visual/Body") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTroll, emissive = true, ragdollPath = "troll_base_2/Body" },
                new RecolorCreatureData("Visual/Hair") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorTroll, emissive = true, ragdollPath = "troll_base_2/Hair" },
            }},
            { "Ulv", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Ulv/Fenring.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorFenring, emissive = true, ragdollPath = "Ulv_Ragdoll/Fenring.001" },
            }},
            { "Unbjorn", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Asora") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = false, zebraMaterial = WizshBoneTwitchIntegration.Instance.materials.RecolorBjornZebra, ragdollPath = "Asora" },
                new RecolorCreatureData("Visual/Plane") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = false, ragdollPath = "Plane" },
                new RecolorCreatureData("Visual/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true, ragdollPath = "Cube.001" },
                new RecolorCreatureData("Visual/Cube.002") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true, ragdollPath = "Cube.002" },
                new RecolorCreatureData("Visual/Cube.002 (1)") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true, ragdollPath = "Cube.002 (1)" },
                new RecolorCreatureData("Visual/Sphere") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true, ragdollPath = "Sphere" },
                new RecolorCreatureData("Visual/Sphere (1)") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorVile, emissive = true, ragdollPath = "Sphere (1)" },
            }},
            { "Wolf", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/WolfSmooth/M") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWolf, emissive = true, ragdollPath = "WolfSmooth/M" },
            }},
            { "Wraith", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/wraith/wraith") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = true, zebraMaterial = WizshBoneTwitchIntegration.Instance.materials.RecolorWraithZebra },
                new RecolorCreatureData("Visual/wraith/chain") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = true },
                new RecolorCreatureData("Visual/wraith/RagPlanes") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWraith, emissive = false },
                new RecolorCreatureData("Visual/wraith/Armature/root/spine1/spine2/spine3/neck/head/Point light") { isLight = true },
            }},
            { "Writhan", new List<RecolorCreatureData>() {
                new RecolorCreatureData("Visual/Writhan/Cube.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWrithan, emissive = true },
                new RecolorCreatureData("Visual/Writhan/Sphere.001") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWrithan, emissive = true },
                new RecolorCreatureData("Visual/Writhan/BezierCurve") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorWrithan, emissive = true },
                new RecolorCreatureData("Visual/particles/Point Light") { isLight = true },
                new RecolorCreatureData("Visual/particles/ooz") { isParticle = true },
                //new RecolorCreatureData("Visual/particles/wetsplsh") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
                new RecolorCreatureData("Visual/particles/wetsplsh (1)") { material = WizshBoneTwitchIntegration.Instance.materials.RecolorBlobSlime, emissive = true, emissiveMultiplier = 0.5f, isParticle = true },
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

        /// <summary>
        /// The two colors a recolor applies: <see cref="Main"/> tints the main texture, while
        /// <see cref="Emission"/> drives everything that glows (emission colors, lights, particles).
        /// </summary>
        public readonly struct RecolorColors
        {
            public readonly Color Main;
            public readonly Color Emission;

            public RecolorColors(Color main, Color emission)
            {
                Main = main;
                Emission = emission;
            }

            public RecolorColors(Color both) : this(both, both) { }
        }

        // A redeem's color/emission override strings, parsed like a viewer's colors: a missing
        // emission follows the main color, and a missing color is white when only an emission is set.
        private static bool TryParseColorOverride(string colorOverride, string emissionColorOverride, out RecolorColors colors)
        {
            Color main = Color.white;
            bool hasColor = !string.IsNullOrEmpty(colorOverride) && ColorUtility.TryParseHtmlString(colorOverride, out main);

            if (!hasColor)
                main = Color.white;

            Color emission = main;
            bool hasEmission = !string.IsNullOrEmpty(emissionColorOverride) && ColorUtility.TryParseHtmlString(emissionColorOverride, out emission);

            if (!hasEmission)
                emission = main;

            colors = new RecolorColors(main, emission);
            return hasColor || hasEmission;
        }

        // Which colors a creature gets: a forced redeem override wins, then the redeemer's own
        // viewer colors, then the redeem's (non-forced) override. False when none apply.
        private static bool TryResolveColors(string redeemerName, string colorOverride, bool forceColor, string emissionColorOverride, out RecolorColors colors)
        {
            ViewerEntry viewerEntry = GetViewer(redeemerName);

            if (forceColor && TryParseColorOverride(colorOverride, emissionColorOverride, out RecolorColors forcedColors))
            {
                colors = forcedColors;
                return true;
            }

            if (viewerEntry != null)
            {
                viewerEntry.Init();
                colors = new RecolorColors(viewerEntry.parsedColor, viewerEntry.parsedEmissionColor);
                return true;
            }

            return TryParseColorOverride(colorOverride, emissionColorOverride, out colors);
        }

        // Whether the creature has any entry that applies to its ragdoll (a ragdollPath), i.e. whether
        // handing its recolor to a ragdoll can do anything.
        public static bool HasRagdollRecolor(string creatureKey)
        {
            List<RecolorCreatureData> list = GetRecolorCreatureDataByKey(creatureKey);

            return list != null && list.Exists(data => !string.IsNullOrEmpty(data.ragdollPath));
        }

        // Applies the same recolor to a dead creature's ragdoll (spawned from its death effects),
        // looked up by the creature's prefab name. The ragdoll has its own hierarchy, so only the
        // entries that declare a ragdollPath apply - the rest (gear, lights, particles, show/hide
        // toggles, renderers the ragdoll doesn't have) belong to the live body and are skipped.
        public static void RecolorRagdoll(string creatureKey, GameObject ragdoll, string redeemerName, string colorOverride, bool forceColor, string emissionColorOverride)
        {
            if (!TryResolveColors(redeemerName, colorOverride, forceColor, emissionColorOverride, out RecolorColors colors))
                return;

            List<RecolorCreatureData> list = GetRecolorCreatureDataByKey(creatureKey);

            if (list == null)
                return;

            foreach (RecolorCreatureData recolorCreatureData in list)
            {
                if (string.IsNullOrEmpty(recolorCreatureData.ragdollPath))
                    continue;

                Transform transform = ragdoll.transform.Find(recolorCreatureData.ragdollPath);

                if (transform == null)
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Could not find transform to recolor ragdoll {ragdoll.name}: {recolorCreatureData.ragdollPath}");
                    continue;
                }

                SkinnedMeshRenderer skinnedMeshRenderer = transform.gameObject.GetComponent<SkinnedMeshRenderer>();
                MeshRenderer meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();

                if (skinnedMeshRenderer == null && meshRenderer == null)
                    continue;

                // A ragdoll has no Character, so LevelEffects never swaps its materials and there is
                // nothing to detach here (unlike RecolorCreature).
                if (!TryApplyRecolorMaterial(skinnedMeshRenderer, meshRenderer, recolorCreatureData, redeemerName, out Material mat))
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Recolor material is null for ragdoll {ragdoll.name}: {recolorCreatureData.ragdollPath}, skipping");
                    continue;
                }

                Recolor(recolorCreatureData, colors, mat, recolorCreatureData.emissiveMultiplier);
            }
        }

        // Puts the entry's recolor material (the zebra variant for that one viewer) into the
        // renderer's material slot and returns the renderer's own copy of it, ready to tint. False
        // when the entry has no material: creatureList captures Instance.materials.* once, in a
        // static initializer, so a null means that material was missing from the asset bundle (or not
        // loaded yet when RecolorHelper was first touched) and the entry is skipped, not thrown on.
        private static bool TryApplyRecolorMaterial(SkinnedMeshRenderer skinnedMeshRenderer, MeshRenderer meshRenderer, RecolorCreatureData recolorCreatureData, string redeemerName, out Material mat)
        {
            mat = null;

            Material recolorMaterial = redeemerName == "yummyzebracakes" && recolorCreatureData.zebraMaterial != null
                ? recolorCreatureData.zebraMaterial
                : recolorCreatureData.material;

            if (recolorMaterial == null)
                return false;

            SetupMaterials(skinnedMeshRenderer, meshRenderer, recolorMaterial, recolorCreatureData.materialIndex);

            // Get the material from the renderer as its now a copy
            mat = skinnedMeshRenderer != null
                ? skinnedMeshRenderer.materials[recolorCreatureData.materialIndex]
                : meshRenderer.materials[recolorCreatureData.materialIndex];

            return mat != null;
        }

        public static void RecolorCreature(string redeemerName, GameObject creature, string colorOverride = null, bool forceColor = false, string emissionColorOverride = null)
        {
            if (!TryResolveColors(redeemerName, colorOverride, forceColor, emissionColorOverride, out RecolorColors colors))
                return;

            List<RecolorCreatureData> list = GetRecolorCreatureDataByKey(creature.name);

            foreach (RecolorCreatureData recolorCreatureData in list)
            {
                if (recolorCreatureData.isGear)
                {
                    RecolorGear(creature, recolorCreatureData, colors);
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
                    light.color = colors.Emission;
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
                        Recolor(recolorCreatureData, colors, particleRenderder.materials[0], recolorCreatureData.emissiveMultiplier);
                    }
                    else
                    {
                        ParticleSystem.MainModule main = particleSystem.main;
                        float h, s, v;

                        Color.RGBToHSV(colors.Emission, out h, out s, out v);
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
                            keys1[i].color = colors.Emission * decrease;
                        }

                        GradientColorKey[] keys2 = gradient2.gradient.colorKeys;
                        for (int i = 0; i < keys2.Length; i++)
                        {
                            float decrease = 1f - (0.15f * i);
                            keys2[i].color = colors.Emission * decrease;
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

                if (!TryApplyRecolorMaterial(skinnedMeshRenderer, meshRenderer, recolorCreatureData, redeemerName, out Material mat))
                {
                    Jotunn.Logger.LogWarning($"[WBTI] Recolor material is null for {creature.name}: {recolorCreatureData.transformPath}, skipping");
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

                    // SetupLevelVisualization() (Start() and every SetLevel) replaces m_mainRender's
                    // material with a copy cached in a static dictionary keyed by prefab name + level -
                    // shared by every creature of that prefab/level, and built from whatever material the
                    // renderer had when the first one was set up. For any starred creature that either
                    // overwrites our recolored material with the vanilla one (starred deer/wolf skin
                    // staying uncolored) or leaks one viewer's recolor to the next. Detach the renderer
                    // from LevelEffects so that swap is skipped; scale and the per-star enable objects
                    // are unaffected. UnColorCreature() restores it.
                    Renderer recoloredRenderer = skinnedMeshRenderer != null ? (Renderer)skinnedMeshRenderer : meshRenderer;

                    if (levelEffects.m_mainRender == recoloredRenderer)
                        levelEffects.m_mainRender = null;
                }

                Recolor(recolorCreatureData, colors, mat, recolorCreatureData.emissiveMultiplier);
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
                    {
                        // Copy the tint values back onto this instance's own setups rather than assigning
                        // the prefab's list: that would share the list (RecolorCreature() would then zero
                        // the prefab's values) and point m_enableObject at the prefab's objects instead
                        // of this instance's.
                        int setupCount = Mathf.Min(levelEffects.m_levelSetups.Count, levelEffectsOriginal.m_levelSetups.Count);

                        for (int i = 0; i < setupCount; i++)
                        {
                            LevelEffects.LevelSetup setup = levelEffects.m_levelSetups[i];
                            LevelEffects.LevelSetup setupOriginal = levelEffectsOriginal.m_levelSetups[i];

                            setup.m_hue = setupOriginal.m_hue;
                            setup.m_saturation = setupOriginal.m_saturation;
                            setup.m_value = setupOriginal.m_value;
                            setup.m_setEmissiveColor = setupOriginal.m_setEmissiveColor;
                            setup.m_emissiveColor = setupOriginal.m_emissiveColor;
                        }

                        // Re-attach the main renderer RecolorCreature() detached (the original's
                        // m_mainRender points at the prefab's renderer, so map it to this instance's).
                        Renderer originalRenderer = skinnedMeshRendererOriginal != null ? (Renderer)skinnedMeshRendererOriginal : meshRendererOriginal;

                        if (levelEffects.m_mainRender == null && levelEffectsOriginal.m_mainRender == originalRenderer)
                            levelEffects.m_mainRender = skinnedMeshRenderer != null ? (Renderer)skinnedMeshRenderer : meshRenderer;
                    }
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

        private static void RecolorGear(GameObject creature, RecolorCreatureData recolorCreatureData, RecolorColors colors)
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
                        Recolor(recolorCreatureData, colors, mat, recolorCreatureData.emissiveMultiplier);
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

        public static void Recolor(RecolorCreatureData recolorCreatureData, RecolorColors colors, Material mat, float multiplier = 2f)
        {
            if (recolorCreatureData.emissive)
            {
                Color emission = colors.Emission;
                Color emissionScaled = new Color(emission.r * multiplier, emission.g * multiplier, emission.b * multiplier);

                // Creature.shader's [HDR] _EmissionColor appears not to get Unity's usual gamma->linear
                // conversion on SetColor (a #FF004C emission showed up magenta-pink on Creature-shader
                // creatures but correct on Bonemass/Blob, which use _EmissiveColor/_FlowColor instead),
                // so convert it ourselves.
                mat.SetColor("_EmissionColor", emissionScaled.linear);
                mat.EnableKeyword("_EMISSION");
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;

                mat.SetColor("_EmissiveColor", emissionScaled);
                mat.SetColor("_FlowColor", emission);
                mat.SetColor("_SSS", colors.Main);
            }

            mat.color = colors.Main;
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

        // Strength of the emission layer (emission color x main texture x this). 0 = no glow.
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
            // The Standard shader multiplies the color into the rock's grey albedo texture, so the raw
            // viewer color ends up noticeably darker than it looks in the viewers list - blend it toward
            // white. The emission layer is texture x emission color, so it needs the same lift.
            RecolorColors colors = new RecolorColors(
                Color.Lerp(viewerEntry.parsedColor, Color.white, PetRockBrighten),
                Color.Lerp(viewerEntry.parsedEmissionColor, Color.white, PetRockBrighten));
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
                    ApplyPetRockTint(copy, colors);
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

                    ApplyPetRockTint(materials[i], colors);
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
        // pattern, so the face detail still reads) - light-colored areas glow in the viewer's emission
        // color, fully dark pixels (eyes/mouth) emit nothing.
        private static void ApplyPetRockTint(Material material, RecolorColors colors)
        {
            material.color = colors.Main;

            Texture mainTexture = material.mainTexture;

            if (mainTexture == null || !material.HasProperty("_EmissionColor"))
                return;

            material.SetTexture("_EmissionMap", mainTexture);
            material.SetColor("_EmissionColor", colors.Emission * PetRockEmission);
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

        public static bool CanRecolorCreature(string name, string creature, string colorOverride = null, string emissionColorOverride = null)
        {
            return IsCreatureInList(creature) && (IsRedeemerSpecialViewer(name) || colorOverride != null || !string.IsNullOrEmpty(emissionColorOverride));
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
