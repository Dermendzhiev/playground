# Basic implementation of hash table in C#

#### basic-hash-table
- The `basic-hash-table` example is implemented with buckets, meaning each item (or node) is stored in array and each node represents a [Linked List](https://en.wikipedia.org/wiki/Linked_list) node
- The key position in the array is located by computing its hash mod number of buckets for insert/remove/get operations
- When collision occurs, we create a new node in the linked list for the located index
- The array is resized when the load factor is greater or equal to 0.75. The load factor is calculated by diving the number of items stored with buckets count. The implementation uses all-at-once resizing, which allocates new array with more buckets and rehash every key to the new array. This solution is *not* optimal for real-time systems
- The bucket size is always a prime number. In the case of non-random data, this will produce the most wide-spread distribution of integers to indices.