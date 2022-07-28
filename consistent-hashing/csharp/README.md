# Experimental implementation of consistent hashing in C#.

### The project contains two implementations
- **1 - ring-only:** contains basic implementation for building a ring of nodes
- **2 - ring-with-data:** extends the basic hashing ring and introduces work with node data (simulated in-memory) and migrating data on add or remove node operations


### Description
Assuming you're already familiar with consistent hashing concepts, here's a short explanation of what you will see in the project:

- All nodes are stored in a "ring", where each one keeps the data for keys with position lower or equal to node's position.
- Position is calculated by hashing a key (node ip address or item key) mod total node space.
- Adding a node:
  - When adding a node, we do rehash and migration of relevant keys from it's successor node.
  - If there is a collision, when calculating node hash position, we solve it with [Open Addressing](https://www.geeksforgeeks.org/hashing-set-3-open-addressing/), meaning we search for the next free hash position
- Removing a node:
  - When removing a node, we migrate it's data to the successor node 
- Search node by key:
  - We find the relevant node for key by searching for the first node with hash position >= key hash. If key hash value is greater than max hash position - we assign it to the first node
- Replicas
  - In order to have more "even" distribution of data, we create number of replicas or virtual nodes, which are only links in the hashing ring to their real node.


### Further notes
- Can consider adding **weighted nodes**. If a node has more resources compared to others, we can add more "weight" to it. To achieve that, we can increase the number of labels (replicas) to adjust the probability of keys ending up in the nodes with greater weight.
- Consider replacing open addressing with [Double Hashing](https://www.geeksforgeeks.org/double-hashing/?ref=lbp) for node hash collisions, as it reduces clustering in an optimized way, but for the cost of poor cache performance. The implementation should follow `(hash1(key) + i * hash2(key)) % TABLE_SIZE`.


### Resources
- Inspired by [Dynamo White Paper](https://www.allthingsdistributed.com/files/amazon-dynamo-sosp2007.pdf)
- [Implementation guideline](https://en.wikipedia.org/wiki/Consistent_hashing#Implementation)
- [Open addressing](https://en.wikipedia.org/wiki/Open_addressing)
