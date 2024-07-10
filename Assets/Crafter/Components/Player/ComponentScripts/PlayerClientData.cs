﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Player.ComponentScripts
{
    public class PlayerClientData
    {
        public static readonly int WalkAnimStateId = Animator.StringToHash("Walking");

        private static readonly int WalkAnimFullPathHash = Animator.StringToHash("Base Layer.Walking");

        public PlayerComponent PlayerComponent;

        public PlayerClientData(PlayerComponent playerComponent)
        {
            PlayerComponent = playerComponent;
        }

        public void PlayWalkingState()
        {
            PlayerComponent.Animator.Play(WalkAnimFullPathHash, 0);
        }
    }
}
