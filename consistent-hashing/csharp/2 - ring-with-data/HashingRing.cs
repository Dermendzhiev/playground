using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ConsistentHashing.RingWithData
{
    public class HashingRing : IHashingRing
    {
        private int totalNodeSpace;
        private int replicas;
        private List<int> nodeHashes;
        private Dictionary<int, Node> nodes;
        private INodeDataRepository repository;

        public HashingRing(INodeDataRepository repository, int totalNodeSpace = int.MaxValue, int replicas = 100)
        {
            this.repository = repository;
            this.totalNodeSpace = totalNodeSpace;
            this.replicas = replicas;
            this.nodeHashes = new List<int>();
            this.nodes = new Dictionary<int, Node>();
        }

        public void Add(string ipAddress)
        {
            if (this.nodeHashes.Count == this.totalNodeSpace)
            {
                throw new Exception("Max node capacity reached");
            }

            // For each node, we add one or more virtual nodes (or replicas) accross the ring. This helps for better distribution of data
            for (var i = 0; i < replicas; i++)
            {
                var nodeKey = this.FormatNodeKey(ipAddress, i);
                this.AddNode(ipAddress, nodeKey);
            }
        }

        public Node GetNode(string key)
        {
            var hashPosition = GetHashPosition(key);
            var node = this.GetSuccessorNode(hashPosition);

            return node;
        }

        public void Remove(string ipAddress)
        {
            for (var i = 0; i < replicas; i++)
            {
                var nodeKey = this.FormatNodeKey(ipAddress, i);
                var hashPosition = this.GetNodeHashPosition(nodeKey, false);

                var nodeHashIndex = this.SearchNodeIndex(hashPosition);

                this.nodeHashes.RemoveAt(nodeHashIndex.Value);
                this.nodes.Remove(hashPosition);
            }

            MigrateDataFromRemovedNode(ipAddress);
        }

        // Perform binary search across nodeHashes list, supposing it's already sorted
        // `getPrevious` determines if we search for the smaller or bigger value than `hashPosition`
        private int? SearchNodeIndex(int hashPosition, bool getPrevious = false)
        {
            if (this.nodeHashes.Count == 0)
            {
                return null;
            }

            // If the key is greater than the max hash, circle back and return first node
            if (!getPrevious && hashPosition > this.nodeHashes[this.nodeHashes.Count - 1])
            {
                return 0;
            }

            // If the key is smaller than minimum hash, return the biggest one as it's previous
            if (getPrevious && hashPosition < this.nodeHashes[0])
            {
                return this.nodeHashes.Count - 1;
            }

            return this.BinarySearch(hashPosition, getPrevious, this.nodeHashes);
        }

        private void MigrateDataFromRemovedNode(string ipAddress)
        {
            var removedNodeCache = this.repository.GetItems(ipAddress);

            foreach (var cachedItem in removedNodeCache)
            {
                var itemHashPosition = this.GetHashPosition(cachedItem.Key);
                var node = this.GetSuccessorNode(itemHashPosition);

                this.repository.SetItem(node.IPAddress, cachedItem.Key, cachedItem.Value);
            }
        }

        private Node GetSuccessorNode(int hashPosition)
        {
            var successorNodeHashIndex = this.SearchNodeIndex(hashPosition);
            if (successorNodeHashIndex is null)
            {
                return null;
            }

            var successorNodeHash = this.nodeHashes[successorNodeHashIndex.Value];
            var successorNode = this.nodes[successorNodeHash];

            return successorNode;
        }

        private void AddNode(string ipAddress, string nodeKey)
        {
            var hashPosition = GetNodeHashPosition(nodeKey, true);
            var successorNode = GetSuccessorNode(hashPosition);

            if (this.nodes.Count > 0 && successorNode != null)
            {
                MigrateDataToNewNode(ipAddress, hashPosition, successorNode);
            }

            var node = new Node(ipAddress, nodeKey);

            this.nodes.Add(hashPosition, node);
            this.nodeHashes.Add(hashPosition);

            this.nodeHashes.Sort();
        }

        private void MigrateDataToNewNode(string ipAddress, int hashPosition, Node successorNode)
        {
            var predecessorNodeHashIndex = SearchNodeIndex(hashPosition, true);
            var predecessorNodeHash = this.nodeHashes[predecessorNodeHashIndex.Value];
            var predecessorNode = this.nodes[predecessorNodeHash];

            if (ipAddress == successorNode.IPAddress)
            {
                // Reached node replica, nothing to migrate
                return;
            }

            var successorNodeCache = this.repository.GetItems(successorNode.IPAddress);
            foreach (var cachedItem in successorNodeCache)
            {
                var itemHashPosition = this.GetHashPosition(cachedItem.Key);
                bool migrateData = false;

                // Check if item hash is bigger than max hash and smaller or equal to new node hash
                if (predecessorNodeHash > hashPosition && (itemHashPosition <= hashPosition || itemHashPosition > predecessorNodeHash))
                {
                    migrateData = true;
                }
                // Check if item hash is bigger than predecessor node hash and smaller or equal to the new node hash
                else if (predecessorNodeHash < itemHashPosition && itemHashPosition <= hashPosition)
                {
                    migrateData = true;
                }

                if (migrateData)
                {
                    this.repository.SetItem(ipAddress, cachedItem.Key, cachedItem.Value);
                    this.repository.RemoveItem(successorNode.IPAddress, cachedItem.Key);
                }
            }
        }

        // Open-addressing used for collision resolution -> https://en.wikipedia.org/wiki/Open_addressing
        // Quadratic Probing
        private int GetNodeHashPosition(string nodeKey, bool findEmptyLocation)
        {
            var hash = this.Hash(nodeKey);
            var hashPosition = Math.Abs(hash % totalNodeSpace);

            var foundNodeHash = this.nodeHashes.Contains(hashPosition);
            Node foundNode = null;
            if (foundNodeHash)
            {
                foundNode = this.nodes[hashPosition];
            }

            var probingValue = 1;

            // Based on `findEmptyLocation` parameter, find empty hash location for new node or hash location of existing one
            while (findEmptyLocation
                    ? foundNodeHash
                    : !foundNodeHash || (foundNodeHash && nodeKey != foundNode.NodeKey))
            {
                hash += probingValue * probingValue;
                hashPosition = Math.Abs(hash % totalNodeSpace);

                foundNodeHash = this.nodeHashes.Contains(hashPosition);
                if (foundNodeHash)
                {
                    foundNode = this.nodes[hashPosition];
                }

                probingValue++;
            }

            return hashPosition;
        }

        private int BinarySearch(int searchValue, bool getPrevious, List<int> sortedCollection)
        {
            var start = 0;
            var end = sortedCollection.Count - 1;

            while (start <= end)
            {
                var mid = (end + start) / 2;

                if (searchValue == sortedCollection[mid])
                {
                    return mid;
                }

                if (searchValue > sortedCollection[mid])
                {
                    start = mid + 1;
                }
                else if (searchValue < sortedCollection[mid])
                {
                    end = mid - 1;
                }
            }

            return getPrevious ? end : start;
        }

        private int GetHashPosition(string key)
        {
            var hash = this.Hash(key);
            var hashPosition = hash % totalNodeSpace;

            return Math.Abs(hashPosition);
        }

        private int Hash(string key)
        {
            using (var sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(key));
                var hash = BitConverter.ToInt32(bytes, 0);

                return hash;
            }
        }

        private string FormatNodeKey(string ipAddress, int replicaNumber)
        {
            return $"{ipAddress}:{replicaNumber}";
        }
    }
}