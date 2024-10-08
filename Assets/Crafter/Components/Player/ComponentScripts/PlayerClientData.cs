using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Player.ComponentScripts
{
    public class PlayerClientData
    {
        private static readonly int WalkAnimFullPathHash = Animator.StringToHash("Base Layer.Walking");
        private static readonly int JumpStartFXFullPathHash = Animator.StringToHash("Base Layer.JumpStartFX");

        public Guid Id;
        public PlayerComponent PlayerComponent;

        public PlayerClientData(Guid id, PlayerComponent playerComponent)
        {
            Id = id;
            PlayerComponent = playerComponent;
        }

        public void PlayWalkingState()
        {
            PlayerComponent.Animator.Play(WalkAnimFullPathHash, 0);
        }
        // Note: Replace this with the full anim state machine.
        public void PlayJumpStartFXState(float normalizedTime)
        {
            PlayerComponent.Animator.Play(JumpStartFXFullPathHash, 0, normalizedTime);
            PlayerComponent.Animator.Update(PartialMathUtil.ONE_FRAME);
        }

        public bool isAnimStateTimeMet(float animFullPathHash, float animClipFrameNormalized)
        {
            var animStateInfo = PlayerComponent.Animator.GetCurrentAnimatorStateInfo(layerIndex: 0);
            return animStateInfo.fullPathHash == animFullPathHash && animStateInfo.normalizedTime >= animClipFrameNormalized;
        }
    }
}
