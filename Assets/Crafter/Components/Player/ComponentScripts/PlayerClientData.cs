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
        private static readonly int JumpStartFXId = Animator.StringToHash("JumpStartFX");
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
        public void SetJumpStartFXState(bool enable)
        {
            PlayerComponent.Animator.SetBool(JumpStartFXId, enable);
        }

        //public bool IsAnimStateTimeMet(float animFullPathHash, float animClipFrameNormalized)
        //{
        //    var animStateInfo = PlayerComponent.Animator.GetCurrentAnimatorStateInfo(layerIndex: 0);
        //    return animStateInfo.fullPathHash == animFullPathHash && animStateInfo.normalizedTime >= animClipFrameNormalized;
        //}
        public bool IsNextOrCurrentAnimStateTimeMet(float animFullPathHash, float animClipFrameNormalized)
        {
            var nextAnimStateInfo = PlayerComponent.Animator.GetNextAnimatorStateInfo(layerIndex: 0);
            if (nextAnimStateInfo.fullPathHash == animFullPathHash && nextAnimStateInfo.normalizedTime >= animClipFrameNormalized)
            {
                return true;
            }
            var animStateInfo = PlayerComponent.Animator.GetCurrentAnimatorStateInfo(layerIndex: 0);
            return animStateInfo.fullPathHash == animFullPathHash && animStateInfo.normalizedTime >= animClipFrameNormalized;
        }
    }
}
