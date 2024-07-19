using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder.Chains;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class DashAbilityTriggerObserverProps : AbstractAbilityTriggerObserverProps
    {
        public DashAbilityTriggerObserverProps(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem, ObserverUpdateProps observerUpdateProps) :
            base(skillAndAttackIndicatorSystem, observerUpdateProps)
        {
        }
    }
    public class DashAbilityTriggerObserver<P> : AbstractAbilityTriggerObserverSimpleTimed_InstanceArray<P> where P: DashAbilityTriggerObserverProps
    {
        private PortalBuilderChain PortalBuilderChain;

        private Vector3 TargetPosition;
        
        public DashAbilityTriggerObserver(
            Vector3 targetPosition,
            P props) : base(
                requiredDuration: 1000L,
                AbilityTriggerFXType.DashTrigger, props)
        {
            TargetPosition = targetPosition;
        }

        protected override void PostInstantiateItems(AbstractAbilityFX[] abstractAbilityFXes)
        {
            PlayerClientData playerClientData = Props.SkillAndAttackIndicatorSystem.PlayerClientData;
            Vector3 playerRotation = playerClientData.PlayerComponent.transform.localEulerAngles;

            CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.CrimsonAuraBlack];
            crimsonAura.transform.localEulerAngles = playerRotation;

            PortalOrbPurple portalOrb = (PortalOrbPurple)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.PortalOrbPurple];
            portalOrb.transform.localEulerAngles = playerRotation;

            long portalRequiredDuration = (long)(Timer.RequiredDuration * 0.4f);
            PortalBuilder portalSource = (PortalBuilder)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.PortalBuilder_Source];
            portalSource.transform.position = TargetPosition;
            portalSource.transform.localEulerAngles = playerRotation;
            portalSource.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, portalRequiredDuration);

            PortalBuilder portalDest = (PortalBuilder)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.PortalBuilder_Dest];
            portalDest.transform.position = TargetPosition;
            portalDest.transform.localEulerAngles = playerRotation;
            portalDest.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, portalRequiredDuration);

            PortalBuilderChain = new PortalBuilderChain(portalSource, portalDest, 
                startTime: 0L, 
                endTime: (long) (Timer.RequiredDuration * 0.8f),
                inverted: false);
        }

        protected override void TimerConstrainedFixedUpdate()
        {
            PortalBuilderChain.UpdatePortals(Timer.ElapsedTime_FixedUpdateThread());
        }
    }
    public enum DashAbilityTriggerTypeInstancePools
    {
        CrimsonAuraBlack,
        PortalOrbPurple,
        PortalBuilder_Source,
        PortalBuilder_Dest
    }
}
