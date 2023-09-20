using Jotunn.Entities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RideableSeekerBrute
{
    internal class MountableManager
    {
        private static MountableManager _instance;
        public static MountableManager Instance => _instance ?? (_instance = new MountableManager());
        internal readonly HashSet<CustomItem> Configs = new HashSet<CustomItem>();

        public void MakeMountable(GameObject prefab, SaddleConfig saddleConfig, MountableConfig mountableConfig)
        {
            try
            {
                Transform parentOfSaddle = Utils.FindChild(prefab.transform, saddleConfig.parentTransform);
                Transform parentOfCharacterAttach = Utils.FindChild(prefab.transform, mountableConfig.parentTransform);

                if (parentOfSaddle == null)
                {
                    Jotunn.Logger.LogWarning("Could not find parent Transform " + saddleConfig.parentTransform + " for the saddle, skipping"+ prefab.name);
                    return;
                }
                if (parentOfCharacterAttach == null)
                {
                    Jotunn.Logger.LogWarning("Could not find parent Transform " + mountableConfig.parentTransform + " for the mount point, skipping" + prefab.name);
                    return;
                }

                Jotunn.Logger.LogWarning("Found saddle parent: '" + parentOfSaddle.name + "' and mount point parent: " + parentOfCharacterAttach.name + "'");

                GameObject saddleObj = new GameObject();
                saddleObj.name = "Saddle";
                saddleObj.transform.SetParent(parentOfSaddle);
                saddleObj.transform.localPosition = saddleConfig.attach.position;
                saddleObj.transform.localRotation = saddleConfig.attach.rotation;
                saddleObj.transform.localScale = saddleConfig.attach.scale;

                GameObject characterAttachObj = new GameObject();
                characterAttachObj.name = "CharacterAttachPoint";
                characterAttachObj.transform.SetParent(parentOfCharacterAttach);
                characterAttachObj.transform.localPosition = mountableConfig.characterAttach.position;
                characterAttachObj.transform.localRotation = mountableConfig.characterAttach.rotation;
                characterAttachObj.transform.localScale = mountableConfig.characterAttach.scale;

                GameObject SaddleAttachObj = new GameObject();
                SaddleAttachObj.name = "SaddleAttachPoint";
                SaddleAttachObj.transform.SetParent(characterAttachObj.transform);
                SaddleAttachObj.transform.localPosition = mountableConfig.saddleAttach.position;
                SaddleAttachObj.transform.localRotation = mountableConfig.saddleAttach.rotation;
                SaddleAttachObj.transform.localScale = mountableConfig.saddleAttach.scale;

                saddleObj.AddComponent<Sadle>();
                Sadle saddleComp = saddleObj.GetComponent<Sadle>();
                saddleComp.m_attachAnimation = saddleConfig.attachAnimation;
                saddleComp.m_attachPoint = characterAttachObj.transform;
                saddleComp.m_detachOffset = saddleConfig.detachOffset;
                saddleComp.m_hoverText = saddleConfig.hoverText;
                saddleComp.m_maxStamina = saddleConfig.maxStamina;
                saddleComp.m_maxUseRange = saddleConfig.maxUseRange;
                saddleComp.m_runStaminaDrain = saddleConfig.runStaminaDrain;
                saddleComp.m_staminaRegen = saddleConfig.staminaRegen;
                saddleComp.m_staminaRegenHungry = saddleConfig.staminaRegenHungry;
                saddleComp.m_swimStaminaDrain = saddleConfig.swimStaminaDrain;

                saddleObj.AddComponent<SphereCollider>();
                SphereCollider sphereComp = saddleObj.GetComponent<SphereCollider>();
                sphereComp.center = saddleConfig.sphereCenter;
                Jotunn.Logger.LogWarning("sphereRadius: "+ saddleConfig.sphereRadius);
                sphereComp.radius = saddleConfig.sphereRadius;

                // RenderDebugSphere(saddleObj, saddleConfig);

                SaddleAttachObj.AddComponent<MeshFilter>();
                SaddleAttachObj.AddComponent<MeshRenderer>();

                prefab.AddComponent<SaddleAnchor>();
                SaddleAnchor saddleAnchorComp = prefab.GetComponent<SaddleAnchor>();
                saddleAnchorComp.m_saddle_anchor = SaddleAttachObj.transform;

                UpdateTameable(prefab, saddleConfig);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Error while adding Saddle component, "+ error.Message);
            }
        }

        public void UpdateTameable(GameObject prefab, SaddleConfig saddleConfig)
        {
            try
            {
                Transform objWithMesh = Utils.FindChild(saddleConfig.prefab.transform, "Cube");
                Transform saddleAttach = Utils.FindChild(prefab.transform, "SaddleAttachPoint");
                Transform saddleParent = Utils.FindChild(prefab.transform, "Saddle");
                Tameable tameableComp = prefab.GetComponent<Tameable>();
                Sadle saddleComp = saddleParent.GetComponent<Sadle>();

                // Update Tameable component with saddle config
                tameableComp.m_saddleItem = saddleConfig.prefab.GetComponent<ItemDrop>();
                tameableComp.m_saddle = saddleComp;
                tameableComp.m_dropItemVel = saddleConfig.dropItemVelocity;
                tameableComp.m_dropSaddleOnDeath = saddleConfig.dropSaddleOnDeath;
                tameableComp.m_dropSaddleOffset = saddleConfig.dropSaddleOffset;

                // Update the saddle attach GameObject with the saddle mesh
                MeshFilter meshFilter = saddleAttach.GetComponent<MeshFilter>();
                meshFilter.mesh = objWithMesh.GetComponent<MeshFilter>().mesh;
                MeshRenderer meshRenderer = saddleAttach.GetComponent<MeshRenderer>();
                meshRenderer.materials = objWithMesh.GetComponent<MeshRenderer>().materials;

                Jotunn.Logger.LogWarning("Updated the saddle ItemDrop & saddle GameObject in Tameable component on "+ prefab.name);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Error while setting the saddle, " + error.Message);
            }
        }

        public void RenderDebugSphere(GameObject saddleObj, SaddleConfig saddleConfig)
        {
            Material debugMat = new Material(Shader.Find("Standard"));
            GameObject debugObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            debugObj.name = "DebugSphere";
            debugObj.transform.SetParent(saddleObj.transform);
            debugObj.transform.localPosition = new Vector3(0, 0, 0);
            debugObj.transform.localScale = new Vector3(saddleConfig.sphereRadius * 2, saddleConfig.sphereRadius * 2, saddleConfig.sphereRadius * 2);

            MeshRenderer meshRenderer = debugObj.GetComponent<MeshRenderer>();
            meshRenderer.material = debugMat;

            SphereCollider sphereCollider = debugObj.GetComponent<SphereCollider>();
            sphereCollider.enabled = false;
        }
    }

    class SaddleConfig
    {
        public MountableAttachConfig attach = new MountableAttachConfig();
        public string attachAnimation = "attach_lox";
        public Vector3 detachOffset = new Vector3(0, 0, 0);
        public float dropItemVelocity = 5;
        public bool dropSaddleOnDeath = true;
        public Vector3 dropSaddleOffset = new Vector3(0, 1, 0);
        public string hoverText = "Saddle";
        public float maxStamina = 100;
        public float maxUseRange = 6;
        public string parentTransform;
        public GameObject prefab;
        public float runStaminaDrain = 1;
        public float sphereRadius = 1;
        public Vector3 sphereCenter = new Vector3(0, 0, 0);
        public float staminaRegen = 0.2f;
        public float staminaRegenHungry = 0.1f;
        public float swimStaminaDrain = 5;

        public SaddleConfig(GameObject prefab, string parentTransform)
        {
            this.parentTransform = parentTransform;
            this.prefab = prefab;
        }
    }

    class MountableConfig
    {
        public string parentTransform;
        public MountableAttachConfig characterAttach = new MountableAttachConfig();
        public MountableAttachConfig saddleAttach = new MountableAttachConfig();

        public MountableConfig(string parentTransform)
        {
            this.parentTransform = parentTransform;
        }
    }

    class MountableAttachConfig
    {
        public Vector3 position = new Vector3(0, 0, 0);
        public Quaternion rotation = new Quaternion(0, 0, 0, 0);
        public Vector3 scale = new Vector3(0, 0, 0);

        public MountableAttachConfig()
        {
            rotation.eulerAngles = new Vector3(0, 0, 0);
        }
    }
}
