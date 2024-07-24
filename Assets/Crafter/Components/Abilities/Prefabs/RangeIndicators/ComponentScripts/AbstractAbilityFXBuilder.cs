using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public abstract class AbstractAbilityFXBuilder : AbstractAbilityFX
    {
        protected bool AwakeInitialized = false;
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
