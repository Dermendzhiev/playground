/// <summary>
/// Basic implementation of hash table in C# with Robin Hood hashing (based on open addressing with linear probing)
/// and all-at-once resizing
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;

namespace csharp.robin_hood_hashing
{
    public class RobinHoodHashTable : IHashTable
    {
        private const int MIN_CAPACITY = 5;
        private const double DEFAULT_LOAD_FACTOR = 0.99;

        private HashNode[] items;
        private long capacity;
        private double loadFactor;
        private int longestProbe = 0;

        public RobinHoodHashTable(long? capacity, double? loadFactor)
        {
            if (capacity == null || capacity < MIN_CAPACITY)
            {
                this.capacity = MIN_CAPACITY;
            }
            else
            {
                this.capacity = this.GetNextPrimeNumber(capacity.Value);
            }

            if (loadFactor == null || loadFactor <= 0)
            {
                this.loadFactor = DEFAULT_LOAD_FACTOR;
            }

            this.items = new HashNode[this.capacity];
        }

        public int Count { get; private set; }

        public bool ContainsKey(string key)
        {
            var targetIndex = this.GetIndex(key, this.capacity);

            try
            {
                this.LocateNode(key, targetIndex);
                return true;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }

        public void Upsert(string key, object value)
        {
            var targetIndex = this.GetIndex(key, this.capacity);
            InsertOrUpdateItem(items, targetIndex, key, value, ref this.longestProbe);
            this.Count++;
            this.Resize();
        }

        public object Get(string key)
        {
            var targetIndex = this.GetIndex(key, this.capacity);
            var item = this.LocateNode(key, targetIndex);

            return item.Value;
        }

        public void Remove(string key)
        {
            var targetIndex = this.GetIndex(key, this.capacity);
            var node = this.items[targetIndex];
            bool removed = false;
            var distance = 0;

            while (node != null)
            {
                if (distance > this.longestProbe)
                {
                    break;
                }

                if (targetIndex == this.items.Length)
                {
                    targetIndex = 0;
                }

                if (this.items[targetIndex].Key == key)
                {
                    this.items[targetIndex] = null;
                    removed = true;
                    break;
                }

                targetIndex++;
                node = this.items[targetIndex];
                distance++;
            }

            if (!removed)
            {
                throw new KeyNotFoundException();
            }

            while (true)
            {
                var nextIndex = targetIndex + 1;
                if (nextIndex >= this.items.Length)
                {
                    nextIndex = 0;
                }

                if (this.items[targetIndex] != null || this.items[nextIndex] == null || this.items[nextIndex].ProbeSequenceLength <= 0)
                {
                    return;
                }

                this.items[targetIndex] = this.items[nextIndex];
                this.items[targetIndex].ProbeSequenceLength--;
                this.items[nextIndex] = null;

                targetIndex++;
            }
        }

        private HashNode LocateNode(string key, long targetIndex)
        {
            var distance = 0;
            var node = this.items[targetIndex];

            while (node != null)
            {
                if (distance > this.longestProbe)
                {
                    break;
                }

                if (node.Key == key)
                {
                    return node;
                }

                targetIndex++;
                distance++;

                if (targetIndex == this.items.Length)
                {
                    targetIndex = 0;
                }

                node = this.items[targetIndex];
            }

            throw new KeyNotFoundException();
        }

        private static void InsertOrUpdateItem(HashNode[] items, long targetIndex, string key, object value, ref int longestProbe)
        {
            var hashNode = new HashNode
            {
                Key = key,
                Value = value
            };

            var distance = 0;

            while (true)
            {
                if (targetIndex == items.Length)
                {
                    targetIndex = 0;
                }
                longestProbe = Math.Max(longestProbe, distance);

                if (items[targetIndex] == null)
                {
                    hashNode.ProbeSequenceLength = distance;
                    items[targetIndex] = hashNode;
                    return;
                }

                if (items[targetIndex].Key == key)
                {
                    items[targetIndex].Value = value;
                    return;
                }

                if (distance > items[targetIndex].ProbeSequenceLength)
                {
                    var temp = items[targetIndex];
                    hashNode.ProbeSequenceLength = distance;
                    items[targetIndex] = hashNode;

                    hashNode = temp;
                    distance = hashNode.ProbeSequenceLength;
                }

                targetIndex++;
                distance++;
            }
        }

        private long GetIndex(string key, long capacity)
        {
            var hashCode = this.GetHash(key);
            return hashCode % capacity;
        }

        private int GetHash(string key)
        {
            return key.GetHashCode() & 0x7FFFFFFF;
        }

        private void Resize()
        {
            var loadFactor = this.GetLoadFactor();
            if (loadFactor <= this.loadFactor)
            {
                return;
            }

            Rehash();
        }

        private void Rehash()
        {
            var increasedCapacity = Convert.ToInt64(this.capacity * 1.5);
            var nextPrime = this.GetNextPrimeNumber(increasedCapacity);
            var newItemsArray = new HashNode[nextPrime];
            var longestProbe = 0;

            for (var i = 0; i < this.items.Length; i++)
            {
                var node = this.items[i];

                if (node == null)
                {
                    continue;
                }

                var index = this.GetIndex(node.Key, nextPrime);
                InsertOrUpdateItem(newItemsArray, index, node.Key, node.Value, ref longestProbe);
            }

            this.longestProbe = longestProbe;
            this.capacity = nextPrime;
            this.items = newItemsArray;
        }

        private double GetLoadFactor()
        {
            return (1.0 * this.Count) / this.capacity;
        }

        private long GetNextPrimeNumber(long startingNumber)
        {
            while (true)
            {
                bool isPrime = true;
                startingNumber = startingNumber + 1;

                int squaredNumber = (int)Math.Sqrt(startingNumber);

                for (int i = 2; i <= squaredNumber; i++)
                {
                    if (startingNumber % i == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                {
                    return startingNumber;
                }
            }
        }
    }

    public class HashNode
    {
        public string Key { get; set; }

        public object Value { get; set; }

        public int ProbeSequenceLength { get; set; }
    }
}