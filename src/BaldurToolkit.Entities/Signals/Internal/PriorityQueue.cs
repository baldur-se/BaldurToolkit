using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BaldurToolkit.Entities.Signals.Internal
{
    /// <summary>
    /// TODO: Replace with System.Collections.Generic.PriorityQueue&lt;T&gt; when it's out: https://github.com/dotnet/corefx/issues/574
    ///
    /// The _heap array represents a binary tree with the "shape" property.
    /// If we number the nodes of a binary tree from left-to-right and top-
    /// to-bottom as shown,
    ///
    ///             0
    ///           /   \
    ///          /     \
    ///         1       2
    ///       /  \     / \
    ///      3    4   5   6
    ///     /\    /
    ///    7  8  9
    ///
    /// The shape property means that there are no gaps in the sequence of
    /// numbered nodes, i.e., for all N > 0, if node N exists then node N-1
    /// also exists. For example, the next node added to the above tree would
    /// be node 10, the right child of node 4.
    ///
    /// Because of this constraint, we can easily represent the "tree" as an
    /// array, where node number == array index, and parent/child relationships
    /// can be calculated instead of maintained explicitly. For example, for
    /// any node N > 0, the parent of N is at array index (N - 1) / 2.
    ///
    /// In addition to the above, the first _count members of the _heap array
    /// compose a "heap", meaning each child node is greater than or equal to
    /// its parent node; thus, the root node is always the minimum (i.e., the
    /// best match for the specified style, weight, and stretch) of the nodes
    /// in the heap.
    ///
    /// Initially _count &lt; 0, which means we have not yet constructed the heap.
    /// On the first call to MoveNext, we construct the heap by "pushing" all
    /// the nodes into it. Each successive call "pops" a node off the heap
    /// until the heap is empty (_count == 0), at which time we've reached the
    /// end of the sequence.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    internal class PriorityQueue<T>
    {
        private const int DefaultCapacity = 6;

        private T[] heap;
        private int count;
        private IComparer<T> comparer;

        public PriorityQueue(int capacity, IComparer<T> comparer)
        {
            this.heap = new T[capacity > 0 ? capacity : DefaultCapacity];
            this.count = 0;
            this.comparer = comparer;
        }

        /// <summary>
        /// Gets the number of items in the priority queue.
        /// </summary>
        public int Count
        {
            get { return this.count; }
        }

        /// <summary>
        /// Gets the first or topmost object in the priority queue, which is the
        /// object with the minimum value.
        /// </summary>
        public T Top
        {
            get
            {
                Debug.Assert(this.count > 0, "Invalid count.");
                return this.heap[0];
            }
        }

        /// <summary>
        /// Adds an object to the priority queue.
        /// </summary>
        /// <param name="value">Value.</param>
        public void Push(T value)
        {
            // Increase the size of the array if necessary.
            if (this.count == this.heap.Length)
            {
                T[] temp = new T[this.count * 2];
                for (int i = 0; i < this.count; ++i)
                {
                    temp[i] = this.heap[i];
                }

                this.heap = temp;
            }

            // Loop invariant:
            //
            //  1.  index is a gap where we might insert the new node; initially
            //      it's the end of the array (bottom-right of the logical tree).
            int index = this.count;
            while (index > 0)
            {
                int parentIndex = HeapParent(index);
                if (this.comparer.Compare(value, this.heap[parentIndex]) < 0)
                {
                    // value is a better match than the parent node so exchange
                    // places to preserve the "heap" property.
                    this.heap[index] = this.heap[parentIndex];
                    index = parentIndex;
                }
                else
                {
                    // we can insert here.
                    break;
                }
            }

            this.heap[index] = value;
            this.count++;
        }

        /// <summary>
        /// Removes the first node (i.e., the logical root) from the heap.
        /// </summary>
        public void Pop()
        {
            Debug.Assert(this.count != 0, "Invalid count.");

            if (this.count > 1)
            {
                // Loop invariants:
                //
                //  1.  parent is the index of a gap in the logical tree
                //  2.  leftChild is
                //      (a) the index of parent's left child if it has one, or
                //      (b) a value >= _count if parent is a leaf node
                int parent = 0;
                int leftChild = HeapLeftChild(parent);

                while (leftChild < this.count)
                {
                    int rightChild = HeapRightFromLeft(leftChild);
                    int bestChild =
                        (rightChild < this.count && this.comparer.Compare(this.heap[rightChild], this.heap[leftChild]) < 0) ?
                        rightChild : leftChild;

                    // Promote bestChild to fill the gap left by parent.
                    this.heap[parent] = this.heap[bestChild];

                    // Restore invariants, i.e., let parent point to the gap.
                    parent = bestChild;
                    leftChild = HeapLeftChild(parent);
                }

                // Fill the last gap by moving the last (i.e., bottom-rightmost) node.
                this.heap[parent] = this.heap[this.count - 1];
            }

            this.count--;
        }

        /// <summary>
        /// Calculate the parent node index given a child node's index, taking advantage
        /// of the "shape" property.
        /// </summary>
        /// <param name="i">Inddex.</param>
        /// <returns>Parent node index.</returns>
        private static int HeapParent(int i)
        {
            return (i - 1) / 2;
        }

        /// <summary>
        /// Calculate the left child's index given the parent's index, taking advantage of
        /// the "shape" property. If there is no left child, the return value is >= _count.
        /// </summary>
        /// <param name="i">Inddex.</param>
        /// <returns>Index.</returns>
        private static int HeapLeftChild(int i)
        {
            return (i * 2) + 1;
        }

        /// <summary>
        /// Calculate the right child's index from the left child's index, taking advantage
        /// of the "shape" property (i.e., sibling nodes are always adjacent). If there is
        /// no right child, the return value >= _count.
        /// </summary>
        /// <param name="i">Inddex.</param>
        /// <returns>Index.</returns>
        private static int HeapRightFromLeft(int i)
        {
            return i + 1;
        }
    }
}
