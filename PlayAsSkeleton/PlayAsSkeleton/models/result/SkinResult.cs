using System;
using UnityEngine;

namespace PlayAsSkeleton.Models
{
    internal class SkinResult
    {
        public Material body;
        public Material eyes;

        public SkinResult(Material body, Material eyes)
        {
            this.body = body;
            this.eyes = eyes;
        }
    }
}
