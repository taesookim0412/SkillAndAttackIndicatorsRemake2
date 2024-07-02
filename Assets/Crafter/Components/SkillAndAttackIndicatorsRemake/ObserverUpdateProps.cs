using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class ObserverUpdateProps
    {
        public long UpdateTickTime;

        public ObserverUpdateProps(long updateTickTime)
        {
            UpdateTickTime = updateTickTime;
        }

        public void Update_MainThread()
        {
            UpdateTickTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
