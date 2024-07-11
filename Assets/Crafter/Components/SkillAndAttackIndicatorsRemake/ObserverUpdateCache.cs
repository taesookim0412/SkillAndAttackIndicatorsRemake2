﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class ObserverUpdateCache
    {
        public long UpdateTickTimeFixedUpdate;

        public ObserverUpdateCache(long newTime)
        {
            UpdateTickTimeFixedUpdate = newTime;
        }

        public void Update_FixedUpdate()
        {
            UpdateTickTimeFixedUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
