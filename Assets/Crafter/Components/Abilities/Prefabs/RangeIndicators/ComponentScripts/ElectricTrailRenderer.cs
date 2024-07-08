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
        public TrailRenderer[] TrailRenderer;

        public void ClearAll()
        {
            foreach (TrailRenderer trailRenderer in TrailRenderer)
            {
                trailRenderer.Clear();
            }
        }
        public void OverwritePositions(Vector3[] worldElectricTrailRendererPositions)
        {
            Vector3 lastPosition = worldElectricTrailRendererPositions[worldElectricTrailRendererPositions.Length - 1];
            foreach (TrailRenderer trailRenderer in TrailRenderer) 
            {
                trailRenderer.Clear();
                trailRenderer.AddPositions(worldElectricTrailRendererPositions);
                trailRenderer.transform.position = lastPosition;
            }
        }
    }
}
