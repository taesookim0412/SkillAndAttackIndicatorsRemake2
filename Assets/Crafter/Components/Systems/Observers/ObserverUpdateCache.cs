using Assets.Crafter.Components.Models;
using Assets.Crafter.Components.Models.dco;
using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Systems.Observers
{
    public class ObserverUpdateCache
    {
        public long UpdateTickTimeRenderThread;
        public float UpdateTickTimeRenderThreadDeltaTimeSec;
        public float UpdateRenderThreadAverageTimeStep = 0.02f;
        public FixedSizeArrayDco_Average UpdateRenderThreadPreviousTimeSteps = new FixedSizeArrayDco_Average(5);

        public ObserverUpdateCache(long newTime)
        {
            UpdateTickTimeRenderThread = newTime;
        }

        public void Update_RenderThread()
        {
            long newTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            float renderThreadDeltaTimeSec = (newTime - UpdateTickTimeRenderThread) * PartialMathUtil.SECOND_PER_MILLISECOND;
            UpdateTickTimeRenderThreadDeltaTimeSec = renderThreadDeltaTimeSec;
            UpdateRenderThreadAverageTimeStep = UpdateRenderThreadPreviousTimeSteps.CalculateNewAverage(renderThreadDeltaTimeSec);

            UpdateTickTimeRenderThread = newTime;
        }
    }
}
