namespace ConsistentHashing
{
    public interface IHashingRing
    {
        void Add(string ipAddress);

        void Remove(string ipAddress);

        Node GetNode(string key);
    }
}