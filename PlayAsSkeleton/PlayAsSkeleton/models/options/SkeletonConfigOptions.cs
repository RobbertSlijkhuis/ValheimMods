using PlayAsSkeleton.Types;
using UnityEngine;

namespace PlayAsSkeleton.Models
{
    internal class SkeletonConfigOptions
    {
        public bool enabled = false;
        public string sectionName = "General";
        public string skin = SkinType.Normal;
        public bool canSwim = true;
        public bool hideHelmet = false;
        public bool hideCape = false;
        public bool hideChest = false;
        public bool hideUtility = false;
        public bool hideLegs = false;
    }
}
