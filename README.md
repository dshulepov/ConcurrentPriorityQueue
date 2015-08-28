#Not supported
Moved to https://github.com/dshulepov/DataStructures

##ConcurrentPriorityQueue

C# implementation of generic heap-based concurrent [priority queue](http://en.wikipedia.org/wiki/Priority_queue) for .NET

>Priority queue is an abstract data type which is like a regular queue or 
>stack data structure, but where additionally each element has a "priority" 
>associated with it. In a priority queue, an element with high priority is 
>served before an element with low priority. If two elements have the same 
>priority, they are served according to their order in the queue.

###Features
- Generic
- Concurrent (i.e. supports multi-threading)
- Performant ( `O(n) = nlog(n)` for enqueue and dequeue )
- Fixed size support (items are dequeued before enqueueing when queue is full)
- Resizing support (queue grows and shrinks depending on the number of items)
- Alter priority of already enqueued item

###NuGet
- Install `PM> Install-Package PriorityQueue`
- [https://www.nuget.org/packages/PriorityQueue](https://www.nuget.org/packages/PriorityQueue/)

###Applications

- Bandwidth management
- [Discrete event simulation](http://en.wikipedia.org/wiki/Discrete_event_simulation)
- [Huffman coding](http://en.wikipedia.org/wiki/Huffman_coding)
- [Best-first search algorithms](http://en.wikipedia.org/wiki/Best-first_search)
- [ROAM triangulation algorithm](http://en.wikipedia.org/wiki/ROAM)
- [Dijkstra's algorithm](http://en.wikipedia.org/wiki/Dijkstra%27s_algorithm)
- [Prim's Algorithm](http://en.wikipedia.org/wiki/Prim%27s_algorithm) for Minimum Spanning Tree

##License
Released under the MIT license.
