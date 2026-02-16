using System;
using System.Collections.Generic;
using UnityEngine;

namespace WizshBoneTwitchIntegration.helpers
{
    internal class RecolorHelper
    {
        public static Dictionary<string, Color> specialViewers = new Dictionary<string, Color>()
        {
            {"1mmun1tyy", new Color(0.149f, 0.745f, 0.02f)},
            {"ItzSjoeky", new Color(0.851f, 0.239f, 0.447f)},
            {"kassiethelittlepumpkin", new Color(0.957f, 0.827f, 0.388f)},
            {"Lothren", new Color(0.859f, 0.29f, 0.247f)},
            {"Phenazo", new Color(0.992f, 0.447f, 0.016f)},
            {"Soma_af", new Color(1f, 0.733f, 0.875f)},
            {"WaterFox1316", new Color(0.259f, 0.773f, 0.886f)},
        };

        public bool IsRedeemerSpecialViewer(string name)
        {
            return specialViewers.ContainsKey(name);
        }
    }
}
