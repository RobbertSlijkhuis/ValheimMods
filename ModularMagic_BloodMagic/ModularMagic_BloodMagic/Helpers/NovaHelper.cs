using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using UnityEngine;
using MMAoe = ModularMagic_Core.Components.MMAoe;

namespace ModularMagic_BloodMagic.Helpers
{
    /// <summary>
    /// The Nova rune: the secondary attack of the scythe makes every active apparition of the caster explode. The caster marks its loaded
    /// active apparitions in its own ZDO, with the damage of the explosion (Release). The client that owns an apparition sees the mark and
    /// explodes it (ApparitionController.TryExplode). Nothing is sent that can be lost, it does not matter which client owns an apparition,
    /// and an apparition that is owned by another client later still finds the mark. The death removes the apparition, so it explodes once.
    /// Like the reanimation (ApparitionHelper.TryRequestReanimation), the owner needs the summoner loaded to read the mark.
    /// </summary>
    internal static class NovaHelper
    {
        /// <summary>The key of the explosion of the apparition with this ZDO in the ZDO of its summoner.</summary>
        public static int GetKey(ZDOID apparition)
        {
            return ("MMBM_Nova_" + apparition).GetStableHashCode();
        }

        /// <summary>
        /// Called by Core before the Nova starts (SecondaryAttackHelper.Register). Without an active apparition there is nothing to explode,
        /// so the attack is blocked and costs nothing.
        /// </summary>
        public static bool PrepareStart(Humanoid character, Attack attack)
        {
            Player? player = character as Player;

            if (player == null || player != Player.m_localPlayer)
                return true;

            ApparitionHelper.Count(player.GetPlayerID(), out int active);

            if (active > 0)
                return true;

            player.Message(MessageHud.MessageType.Center, "You have no apparitions to release");
            return false;
        }

        /// <summary>
        /// Called on the client of the caster when the Nova fires (NovaTrigger). The damage of the hit is the damage of the scythe with the
        /// skill and the status effects of the caster applied, the explosion deals that times the config multiplier. Every loaded active
        /// apparition of the caster gets the explosion in the ZDO of the caster.
        /// </summary>
        public static void Release(Character owner, HitData hitData)
        {
            Player? player = owner as Player;

            if (player == null || player != Player.m_localPlayer || hitData == null || player.m_nview == null || !player.m_nview.IsValid())
                return;

            HitData.DamageTypes damage = hitData.m_damage.Clone();
            damage.Modify(PluginConfig.novaDamageMultiplier.Value);
            byte[] data = WriteDamage(damage);

            ZDO zdo = player.m_nview.GetZDO();
            long playerID = player.GetPlayerID();
            int marked = 0;

            foreach (ApparitionController apparition in ApparitionHelper.Loaded)
            {
                if (apparition == null || apparition.Summoner != playerID || apparition.IsHusk || apparition.NovaKey == 0)
                    continue;

                zdo.Set(apparition.NovaKey, data);
                marked++;
            }

            Jotunn.Logger.LogWarning($"[Nova] Released {marked} apparitions, each explodes for {damage.GetTotalDamage():F1} damage");
        }

        /// <summary>The explosion the summoner gave the apparition with this key, false when it has none.</summary>
        public static bool TryGetExplosion(Player summoner, int key, out HitData.DamageTypes damage)
        {
            damage = new HitData.DamageTypes();

            ZDO? zdo = summoner.m_nview != null && summoner.m_nview.IsValid() ? summoner.m_nview.GetZDO() : null;
            byte[]? data = zdo != null && key != 0 ? zdo.GetByteArray(key) : null;

            if (data == null || data.Length == 0)
                return false;

            damage = ReadDamage(data);
            return true;
        }

        /// <summary>
        /// The owner of the apparition explodes it: an area hit around it with the apparition as the attacker (see MMAoe, which sends every
        /// hit to the owner of the target and only hits enemies of the apparition). The apparition is the attacker, so the hits do not charge
        /// the scythe of its summoner. The effects are networked, so only the owner spawns them.
        /// </summary>
        public static void Explode(Character apparition, HitData.DamageTypes damage)
        {
            Vector3 center = apparition.GetCenterPoint();

            GameObject hitObject = new GameObject("nova_explosion_MMBM");
            hitObject.transform.position = center;

            MMAoe aoe = hitObject.AddComponent<MMAoe>();
            aoe.sphere = true;
            aoe.radius = PluginConfig.novaRadius.Value;
            aoe.hitProps = false;

            HitData hit = new HitData();
            hit.m_damage = damage;
            hit.SetAttacker(apparition);

            // MMAoe destroys its object after the hit
            aoe.Setup(apparition, Vector3.zero, 0f, hit, null, null);

            SpawnEffects(center);
        }

        private static void SpawnEffects(Vector3 position)
        {
            GameObject spikes = ModularMagic_BloodMagic.Instance.prefabs.SpikeEffect;
            GameObject spores = ModularMagic_BloodMagic.Instance.prefabs.SporeEffect;

            if (spikes != null)
                Object.Instantiate(spikes, position, Quaternion.identity);

            if (spores != null)
                Object.Instantiate(spores, position, Quaternion.identity);
        }

        // The damage of an explosion, in the order of the fields of HitData.DamageTypes
        private static byte[] WriteDamage(HitData.DamageTypes damage)
        {
            ZPackage package = new ZPackage();
            package.Write(damage.m_damage);
            package.Write(damage.m_blunt);
            package.Write(damage.m_slash);
            package.Write(damage.m_pierce);
            package.Write(damage.m_chop);
            package.Write(damage.m_pickaxe);
            package.Write(damage.m_fire);
            package.Write(damage.m_frost);
            package.Write(damage.m_lightning);
            package.Write(damage.m_poison);
            package.Write(damage.m_spirit);
            package.Write(damage.m_nonPlayer);

            return package.GetArray();
        }

        private static HitData.DamageTypes ReadDamage(byte[] data)
        {
            ZPackage package = new ZPackage(data);
            HitData.DamageTypes damage = new HitData.DamageTypes();
            damage.m_damage = package.ReadSingle();
            damage.m_blunt = package.ReadSingle();
            damage.m_slash = package.ReadSingle();
            damage.m_pierce = package.ReadSingle();
            damage.m_chop = package.ReadSingle();
            damage.m_pickaxe = package.ReadSingle();
            damage.m_fire = package.ReadSingle();
            damage.m_frost = package.ReadSingle();
            damage.m_lightning = package.ReadSingle();
            damage.m_poison = package.ReadSingle();
            damage.m_spirit = package.ReadSingle();
            damage.m_nonPlayer = package.ReadSingle();

            return damage;
        }
    }
}
