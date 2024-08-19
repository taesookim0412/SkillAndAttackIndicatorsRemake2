using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.Projectors
{
    public abstract class AbstractProjector : MonoBehaviour
    {
        public abstract void ManualUpdate(float fillProgress);
    }
}
