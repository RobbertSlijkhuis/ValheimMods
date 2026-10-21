using RestingRockFace.Gui;
using RestingRockFace.Helpers;
using RestingRockFace.Models;
using RestingRockFace.Types;
using Splatform;
using UnityEngine;

namespace RestingRockFace.Components
{
    internal class RockyControls : MonoBehaviour
    {
        public ZNetView m_netView;
        public RockyGUI m_gui;
        public Tameable m_tameable;
        public MaterialVariation materialVar;
        public Transform m_faceTransform;
        private readonly int faceDataHash = "FaceData_RRF".GetStableHashCode();

        // Read from the ZDO instead of caching: only the owner runs RPC_SetFace, so a cached field goes stale on
        // every other client and the Pet.SetFace patch would keep blocking the game's faces once ownership moves there.
        public int m_face => m_netView != null && m_netView.GetZDO() != null
            ? m_netView.GetZDO().GetInt(faceDataHash, (int)RockyFaceEnum.Random)
            : (int)RockyFaceEnum.Random;

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO() == null)
                return;

            m_netView.Register<int>("SetFace_RRF", RPC_SetFace);
            m_netView.Register<string, string>("SetNameDirect_RRF", RPC_SetNameDirect);

            m_faceTransform = gameObject.transform.Find("default");
            materialVar = m_faceTransform.gameObject.GetComponent<MaterialVariation>();
            m_tameable =  gameObject.GetComponent<Tameable>();

            m_gui = new RockyGUI(this);
            m_gui.onAccept.AddListener(SetNameAndFace);
        }

        public void Start()
        {
            if (m_netView == null || m_netView.GetZDO() == null)
                return;

            if (m_face != (int)RockyFaceEnum.Random)
                materialVar.SetMaterial(m_face);
        }

        public void SetNameAndFace(RockyGuiResult result)
        {
            if (!m_netView.IsValid())
                return;

            // Only touch the name when it actually changed - the GUI is prefilled with the current name,
            // so a face-only change must not write the default piece name as a custom one.
            bool nameChanged = result.name != m_tameable.GetName();

            // Take ownership first so the RPCs below target us; sent to a previous owner they would be dropped
            // by the IsOwner checks if ownership moved before they arrived. Skipped when nothing changed so an
            // untouched Accept doesn't steal ownership.
            if (nameChanged || result.face != m_face)
                m_netView.ClaimOwnership();

            if (nameChanged)
                SetName(result.name);

            m_netView.InvokeRPC("SetFace_RRF", result.face);
        }

        // Goes through vanilla Tameable.SetText (the same call the vanilla rename UI makes) so other mods
        // patching it (e.g. WizshBoneTwitchIntegration's "claim:") also see names set from our GUI.
        // Vanilla's RPC_SetName ignores untamed Tameables though, so those fall back to writing the ZDO directly.
        private void SetName(string name)
        {
            if (m_tameable.IsTamed())
            {
                m_tameable.SetText(name);
                return;
            }

            PlatformUserID platformUserID = PlatformManager.DistributionPlatform.LocalUser.PlatformUserID;
            m_netView.InvokeRPC("SetNameDirect_RRF", name, PlatformManager.DistributionPlatform.LocalUser.IsSignedIn ? platformUserID.ToString() : "host");
        }

        public void RPC_SetNameDirect(long sender, string name, string authorId)
        {
            if (m_netView.IsValid() && m_netView.IsOwner())
            {
                m_netView.GetZDO().Set(ZDOVars.s_tamedName, name);
                m_netView.GetZDO().Set(ZDOVars.s_tamedNameAuthor, authorId);
            }
        }

        public void RPC_SetFace(long sender, int face)
        {
            if (m_netView.IsValid() && m_netView.IsOwner())
            {
                UpdateFace(face);

                if (face == (int)RockyFaceEnum.Random)
                    RockyFaceHelper.ResetToGameDecided(materialVar, m_netView);
                else
                    materialVar.SetMaterial(face);
            }
        }

        public void UpdateFace(int face)
        {
            if (face == (int)RockyFaceEnum.Random)
                m_netView.GetZDO().RemoveInt(faceDataHash);
            else
                m_netView.GetZDO().Set(faceDataHash, face);
        }
    }
}
