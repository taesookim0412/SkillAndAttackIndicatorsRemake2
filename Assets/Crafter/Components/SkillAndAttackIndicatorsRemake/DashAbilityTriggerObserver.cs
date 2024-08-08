using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFX;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder;
using Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder.Chains;
using Assets.Crafter.Components.Constants;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Models.dpo.TrailEffectsDpo;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
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
        private PlayerClientData PlayerClientData;

        private PortalBuilderChain PortalBuilderChain;

        private Vector3 TargetPosition;
        
        public DashAbilityTriggerObserver(
            Vector3 targetPosition,
            P props) : base(
                requiredDuration: 2000L,
                AbilityTriggerFXType.DashTrigger, props)
        {
            TargetPosition = targetPosition;
        }

        protected override bool PostInstantiateItems(AbstractAbilityFX[] abstractAbilityFXes)
        {
            if (TrailEffectsConstants.BlinkRibbonTrailProps.TryGetValue(BlinkRibbonTrailType.Dual, out BlinkRibbonTrailProps blinkRibbonTrailProps))
            {
                PlayerClientData playerClientData = Props.SkillAndAttackIndicatorSystem.PlayerClientData;
                Vector3 playerPosition = playerClientData.PlayerComponent.transform.position;
                Vector3 playerRotation = playerClientData.PlayerComponent.transform.localEulerAngles;

                float startRotationY = playerRotation.y;
                float startRotationYCosYAngle = (float)Math.Cos(startRotationY * Mathf.Deg2Rad);
                float startRotationYSinYAngle = (float)Math.Sin(startRotationY * Mathf.Deg2Rad);

                CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.CrimsonAuraBlack];
                crimsonAura.transform.localEulerAngles = playerRotation;

                PortalOrbPurple portalOrb = (PortalOrbPurple)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.PortalOrbPurple];
                portalOrb.transform.localEulerAngles = playerRotation;

                long blinkRibbonTrailRequiredDuration = 1000L;
                float blinkRibbonTrailRequiredDurationSec = blinkRibbonTrailRequiredDuration * 0.001f;
                long portalRequiredDuration = (long)((Timer.RequiredDuration - blinkRibbonTrailRequiredDuration) * 0.4f);
                PortalBuilder portalSource = (PortalBuilder)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.PortalBuilder_Source];
                portalSource.transform.position = playerPosition;
                portalSource.transform.localEulerAngles = playerRotation;
                portalSource.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, portalRequiredDuration,
                    setPlayerInactive: true, isClone: false);

                PortalBuilder portalDest = (PortalBuilder)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.PortalBuilder_Dest];
                portalDest.transform.position = TargetPosition;
                portalDest.transform.localEulerAngles = playerRotation;
                portalDest.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache, playerClientData, portalOrb, crimsonAura, portalRequiredDuration,
                    setPlayerInactive: false, isClone: false);

                BlinkRibbonTrailRenderer blinkRibbonTrailRenderer1 = (BlinkRibbonTrailRenderer)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.BlinkRibbonTrailRenderer1];
                blinkRibbonTrailRenderer1.transform.localEulerAngles = playerRotation;

                BlinkRibbonTrailRenderer blinkRibbonTrailRenderer2 = (BlinkRibbonTrailRenderer)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.BlinkRibbonTrailRenderer2];
                blinkRibbonTrailRenderer2.transform.localEulerAngles = playerRotation;

                TrailMoverBuilder_TargetPos trailMoverBuilderTargetPos = (TrailMoverBuilder_TargetPos)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.TrailMoverBuilder_TargetPos];
                trailMoverBuilderTargetPos.transform.position = playerPosition + portalSource.PortalOrbOffsetPosition;
                trailMoverBuilderTargetPos.transform.localEulerAngles = playerRotation;
                trailMoverBuilderTargetPos.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache, new BlinkRibbonTrailRenderer[2] { blinkRibbonTrailRenderer1, blinkRibbonTrailRenderer2 },
                    blinkRibbonTrailProps: blinkRibbonTrailProps,
                    startRotationY: startRotationY,
                    startRotationYCosYAngle: startRotationYCosYAngle,
                    startRotationYSinYAngle: startRotationYSinYAngle,
                    timeRequiredSec: blinkRibbonTrailRequiredDurationSec,
                    endPositionWorld: TargetPosition);

                PlayerClientData = playerClientData;
                PortalBuilderChain = new PortalBuilderChain(
                    portalSource: portalSource, 
                    portalDest: portalDest,
                    trailMoverBuilderTargetPos,
                    startTime: 0L,
                    endTime: Timer.RequiredDuration - 200L);
                return true;
            }

            return false;
        }

        protected override void TimerConstrainedFixedUpdate()
        {
            PortalBuilderChain.UpdatePortals(Timer.ElapsedTime_FixedUpdateThread());
        }
        protected override void OnObserverCompleted()
        {
            GameObject playerGameObject = PlayerClientData.PlayerComponent.gameObject;
            if (!playerGameObject.activeSelf)
            {
                playerGameObject.SetActive(true);
            }
        }
    }
    public enum DashAbilityTriggerTypeInstancePools
    {
        CrimsonAuraBlack,
        PortalOrbPurple,
        PortalBuilder_Source,
        PortalBuilder_Dest,
        BlinkRibbonTrailRenderer1,
        BlinkRibbonTrailRenderer2,
        TrailMoverBuilder_TargetPos
    }
}
