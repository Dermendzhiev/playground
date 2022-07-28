namespace ConsistentHashing
{
    public class Node
    {
        public Node(string ipAddress, string nodeKey)
        {
            this.IPAddress = ipAddress;
            this.NodeKey = nodeKey;
        }

        public string IPAddress { get; }

        public string NodeKey { get; set; }
    }
}