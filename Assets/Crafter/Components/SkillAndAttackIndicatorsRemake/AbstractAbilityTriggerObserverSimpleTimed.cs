using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public abstract class AbstractAbilityTriggerObserverSimpleTimed<P> : AbstractAbilityTriggerObserver<P> where P : AbstractAbilityTriggerObserverProps
    {
        protected TimerStructDco_Observer Timer;
        protected AbstractAbilityTriggerObserverSimpleTimed(
            long requiredDuration,
            AbilityTriggerFXType abilityTriggerFXType, P props) : base(abilityTriggerFXType, props)
        {
            Timer.ObserverUpdateCache = props.ObserverUpdateProps.ObserverUpdateCache;
            Timer.RequiredDuration = requiredDuration;
        }
        protected override bool TrySetItems()
        {
            Timer.LastCheckedTime = Props.ObserverUpdateProps.ObserverUpdateCache.UpdateTickTimeFixedUpdate;
            return true;
        }
        protected override void ActiveUpdate()
        {
            if (Timer.IsTimeNotElapsed_FixedUpdateThread())
            {
                TimerConstrainedFixedUpdate();
            }
            else
            {
                OnTimerExpired();
                CompleteObserver();
            }
        }
        protected abstract void TimerConstrainedFixedUpdate();
        protected abstract void OnTimerExpired();
    }
}
