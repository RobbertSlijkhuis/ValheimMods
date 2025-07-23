using JetBrains.Annotations;
using Jotunn;
using PlayAsSkeleton.Components;
using PlayAsSkeleton.Configs;
using PlayAsSkeleton.models;
using PlayAsSkeleton.Models;
using PlayAsSkeleton.Types;
using PlayFab.EconomyModels;
using PlayFab.ExperimentationModels;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using static ItemDrop;

namespace PlayAsSkeleton.Helpers
{
    internal class SkeletonHelper
    {
        private static VisEquipmentSnapshot visEquipmentSnapshot;

        public static void Apply(Player player, SkeletonPAS PASComp = null)
        {
            Jotunn.Logger.LogWarning("Apply: " + player.GetPlayerID());

            if (PASComp == null)
                PASComp = player.gameObject.GetComponent<SkeletonPAS>();

            GameObject skeletonBase = PlayAsSkeleton.Instance.prefabs.SkeletonBase;
            Transform visualTrans = player.gameObject.transform.Find("Visual");
            Transform skeletonTrans = player.gameObject.transform.Find(PlayAsSkeleton.Instance.prefabs.SkeletonBase.name + "(Clone)");

            if (skeletonTrans == null)
                player.m_visual = UnityEngine.Object.Instantiate(skeletonBase, player.transform);
            else
                player.m_visual = skeletonTrans.gameObject;

            SkinnedMeshRenderer skinnedMeshRenderer = UpdateSkin(player, PASComp.GetSkin());
            VisEquipment vis = player.GetComponent<VisEquipment>();
            SetAnimator(player, visualTrans);
            SetCanSwim(player, PASComp.GetCanSwim());
            SetVisEquipmentSnapthot(vis);

            vis.m_bodyModel = skinnedMeshRenderer;
            vis.m_models = new VisEquipment.PlayerModel[1] { new VisEquipment.PlayerModel() { m_mesh = PlayAsSkeleton.Instance.meshes.Skeleton, m_baseMaterial = GetSkinByName(PASComp.GetSkin()).body } };
            vis.m_helmet = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/Spine2/Neck/Head/Helmet_attach");
            vis.m_leftHand = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHand_attach");
            vis.m_rightHand = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/RightHand_attach");
            vis.m_backShield = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/BackShield_attach");
            vis.m_backMelee = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/BackOneHanded_attach");
            vis.m_backTwohandedMelee = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/BackTwohanded_attach");
            vis.m_backBow = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/BackBow_attach");
            vis.m_backAtgeir = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/BackAtgeir_attach");
            vis.m_backTool = player.m_visual.transform.Find("Armature/Hips/BackTool_attach");
            vis.m_isPlayer = false;

            player.m_visual.SetActive(true);
            visualTrans.gameObject.SetActive(false);
            UnequipHands(player);
        }

        public static void Reset(Player player)
        {
            Jotunn.Logger.LogWarning("Reset: " + player.GetPlayerID());
            Transform visualTrans = player.transform.Find("Visual");
            Transform skeletonTrans = player.transform.Find(PlayAsSkeleton.Instance.prefabs.SkeletonBase.name + "(Clone)");
            VisEquipment vis = player.GetComponent<VisEquipment>();

            player.m_visual = visualTrans.gameObject;
            SetAnimator(player, visualTrans);
            SetCanSwim(player, true);
            visEquipmentSnapshot.Apply(vis);

            //SkeletonHelper.HideEquipment(vis, VisEquipmentSlot.Helmet, false);
            //SkeletonHelper.HideEquipment(vis, VisEquipmentSlot.Cape, false);
            //SkeletonHelper.HideEquipment(vis, VisEquipmentSlot.Chest, false);
            //SkeletonHelper.HideEquipment(vis, VisEquipmentSlot.Utility, false);
            //SkeletonHelper.HideEquipment(vis, VisEquipmentSlot.Legs, false);

            skeletonTrans.gameObject.SetActive(false);
            visualTrans.gameObject.SetActive(true);
            UnequipHands(player);
        }

        private static void SetAnimator(Player player, Transform visualTrans)
        {
            if (player == null)
                throw new Exception("Player is null");

            if (visualTrans == null)
                throw new Exception("Transform is null");

            player.m_animator = player.m_visual.GetComponent<Animator>();
            player.m_zanim.m_animator = player.m_animator;
            player.m_animator.runtimeAnimatorController = visualTrans.GetComponent<Animator>().runtimeAnimatorController;
        }

