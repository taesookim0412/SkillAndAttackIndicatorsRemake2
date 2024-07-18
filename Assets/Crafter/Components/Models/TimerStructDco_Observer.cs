using Assets.Crafter.Components.Systems.Observers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/** Copyright (C) Tae Soo Kim
 *  All Rights Reserved
 *  You may not copy, distribute, or use this file
 *  without making modifications for personalization.
 */

namespace Assets.Crafter.Components.Models
{
    public struct TimerStructDco_Observer
    {
        public ObserverUpdateCache ObserverUpdateCache;
        public long LastCheckedTime;
        public long RequiredDuration;

        public TimerStructDco_Observer(long requiredDuration) : this()
        {
            RequiredDuration = requiredDuration;
        }

        public bool IsTimeElapsed_FixedUpdateThread()
        {
            return ObserverUpdateCache.UpdateTickTimeFixedUpdate - LastCheckedTime > RequiredDuration;
        }
        public bool IsTimeNotElapsed_FixedUpdateThread()
        {
            return ObserverUpdateCache.UpdateTickTimeFixedUpdate - LastCheckedTime < RequiredDuration;
        }
        public float RemainingDurationPercentage()
        {
            return (ObserverUpdateCache.UpdateTickTimeFixedUpdate - LastCheckedTime) / (float) RequiredDuration;
        }
    }
}
