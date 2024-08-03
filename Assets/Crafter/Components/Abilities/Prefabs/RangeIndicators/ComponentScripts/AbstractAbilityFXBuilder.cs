using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public abstract class AbstractAbilityFXBuilder : AbstractAbilityFX
    {
        [NonSerialized]
        protected ObserverUpdateCache ObserverUpdateCache;
        protected bool AwakeInitialized = false;
        protected void Initialize(ObserverUpdateCache observerUpdateCache)
        {
            ObserverUpdateCache = observerUpdateCache;
            InitializeManualAwake();
        }
        protected void InitializeManualAwake()
        {
            if (!AwakeInitialized)
            {
                ManualAwake();
                AwakeInitialized = true;
            }
        }
        public abstract void ManualAwake();
    }
}
