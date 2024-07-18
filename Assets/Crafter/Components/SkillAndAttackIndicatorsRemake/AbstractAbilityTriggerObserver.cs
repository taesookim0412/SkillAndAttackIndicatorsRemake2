using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Systems.Observers;
using Assets.Crafter.Components.Systems.Observers.AbstractObservers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public abstract class AbstractAbilityTriggerObserverProps : AbstractObserverProps
    {
        public SkillAndAttackIndicatorSystem SkillAndAttackIndicatorSystem;
        protected AbstractAbilityTriggerObserverProps(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem, ObserverUpdateProps observerUpdateProps) : base(observerUpdateProps)
        {
            SkillAndAttackIndicatorSystem = skillAndAttackIndicatorSystem;
        }
    }
    public abstract class AbstractAbilityTriggerObserver<P> : AbstractUpdateObserver<P> where P: AbstractAbilityTriggerObserverProps
    {
        private AbilityTriggerFXType AbilityTriggerFXType;

        private bool TriggerFXSet = false;
        private PoolBagDco<AbstractAbilityFX>[] AbilityTriggerFXInstancePools;

        public AbstractAbilityTriggerObserver(AbilityTriggerFXType abilityTriggerFXType,
            P props) : base(props)
        {
            AbilityTriggerFXType = abilityTriggerFXType;
        }

        public void OnUpdate()
        {
            if (!TriggerFXSet)
            {
                if (Props.SkillAndAttackIndicatorSystem.AbilityTriggerFXInstancePools.TryGetValue(AbilityTriggerFXType, out AbilityTriggerFXInstancePools) &&
                    TrySetItems())
                {
                    TriggerFXSet = true;
                }
                else
                {
                    ObserverStatus = ObserverStatus.Remove;
                    return;
                }
            }
            else
            {
                UpdateLoop();
            }
        }
        protected virtual bool TrySetItems()
        {
            return true;
        }

        protected abstract void UpdateLoop();
    }
    public enum AbilityTriggerFXType
    {
        None,
        DashTrigger
    }
}
