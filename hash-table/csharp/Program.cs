using System;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        var hashTable = new csharp.basic_hash_table.BasicHashTable(capacity: 2, null);
        // var hashTable = new csharp.robin_hood_hashing.RobinHoodHashTable(capacity: 2, null);

        InsertOrUpdate(hashTable, 100_000, 0, "value-");
        InsertOrUpdate(hashTable, 100_000, 0, "new-value-");

        hashTable.Remove("key-1");
        try { hashTable.Remove("key-1"); }
        catch (KeyNotFoundException) { /* expected exception */ }

        try { hashTable.Get("key-1"); }
        catch (KeyNotFoundException) { /* expected exception */ }

        Remove(hashTable, 2, 10_000);
        InsertOrUpdate(hashTable, 50_000, 0, "value-");
    }

    private static void InsertOrUpdate(IHashTable hashTable, int count, int from, string valuePrefix)
    {
        for (var i = 0; i < count; i++)
        {
            var id = i + from;

            var key = $"key-{id}";
            var value = valuePrefix + id;

            hashTable.Upsert(key, value);
            hashTable.Get(key); // Left for testing purpose. Make sure we can retieve item after insert/update operation

            Console.WriteLine($"Inserted/updated item with key {key} and value {value}");
        }
    }

    private static void Remove(IHashTable hashTable, int from, int to)
    {
        for (var i = from; i < to; i++)
        {
            var key = $"key-{i}";
            hashTable.Remove(key);
            Console.WriteLine($"Removed item with key {key}");
        }
    }
}
