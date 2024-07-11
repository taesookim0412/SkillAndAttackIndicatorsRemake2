using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public class ElectricTrailRenderer : AbstractAbilityFX
    {
        [SerializeField]
        public TrailRenderer[] TrailRenderers;

        public void ClearAll()
        {
            foreach (TrailRenderer trailRenderer in TrailRenderers)
            {
                trailRenderer.Clear();
            }
        }
        public void OverwritePositions(Vector3[] worldElectricTrailRendererPositions)
        {
            foreach (TrailRenderer trailRenderer in TrailRenderers) 
            {
                trailRenderer.Clear();
                trailRenderer.AddPositions(worldElectricTrailRendererPositions);
            }
            transform.position = worldElectricTrailRendererPositions[worldElectricTrailRendererPositions.Length - 1];
        }
    }
}
