﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Abilities.Prefabs.RangeIndicators.ComponentScripts
{
    public abstract class AbstractAbilityFXBuilder_Followable : AbstractAbilityFXBuilder
    {
        public abstract Transform GetFollowTransform();
    }
}
