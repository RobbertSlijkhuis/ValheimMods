using RestingRockFace.Gui;
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
        public string m_name;
        public int m_face;

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO() == null)
                return;

            m_netView.Register<string, string, int>("SetNameAndFace", RPC_SetNameAndFace);

            m_faceTransform = gameObject.transform.Find("default");
            materialVar = m_faceTransform.gameObject.GetComponent<MaterialVariation>();
            m_tameable =  gameObject.GetComponent<Tameable>();
            
            m_gui = new RockyGUI(this);
            m_gui.onAccept.AddListener(SetNameAndFace);

            m_name = m_tameable.GetName();
            m_face = m_netView.GetZDO().GetInt(faceDataHash, (int)RockyFaceEnum.Nothing);
            UpdateFace(m_face);
            Jotunn.Logger.LogWarning("Awake name: " + m_name);
            Jotunn.Logger.LogWarning("Awake face: " + m_face);
        }

        public void Start()
        {
            if (m_netView == null || m_netView.GetZDO() == null)
                return;

            Jotunn.Logger.LogWarning("Start SetMaterial: " + (m_face != (int)RockyFaceEnum.Nothing));

            if (m_face != (int)RockyFaceEnum.Nothing)
                materialVar.SetMaterial(m_face);
        }

        public void SetNameAndFace(RockyGuiResult result)
        {
            Jotunn.Logger.LogWarning($"Result: {result.name}, {result.face}");
            m_name = result.name;
            m_face = result.face;

            if (m_netView.IsValid())
            {
                PlatformUserID platformUserID = PlatformManager.DistributionPlatform.LocalUser.PlatformUserID;
                m_netView.InvokeRPC("SetNameAndFace", result.name, PlatformManager.DistributionPlatform.LocalUser.IsSignedIn ? platformUserID.ToString() : "host", result.face);
            }
        }

        public void RPC_SetNameAndFace(long sender, string name, string authorId, int face)
        {
            if (m_netView.IsValid() && m_netView.IsOwner())
            {
                m_netView.GetZDO().Set(ZDOVars.s_tamedName, name);
                m_netView.GetZDO().Set(ZDOVars.s_tamedNameAuthor, authorId);
                UpdateFace(face);

                materialVar.SetMaterial(face);
            }
        }

        public void UpdateFace(int face)
        {
            if (face == (int)RockyFaceEnum.Nothing)
            {
                m_netView.GetZDO().RemoveInt(faceDataHash);
                Jotunn.Logger.LogWarning("UpdateFace: removed ZDO");
            }
            else
            {
                m_netView.GetZDO().Set(faceDataHash, face);
                Jotunn.Logger.LogWarning("UpdateFace: set ZDO");
            }
        }
    }
}
