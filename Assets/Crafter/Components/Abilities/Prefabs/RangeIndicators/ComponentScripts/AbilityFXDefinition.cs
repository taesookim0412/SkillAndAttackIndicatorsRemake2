using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public static class AbilityFXDefinition
    {
        public static readonly int AbilityTriggerFXTypeEnumLength = Enum.GetNames(typeof(AbilityTriggerFXType)).Length;
    }
    
    public enum AbilityTriggerFXType
    {
        None,
        DashTrigger
    }
}
