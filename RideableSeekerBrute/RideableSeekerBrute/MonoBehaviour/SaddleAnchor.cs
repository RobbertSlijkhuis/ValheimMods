using System;
using UnityEngine;

namespace RideableSeekerBrute
{ 
    public class SaddleAnchor : MonoBehaviour
    {
        /**
         * Field that holds the Transform of our custom GameObject (SaddleAttachPoint) that renders the Saddle
         * This field is used in the prefix of the Harmony SetSaddle_Patch
         */
        public Transform m_saddle_anchor;
    }
}
