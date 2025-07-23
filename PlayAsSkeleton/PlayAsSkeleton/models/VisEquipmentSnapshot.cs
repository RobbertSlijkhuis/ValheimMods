using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlayAsSkeleton.models
{
    internal class VisEquipmentSnapshot
    {
        public SkinnedMeshRenderer bodyModel;
        public VisEquipment.PlayerModel[] models;
        public Transform helmet;
        public Transform leftHand;
        public Transform rightHand;
        public Transform backShield;
        public Transform backMelee;
        public Transform backTwohandedMelee;
        public Transform backBow;
        public Transform backAtgeir;
        public Transform backTool;
        public bool isPlayer;

        public VisEquipmentSnapshot(VisEquipment vis)
        {
            Create(vis);
        }

        public void Create(VisEquipment vis)
        {
            bodyModel = vis.m_bodyModel;
            models = vis.m_models;
            helmet = vis.m_helmet;
            leftHand = vis.m_leftHand;
            rightHand = vis.m_rightHand;
            backShield = vis.m_backShield;
            backMelee = vis.m_backMelee;
            backTwohandedMelee = vis.m_backTwohandedMelee;
            backBow = vis.m_backBow;
            backAtgeir = vis.m_backAtgeir;
            backTool = vis.m_backTool;
            isPlayer = vis.m_isPlayer;
        }

        public void Apply(VisEquipment vis)
        {
            vis.m_bodyModel = bodyModel;
            vis.m_models = models;
            vis.m_helmet = helmet;
            vis.m_leftHand = leftHand;
            vis.m_rightHand = rightHand;
            vis.m_backShield = backShield;
            vis.m_backMelee = backMelee;
            vis.m_backTwohandedMelee = backTwohandedMelee;
            vis.m_backBow = backBow;
            vis.m_backAtgeir = backAtgeir;
            vis.m_backTool = backTool;
            vis.m_isPlayer = isPlayer;
        }
    }
}
