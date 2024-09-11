using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Crafter.Components.Models
{
    public static class JObjectHelpers
    {
        public static JObject ToJObject(this Vector4 vector4)
        {
            return new JObject
            {
                ["x"] = vector4.x,
                ["y"] = vector4.y,
                ["z"] = vector4.z,
                ["w"] = vector4.w
            };
        }
        public static JObject ToJObject(this Vector3 vector3)
        {
            return new JObject
            {
                ["x"] = vector3.x,
                ["y"] = vector3.y,
                ["z"] = vector3.z
            };
        }
        public static JObject ToJObject(this Color color)
        {
            return new JObject
            {
                ["r"] = color.r,
                ["g"] = color.g,
                ["b"] = color.b,
                ["a"] = color.a
            };
        }
    }
}
