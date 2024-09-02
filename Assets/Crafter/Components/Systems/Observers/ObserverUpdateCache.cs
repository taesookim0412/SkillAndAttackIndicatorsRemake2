using Assets.Crafter.Components.Models.dco;
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
        public FixedSizeArrayDco<float> UpdateRenderThreadPreviousTimeSteps = new FixedSizeArrayDco<float>(5);

        public ObserverUpdateCache(long newTime)
        {
            UpdateTickTimeRenderThread = newTime;
        }

        public void Update_RenderThread()
        {
            long newTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            float renderThreadDeltaTimeSec = (newTime - UpdateTickTimeRenderThread) * 0.001f;
            UpdateTickTimeRenderThreadDeltaTimeSec = renderThreadDeltaTimeSec;
            UpdateRenderThreadPreviousTimeSteps.AddTail(renderThreadDeltaTimeSec);
            UpdateRenderThreadAverageTimeStep = UpdateRenderThreadPreviousTimeSteps.CalculateAverage();

            UpdateTickTimeRenderThread = newTime;
        }
    }
}
