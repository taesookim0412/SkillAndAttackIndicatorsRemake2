using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class CameraMoverBuilderProps : MonoBehaviour
    {
        [SerializeField]
        public Transform FollowTransform;
        [SerializeField]
        public Transform LookAtTransform;
    }
}