        public static void SetCanSwim(Player player, bool value)
        {
            if (player == null)
                throw new Exception("Player is null");

            Humanoid humanoid = player.gameObject.GetComponent<Humanoid>();

            if (humanoid == null)
                throw new Exception("Humanoid is null");

            humanoid.m_canSwim = value;
        }

        private static void UnequipHands(Player player)
        {
            if (player == null)
                throw new Exception("Player is null");

            ItemData rightItem = player.GetRightItem();
            ItemData leftItem = player.GetLeftItem();

            if (rightItem != null)
                player.UnequipItem(player.GetRightItem());

            if (leftItem != null)
                player.UnequipItem(player.GetLeftItem());
        }

        public static SkinnedMeshRenderer UpdateSkin(Player player, string name)
        {
            // Jotunn.Logger.LogWarning("UpdateSkin");
            if (player == null)
                throw new Exception("Player is null");

            if (name == null)
                throw new Exception("Skin name is null");

            SkeletonPAS skeletonPAS = player.GetComponent<SkeletonPAS>();

            if (skeletonPAS == null || !skeletonPAS.GetIsSkeleton())
                return null;

            SkinnedMeshRenderer comp = player.m_visual.GetComponentInChildren<SkinnedMeshRenderer>();
            SkinResult skinResult = GetSkinByName(name);
            comp.SetMaterials(new List<Material> { skinResult.body });

            Transform eyeLeft = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_l");
            Transform eyeRight = player.m_visual.transform.Find("Armature/Hips/Spine/Spine1/Spine2/Neck/Head/eye_r");

            if (skinResult.eyes != null) {
                eyeLeft.gameObject.SetActive(true);
                eyeLeft.gameObject.GetComponent<MeshRenderer>().materials = new Material[1] { skinResult.eyes };

                eyeRight.gameObject.SetActive(true);
                eyeRight.gameObject.GetComponent<MeshRenderer>().materials = new Material[1] { skinResult.eyes };
            }
            else
            {
                eyeLeft.gameObject.SetActive(false);
                eyeRight.gameObject.SetActive(false);
            }

            return comp;
        }

        private static SkinResult GetSkinByName(string name)
        {
            switch (name)
            {
                case nameof(SkinType.Normal):
                    return new SkinResult(PlayAsSkeleton.Instance.materials.Normal, PlayAsSkeleton.Instance.materials.NormalEyes);

                case nameof(SkinType.Poison):
                    return new SkinResult(PlayAsSkeleton.Instance.materials.Poison, PlayAsSkeleton.Instance.materials.PoisonEyes);

                case nameof(SkinType.Burned):
                    return new SkinResult(PlayAsSkeleton.Instance.materials.Burned, PlayAsSkeleton.Instance.materials.BurnedEyes);

                case nameof(SkinType.Dark):
                    return new SkinResult(PlayAsSkeleton.Instance.materials.Dark, PlayAsSkeleton.Instance.materials.DarkEyes);

                case nameof(SkinType.LoyalBones):
                    return new SkinResult(PlayAsSkeleton.Instance.materials.LoyalBones, null);

                default:
                    return new SkinResult(PlayAsSkeleton.Instance.materials.Normal, PlayAsSkeleton.Instance.materials.NormalEyes);
            }
        }

        public static void HideEquipment(Player player, string slot, bool isHidden)
        {
            if (player == null)
                throw new Exception("Player is null");

            VisEquipment vis = player.GetComponent<VisEquipment>();
            HideEquipment(vis, slot, isHidden);
        }

