using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Models.dco
{
    [Serializable]
    public class SerializeableArray<T>
    {
        public T[] Items;

        public SerializeableArray(T[] items)
        {
            Items = items;
        }
    }
}
