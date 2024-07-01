using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/** Copyright (C) Tae Soo Kim
 *  All Rights Reserved
 *  You may not copy, distribute, or use this file
 *  without making modifications for personalization.
 */ 

namespace Assets.Crafter.Components.Models
{
    public class BagDco<T> : IEnumerable<T>
    {
        private readonly Random Random = new Random();
        private readonly List<T> _items;
        public int Count => _items.Count;
        public BagDco(List<T> items)
        {
            _items = items;
        }
        public BagDco(int capacity)
        {
            _items = new List<T>(capacity);
        }

        public void Add(T item)
        {
            // When adding an item, add it to a random location to avoid callers assuming an ordering.
            if (_items.Count == 0)
            {
                _items.Add(item);
                return;
            }

            int index = Random.Next(0, _items.Count);
            _items.Add(_items[index]);
            _items[index] = item;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool TryTake(out T result)
        {
            if (_items.Count == 0)
            {
                result = default;
                return false;
            }

            result = _items[_items.Count - 1];
            _items.RemoveAt(_items.Count - 1);
            return true;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }
}
