using ModularMagic_Core.Helpers;
using ModularMagic_Core.Models;
using UnityEngine;

namespace ModularMagic_Core.Components
{
    /// <summary>
    /// Colours the save effect in the colour of the staff. The save effect is a networked object, so every player gets
    /// its own copy from the ZNetScene. The preset travels in the ZDO of the effect, so all copies get the same colour.
    /// </summary>
    internal class SaveFXColor : MonoBehaviour
    {
        private const string PresetKey = "MMC_saveFxPreset";

        // Called by the player that saves, right after the effect was created. Start of the component runs later
        // than this, and the remote copies are created from the ZDO that already holds the preset
        public static void SetPreset(GameObject effect, string preset)
        {
            ZNetView netView = effect != null ? effect.GetComponent<ZNetView>() : null;

            if (netView == null || !netView.IsValid())
            {
                Jotunn.Logger.LogWarning("[Imbuements] The save effect has no valid ZNetView, its colour will not be set");
                return;
            }

            netView.GetZDO().Set(PresetKey, preset ?? "");
        }

        public void Start()
        {
            ZNetView netView = GetComponent<ZNetView>();

            if (netView == null || !netView.IsValid())
                return;

            string preset = netView.GetZDO().GetString(PresetKey, "");
            Color particleColor = LightColorPresetHelper.GetParticleColor(preset);
            Color lightColor = LightColorPresetHelper.GetColors(preset).lightColor;

            foreach (Light light in GetComponentsInChildren<Light>(true))
                light.color = lightColor;

            foreach (ParticleSystem system in GetComponentsInChildren<ParticleSystem>(true))
                Recolor(system, particleColor);
        }

        private static void Recolor(ParticleSystem system, Color color)
        {
            ParticleSystem.MainModule main = system.main;
            main.startColor = Recolor(main.startColor, color);

            ParticleSystem.ColorOverLifetimeModule overLifetime = system.colorOverLifetime;
            if (overLifetime.enabled)
                overLifetime.color = Recolor(overLifetime.color, color);

            ParticleSystem.ColorBySpeedModule bySpeed = system.colorBySpeed;
            if (bySpeed.enabled)
                bySpeed.color = Recolor(bySpeed.color, color);

            ParticleSystem.TrailModule trails = system.trails;
            if (trails.enabled)
            {
                trails.colorOverLifetime = Recolor(trails.colorOverLifetime, color);
                trails.colorOverTrail = Recolor(trails.colorOverTrail, color);
            }
        }

        // Replaces the colour and keeps the alpha, so the fading of the effect stays as it was made
        private static ParticleSystem.MinMaxGradient Recolor(ParticleSystem.MinMaxGradient source, Color color)
        {
            switch (source.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return WithAlpha(source.color, color);
                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(WithAlpha(source.colorMin, color), WithAlpha(source.colorMax, color));
                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(Recolor(source.gradient, color));
                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(Recolor(source.gradientMin, color), Recolor(source.gradientMax, color));
                case ParticleSystemGradientMode.RandomColor:
                    ParticleSystem.MinMaxGradient random = new ParticleSystem.MinMaxGradient(Recolor(source.gradient, color));
                    random.mode = ParticleSystemGradientMode.RandomColor;
                    return random;
                default:
                    return source;
            }
        }

        private static Gradient Recolor(Gradient source, Color color)
        {
            GradientColorKey[] colorKeys = source.colorKeys;

            for (int i = 0; i < colorKeys.Length; i++)
                colorKeys[i].color = color;

            Gradient result = new Gradient();
            result.mode = source.mode;
            result.SetKeys(colorKeys, source.alphaKeys);
            return result;
        }

        private static Color WithAlpha(Color original, Color color)
        {
            return new Color(color.r, color.g, color.b, original.a);
        }
    }
}
