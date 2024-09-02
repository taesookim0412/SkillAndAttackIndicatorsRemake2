using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/** Copyright (C) Tae Soo Kim
*  All Rights Reserved
*  You may not copy, distribute, or use this file
*  without making modifications for personalization.
*/
namespace Assets.Crafter.Components.Models.dco
{
    public class FixedSizeArrayDco<T>
    {
        public readonly int CapacityMaxSize;

        public T[] Items;

        public int Head = 0;

        public int Count;

        private EqualityComparer<T> Comparer;

        public FixedSizeArrayDco(int capacityMaxSize)
        {
            CapacityMaxSize = capacityMaxSize;
            Items = new T[CapacityMaxSize];
            Count = 0;
            Comparer = EqualityComparer<T>.Default;
        }

        public void RemoveHeadPrimitive(out T item)
        {
            item = Items[Head];
            if (++Head >= CapacityMaxSize)
            {
                Head = 0;
            }
            Count--;
        }

        public void AddTail(T item)
        {
            int nextIndex = (Head + Count) % CapacityMaxSize;
            Items[nextIndex] = item;
            Count++;
        }

        public void MoveToTail(T item)
        {
            bool itemPast = false;

            int lastIndex = Head;
            for (int i = 0; i < Count; i++)
            {
                int arrayIndex = (Head + i) % CapacityMaxSize;
                if (itemPast)
                {
                    Items[lastIndex] = Items[arrayIndex];
                }
                else if(Comparer.Equals(Items[arrayIndex], item))
                {
                    itemPast = true;
                }
                lastIndex = arrayIndex;
            }
            Items[lastIndex] = item;
        }

    }

    public static class FixedSizeArrayHelpers
    {
        public static float CalculateAverage(this FixedSizeArrayDco<float> array)
        {
            if (array.Count == 0)
            {
                return 0f;
            }

            float sum = 0;

            int lastIndex = array.Head;
            for (int i = 0; i < array.Count; i++)
            {
                int arrayIndex = (array.Head + i) % array.CapacityMaxSize;
                sum += array.Items[arrayIndex];
            }
            return sum / (float)array.Count;
        }
    }
}
