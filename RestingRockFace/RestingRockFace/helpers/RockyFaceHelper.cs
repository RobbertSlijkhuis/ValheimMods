using HarmonyLib;

namespace RestingRockFace.Helpers
{
    internal static class RockyFaceHelper
    {
        // Hands the face back to vanilla: drops the stored MatVar and resets MaterialVariation to its
        // "never picked" state, then re-runs its own CheckMaterial. That is the same path a freshly spawned
        // Rocky takes - the owner rolls GetWeightedVariation and broadcasts it. Must run on the ZDO owner.
        public static void ResetToGameDecided(MaterialVariation materialVar, ZNetView netView)
        {
            netView.GetZDO().RemoveInt("MatVar" + materialVar.m_materialIndex);

            Traverse variation = Traverse.Create(materialVar);
            variation.Field("m_variation").SetValue(-1);
            variation.Field("m_isSet").SetValue(false);
            variation.Method("CheckMaterial").GetValue();
        }
    }
}
