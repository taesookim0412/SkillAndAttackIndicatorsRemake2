using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public static class GameObjectUtil
    {
        public static string RemoveCloneName(string instanceName)
        {
            int cloneIndex = instanceName.LastIndexOf("(Clone)");
            if (cloneIndex > -1)
            {
                return instanceName.Substring(0, cloneIndex);
            }
            return instanceName;
        }
    }
}
