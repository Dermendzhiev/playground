using System;
using ConsistentHashing.RingWithData;

namespace ConsistentHashing
{
    public class Program
    {
        public static void Main()
        {
            INodeDataRepository inMemoryRepository = new InMemoryRepository();
            const int totalNodeSpace = 1_000_000;
            const int replicas = 1000;

            var ring = new HashingRing(inMemoryRepository, totalNodeSpace, replicas);

            var nodesCount = 60;
            AddNodes(ring, nodesCount);

            var itemsCount = 1_000_000;
            SetItems(inMemoryRepository, ring, itemsCount);
            PrintItems(inMemoryRepository, ring, itemsCount);

            var removeNodesCount = 25;
            RemoveNodes(ring, itemsCount, removeNodesCount);

            SetItems(inMemoryRepository, ring, 100);
            PrintItems(inMemoryRepository, ring, itemsCount);

            inMemoryRepository.PrintCacheStats();
        }

        private static void RemoveNodes(HashingRing ring, int itemsCount, int nodesToRemove)
        {
            Console.WriteLine("Remove nodes:");

            var random = new Random();

            for (var i = 0; i < nodesToRemove; i++)
            {
                var key = random.Next(0, itemsCount);
                var ip = ring.GetNode($"key-{key}").IPAddress; // Get random node to remove
                ring.Remove(ip);
                Console.WriteLine($"Removed node with ip {ip}");
            }

            Console.WriteLine();
        }

        private static void SetItems(INodeDataRepository nodeDataRepository, HashingRing ring, int itemsCount)
        {
            Console.WriteLine("Set items:");

            for (var i = 1; i <= itemsCount; i++)
            {
                var key = $"key-{i}";
                var value = $"value-{i}";

                var node = ring.GetNode(key);
                nodeDataRepository.SetItem(node.IPAddress, key, value);

                Console.WriteLine($"Set item with key \"{key}\" and value \"{value}\" to node \"{node.IPAddress}\"");
            }

            Console.WriteLine();
        }

        private static void AddNodes(HashingRing ring, int nodesCount)
        {
            Console.WriteLine("Add nodes:");

            for (var i = 1; i <= nodesCount; i++)
            {
                var ipAddress = $"10.0.0.{i}";
                ring.Add(ipAddress);

                Console.WriteLine($"Added node with ip {ipAddress}");
            }

            Console.WriteLine();
        }

        private static void PrintItems(INodeDataRepository nodeDataRepository, HashingRing ring, int itemsCount)
        {
            Console.WriteLine("Print items:");

            for (var i = 1; i <= itemsCount; i++)
            {
                var key = $"key-{i}";
                var node = ring.GetNode(key);
                var value = nodeDataRepository.GetItem(node.IPAddress, key);

                Console.WriteLine($"Found key \"{key}\" with value \"{value}\" at node \"{node.IPAddress}\"");
            }

            Console.WriteLine();
        }
    }
}