        public static void HideEquipment(VisEquipment vis, string slot, bool isHidden)
        {
            if (vis == null)
                throw new Exception("VisEquipment is null");

            switch (slot)
            {
                case nameof(VisEquipmentSlot.Helmet):
                    if (!isHidden)
                    {
                        int helmetItemHash = vis.m_currentHelmetItemHash;
                        int hairItemHash = vis.m_currentHairItemHash;
                        vis.m_currentHelmetItemHash = 0;
                        vis.m_currentHairItemHash = 0;
                        vis.SetHelmetEquipped(helmetItemHash, hairItemHash);
                        break;
                    }

                    if ((bool)vis.m_helmetItemInstance)
                    {
                        UnityEngine.Object.Destroy(vis.m_helmetItemInstance);
                        vis.m_helmetItemInstance = null;
                    }

                    break;
                case nameof(VisEquipmentSlot.Cape):
                    if (!isHidden)
                    {
                        int shoulderItemHash = vis.m_currentShoulderItemHash;
                        int variantShoulderHash = vis.m_currentShoulderItemVariant;
                        vis.m_currentShoulderItemHash = 0;
                        vis.m_currentShoulderItemVariant = 0;
                        vis.SetShoulderEquipped(shoulderItemHash, variantShoulderHash);
                        break;
                    }

                    if (vis.m_shoulderItemInstances != null)
                    {
                        foreach (GameObject shoulderItemInstance in vis.m_shoulderItemInstances)
                        {
                            if ((bool)vis.m_lodGroup)
                            {
                                Utils.RemoveFromLodgroup(vis.m_lodGroup, shoulderItemInstance);
                            }

                            UnityEngine.Object.Destroy(shoulderItemInstance);
                        }

                        vis.m_shoulderItemInstances = null;
                    }
                    break;
                case nameof(VisEquipmentSlot.Chest):
                    if (!isHidden)
                    {
                        int chestItemHash = vis.m_currentChestItemHash;
                        vis.m_currentChestItemHash = 0;
                        vis.SetChestEquipped(chestItemHash);
                        break;
                    }

                    if (vis.m_chestItemInstances != null)
                    {
                        foreach (GameObject chestItemInstance in vis.m_chestItemInstances)
                        {
                            if ((bool)vis.m_lodGroup)
                            {
                                Utils.RemoveFromLodgroup(vis.m_lodGroup, chestItemInstance);
                            }

                            UnityEngine.Object.Destroy(chestItemInstance);
                        }
                        vis.m_chestItemInstances = null;
                        vis.m_bodyModel.material.SetTexture("_ChestTex", vis.m_emptyBodyTexture);
                        vis.m_bodyModel.material.SetTexture("_ChestBumpMap", null);
                        vis.m_bodyModel.material.SetTexture("_ChestMetal", null);
                    }
                    break;
                case nameof(VisEquipmentSlot.Utility):
                    if (!isHidden)
                    {
                        int utilityItemHash = vis.m_currentUtilityItemHash;
                        vis.m_currentUtilityItemHash = 0;
                        vis.SetUtilityEquipped(utilityItemHash);
                        break;
                    }

                    if (vis.m_utilityItemInstances != null)
                    {
                        foreach (GameObject utilityItemInstance in vis.m_utilityItemInstances)
                        {
                            if ((bool)vis.m_lodGroup)
                            {
                                Utils.RemoveFromLodgroup(vis.m_lodGroup, utilityItemInstance);
                            }

                            UnityEngine.Object.Destroy(utilityItemInstance);
                        }

                        vis.m_utilityItemInstances = null;
                    }
                    break;
                case nameof(VisEquipmentSlot.Legs):
                    if (!isHidden)
                    {
                        int legItemHash = vis.m_currentLegItemHash;
                        vis.m_currentLegItemHash = 0;
                        vis.SetLegEquipped(legItemHash);
                        break;
                    }

                    if (vis.m_legItemInstances != null)
                    {
                        foreach (GameObject legItemInstance in vis.m_legItemInstances)
                        {
                            UnityEngine.Object.Destroy(legItemInstance);
                        }

                        vis.m_legItemInstances = null;
                        vis.m_bodyModel.material.SetTexture("_LegsTex", vis.m_emptyLegsTexture);
                        vis.m_bodyModel.material.SetTexture("_LegsBumpMap", null);
                        vis.m_bodyModel.material.SetTexture("_LegsMetal", null);
                    }
                    break;
            }
        }

        public static void SetVisEquipmentSnapthot(VisEquipment vis)
        {
            if (visEquipmentSnapshot == null)
                visEquipmentSnapshot = new VisEquipmentSnapshot(vis);
        }

        public static string GetSlotFromItemData(ItemData itemData)
        {
            if (itemData == null)
                throw new Exception("ItemData is null");

            switch (itemData.m_shared.m_itemType)
            {
                case ItemData.ItemType.Helmet:
                    return VisEquipmentSlot.Helmet;
                case ItemData.ItemType.Shoulder:
                    return VisEquipmentSlot.Cape;
                case ItemData.ItemType.Chest:
                    return VisEquipmentSlot.Chest;
                case ItemData.ItemType.Utility:
                    return VisEquipmentSlot.Utility;
                case ItemData.ItemType.Legs:
                    return VisEquipmentSlot.Legs;
                default:
                    return null;
            }
        }
    }
}
