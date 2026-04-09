using TwitchSDK.Interop;
using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchPiecePersistentData : MonoBehaviour
    {
        private ZNetView m_netView;
        private string m_redeemTitle;
        private string m_redeemerName;
        private bool m_allowDrops;

        private readonly int pieceDataHash = "WTBIPersistentPieceData".GetStableHashCode();

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO() == null)
            {
                Jotunn.Logger.LogError("Could not find ZNetView in persistent data!");
                return;
            }

            string pieceDataString = m_netView.GetZDO().GetString(pieceDataHash, "");

            if (pieceDataString == "")
                return;

            string[] data = pieceDataString.Split('|');
            m_redeemerName = data[0];
            m_redeemTitle = data[1];
            Jotunn.Logger.LogWarning("Retrieved Piece persitent data: " + m_redeemerName + ", " + m_redeemTitle);

            RedeemEntry redeem = RedeemHelper.GetRedeemByTitle(m_redeemTitle);

            if (redeem == null || redeem.spawnAbilityData == null)
            {
                Jotunn.Logger.LogError("Could not find redeem by title or spawnAbilityData is null!");
                return;
            }

            m_allowDrops = redeem.spawnAbilityData.allowDrops;
            ApplyAllowDrops(m_allowDrops);
        }

        public void SetData(CustomRewardEvent customRewardEvent, SpawnAbilityData spawnAbilityData)
        {
            m_redeemerName = customRewardEvent.RedeemerName;
            m_redeemTitle = customRewardEvent.CustomRewardTitle;
            m_allowDrops = spawnAbilityData.allowDrops;

            m_netView.GetZDO().Set(pieceDataHash, $"{m_redeemerName}|{m_redeemTitle}");

            ApplyAllowDrops(spawnAbilityData.allowDrops);
        }

        public void ApplyAllowDrops(bool allowDrops)
        {
            if (allowDrops)
                return;

            Piece piece = gameObject.GetComponent<Piece>();

            if (piece != null)
                piece.m_resources = new Piece.Requirement[0];
        }
    }
}
