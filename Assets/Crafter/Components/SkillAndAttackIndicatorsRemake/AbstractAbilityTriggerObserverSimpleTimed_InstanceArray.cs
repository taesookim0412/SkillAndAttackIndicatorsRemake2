using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public abstract class AbstractAbilityTriggerObserverSimpleTimed_InstanceArray<P> : AbstractAbilityTriggerObserverSimpleTimed<P> where P : AbstractAbilityTriggerObserverProps
    {
        protected AbstractAbilityFX[] AbilityFXInstances;

        protected AbstractAbilityTriggerObserverSimpleTimed_InstanceArray(long requiredDuration, AbilityTriggerFXType abilityTriggerFXType, P props) : base(requiredDuration, abilityTriggerFXType, props)
        {
        }

        protected override bool TrySetItems()
        {
            PoolBagDco<AbstractAbilityFX>[] abilityTriggerFXInstancePools = AbilityTriggerFXInstancePools;

            AbstractAbilityFX[] abilityFXInstances = new AbstractAbilityFX[abilityTriggerFXInstancePools.Length];

            for (int i = 0; i < abilityTriggerFXInstancePools.Length; i++)
            {
                abilityFXInstances[i] = abilityTriggerFXInstancePools[i].InstantiatePooled(null);
            }

            PostInstantiateItems(abilityFXInstances);
            AbilityFXInstances = abilityFXInstances;
            return base.TrySetItems();
        }

        protected abstract void PostInstantiateItems(AbstractAbilityFX[] abstractAbilityFXes);

        protected override void OnTimerExpired()
        {
            PoolBagDco<AbstractAbilityFX>[] abilityTriggerFXInstancePools = AbilityTriggerFXInstancePools;
            AbstractAbilityFX[] abilityFXInstances = AbilityFXInstances;
            for (int i = 0; i < abilityFXInstances.Length; i++)
            {
                AbstractAbilityFX abilityFXInstance = abilityFXInstances[i];
                abilityFXInstance.CleanUpInstance();
                AbilityTriggerFXInstancePools[i].ReturnPooled(abilityFXInstance);
            }
        }
    }
}
