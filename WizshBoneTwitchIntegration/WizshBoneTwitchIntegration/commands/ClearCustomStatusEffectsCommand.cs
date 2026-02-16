using Jotunn.Entities;
using System;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Models;

namespace WizshBoneTwitchIntegration.Commands
{
    internal class ClearCustomStatusEffectsCommand : ConsoleCommand
    {
        public override string Name => "ClearCustomStatusEffects";

        public override string Help => "Remove all custom status effects set by the Twitch integration";

        public override void Run(string[] args)
        {
            try
            {
                TwitchCustomStatusEffect customStatusEffect = Game.instance.gameObject.GetComponent<TwitchCustomStatusEffect>();
                foreach (StatusEffectData statusEffect in customStatusEffect.GetStatusEffects())
                {
                    customStatusEffect.RemoveStatusEffect(statusEffect);
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }
    }
}
