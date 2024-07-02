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
    public class SkillAndAttackIndicatorInput : MonoBehaviour
    {
        [SerializeField]
        private SkillAndAttackIndicatorSystem SkillAndAttackIndicatorSystem;

        public void OnAbility1(CallbackContext ctx)
        {
            if (ctx.performed)
            {
                SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.Arc,
                    AbilityProjectorMaterialType.First,
                    AbilityIndicatorCastType.ShowDuringCast,
                    AbilityFXType.None);
            }
        }
        public void OnAbility2(CallbackContext ctx)
        {
            if (ctx.performed)
            {
                SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.Circle,
                    AbilityProjectorMaterialType.First,
                    AbilityIndicatorCastType.ShowDuringCast,
                    AbilityFXType.None);
            }
        }
        public void OnAbility3(CallbackContext ctx)
        {
            if (ctx.performed)
            {
                SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.Line,
                    AbilityProjectorMaterialType.First,
                    AbilityIndicatorCastType.ShowDuringCast,
                    AbilityFXType.DashParticles);
            }
        }
        public void OnAbility4(CallbackContext ctx)
        {
            if (ctx.performed)
            {
                SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.ScatterLine,
                    AbilityProjectorMaterialType.First,
                    AbilityIndicatorCastType.ShowDuringCast,
                    AbilityFXType.None);
            }
        }
    }
}
