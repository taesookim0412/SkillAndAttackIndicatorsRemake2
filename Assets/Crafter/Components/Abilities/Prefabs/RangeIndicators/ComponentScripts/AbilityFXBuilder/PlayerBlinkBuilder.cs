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
        public void Initialize(ObserverUpdateCache observerUpdateCache, PlayerClientData playerClientData,
            PlayerComponent playerTransparentClone, Vector3 playerVertexTargetPos, long? durationAllowed)
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

            PlayerVertexTargetPos = playerVertexTargetPos;

            PlayerComponent playerComponent = playerClientData.PlayerComponent;

            if (!IsTeleportSource)
            {
                playerComponent.gameObject.SetActive(false);
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
                    PlayerTransparentClone.SetMaterialVertexTargetPos(PlayerVertexTargetPos, PlayerOpacityDuration, !IsTeleportSource);
                    PlayerTransparentClone.transform.position = transform.position;
                    PlayerClientData.PlayerComponent.transform.position = transform.position;
                    float playerTransparentCloneOpacity;
                    if (IsTeleportSource)
                    {
                        PlayerClientData.PlayerComponent.gameObject.SetActive(false);
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
                        PlayerClientData.PlayerComponent.gameObject.SetActive(!IsTeleportSource);

                        PlayerOpaqueTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeRenderThread;
                        PlayerBlinkState = PlayerBlinkState.PlayerDespawn;
                    }
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

#if UNITY_EDITOR
    [CustomEditor(typeof(PlayerBlinkBuilder))]
    public class PlayerBlinkBuilderEditor : AbstractEditor<PlayerBlinkBuilder>
    {
        public Vector3 PlayerVertexTargetPos = new Vector3(0f, 1f, 1f);
        public long? RequiredDuration = null;

        public void SetOverrides(Vector3 playerVertexTargetPos)
        {
            PlayerVertexTargetPos = playerVertexTargetPos;
        }
        protected override bool OnInitialize(PlayerBlinkBuilder instance, ObserverUpdateCache observerUpdateCache)
        {
            SkillAndAttackIndicatorSystem system = GameObject.FindFirstObjectByType<SkillAndAttackIndicatorSystem>();
            if (system != null)
            {
                PlayerComponent playerComponentPrefab = system.PlayerComponent;

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
                    PlayerClientData playerClientData = new PlayerClientData(system.PlayerGuid, playerComponentInstance);

                    if (observerUpdateCache == null)
                    {
                        SetObserverUpdateCache();
                        observerUpdateCache = ObserverUpdateCache;
                    }
                    
                    instance.Initialize(observerUpdateCache, playerClientData, playerTransparentClone,
                        playerVertexTargetPos: PlayerVertexTargetPos,
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

        protected override void ManualUpdate()
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
