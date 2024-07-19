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

        private readonly long StartTime;
        private readonly long EndTime;

        private readonly bool Inverted;
        private bool Completed = false;

        public PortalBuilderChain(PortalBuilder portalSource, PortalBuilder portalDest, long startTime, long endTime, bool inverted)
        {
            PortalSource = portalSource;
            PortalDest = portalDest;
            StartTime = startTime;
            EndTime = endTime;
            Inverted = inverted;
        }

        public bool UpdatePortals(long elapsedTime)
        {
            if (Completed)
            {
                return false;
            }

            PortalBuilder startPortal;
            PortalBuilder endPortal;
            if (Inverted)
            {
                startPortal = PortalDest;
                endPortal = PortalSource;
            }
            else
            {
                startPortal = PortalSource;
                endPortal = PortalDest;
            }

            if (elapsedTime >= StartTime)
            {
                if (elapsedTime <= EndTime)
                {
                    if (!startPortal.Completed)
                    {
                        if (!startPortal.Active)
                        {
                            startPortal.gameObject.SetActive(true);
                        }
                        startPortal.ManualUpdate();
                    }
                    else
                    {
                        if (!endPortal.Active)
                        {
                            endPortal.gameObject.SetActive(true);
                        }
                        endPortal.ManualUpdate();
                    }
                }
                else
                {
                    if (!startPortal.Completed)
                    {
                        startPortal.Complete();
                    }
                    if (!endPortal.Completed)
                    {
                        endPortal.Complete();
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
