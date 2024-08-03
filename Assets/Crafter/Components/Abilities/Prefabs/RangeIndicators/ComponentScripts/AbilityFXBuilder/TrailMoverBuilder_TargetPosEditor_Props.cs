﻿using Assets.Crafter.Components.Models.dpo.TrailEffectsDpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
#if UNITY_EDITOR
    [Serializable]
    public class TrailMoverBuilder_TargetPosEditor_Props : MonoBehaviour
    {
        [SerializeField]
        public Vector3 EndPositionLocal;
        [SerializeField]
        public int PropsIndex;
        [SerializeField]
        public int NumProps;
        [SerializeField]
        public BlinkRibbonTrailProps[] BlinkRibbonTrailProps = new BlinkRibbonTrailProps[0];

        [SerializeField]
        public Transform MarkersParent;
    }
#endif
}
