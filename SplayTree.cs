using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplayTree
{
    // https://en.wikipedia.org/wiki/Splay_tree
    // http://www.cs.cmu.edu/~sleator/papers/self-adjusting.pdf
    public class SplayTree<T> : IEnumerable<T> where T : IComparable<T>, IComparable
    {
        private Node _root;

        #region "Properties"
        public int Count { get; private set; }

        //public T this[int i]
        //{
        //    get
        //    {
        //        List<Node> path = new List<Node>();
        //        var current = _root;
        //        path.Add(current);
        //        while (current.Left)
        //        {
        //            current = current.Left;
        //            path.Add(current);
        //        }
        //    }
        //}
        #endregion

        #region "Node class"
        class Node
        {
            public Node Left, Right;
            public T Key;

            public Node(T pKey)
            {
                Key = pKey;
                Left = null;
                Right = null;
            }

            public Node(Node pNode)
            {
                Key = pNode.Key;
                Left = pNode.Left;
                Right = pNode.Right;
            }

            public static bool operator true(Node pN)
            {
                return pN != null;
            }

            public static bool operator false(Node pN)
            {
                return pN == null;
            }

            public static bool operator !(Node pN)
            {
                return pN == null;
            }
        }
        #endregion

        #region "Constructor"
        public SplayTree()
        {
            _root = null;
            Count = 0;
        }

        public SplayTree(T pRoot)
        {
            _root = new Node(pRoot);
            Count = 1;
        }

        public SplayTree(T[] pItems)
        {
            _root = new Node(pItems[0]);
            for (int i = 1; i < pItems.Length; i++)
            {
                Add(ref _root, pItems[i]);
            }
        }
        #endregion

        #region "Public functions"
        /// <summary>
        /// Removes the specified element from the SplayTree.
        /// </summary>
        /// <param name="pKey">Item to remove from the SplayTree.</param>
        /// <returns>True if the item was successfully removed, otherwise false.</returns>
        public bool Remove(T pKey)
        {
            Node n;
            Node parent;
            FindWithParent(_root, pKey, out n, out parent);
            if (!n) return false;

            if (!n.Right && !n.Left)
            {
                //replace the node with the next in-order node
                Node next = SubtreeMax(n.Right);
                n.Key = next.Key;

                //Find the parent of the replaced node
                FindWithParent(n.Right, next.Key, out n, out parent);

                //delete the replaced node (use right since we took the max)
                parent.Right = null;
            }
            else //only one child
            {
                //check if the left or the right node of the parent = child
                if (parent.Right != null && parent.Right.Key.CompareTo(pKey) == 0)
                {
                    //set the child to the removed nodes child
                    parent.Right = !n.Right ? n.Right : n.Left;
                }
                else
                {
                    //set the child to the removed nodes child
                    parent.Left = !n.Right ? n.Right : n.Left;
                }
            }

            return true;
        }

        /// <summary>
        /// Retrurns if the SplayTree contains the specified item.
        /// </summary>
        /// <param name="pKey">Item to check if it is contained.</param>
        /// <returns>True if the element is found, otherwise false.</returns>
        public bool Contains(T pKey)
        {
            Node currentNode = _root;
            Node previousNode = null; //Need additional pointer so we can remove the contained node
            if (!_root) return false;
            if (pKey.CompareTo(currentNode.Key) == 0) return true;

            Node leftTree = null;
            Node rightTree = null;

            //While we are not at a leaf node
            while (currentNode)
            {

                //if the key is less than the current one move down the left
                if (pKey.CompareTo(currentNode.Key) < 0)
                {
                    //Since we are moving to the left hand side, the current node should be the first one in the RIGHT tree
                    if (!rightTree)
                    {
                        rightTree = new Node(currentNode);
                        rightTree.Left = null;
                    }

                    //Move to the left
                    previousNode = currentNode;
                    currentNode = currentNode.Left;

                    //If we are still on an active node
                    if (currentNode)
                    {
                        //Check if we need to add this child node to the left or right tree
                        if (pKey.CompareTo(currentNode.Key) > 0)
                        {
                            //Left tree... either create a new one or add it to the existing
                            if (!leftTree)
                            {
                                leftTree = new Node(currentNode);
                                leftTree.Right = null;
                            }
                            else
                            {
                                Add(ref leftTree, currentNode.Key);
                            }
                        }
                        else //Right tree
                        {
                            Add(ref rightTree, currentNode.Key);
                        }
                    }
                }
                else if (pKey.CompareTo(currentNode.Key) > 0) //Key is greater than the current node, move down the right
                {
                    //Since we are moving to the left hand side, the current node should be the first one in the LEFT tree
                    if (!leftTree)
                    {
                        leftTree = new Node(currentNode);
                        leftTree.Right = null;
                    }

                    //Move down the right
                    previousNode = currentNode;
                    currentNode = currentNode.Right;

                    //If we are still on an active node
                    if (currentNode)
                    {
                        //Check if we need to add this child node to the left or right tree
                        if (pKey.CompareTo(currentNode.Key) > 0)
                        {
                            Add(ref leftTree, currentNode.Key);
                        }
                        else
                        {
                            //Right tree... either create a new one or add it to the existing
                            if (!rightTree)
                            {
                                rightTree = new Node(currentNode);
                                rightTree.Left = null;
                            }
                            else
                            {
                                Add(ref rightTree, currentNode.Key);
                            }
                        }
                    }
                }
                else //we have found the node
                {
                    //Attach any subtrees to the left or right tree
                    if (currentNode.Left)
                    {
                        Attach(ref leftTree, ref currentNode.Left);
                    }

                    if (currentNode.Right)
                    {
                        Attach(ref rightTree, ref currentNode.Right);
                    }

                    Node n;
                    //Find the previous node in the tree
                    Find(pKey.CompareTo(previousNode.Key) < 0 ? leftTree : rightTree, previousNode.Key, out n);
                    if (!n || !n.Left) break;

                    //Find the pKey node and remove the link to it
                    if (n.Left.Key.CompareTo(pKey) == 0)
                    {
                        n.Left = null;
                    }
                    else
                    {
                        n.Right = null;
                    }

                    break;
                }

            }

            //If we found the node, splay the tree and return
            if (currentNode)
            {
                _root = currentNode;
                _root.Left = leftTree;
                _root.Right = rightTree;

                return true;
            }
            else //not found.  don't splay tree
            {
                leftTree = null;
                rightTree = null;
                return false;
            }

        }

        /// <summary>
        /// Adds the specified item to the SplayTree.
        /// </summary>
        /// <param name="pKey">Item to add to the SplayTree.</param>
        public void Add(T pKey)
        {
            Node currentNode = _root;
            Node leftTree = null;
            Node rightTree = null;

            //While we are not at a leaf node
            while (currentNode)
            {
                //if the key is less than the current one move down the left
                if (pKey.CompareTo(currentNode.Key) < 0)
                {
                    //Since we are moving to the left hand side, the current node should be the first one in the RIGHT tree
                    if (!rightTree)
                    {
                        rightTree = new Node(currentNode);
                        rightTree.Left = null;
                    }

                    //Move to the left
                    currentNode = currentNode.Left;

                    //If we are still on an active node
                    if (currentNode)
                    {
                        //Check if we need to add this child node to the left or right tree
                        if (pKey.CompareTo(currentNode.Key) > 0)
                        {
                            //Left tree... either create a new one or add it to the existing
                            if (!leftTree)
                            {
                                leftTree = new Node(currentNode);
                                leftTree.Right = null;
                            }
                            else
                            {
                                Add(ref leftTree, currentNode.Key);
                            }
                        }
                        else
                        {
                            Add(ref rightTree, currentNode.Key);
                        }
                    }
                }
                else //Key is greater than the current node, move down the right
                {
                    //Since we are moving to the left hand side, the current node should be the first one in the LEFT tree
                    if (!leftTree)
                    {
                        leftTree = new Node(currentNode);
                        leftTree.Right = null;
                    }

                    //Move down the right
                    currentNode = currentNode.Right;

                    //If we are still on an active node
                    if (currentNode)
                    {
                        //Check if we need to add this child node to the left or right tree
                        if (pKey.CompareTo(currentNode.Key) > 0)
                        {
                            Add(ref leftTree, currentNode.Key);
                        }
                        else
                        {
                            //Right tree... either create a new one or add it to the existing
                            if (!rightTree)
                            {
                                rightTree = new Node(currentNode);
                                rightTree.Left = null;
                            }
                            else
                            {
                                Add(ref rightTree, currentNode.Key);
                            }
                        }
                    }
                }
            }

            //Add the new node as the root node and splay
            _root = new Node(pKey);
            _root.Left = leftTree;
            _root.Right = rightTree;

            Count++;
        }

        /// <summary>
        /// Retrieves the item with the lowest value from the SplayTree.
        /// </summary>
        /// <returns>Item with the lowest value.</returns>
        public T Minimum()
        {
            return SubtreeMin(_root).Key;
        }

        /// <summary>
        /// Retrieves the item with the highest value from the SplayTree.
        /// </summary>
        /// <returns>Item with the highest value.</returns>
        public T Maximum()
        {
            return SubtreeMax(_root).Key;
        }
        #endregion

        #region "Private functions"
        /// <summary>
        /// Adds a key as a child to the given node without splaying the tree
        /// </summary>
        private void Add(ref Node pNode, T pKey)
        {
            if (!pNode)
            {
                pNode = new Node(pKey);
            }
            else if (pKey.CompareTo(pNode.Key) < 0)
            {
                Add(ref pNode.Left, pKey);
            }
            else
            {
                Add(ref pNode.Right, pKey);
            }
        }

        /// <summary>
        /// Adds a key as a child to the given node while splaying the tree
        /// </summary>
        private void Attach(ref Node pNode, ref Node pAttachNode)
        {
            if (!pNode)
            {
                pNode = pAttachNode;
            }
            else if (pAttachNode.Key.CompareTo(pNode.Key) < 0)
            {
                Attach(ref pNode.Left, ref pAttachNode);
            }
            else
            {
                Attach(ref pNode.Right, ref pAttachNode);
            }
        }

        private void Find(T pKey, out Node pNode)
        {
            Find(_root, pKey, out pNode);
        }

        private void Find(Node pSubtree, T pKey, out Node pNode)
        {
            Node currentNode = pSubtree;
            while (currentNode)
            {
                if (pKey.CompareTo(currentNode.Key) < 0)
                {
                    currentNode = currentNode.Left;
                }
                else if (pKey.CompareTo(currentNode.Key) > 0)
                {
                    currentNode = currentNode.Right;
                }
                else
                {
                    pNode = currentNode;
                    return;
                }
            }

            pNode = null;
        }

        private void FindWithParent(Node pRoot, T pKey, out Node pNode, out Node pParent)
        {
            Node currentNode = pRoot;
            Node previousNode = null;
            while (currentNode)
            {
                if (pKey.CompareTo(currentNode.Key) < 0)
                {
                    previousNode = currentNode;
                    currentNode = currentNode.Left;
                }
                else if (pKey.CompareTo(currentNode.Key) > 0)
                {
                    previousNode = currentNode;
                    currentNode = currentNode.Right;
                }
                else
                {
                    pParent = previousNode;
                    pNode = currentNode;
                    return;
                }
            }

            pNode = null;
            pParent = null;
        }

        private Node SubtreeMin(Node pNode)
        {
            Node retval = pNode;
            while (pNode.Left)
            {
                retval = pNode.Left;
            }

            return retval;
        }

        private Node SubtreeMax(Node pNode)
        {
            Node retval = pNode;
            while (pNode.Right)
            {
                retval = pNode.Right;
            }

            return retval;
        }
        #endregion

        #region "IEnumerator support"
        public IEnumerator<T> GetEnumerator()
        {
            return new SplayTreeEnumerator(_root);
        }

        private IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        #region "SplayTreeEnumerator"
        private class SplayTreeEnumerator : IEnumerator<T>
        {
            private List<Node> _path;
            private Node _root;
            private Node _current;
            private Node _next;

            public T Current
            {
                get { return _current.Key; }
            }

            private object Current1
            {
                get { return Current; }
            }
            object IEnumerator.Current
            {
                get { return Current1; }
            }

            public SplayTreeEnumerator(Node pRoot)
            {
                _path = new List<Node>();
                _root = pRoot;
                _current = pRoot;

                _path.Add(_current);
                var t = _current.Left;
                while (t)
                {
                    _path.Add(t);
                    t = t.Left;
                }
                _next = _path[_path.Count - 1];
            }

            public bool MoveNext()
            {
                _current = _next;
                _path.RemoveAt(_path.Count - 1);

                if (_path.Count > 0)
                {
                    _next = _path[_path.Count - 1];
                }
                else if (_next.Right)
                {
                    _path.Add(_next.Right);

                    var t = _next.Right.Left;
                    while (t)
                    {
                        _path.Add(t);
                        t = t.Left;
                    }

                    _next = _path[_path.Count - 1];
                }
                else
                {
                    return false;
                }

                return true;
            }

            public void Reset()
            {
                _path = new List<Node>();
                _current = _root;

                _path.Add(_current);
                var t = _current.Left;
                while (t)
                {
                    _path.Add(t);
                    t = t.Left;
                }
                _next = _path[_path.Count - 1];
            }

            public void Dispose()
            {
                _path = null;
            }
        }
        #endregion
        #endregion
    }
}
