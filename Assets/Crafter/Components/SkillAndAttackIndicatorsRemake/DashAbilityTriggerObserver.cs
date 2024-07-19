using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder;
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
        public ObserverUpdateCache ObserverUpdateCache;
        public DashAbilityTriggerObserverProps(SkillAndAttackIndicatorSystem skillAndAttackIndicatorSystem, ObserverUpdateProps observerUpdateProps) :
            base(skillAndAttackIndicatorSystem, observerUpdateProps)
        {
        }
    }
    public class DashAbilityTriggerObserver : AbstractAbilityTriggerObserver<DashAbilityTriggerObserverProps>
    {
        private Vector3 TargetPosition;

        private TimerStructDco_Observer Timer;

        private PortalBuilder PortalSource;
        private PortalBuilder PortalDest;
        public DashAbilityTriggerObserver(
            Vector3 targetPosition,
            AbilityTriggerFXType abilityTriggerFXType,
            DashAbilityTriggerObserverProps props) : base(abilityTriggerFXType, props)
        {
            TargetPosition = targetPosition;
            // hardcoded
            Timer.RequiredDuration = 1000L;
            Timer.ObserverUpdateCache = props.ObserverUpdateCache;
        }

        protected override bool TrySetItems()
        {
            Timer.LastCheckedTime = Props.ObserverUpdateCache.UpdateTickTimeFixedUpdate;
            CreateItems();

            return true;
        }
        private void CreateItems()
        {
            PlayerClientData playerClientData = Props.SkillAndAttackIndicatorSystem.PlayerClientData;
            Vector3 playerRotation = playerClientData.PlayerComponent.transform.localEulerAngles;

            PoolBagDco<AbstractAbilityFX> crimsonAuraInstancePool = AbilityTriggerFXInstancePools[(int)DashAbilityTriggerTypeInstancePools.CrimsonAuraBlack];
            CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack)crimsonAuraInstancePool.InstantiatePooled(null);
            crimsonAura.transform.localEulerAngles = playerRotation;

            PoolBagDco<AbstractAbilityFX> portalOrbInstancePool = AbilityTriggerFXInstancePools[(int)DashAbilityTriggerTypeInstancePools.PortalOrbPurple];
            PortalOrbPurple portalOrb = (PortalOrbPurple)portalOrbInstancePool.InstantiatePooled(null);
            portalOrb.transform.localEulerAngles = playerRotation;

            PoolBagDco<AbstractAbilityFX> portalBuilderSrcInstancePool = AbilityTriggerFXInstancePools[(int)DashAbilityTriggerTypeInstancePools.PortalBuilder_Source];
            PortalBuilder portalSource = (PortalBuilder)portalBuilderSrcInstancePool.InstantiatePooled(playerClientData.PlayerComponent.transform.position);
            portalSource.transform.localEulerAngles = playerRotation;
            portalSource.Initialize(Props.ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, (long)(Timer.RequiredDuration * 0.5f));
            PortalSource = portalSource;

            PoolBagDco<AbstractAbilityFX> portalBuilderDestInstancePool = AbilityTriggerFXInstancePools[(int)DashAbilityTriggerTypeInstancePools.PortalBuilder_Dest];
            PortalBuilder portalDest = (PortalBuilder)portalBuilderDestInstancePool.InstantiatePooled(TargetPosition);
            portalDest.transform.localEulerAngles = playerRotation;
            portalDest.Initialize(Props.ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, (long)(Timer.RequiredDuration * 0.5f));
            PortalDest = portalDest;

        }

        protected override void ActiveUpdate()
        {
            
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
