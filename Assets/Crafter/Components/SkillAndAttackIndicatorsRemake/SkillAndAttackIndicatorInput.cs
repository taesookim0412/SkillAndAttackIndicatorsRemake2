﻿using StarterAssets;
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
            SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.Arc,
                AbilityProjectorMaterialType.First,
                AbilityIndicatorCastType.ShowDuringCast,
                null);
        }
        public void OnAbility2(InputValue value)
        {
            return;
            SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.Circle,
                AbilityProjectorMaterialType.First,
                AbilityIndicatorCastType.ShowDuringCast,
                null);
        }
        public void OnAbility3(InputValue value)
        {
            SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.Line,
                AbilityProjectorMaterialType.First,
                AbilityIndicatorCastType.ShowDuringCast,
                new AbilityIndicatorFXType[1] { AbilityIndicatorFXType.DashParticles });
        }
        public void OnAbility4(InputValue value)
        {
            return;
            SkillAndAttackIndicatorSystem.TriggerSkillAndAttackIndicatorObserver(AbilityProjectorType.ScatterLine,
                AbilityProjectorMaterialType.First,
                AbilityIndicatorCastType.ShowDuringCast,
                null);
        }
    }
}
