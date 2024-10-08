using Assets.Crafter.Components.Player.ComponentScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder
{
    public class DashBlinkAbilityChainEditorProps : MonoBehaviour
    {
        [SerializeField]
        public Vector3 PlayerBlinkSourceTargetPos = new Vector3(1.13f, 3.76f, 7.53f);
        [SerializeField]
        public Vector3 PlayerBlinkDestTargetPos = new Vector3(0.2f, 0f, -1.2f);
        [SerializeField]
        public Vector3 PlayerVertexSourceTargetPosOffset = Vector3.zero;
        [SerializeField]
        public Vector3 PlayerVertexDestTargetPosOffset = Vector3.zero;
        [SerializeField]
        public PlayerComponent PlayerComponent;
        [SerializeField]
        public Animator Animator;
        [SerializeField]
        public string AnimStateName;
        [SerializeField]
        public int AnimLayerIndex;
        [SerializeField]
        public AnimationClip AnimClip;
        [SerializeField]
        public float AnimClipFrame;
        [SerializeField]
        public bool PlayAnimFrame = false;
    }
}
