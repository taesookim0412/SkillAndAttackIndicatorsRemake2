using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class ObserverUpdateProps
    {
        public long UpdateTickTimeFixedUpdate;

        public ObserverUpdateProps(long updateTickTime)
        {
            UpdateTickTimeFixedUpdate = updateTickTime;
        }

        public void Update_FixedUpdate()
        {
            UpdateTickTimeFixedUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
