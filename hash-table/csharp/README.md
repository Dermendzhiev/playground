# Basic implementation of hash tables

### Summary
The project contains two implementations of hash table. We have `basic-hash-table` and `robin-hood-hashing` examples.

#### basic-hash-table
- The `basic-hash-table` example is implemented with buckets, meaning items are stored in array and each element is a [Linked List](https://en.wikipedia.org/wiki/Linked_list) node
- The item key position in the array is located by computing its hash mod number of buckets for insert/remove/get operations
- When collision occurs, we append a new node in the linked list for the located index element
- The array is resized when the load factor is greater or equal to 0.75. The load factor is calculated by diving the number of items stored with buckets count. The implementation uses all-at-once resizing, which allocates new array with more buckets and rehash every key to the new array. This solution is *not* optimal for real-time systems
- The bucket size is always a prime number. In the case of non-random data, this will produce the most wide-spread distribution of integers to indices.

#### robin-hood-hashing
- The hash table in the `robin-hood-hashing` example is implemented by following [Robin Hood Hashing](https://en.wikipedia.org/wiki/Hash_table#Robin_Hood_hashing) algorithm.
- In this example, when collision occurs, we're using open addressing based resolution algorithm. The algorithm is based on the notion of probe sequence lengths (PSL). The PSL of a key is the number of probes required to find the key during lookup or in other words, the distance from the original index position. For example when inserting an item:
  - If the slot of the hashed key is empty, we insert the key there and return. If not, we start probing for an empty slot.
  - When encountering an occupied slot we compare the PSL of the existing key, with the PSL of the new key. If the new key has a higher PSL it is "poorer" and it would be unfair to let go on further, so we swap
- The removal logic is also different as we need to do **backward shifting**.
  - The slot holding the key to remove is cleared out.
  - The algorithm then starts shifting the keys in the following slots back one step to fill the gap. The backward shifting continues until a key is encountered with PSL 0 or an empty slot is found.


### Further notes
- I have chosen to use linear probing when searching for a node, but this can be optimized by implementing organ-pipe search or smart search.


### References
- [Hash table - Wikipedia](https://en.wikipedia.org/wiki/Hash_table)
- [Robin Hood Hashing, Pedro Celis, 1986](https://cs.uwaterloo.ca/research/tr/1986/CS-86-14.pdf)
- [Robin Hood Hashing, Emmanuel Goossaert](https://codecapsule.com/2013/11/11/robin-hood-hashing/)
- [Robin Hood Hashing, Sebastian Sylvan](https://www.sebastiansylvan.com/post/robin-hood-hashing-should-be-your-default-hash-table-implementation/)
