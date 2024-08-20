using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.Projectors
{
    public class LineProjector : AbstractProjector
    {
        [SerializeField]
        public DecalProjector Projector;

        /// <summary>
        /// The shader ID of the <c>_FillProgress</c> property.
        /// </summary>
        private static readonly int FillProgressShaderID = Shader.PropertyToID("_FillProgress");

        public void Initialize(Vector3 abilityScale, float requiredProjectorYHeight)
        {
            Vector3 projectorSize = new Vector3(abilityScale.x, abilityScale.z, requiredProjectorYHeight);

            DecalProjector projector = Projector;
            projector.size = projectorSize;
            projector.pivot = new Vector3(0f, projectorSize.y * 0.5f, projectorSize.z * 0.5f);
            projector.material.SetFloat(FillProgressShaderID, 0f);
        }

        public override void ManualUpdate(float fillProgress)
        {
            Projector.material.SetFloat(FillProgressShaderID, fillProgress);
        }
    }
}
