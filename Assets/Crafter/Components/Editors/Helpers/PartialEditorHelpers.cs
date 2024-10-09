using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Animations;
using UnityEngine;

namespace Assets.Crafter.Components.Editors.Helpers
{
#if UNITY_EDITOR
    public class PartialEditorHelpers
    {
        public static AnimationClip GetAnimStateClip(AnimatorController animController, int layerIndex, string animStateName)
        {
            AnimatorControllerLayer layer = animController.layers[layerIndex];
            foreach (var s in layer.stateMachine.states)
            {
                if (s.state.name == animStateName)
                {
                    return (AnimationClip)s.state.motion;
                }
            }
            foreach (var subsm in layer.stateMachine.stateMachines)
            {
                foreach (var state in subsm.stateMachine.states)
                {
                    if (state.state.name == animStateName)
                    {
                        return (AnimationClip)state.state.motion;
                    }
                }
            }
            return null;
        }
    }
#endif
}
