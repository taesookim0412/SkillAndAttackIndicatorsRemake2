using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class DashAbilityTriggerObserverProps : AbstractAbilityTriggerObserverProps
    {
        public ObserverUpdateCache ObserverUpdateCache;
        public DashAbilityTriggerObserverProps(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem, ObserverUpdateProps observerUpdateProps) :
            base(skillAndAttackIndicatorSystem, observerUpdateProps)
        {
        }
    }
    public class DashAbilityTriggerObserver : AbstractAbilityTriggerObserver<DashAbilityTriggerObserverProps>
    {

        public DashAbilityTriggerObserver(AbilityTriggerFXType abilityTriggerFXType,
            DashAbilityTriggerObserverProps props) : base(abilityTriggerFXType, props)
        {
        }

        protected override void UpdateLoop()
        {
            throw new NotImplementedException();
        }
    }
}
