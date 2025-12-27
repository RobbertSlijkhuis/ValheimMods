using Jotunn.Managers;
using UnityEngine;

namespace ModularMagic_Core.Helpers
{
    internal class PrefabHelper
    {
        public static GameObject CreateClonedVariant(string cloneName, string orignalName, int level)
        {
            GameObject cloned = PrefabManager.Instance.CreateClonedPrefab(cloneName, orignalName);
            cloned.transform.Find("attach/rune_1").gameObject.SetActive(false);
            cloned.transform.Find($"attach/rune_{level}").gameObject.SetActive(true);
            return cloned;
        }
    }
}
