using Assets.Crafter.Components.SkillAndAttackIndicatorsRemake;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Assets.Crafter.Components.Editors.ComponentScripts
{
    public abstract class AbstractEditor : Editor
    {
        protected ObserverUpdateCache ObserverUpdateCache;

        protected void SetObserverUpdateCache()
        {
            long newTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            ObserverUpdateCache = new ObserverUpdateCache(newTime);
        }
        protected void UpdateObserverUpdateCache_FixedUpdate()
        {
            ObserverUpdateCache.Update_FixedUpdate();
        }
    }
}
