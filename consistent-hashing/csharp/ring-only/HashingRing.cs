using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class HashingRing
{
    private int totalNodeSpace;
    private int replicas;
    private List<int> nodeHashes;
    private Dictionary<int, Node> nodes;

    public HashingRing(int totalNodeSpace = int.MaxValue, int replicas = 100)
    {
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
        var successorNodeHashIndex = this.SearchNode(hashPosition);
        if (successorNodeHashIndex is null)
        {
            return null;
        }

        var successorNodeHash = this.nodeHashes[successorNodeHashIndex.Value];
        var successorNode = this.nodes[successorNodeHash];

        return successorNode;
    }

    public void Remove(string ipAddress)
    {
        for (var i = 0; i < replicas; i++)
        {
            var nodeKey = this.FormatNodeKey(ipAddress, i);
            var hashPosition = this.GetHashPosition(nodeKey);

            var nodeHashIndex = this.SearchNode(hashPosition);

            this.nodeHashes.RemoveAt(nodeHashIndex.Value);
            this.nodes.Remove(hashPosition);
        }
    }

    // Perform binary search across nodeHashes list, supposing it's already sorted
    // `getPrevious` determines if we search for the smaller or bigger value than `hashPosition`
    private int? SearchNode(int hashPosition, bool getPrevious = false)
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

    private void AddNode(string ipAddress, string nodeKey)
    {
        var hashPosition = this.GetHashPosition(nodeKey);
        var node = new Node(ipAddress, nodeKey);

        this.nodes.Add(hashPosition, node);
        this.nodeHashes.Add(hashPosition);

        this.nodeHashes.Sort();
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
