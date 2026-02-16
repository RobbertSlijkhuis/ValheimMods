using UnityEngine;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Components
{
    internal class TwitchPersistentDamage : MonoBehaviour
    {
        public ZNetView m_netView;
        public DamageData m_damage;
        public string m_damageString;
        public string m_damageHash = "PersistentDamage_WBTI";

        public void Awake()
        {
            m_netView = gameObject.GetComponent<ZNetView>();

            if (m_netView == null || m_netView.GetZDO() == null)
            {
                Jotunn.Logger.LogError("Could not find ZNetView in persistent damage!");
                return;
            }

            m_damageString = m_netView.GetZDO().GetString(m_damageHash, "");

            if (m_damageString == "")
                return;

            Jotunn.Logger.LogWarning("Found damage string: " + m_damageString);
            m_damage = StringToDamageData(m_damageString);

            ApplyDamageToAOE(m_damage);
        }

        public void ApplyDamageToAOE(DamageData damageData)
        {
            Aoe aoe = gameObject.GetComponentInChildren<Aoe>(true);

            if (aoe == null)
            {
                Jotunn.Logger.LogError("Could not find AOE to apply persistent damage to!");
                return;
            }
            
            aoe.m_damage = DamageHelper.ConvertToDamageTypes(damageData);
        }

        public void SetData(DamageData damageData)
        {
            string damageDataString = DamageDataToString(damageData);
            m_netView.GetZDO().Set(m_damageHash, damageDataString);
            m_damageString = damageDataString;

            ApplyDamageToAOE(damageData);
        }

        public void SetData(HitData.DamageTypes damages)
        {
            DamageData damageData = new DamageData();
            DamageHelper.SetFromDamageTypes(damageData, damages);
            SetData(damageData);
        }

        public string DamageDataToString(DamageData damageData)
        {
            return $"{damageData.blunt}|{damageData.chop}|{damageData.damage}|{damageData.fire}|{damageData.frost}|{damageData.lightning}|{damageData.pickaxe}|{damageData.pierce}|{damageData.poison}|{damageData.slash}|{damageData.spirit}|{damageData.basedOnMaxHealthAndArmor}|{damageData.maxHealthPercentage}|{damageData.armorPercentage}";
        }

        public DamageData StringToDamageData(string value)
        {
            string[] data = value.Split('|');
            DamageData damageData = new DamageData();
            damageData.blunt = float.Parse(data[0]);
            damageData.chop = float.Parse(data[1]);
            damageData.damage = float.Parse(data[2]);
            damageData.fire = float.Parse(data[3]);
            damageData.frost = float.Parse(data[4]);
            damageData.lightning = float.Parse(data[5]);
            damageData.pickaxe = float.Parse(data[6]);
            damageData.pierce = float.Parse(data[7]);
            damageData.poison = float.Parse(data[8]);
            damageData.slash = float.Parse(data[9]);
            damageData.spirit = float.Parse(data[10]);
            damageData.basedOnMaxHealthAndArmor = bool.Parse(data[11]);
            damageData.maxHealthPercentage = float.Parse(data[12]);
            damageData.armorPercentage = float.Parse(data[13]);

            return damageData;
        }
    }
}
