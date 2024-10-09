using Assets.Crafter.Components.Editors.ComponentScripts;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class PlayerBlinkBuilder : AbstractAbilityFXBuilder
    {
        private static readonly int PlayerBlinkStateLength = Enum.GetNames(typeof(PlayerBlinkState)).Length;

        [NonSerialized]
        public PlayerClientData PlayerClientData;
        [NonSerialized]
        public PlayerComponent PlayerTransparentClone;
        [NonSerialized]
        public Vector3 PlayerVertexTargetPos;

        [Range(0f, 2f), SerializeField]
        private float PlayerOpacityDuration;
        [Range(0f, 2f), SerializeField]
        private float PlayerOpaqueDuration;

        [SerializeField]
        public bool IsTeleportSource;

        // incompatible with onvalidate
        [NonSerialized, HideInInspector]
        private float RequiredDurationMult;

        [NonSerialized, HideInInspector]
        private long VertexTargetPosStartTime;

        [NonSerialized, HideInInspector]
        private float PlayerOpacityDurationReciprocal;

        [NonSerialized, HideInInspector]
        public AnimFrameProps AnimFrameProps;

        [NonSerialized, HideInInspector]
        public bool PlayAnimFrame;

        [NonSerialized, HideInInspector]
        private bool AnimFrameDone = false;

        public override void ManualAwake()
        {
        }
        protected virtual float GetRequiredDurationMillis(ObserverUpdateCache observerUpdateCache)
        {
            return (PlayerOpacityDuration + PlayerOpaqueDuration) * 1000f +
                (observerUpdateCache.UpdateRenderThreadAverageTimeStep * PlayerBlinkStateLength * 2f);
        }
        protected virtual void ResetRequiredDuration()
        {
            PlayerOpacityTimer.RequiredDuration = (long)(PlayerOpacityDuration * 1000f);
            PlayerOpaqueTimer.RequiredDuration = (long)(PlayerOpaqueDuration * 1000f);
        }
        protected virtual void UpdateOnValidatePositions()
        {

        }

        public void OnValidate()
        {
            ResetRequiredDuration();

            ManualAwake();
        }

        [NonSerialized, HideInInspector]
        public PlayerBlinkState PlayerBlinkState;
        [NonSerialized, HideInInspector]
        private TimerStructDco_Observer PlayerOpacityTimer;
        [NonSerialized, HideInInspector]
        private TimerStructDco_Observer PlayerOpaqueTimer;
        [NonSerialized, HideInInspector]
        private bool RequiredDurationsModified = false;

        //public string DebugLogRequiredDurations()
        //{
        //    return $"{PortalScaleTimer.RequiredDuration}, {PlayerOpacityTimer.RequiredDuration}, {PlayerOpaqueTimer.RequiredDuration}";
        //}

        protected virtual void InitializeDurations(float requiredDurationMultTimes1000)
        {
            PlayerOpacityTimer.RequiredDuration = (long)(PlayerOpacityDuration * requiredDurationMultTimes1000);
            PlayerOpaqueTimer.RequiredDuration = (long)(PlayerOpaqueDuration * requiredDurationMultTimes1000);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="observerUpdateCache"></param>
        /// <param name="playerClientData"></param>
        /// <param name="playerTransparentClone"></param>
        /// <param name="playerVertexTargetPos"></param>
        /// <param name="playerVertexTargetPosOffset">Offset specific to the animation.</param>
        /// <param name="animFrameProps"></param>
        /// <param name="playAnimFrame"></param>
        /// <param name="durationAllowed"></param>
        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PlayerComponent playerTransparentClone, Vector3 playerVertexTargetPos,
            Vector3 playerVertexTargetPosOffset,
            AnimFrameProps animFrameProps,
            bool playAnimFrame, long? durationAllowed)
        {
            base.Initialize(observerUpdateCache);

            if (durationAllowed != null)
            {
                float requiredDurationMultTimes1000 = (long)durationAllowed / GetRequiredDurationMillis(observerUpdateCache) * 1000f;
                InitializeDurations(requiredDurationMultTimes1000);
                RequiredDurationsModified = true;
            }
            else
            {
                if (RequiredDurationsModified)
                {
                    ResetRequiredDuration();
                    RequiredDurationsModified = false;
                }
            }

            PlayerOpacityTimer.ObserverUpdateCache = observerUpdateCache;
            PlayerOpaqueTimer.ObserverUpdateCache = observerUpdateCache;

            PlayerClientData = playerClientData;

            PlayerTransparentClone = playerTransparentClone;

            PlayerVertexTargetPos = playerVertexTargetPos + playerVertexTargetPosOffset;
            
            AnimFrameProps = animFrameProps;

            PlayAnimFrame = playAnimFrame;

            AnimFrameDone = false;

            if (!IsTeleportSource)
            {
                playerClientData.PlayerComponent.HideMeshes();
            }

            PlayerBlinkState = PlayerBlinkState.PlayerCreate;
        }
        public virtual void ManualUpdate()
        {
            if (Completed)
            {
                return;
            }
            switch (PlayerBlinkState)
            {
                case PlayerBlinkState.PlayerCreate:
                    // derived class could be active already.
                    if (!Active)
                    {
                        Active = true;
                    }
                    PlayerTransparentClone.gameObject.SetActive(true);
                    VertexTargetPosStartTime = ObserverUpdateCache.UpdateTickTimeRenderThread;
                    PlayerOpacityDurationReciprocal = PlayerOpacityDuration > 0f ? 1f / PlayerOpacityDuration : 0f;
                    PlayerTransparentClone.SetMaterialVertexTargetPos(PlayerVertexTargetPos, !IsTeleportSource);
                    PlayerClientData.PlayerComponent.transform.position = transform.position;
                    float playerTransparentCloneOpacity;
                    if (IsTeleportSource)
                    {
                        PlayerClientData.PlayerComponent.HideMeshes();
                        playerTransparentCloneOpacity = 1f;
                    }
                    else
                    {
                        playerTransparentCloneOpacity = 0f;
                    }

                    PlayerTransparentClone.SetCloneFXOpacity(playerTransparentCloneOpacity);

                    PlayerOpacityTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeRenderThread;
                    PlayerBlinkState = PlayerBlinkState.PlayerOpacity;
                    break;
                case PlayerBlinkState.PlayerOpacity:
                    if (PlayerOpacityTimer.IsTimeNotElapsed_RenderThread())
                    {
                        float scalePercentage = PlayerOpacityTimer.RemainingDurationPercentage_RenderThread();
                        if (IsTeleportSource)
                        {
                            scalePercentage = 1f - scalePercentage;
                        }
                        SetMaterialVertexPosTimeNormalized();
                        PlayerTransparentClone.SetCloneFXOpacity(scalePercentage);
                    }
                    else
                    {
                        PlayerTransparentClone.gameObject.SetActive(false);
                        if (!IsTeleportSource)
                        {
                            PlayerClientData.PlayerComponent.ShowMeshes();
                        }

                        PlayerOpaqueTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeRenderThread;
                        PlayerBlinkState = PlayerBlinkState.PlayerDespawn;
                    }
                    // For some reason, playing the anim will only work the frame after the gameobject is set active.
                    if (!AnimFrameDone)
                    {
                        if (PlayAnimFrame)
                        {
                            PlayerTransparentClone.Animator.Play(AnimFrameProps.AnimFullPathHash, AnimFrameProps.AnimLayerIndex,
                                AnimFrameProps.AnimClipFrameNormalized);
                            PlayerTransparentClone.Animator.Update(PartialMathUtil.ONE_FRAME);
                            PlayerTransparentClone.Animator.speed = 0f;
                            //var stateInfo = PlayerTransparentClone.Animator.GetCurrentAnimatorStateInfo(AnimFrameProps.AnimLayerIndex);
                            //// Doesn't check the normalized time.
                            //if (stateInfo.fullPathHash == AnimFrameProps.AnimFullPathHash)
                            //{
                            //    Debug.Log($"IsSource: {IsTeleportSource}. Hash correct.");
                            //    AnimFrameDone = true;
                            //}
                            //else
                            //{
                            //    Debug.Log($"IsSource: {IsTeleportSource}. Hash incorrect.");
                            //    //var stateInfo = PlayerTransparentClone.Animator.GetCurrentAnimatorStateInfo(AnimFrameProps.AnimLayerIndex);
                            //    //Debug.Log(AnimFrameProps.AnimFullPathHash);
                            //}
                        }
                        AnimFrameDone = true;
                    }
                    //var stateInfo2 = PlayerTransparentClone.Animator.GetCurrentAnimatorStateInfo(AnimFrameProps.AnimLayerIndex);
                    //Debug.Log($"IsSource: {IsTeleportSource}, {stateInfo2.fullPathHash}, {stateInfo2.fullPathHash == AnimFrameProps.AnimFullPathHash}, {AnimFrameProps.AnimFullPathHash}, {stateInfo2.normalizedTime}");
                    break;
                case PlayerBlinkState.PlayerDespawn:
                    if (PlayerOpaqueTimer.IsTimeElapsed_RenderThread())
                    {
                        Complete();
                    }
                    break;
            }
        }

        private void SetMaterialVertexPosTimeNormalized()
        {
            long timeElapsed = ObserverUpdateCache.UpdateTickTimeRenderThread - VertexTargetPosStartTime;
            float timeElapsedNormalized = timeElapsed * PartialMathUtil.SECOND_PER_MILLISECOND * PlayerOpacityDurationReciprocal;
            float timeElapsedNormalizedClamped = Mathf.Clamp01(timeElapsedNormalized);

            PlayerTransparentClone.SetMaterialVertexPosTimeElapsedNormalized(timeElapsedNormalizedClamped);
        }
        // override aswell in the derived class.
        public override void Complete()
        {
            base.Complete();
        }

        public override void CleanUpInstance()
        {
            ObserverUpdateCache = null;
            PlayerClientData = null;
            PlayerTransparentClone = null;
        }
    }
    public enum PlayerBlinkState
    {
        PlayerCreate,
        PlayerOpacity,
        PlayerDespawn
    }
    public class AnimFrameProps
    {
        public int AnimFullPathHash;
        public int AnimLayerIndex;
        public float AnimClipFrameNormalized;

        public AnimFrameProps(int animFullPathHash, int animLayerIndex, float animClipFrameNormalized)
        {
            AnimFullPathHash = animFullPathHash;
            AnimLayerIndex = animLayerIndex;
            AnimClipFrameNormalized = animClipFrameNormalized;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerBlinkBuilder))]
    public class PlayerBlinkBuilderEditor : AbstractEditor<PlayerBlinkBuilder>
    {
        public PlayerComponent PlayerComponent;
        public bool PlayerComponentOverride;
        public Vector3 PlayerVertexTargetPos = new Vector3(0f, 1f, 1f);
        private bool PlayerVertexTargetPosOverride = false;
        public Vector3 PlayerVertexTargetPosOffset = Vector3.zero;
        public long? RequiredDuration = null;
        public AnimFrameProps AnimFrameProps;
        private bool PlayAnimFrame = false;

        public void SetOverrides(PlayerComponent playerComponent, Vector3 playerVertexTargetPos,
            Vector3 playerVertexTargetPosOffset,
            AnimFrameProps animFrameProps, bool playAnimFrame) 
        {
            if (playerComponent != null)
            {
                PlayerComponent = playerComponent;
                PlayerComponentOverride = true;
            }
            PlayerVertexTargetPos = playerVertexTargetPos;
            PlayerVertexTargetPosOverride = true;

            PlayerVertexTargetPosOffset = playerVertexTargetPosOffset;

            AnimFrameProps = animFrameProps;
            PlayAnimFrame = playAnimFrame;
        }
        protected override bool OnInitialize(PlayerBlinkBuilder instance, ObserverUpdateCache observerUpdateCache)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
            if (system != null)
            {
                PlayerComponent playerComponentPrefab = PlayerComponentOverride ? PlayerComponent : system.PlayerComponent;

                if (playerComponentPrefab != null)
                {
                    // for lack of better way to create the same player component, just use a transparent clone for the editor.
                    PlayerComponent playerComponentInstance = playerComponentPrefab.CreateInactiveTransparentCloneInstance();
                    playerComponentInstance.transform.SetParent(instance.transform, false);
                    PlayerComponent playerTransparentClone = playerComponentPrefab.CreateInactiveTransparentCloneInstance();
                    playerTransparentClone.transform.SetParent(instance.transform, false);
                    if (instance.IsTeleportSource)
                    {
                        playerComponentInstance.gameObject.SetActive(true);
                    }
                    if (!PlayerVertexTargetPosOverride)
                    {
                        if (instance.IsTeleportSource) 
                        {
                            PlayerVertexTargetPos = new Vector3(1f, 0f, 0.2f);
                        }
                        else
                        {
                            PlayerVertexTargetPos = new Vector3(0.2f, 0f, -1f);
                        }
                    }
                    PlayerClientData playerClientData = new PlayerClientData(system.PlayerGuid, playerComponentInstance);

                    if (observerUpdateCache == null)
                    {
                        SetObserverUpdateCache();
                        observerUpdateCache = ObserverUpdateCache;
                    }

                    instance.Initialize(observerUpdateCache, playerClientData, playerTransparentClone,
                        playerVertexTargetPos: PlayerVertexTargetPos,
                        playerVertexTargetPosOffset: PlayerVertexTargetPosOffset,
                        animFrameProps: AnimFrameProps,
                        playAnimFrame: PlayAnimFrame,
                        RequiredDuration);
                    TryAddParticleSystem(instance.gameObject);
                    return true;
                }
                else
                {
                    Debug.LogError("Couldn't find FX.");
                }
            }
            else
            {
                Debug.LogError("System null");
            }
            return false;
        }
        //private void Initialize()
        //{
        //    //if (!VariablesSet)
        //    //{
        //    //    Instance = (PortalBuilder) target;
        //    //    VariablesSet = Instance != null && Instance.PlayerClientData != null && Instance.PortalOrb != null && Instance.CrimsonAura != null;
        //    //    if (VariablesSet)
        //    //    {
        //    //        ObserverUpdateCache = Instance.ObserverUpdateCache;
        //    //    }
        //    //}

        //}

        //public override void OnInspectorGUI()
        //{
        //    base.OnInspectorGUI();

        //    EditorGUI.BeginChangeCheck();
        //    PlayerComponent playerComponent = (PlayerComponent) EditorGUILayout.ObjectField("PlayerComponent", Instance.PlayerClientData != null ? Instance.PlayerClientData.PlayerComponent : null, typeof(PlayerComponent), true);
        //    PortalOrbPurple portalOrbPurple = (PortalOrbPurple) EditorGUILayout.ObjectField("PortalOrb", Instance.PortalOrb, typeof(PortalOrbPurple), true);
        //    CrimsonAuraBlack crimsonAura = (CrimsonAuraBlack) EditorGUILayout.ObjectField("CrimsonAura", Instance.CrimsonAura, typeof(CrimsonAuraBlack), true);
        //}

        public override void ManualUpdate()
        {
            Instance.ManualUpdate();
        }

        protected override void EditorDestroy()
        {
            GameObject.DestroyImmediate(Instance.PlayerClientData.PlayerComponent.gameObject);
            GameObject.DestroyImmediate(Instance.PlayerTransparentClone.gameObject);

            Instance.CleanUpInstance();
        }
    }
#endif
}
