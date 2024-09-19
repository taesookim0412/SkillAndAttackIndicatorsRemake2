using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public static class GameObjectUtil
    {
        public static string RemoveInstanceName(string materialName)
        {
            int instanceIndexOf = materialName.IndexOf(" (Instance)");
            if (instanceIndexOf > 0)
            {
                return materialName.Substring(0, instanceIndexOf);
            }
            return materialName;
        }
        public static string RemoveTransparentName(string materialName)
        {
            int transparentIndexOf = materialName.LastIndexOf("_Transparent");
            if (transparentIndexOf > 0)
            {
                return materialName.Substring(0, transparentIndexOf);
            }
            return materialName;
        }
    }
}
