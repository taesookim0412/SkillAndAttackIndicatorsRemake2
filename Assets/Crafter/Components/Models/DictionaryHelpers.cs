using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Models
{
    public static class DictionaryHelpers
    {
        // This belongs in a dictionary help
        public static bool TryGetValuesAll<T, S>(this Dictionary<T, S> dict, T[] keys, out S[] result)
        {
            result = new S[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (!dict.TryGetValue(keys[i], out result[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
