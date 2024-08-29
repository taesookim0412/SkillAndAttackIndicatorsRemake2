using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Range(0f, 2f), SerializeField]
        private float PlayerOpacityDuration;
        [Range(0f, 2f), SerializeField]
        private float PlayerOpaqueDuration;

        [SerializeField]
        public bool IsTeleportSource;

        // incompatible with onvalidate
        [NonSerialized, HideInInspector]
        private float RequiredDurationMult;

        public override void ManualAwake()
        {
        }
        protected virtual float GetRequiredDurationMillis()
        {
            return (PlayerOpacityDuration + PlayerOpaqueDuration) * 1000f +
                (SkillAndAttackIndicatorSystem.FixedTimestep * PlayerBlinkStateLength * 2f);
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
        protected PlayerBlinkState PlayerBlinkState;
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
            PlayerComponent playerTransparentClone, long? durationAllowed)
        {
            base.Initialize(observerUpdateCache);

            if (durationAllowed != null)
            {
                float requiredDurationMultTimes1000 = (long)durationAllowed / GetRequiredDurationMillis() * 1000f;
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

                    PlayerOpacityTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                    PlayerBlinkState = PlayerBlinkState.PlayerOpaque;
                    break;
                case PlayerBlinkState.PlayerOpaque:
                    if (PlayerOpacityTimer.IsTimeNotElapsed_FixedUpdateThread())
                    {
                        float scalePercentage = PlayerOpacityTimer.RemainingDurationPercentage();
                        if (IsTeleportSource)
                        {
                            scalePercentage = 1f - scalePercentage;
                        }
                        PlayerTransparentClone.SetCloneFXOpacity(scalePercentage);
                    }
                    else
                    {
                        PlayerTransparentClone.gameObject.SetActive(false);
                        PlayerClientData.PlayerComponent.gameObject.SetActive(!IsTeleportSource);

                        PlayerOpaqueTimer.LastCheckedTime = ObserverUpdateCache.UpdateTickTimeFixedUpdate;
                        PlayerBlinkState = PlayerBlinkState.PlayerDespawn;
                    }
                    break;
                case PlayerBlinkState.PlayerDespawn:
                    if (PlayerOpaqueTimer.IsTimeElapsed_FixedUpdateThread())
                    {
                        Complete();
                    }
                    break;
            }
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
        PlayerOpaque,
        PlayerDespawn
    }
}
