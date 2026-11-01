using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_Core.Helpers
{
    /// <summary>
    /// The secondary attacks of the staffs (nova, rain, summon, ...). A staff mod registers each secondary attack once,
    /// by the name of the projectile prefab the attack spawns. Core then gives the attack its own eitr cost
    /// (the game charges the eitr cost of the main attack for every attack) and, when a cooldown is given,
    /// a cooldown that shows the icon of the staff that cast it.
    /// </summary>
    public static class SecondaryAttackHelper
    {
        internal class Definition
        {
            public StatusEffect cooldown;
            public Func<float> cooldownSeconds;
        }

        private static readonly Dictionary<string, Definition> registered = new Dictionary<string, Definition>();

        /// <summary>
        /// Registers a secondary attack. Call it once at startup (in Awake, after the config is bound).
        /// </summary>
        /// <param name="projectileName">The name of the projectile prefab the attack spawns. It has to be unique for the attack.</param>
        /// <param name="cooldownEffectName">The unique name of the cooldown status effect, null for an attack without a cooldown.</param>
        /// <param name="cooldownDisplayName">The name of the cooldown status effect as the player sees it.</param>
        /// <param name="cooldownSeconds">The duration of the cooldown. It is read every time the cooldown starts, so a changed config value is used.</param>
        public static void Register(string projectileName, string cooldownEffectName = null, string cooldownDisplayName = null, Func<float> cooldownSeconds = null)
        {
            if (string.IsNullOrEmpty(projectileName))
            {
                Jotunn.Logger.LogWarning("[SecondaryAttack] Cannot register a secondary attack without a projectile name");
                return;
            }

            if (registered.ContainsKey(projectileName))
            {
                Jotunn.Logger.LogWarning($"[SecondaryAttack] '{projectileName}' is already registered, the second registration is ignored");
                return;
            }

            Definition definition = new Definition();

            if (!string.IsNullOrEmpty(cooldownEffectName) && cooldownSeconds != null)
            {
                StatusEffect cooldown = ScriptableObject.CreateInstance<StatusEffect>();
                cooldown.name = cooldownEffectName;
                cooldown.m_name = string.IsNullOrEmpty(cooldownDisplayName) ? cooldownEffectName : cooldownDisplayName;
                cooldown.m_tooltip = "Be patient!";
                cooldown.m_ttl = cooldownSeconds();
                ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(cooldown, fixReference: false));

                definition.cooldown = cooldown;
                definition.cooldownSeconds = cooldownSeconds;
            }

            registered[projectileName] = definition;
        }

        internal static bool TryGetDefinition(Attack attack, out Definition definition)
        {
            definition = null;

            return attack != null && attack.m_attackProjectile != null && registered.TryGetValue(attack.m_attackProjectile.name, out definition);
        }

        /// <summary>
        /// The game takes the eitr cost from the main attack of the weapon, also when the secondary attack is used.
        /// A registered secondary attack has its own cost (with the same skill discount the game applies).
        /// </summary>
        internal static bool TryGetEitr(Attack attack, Character character, ItemData weapon, out float eitr)
        {
            eitr = 0f;

            if (character == null || weapon == null || !TryGetDefinition(attack, out _))
                return false;

            eitr = GetEitr(attack, character, weapon);

            return true;
        }

        private static float GetEitr(Attack attack, Character character, ItemData weapon)
        {
            if (attack.m_attackEitr <= 0f)
                return 0f;

            float skillFactor = character.GetSkillFactor(weapon.m_shared.m_skillType);
            return attack.m_attackEitr - attack.m_attackEitr * 0.33f * skillFactor;
        }

        /// <summary>
        /// Returns false when the attack must not start because the staff is still recharging.
        /// A cooldown of 0 (or less) means there is no cooldown.
        /// </summary>
        internal static bool CheckCooldown(Humanoid character, Attack attack)
        {
            if (character == null || !TryGetDefinition(attack, out Definition definition) || definition.cooldown == null)
                return true;

            if (definition.cooldownSeconds() <= 0f)
                return true;

            // The game keeps status effects by the stable hash of their name
            if (character.GetSEMan().HaveStatusEffect(definition.cooldown.NameHash()))
            {
                character.Message(MessageHud.MessageType.Center, "The staff is still recharging!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts the cooldown of an attack that really started. The game can still refuse an attack after
        /// CheckCooldown (not enough eitr or stamina, a dungeon, ...), so the cooldown only starts when Attack.Start succeeded.
        /// </summary>
        internal static void StartCooldown(Humanoid character, Attack attack)
        {
            if (character == null || !TryGetDefinition(attack, out Definition definition) || definition.cooldown == null)
                return;

            float seconds = definition.cooldownSeconds();

            if (seconds <= 0f)
                return;

            // The template is cloned for the player, so the duration and the icon of the active cooldown are set on the copy.
            // The same cooldown is used by every staff with this attack, it shows the icon of the staff that cast it
            StatusEffect activeCooldown = character.GetSEMan().AddStatusEffect(definition.cooldown);

            if (activeCooldown != null)
            {
                activeCooldown.m_ttl = seconds;

                ItemData weapon = character.GetCurrentWeapon();

                if (weapon != null)
                    activeCooldown.m_icon = weapon.GetIcon();
            }
        }
    }
}
