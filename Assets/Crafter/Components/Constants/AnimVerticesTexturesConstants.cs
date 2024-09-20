using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Constants
{
    public static class AnimVerticesTexturesConstants
    {
        public static string[] AnimVerticesTextureNames = Enum.GetNames(typeof(AnimVerticesTexture));
    }

    public enum AnimVerticesTexture
    {
        None, 
        DashBlinkAbility
    }
}
