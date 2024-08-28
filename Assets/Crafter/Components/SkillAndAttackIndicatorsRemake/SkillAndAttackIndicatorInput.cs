using StarterAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace Assets.Crafter.Components.SkillAndAttackIndicatorsRemake
{
    public class SkillAndAttackIndicatorInput : StarterAssetsInputs
    {
        [SerializeField]
        private SkillAndAttackIndicatorSystem SkillAndAttackIndicatorSystem;

        public void OnAbility1(InputValue value)
        {
            return;
            //SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.ArcProjector,
            //    AbilityProjectorMaterialType.First,
            //    AbilityIndicatorCastType.ShowDuringCast,
            //    null);
        }
        public void OnAbility2(InputValue value)
        {
            return;
            //SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.CircleProjector,
            //    AbilityProjectorMaterialType.First,
            //    AbilityIndicatorCastType.ShowDuringCast,
            //    null);
        }
        public void OnAbility3(InputValue value)
        {
            SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.LineProjector,
                AbilityProjectorMaterialType.DashAbilityLineMaterial,
                AbilityIndicatorCastType.ShowDuringCast,
                AbilityIndicatorFXType.DashPortalAbility);
        }
        public void OnAbility4(InputValue value)
        {
            return;
            //SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.ScatterLinesProjector,
            //    AbilityProjectorMaterialType.First,
            //    AbilityIndicatorCastType.ShowDuringCast,
            //    null);
        }
    }
}
