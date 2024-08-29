using Assets.Crafter.Components.Player.ComponentScripts;
using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder.Chains
{
    public class DashBlinkAbilityChain: AbstractAbilityFXBuilder
    {
        [NonSerialized]
        public PlayerClientData PlayerClientData;
        [NonSerialized]
        public PlayerComponent PlayerTransparentClone;
        [NonSerialized]
        public PortalBuilder PortalDest;
        [NonSerialized]
        public TrailMoverBuilder_TargetPos TrailForPortals;

        [NonSerialized, HideInInspector]
        private long StartTime;
        [NonSerialized, HideInInspector]
        private long EndTime;
        public override void ManualAwake()
        {
        }
        public void Initialize(ObserverUpdateCache observerUpdateCache,
            PlayerClientData playerClientData,
            PlayerComponent playerTransparentClone,
            TrailMoverBuilder_TargetPos trailForPortals,
            long startTime,
            long endTime)
        {
            base.Initialize(observerUpdateCache);
            PlayerClientData = playerClientData;
            PlayerTransparentClone = playerTransparentClone;
            TrailForPortals = trailForPortals;
            StartTime = startTime;
            EndTime = endTime;
        }

    }
}
