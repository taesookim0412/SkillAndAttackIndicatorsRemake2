using Assets.Crafter.Components.Constants;
using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using Assets.Crafter.Components.Systems.Observers;
using StarterAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace Assets.Crafter.Components.Player.ComponentScripts
{
    public class PlayerComponent : MonoBehaviour
    {
        [SerializeField]
        public CharacterController CharacterController;
        [SerializeField]
        public ThirdPersonController ThirdPersonController;
        [SerializeField]
        public StarterAssetsInputs StarterAssetsInputs;
        [SerializeField]
        public PlayerInput PlayerInput;
        [SerializeField]
        public SkinnedMeshRendererContainer[] Meshes;
        [SerializeField]
        public Animator Animator;
        private static readonly int BindPoseAnimFullPathHash = Animator.StringToHash("Base Layer.BindPose");

        [NonSerialized, HideInInspector]
        public Material[] Materials;
        [NonSerialized, HideInInspector]
        public AnimVerticesTextureItems[] AnimVerticesTextureItems;
        [NonSerialized, HideInInspector]
        public LocalKeyword[] LocalKeywords;
        [NonSerialized, HideInInspector]
        public Vector3[] MeshCenters;
        // Note: Refer to PlayerClientData instead of using BodyCenter.
        [NonSerialized, HideInInspector]
        public Vector3 BodyCenter;

        [NonSerialized, HideInInspector]
        private static int EndPositionLocalId = Shader.PropertyToID("_EndPositionLocal");
        [NonSerialized, HideInInspector]
        private static int TimeElapsedNormalizedId = Shader.PropertyToID("_TimeElapsedNormalized");
        [NonSerialized, HideInInspector]
        private static int AnimVerticesId = Shader.PropertyToID("_AnimVertices");
        [NonSerialized, HideInInspector]
        private static int AnimVerticesMaxChannelValuesId = Shader.PropertyToID("_AnimVerticesMaxChannelValues");

        //[NonSerialized, HideInInspector]
        //public PlayerComponentCloneItems PlayerComponentCloneItems;
        private void InitializeMaterials()
        {
            int materialsCount = 0;
            foreach (SkinnedMeshRendererContainer holder in Meshes)
            {
                materialsCount += holder.SkinnedMeshRenderer.materials.Length;
            }

            Material[] materials = new Material[materialsCount];
            LocalKeyword[] localKeywords = new LocalKeyword[materialsCount];
            Vector3[] meshCenters = new Vector3[materialsCount];
            int i = 0;
            foreach (SkinnedMeshRendererContainer holder in Meshes)
            {
                foreach (Material material in holder.SkinnedMeshRenderer.materials)
                {
                    materials[i] = material;
                    localKeywords[i] = new LocalKeyword(material.shader, "_INVERT_TARGET_POS");
                    meshCenters[i] = holder.SkinnedMeshRenderer.sharedMesh.bounds.center;
                    if (i == 0)
                    {
                        BodyCenter = meshCenters[i];
                    }
                    i++;
                }
            }

            Materials = materials;
            LocalKeywords = localKeywords;
            MeshCenters = meshCenters;
        }
        private void SetAnimVerticesTexture(PlayerComponentModel playerComponentModel, AnimVerticesTexture animVerticesTexture)
        {
            int materialsCount = Materials.Length;

            AnimVerticesTextureItems[] animVerticesTextureItems = new AnimVerticesTextureItems[materialsCount];

            if (AnimVerticesTexturesConstants.ANIM_VERTICES_TEXTURES.TryGetValue(animVerticesTexture, out var animVerticesPair) &&
                animVerticesPair.TryGetValue(playerComponentModel, out var playerComponentPair))
            {
                int i = 0;
                foreach (SkinnedMeshRendererContainer holder in Meshes)
                {
                    string meshName = holder.SkinnedMeshRenderer.name;
                    PlayerMesh playerMesh = Enum.Parse<PlayerMesh>(meshName);
                    if (playerComponentPair.TryGetValue(playerMesh, out var playerMeshPair))
                    {
                        foreach (Material material in holder.SkinnedMeshRenderer.materials)
                        {
                            string submeshName = GameObjectUtil.RemoveInstanceName(material.name);
                            submeshName = GameObjectUtil.RemoveTransparentName(submeshName);
                            PlayerSubmesh playerSubmesh = Enum.Parse<PlayerSubmesh>(submeshName);
                            if (playerMeshPair.TryGetValue(playerSubmesh, out var submeshPairItems))
                            {
                                animVerticesTextureItems[i] = submeshPairItems;
                            }
                            i++;
                        }
                    }
                    else
                    {
                        i += holder.SkinnedMeshRenderer.materials.Length;
                    }
                }
            }
            AnimVerticesTextureItems = animVerticesTextureItems;
        }
        public PlayerComponent CreateInactiveTransparentCloneInstance()
        {
            PlayerComponent playerComponentInstance = GameObject.Instantiate(this);
            playerComponentInstance.gameObject.SetActive(false);
            GameObject.Destroy(playerComponentInstance.ThirdPersonController);
            GameObject.Destroy(playerComponentInstance.CharacterController);
            GameObject.Destroy(playerComponentInstance.StarterAssetsInputs);
            GameObject.Destroy(playerComponentInstance.PlayerInput);
            foreach (SkinnedMeshRendererContainer holder in playerComponentInstance.Meshes)
            {
                holder.SkinnedMeshRenderer.materials = holder.TransparentMaterials;
            }

            // 
            playerComponentInstance.InitializeMaterials();

            return playerComponentInstance;
        }
        public void OnCloneFXInit(PlayerComponentModel playerComponentModel, AnimVerticesTexture animVerticesTexture)
        {
            //PlayerComponentCloneItems = new PlayerComponentCloneItems(observerUpdateCache);
            // Since these are NonSerialized, it must be re-created after the first pool item is created.
            if (Materials == null)
            {
                InitializeMaterials();
            }
            SetAnimVerticesTexture(playerComponentModel, animVerticesTexture);
        }
        public void SetMaterialVertexTargetPos(Vector3 targetPos, float requiredTime, bool invertTargetPos)
        {
            Material[] materials = Materials;
            AnimVerticesTextureItems[] animVerticesTextureItemsArray = AnimVerticesTextureItems;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                LocalKeyword invertTargetPosKeyword = LocalKeywords[i];

                Vector3 offsetTargetPos = targetPos - MeshCenters[i];

                material.SetVector(EndPositionLocalId, offsetTargetPos);

                material.SetFloat(TimeElapsedNormalizedId, 0f);

                AnimVerticesTextureItems animVerticesTextureItems = animVerticesTextureItemsArray[i];
                if (animVerticesTextureItems != null)
                {
                    material.SetTexture(AnimVerticesId, animVerticesTextureItems.Texture);
                    material.SetVector(AnimVerticesMaxChannelValuesId, animVerticesTextureItems.MaxChannelValues);
                }
                else
                {
                    material.SetTexture(AnimVerticesId, null);
                    material.SetVector(AnimVerticesMaxChannelValuesId, Vector3.zero);
                }
                

                material.SetKeyword(invertTargetPosKeyword, invertTargetPos);
            }
        }
        public void SetMaterialVertexPosTimeElapsedNormalized(float timeElapsedNormalized)
        {
            foreach (Material material in Materials)
            {
                material.SetFloat(TimeElapsedNormalizedId, timeElapsedNormalized);
            }
        }
        public void SetCloneFXOpacity(float opacity)
        {
            foreach (Material material in Materials)
            {
                material.color = new Color(1f, 1f, 1f, opacity);
            }
        }

        public void PauseAnimatorWithBindPoseState()
        {
            Animator.Play(BindPoseAnimFullPathHash);
            Animator.speed = 0f;
        }
        public void EnableAnimatorSpeed()
        {
            Animator.speed = 1f;
        }
    }

    //public class PlayerComponentCloneItems
    //{
    //    public TimerStructDco_Observer AnimationTimer = new TimerStructDco_Observer(25L);
    //    public bool AnimationTimerCompleted = false;
    //    public bool AnimationTimerSet = false;
    //    public bool AnimationStarted = false;

    //    public PlayerComponentCloneItems(ObserverUpdateCache observerUpdateCache)
    //    {
    //        AnimationTimer.ObserverUpdateCache = observerUpdateCache;
    //    }
    //}
}
