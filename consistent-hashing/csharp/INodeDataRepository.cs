using System.Collections.Generic;

namespace ConsistentHashing
{
    public interface INodeDataRepository
    {
        object GetItem(string nodeIpAddress, string key);

        Dictionary<string, object> GetItems(string nodeIpAddress);

        void RemoveItem(string nodeIpAddress, string key);

        void SetItem(string nodeIpAddress, string key, object value);

        void PrintCacheStats();
    }
}