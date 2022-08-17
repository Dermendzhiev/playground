/// <summary>
/// Basic implementation of hash table in C# with separate chaining (open hashing)
/// and all-at-once resizing
/// </summary>

using System;
using System.Collections.Generic;

namespace csharp.basic_hash_table
{
    public class HashTable
    {
        private const int MIN_BUCKETS = 5;
        private const double DEFAULT_LOAD_FACTOR = 0.75;

        private HashNode[] items;
        private long buckets;
        private double loadFactor;

        public HashTable(long? capacity, double? loadFactor)
        {
            if (capacity == null || capacity < MIN_BUCKETS)
            {
                this.buckets = MIN_BUCKETS;
            }
            else
            {
                this.buckets = this.GetNextPrimeNumber(capacity);
            }

            if (loadFactor == null || loadFactor <= 0)
            {
                this.loadFactor = DEFAULT_LOAD_FACTOR;
            }

            this.items = new HashNode[this.buckets];
        }

        public int Count { get; private set; }

        public bool ContainsKey(string key)
        {
            var targetBucket = this.GetBucket(key, this.buckets);
            var item = this.items[targetBucket];

            while (item != null)
            {
                if (item.Key == key)
                {
                    return true;
                }

                item = item.Next;
            }

            return false;
        }

        public void InsertOrUpdate(string key, object value)
        {
            var targetBucket = this.GetBucket(key, this.buckets);
            InsertOrUpdateItem(items, targetBucket, key, value);
            this.Count++;
            this.Resize();
        }

        public object Get(string key)
        {
            var targetBucket = this.GetBucket(key, this.buckets);
            var item = this.LocateNode(key, targetBucket);

            return item.Value;
        }

        public void Remove(string key)
        {
            var targetBucket = this.GetBucket(key, this.buckets);
            HashNode previousItem = null;
            var item = this.items[targetBucket];

            while (item != null)
            {
                if (item.Key == key)
                {
                    if (previousItem is null)
                    {
                        this.items[targetBucket] = item.Next;
                    }
                    else
                    {
                        previousItem.Next = item.Next;
                    }

                    this.Count--;
                    return;
                }

                previousItem = item;
                item = item.Next;
            }

            throw new KeyNotFoundException();
        }

        private HashNode LocateNode(string key, long targetBucket)
        {
            var node = this.items[targetBucket];

            while (node != null)
            {
                if (node.Key == key)
                {
                    return node;
                }

                node = node.Next;
            }

            throw new KeyNotFoundException();
        }

        private static void InsertOrUpdateItem(HashNode[] items, long targetBucket, string key, object value)
        {
            var item = items[targetBucket];
            while (item != null)
            {
                if (item.Key == key)
                {
                    item.Value = value;
                    return;
                }

                item = item.Next;
            }

            var nextNode = items[targetBucket];
            var hashNode = new HashNode
            {
                Key = key,
                Value = value,
                Next = nextNode
            };
            items[targetBucket] = hashNode;
        }

        private long GetBucket(string key, long buckets)
        {
            var hashCode = this.GetHash(key);
            return hashCode % buckets;
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
            var increasedBucketsSize = Convert.ToInt64(this.buckets * 1.5);
            var nextPrime = this.GetNextPrimeNumber(increasedBucketsSize);
            var newItemsArray = new HashNode[nextPrime];

            for (var i = 0; i < this.items.Length; i++)
            {
                var item = this.items[i];

                while (item != null)
                {
                    var index = this.GetBucket(item.Key, nextPrime);
                    InsertOrUpdateItem(newItemsArray, index, item.Key, item.Value);
                    item = item.Next;
                }
            }

            this.buckets = nextPrime;
            this.items = newItemsArray;
        }

        private double GetLoadFactor()
        {
            return (1.0 * this.Count) / buckets;
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

        public HashNode Next { get; set; }
    }
}