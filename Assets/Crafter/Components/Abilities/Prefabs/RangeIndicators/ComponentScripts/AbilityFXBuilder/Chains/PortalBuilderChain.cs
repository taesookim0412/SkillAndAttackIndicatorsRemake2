using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts.AbilityFXBuilder.Chains
{
    public class PortalBuilderChain
    {
        public readonly PortalBuilder PortalSource;
        public readonly PortalBuilder PortalDest;
        public readonly TrailMoverBuilder_TargetPos TrailForPortals;

        private readonly long StartTime;
        private readonly long EndTime;

        private bool Completed = false;

        public PortalBuilderChain(PortalBuilder portalSource, PortalBuilder portalDest,
            TrailMoverBuilder_TargetPos trailForPortals,
            long startTime, long endTime)
        {
            PortalSource = portalSource;
            PortalDest = portalDest;
            TrailForPortals = trailForPortals;
            StartTime = startTime;
            EndTime = endTime;
        }

        public bool UpdatePortals(long elapsedTime)
        {
            if (Completed)
            {
                return false;
            }

            if (elapsedTime >= StartTime)
            {
                if (elapsedTime <= EndTime)
                {
                    if (!PortalSource.Completed)
                    {
                        if (!PortalSource.Active)
                        {
                            PortalSource.gameObject.SetActive(true);
                        }
                        PortalSource.ManualUpdate();
                    }
                    else if (!TrailForPortals.Completed)
                    {
                        if (!TrailForPortals.Active)
                        {
                            TrailForPortals.gameObject.SetActive(true);
                        }
                        TrailForPortals.ManualUpdate();
                    }
                    else if (!PortalDest.Completed)
                    {
                        if (!PortalDest.Active)
                        {
                            PortalDest.gameObject.SetActive(true);
                        }
                        PortalDest.ManualUpdate();
                    }
                }
                else
                {
                    if (!PortalSource.Completed)
                    {
                        PortalSource.Complete();
                    }
                    if (!TrailForPortals.Completed)
                    {
                        TrailForPortals.Complete();
                    }
                    if (!PortalDest.Completed)
                    {
                        PortalDest.Complete();
                    }
                    Completed = true;
                }
                return true;
            }
            else
            {
                return true;
            }
        }
    }
}
