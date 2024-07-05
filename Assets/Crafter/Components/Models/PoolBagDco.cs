using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

/** Copyright (C) Tae Soo Kim
 *  All Rights Reserved
 *  You may not copy, distribute, or use this file
 *  without making modifications for personalization.
 */

namespace Assets.Crafter.Components.Models
{
    public class PoolBagDco<T> where T : Component
    {
        private readonly T Prefab;
        public readonly BagDco<T> BagDco;
        public readonly Dictionary<int, T> ActiveItems;
        public PoolBagDco(T prefab, int capacity)
        {
            Prefab = prefab;
            BagDco = new BagDco<T>(capacity);
            ActiveItems = new Dictionary<int, T>(capacity);
        }

        private T Get()
        {
            T result;
            if (BagDco.TryTake(out result))
            {
                return result;
            }

            return UnityEngine.Object.Instantiate(Prefab);
        }
        public T InstantiatePooled(Transform parent)
        {
            T component = Get();
            component.transform.SetParent(parent);
            component.gameObject.SetActive(true);
            ActiveItems[component.GetHashCode()] = component;
            return component;
        }
        public T InstantiatePooled(Vector3 position)
        {
            T component = Get();
            component.transform.position = position;
            component.transform.SetParent(null);
            component.gameObject.SetActive(true);
            ActiveItems[component.GetHashCode()] = component;
            return component;
        }
        public void ReturnPooled(T item)
        {
            HideItem(item);
            ActiveItems.Remove(item.GetHashCode());
            BagDco.Add(item);
        }
        public void HideItem(T item)
        {
            item.transform.SetParent(null);
            item.gameObject.SetActive(false);
        }
        public void DestroyAll()
        {
            foreach (KeyValuePair<int, T> trackedItem in ActiveItems)
            {
                HideItem(trackedItem.Value);
                BagDco.Add(trackedItem.Value);
            }
            ActiveItems.Clear();
        }
    }
    public static class PoolBagDcoHelpers
    {

    }
}
