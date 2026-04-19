namespace WizshBoneTwitchIntegration.Models
{
    internal class ValheimCreature
    {
        public float tier;
        public EffectList spawnEffects;

        public ValheimCreature(float tier, EffectList spawnEffects)
        {
            this.spawnEffects = spawnEffects;
            this.tier = tier;
        }
    }
}
