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

        private DashBlinkAbilityChain DashBlinkAbilityChain;

        private Vector3 TargetPosition;

        private PoolBagDco<PlayerComponent> PlayerCloneInstancePool;

        private PlayerComponent PlayerTransparentClone;
        
        public DashAbilityTriggerObserver(
            PlayerClientData playerClientData,
            Vector3 targetPosition,
            P props) : base(
                requiredDuration: 2000L,
                AbilityTriggerFXType.DashTrigger, props)
        {
            PlayerClientData = playerClientData;
            TargetPosition = targetPosition;
        }

        protected override bool PostInstantiateItems(AbstractAbilityFX[] abstractAbilityFXes)
        {
            PlayerClientData playerClientData = PlayerClientData;
            if (TrailEffectsConstants.BlinkRibbonTrailProps.TryGetValue(BlinkRibbonTrailType.DashBlink, out BlinkRibbonTrailProps blinkRibbonTrailProps) &&
                Props.SkillAndAttackIndicatorSystem.PlayerCloneInstancePools.TryGetValue(playerClientData.Id, out PlayerCloneInstancePool))
            {
                // must be casted on the ground.
                Vector3 playerPosition = playerClientData.PlayerComponent.transform.position;
                Vector3 playerRotation = playerClientData.PlayerComponent.transform.localEulerAngles;

                float startRotationY = playerRotation.y;
                float startRotationYCosYAngle = (float)Math.Cos(startRotationY * Mathf.Deg2Rad);
                float startRotationYSinYAngle = (float)Math.Sin(startRotationY * Mathf.Deg2Rad);

                PlayerComponent playerTransparentClone = PlayerCloneInstancePool.InstantiatePooled(playerPosition);
                playerTransparentClone.transform.localEulerAngles = playerRotation;
                playerTransparentClone.OnCloneFXInit();
                PlayerTransparentClone = playerTransparentClone;

                BlinkRibbonTrailRenderer blinkRibbonTrailRenderer = (BlinkRibbonTrailRenderer)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.BlinkRibbonTrailRenderer];
                blinkRibbonTrailRenderer.transform.localEulerAngles = playerRotation;

                long blinkRibbonTrailRequiredDuration = 1000L;
                float blinkRibbonTrailRequiredDurationSec = blinkRibbonTrailRequiredDuration * PartialMathUtil.SECOND_PER_MILLISECOND;

                PlayerClientData = playerClientData;

                (Vector3 localPos, bool useEndPosition)[] trailVertexPositionsLocal = new (Vector3 localPos, bool useEndPosition)[2] {
                        (new Vector3(1.13f, 3.76f, 7.53f), false),
                        (new Vector3(0.2f, 0f, -1.2f), true) };

                TrailMoverBuilder_TargetPos trailMoverBuilderTargetPos = (TrailMoverBuilder_TargetPos)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.TrailMoverBuilder_TargetPos];
                trailMoverBuilderTargetPos.transform.position = playerPosition;
                trailMoverBuilderTargetPos.transform.localEulerAngles = playerRotation;
                trailMoverBuilderTargetPos.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache,
                    Props.SkillAndAttackIndicatorSystem,
                    playerStartOffsetPosition: Vector3.zero,
                    new BlinkRibbonTrailRenderer[1] { blinkRibbonTrailRenderer },
                    blinkRibbonTrailProps: blinkRibbonTrailProps,
                    startRotationY: startRotationY,
                    startRotationYCosYAngle: startRotationYCosYAngle,
                    startRotationYSinYAngle: startRotationYSinYAngle,
                    timeRequiredSec: blinkRibbonTrailRequiredDurationSec,
                    endPositionWorld: TargetPosition,
                    trailVertexPositionsLocal: trailVertexPositionsLocal,
                    trailVertexTrailIndex: 0);

                long blinkRequiredDuration = (long)((Timer.RequiredDuration - blinkRibbonTrailRequiredDuration) * 0.4f);
                PlayerBlinkBuilder playerBlinkSource = (PlayerBlinkBuilder)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.PlayerBlinkBuilder_Source];
                playerBlinkSource.transform.position = playerPosition;
                playerBlinkSource.transform.localEulerAngles = playerRotation;
                playerBlinkSource.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache, playerClientData, playerTransparentClone,
                    playerVertexTargetPos: trailMoverBuilderTargetPos.ClosestTrailPositionsLocal[0],
                    blinkRequiredDuration);

                PlayerBlinkBuilder playerBlinkDest = (PlayerBlinkBuilder)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.PlayerBlinkBuilder_Dest];
                playerBlinkDest.transform.position = TargetPosition;
                playerBlinkDest.transform.localEulerAngles = playerRotation;
                playerBlinkDest.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache, playerClientData, playerTransparentClone,
                    playerVertexTargetPos: trailMoverBuilderTargetPos.ClosestTrailPositionsLocal[1],
                    blinkRequiredDuration);

                DashBlinkAbilityChain dashBlinkAbilityChain = (DashBlinkAbilityChain)abstractAbilityFXes[(int)DashAbilityTriggerTypeInstancePools.DashBlinkAbilityChain];
                dashBlinkAbilityChain.transform.position = playerPosition;
                dashBlinkAbilityChain.transform.localEulerAngles = playerRotation;
                dashBlinkAbilityChain.Initialize(Props.ObserverUpdateProps.ObserverUpdateCache,
                    playerBlinkBuilderSource: playerBlinkSource,
                    blinkTrailBuilder: trailMoverBuilderTargetPos,
                    playerBlinkBuilderDest: playerBlinkDest,
                    startTime: 0L,
                    endTime: Timer.RequiredDuration - 200L);
                DashBlinkAbilityChain = dashBlinkAbilityChain;
                return true;
            }

            return false;
        }

        protected override void TimerConstrainedFixedUpdate()
        {
            DashBlinkAbilityChain.ManualUpdate(Timer.ElapsedTime_RenderThread());
        }
        protected override void OnObserverCompleted()
        {
            // Warning: Potential stale player transparent clone. Workaround: Replace the instance pool reference entirely when meshes change.
            PlayerCloneInstancePool.ReturnPooled(PlayerTransparentClone);

            DashBlinkAbilityChain.CompleteStatefulFX();
        }
    }
    public enum DashAbilityTriggerTypeInstancePools
    {
        PlayerBlinkBuilder_Source,
        PlayerBlinkBuilder_Dest,
        DashBlinkAbilityChain,
        BlinkRibbonTrailRenderer,
        TrailMoverBuilder_TargetPos
    }
}
