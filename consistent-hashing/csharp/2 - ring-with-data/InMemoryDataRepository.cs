using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsistentHashing.RingWithData
{
    public class InMemoryRepository : INodeDataRepository
    {
        private Dictionary<string, Dictionary<string, object>> cache;

        public InMemoryRepository()
        {
            cache = new Dictionary<string, Dictionary<string, object>>();
        }

        public void PrintCacheStats()
        {
            var nodeItemsCount = new List<int>();

            foreach (var node in cache)
            {
                nodeItemsCount.Add(node.Value.Count);
            }

            Console.WriteLine($"Min items in node: {nodeItemsCount.Min()}");
            Console.WriteLine($"Max items in node: {nodeItemsCount.Max()}");
            Console.WriteLine($"Average items per node: {nodeItemsCount.Average()}"); ;
        }

        public void SetItem(string nodeIpAddress, string key, object value)
        {
            if (!cache.ContainsKey(nodeIpAddress))
            {
                cache.Add(nodeIpAddress, new Dictionary<string, object>());
            }

            var nodeCache = cache[nodeIpAddress];
            if (nodeCache.ContainsKey(key))
            {
                nodeCache[key] = value;
            }
            else
            {
                nodeCache.Add(key, value);
            }
        }

        public object GetItem(string nodeIpAddress, string key)
        {
            return cache[nodeIpAddress][key];
        }

        public void RemoveItem(string nodeIpAddress, string key)
        {
            cache[nodeIpAddress].Remove(key);
        }

        public Dictionary<string, object> GetItems(string nodeIpAddress)
        {
            if (!cache.ContainsKey(nodeIpAddress))
            {
                return new Dictionary<string, object>();
            }

            return cache[nodeIpAddress];
        }
    }
}