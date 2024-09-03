using Assets.Crafter.Components.Models.dco;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Crafter.Components.Models
{
    public class FixedSizeArrayDco_Average : FixedSizeArrayDco<float>
    {
        private float Sum = 0f;
        public FixedSizeArrayDco_Average(int capacityMaxSize) : base(capacityMaxSize)
        {
        }
        public float CalculateNewAverage(float newNumber)
        {
            if (Count >= CapacityMaxSize)
            {
                RemoveHeadPrimitive(out float removedItem);
                Sum -= removedItem;
            }
            else
            {
                Count++;
            }

            AddTail(newNumber);
            Sum += newNumber;
            return Sum / CapacityMaxSize;
        }
    }
}
