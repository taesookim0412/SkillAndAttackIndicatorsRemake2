using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Systems.Observers
{
    public class ObserverUpdateCache
    {
        public long UpdateTickTimeFixedUpdate;
        public float UpdateTickTimeFixedUpdateDeltaTimeSec;

        public ObserverUpdateCache(long newTime)
        {
            UpdateTickTimeFixedUpdate = newTime;
        }

        public void Update_FixedUpdate()
        {
            long newTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            UpdateTickTimeFixedUpdateDeltaTimeSec = (newTime - UpdateTickTimeFixedUpdate) * 0.001f;
            UpdateTickTimeFixedUpdate = newTime;
            
        }
    }
}
