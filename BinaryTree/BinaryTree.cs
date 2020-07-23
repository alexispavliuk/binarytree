using System;
using System.Collections;
using System.Collections.Generic;

namespace BinaryTree
{
  public class BinaryTree<T> : IEnumerable<T> 
    {
        #region EventHandlers
        /// <summary>
        /// Handles events for adding and removing elements
        /// </summary>
        /// <param name="sender">Instance of <see cref="BinaryTree<T>"/> that called the event</param>
        /// <param name="args">Arguments passed by sender for subscribers</param>
        public delegate void TreeEventHandler(object sender, TreeEventArgs<T> args);

        /// <summary>
        /// Event that should be called when new element is added
        /// </summary>
        public event TreeEventHandler ElementAdded;

        /// <summary>
        /// Event that should be called when element in tree is removed
        /// </summary>
        public event TreeEventHandler ElementRemoved;
        #endregion

        #region Properties
        /// <summary>
        /// Defines how many elements tree contains
        /// </summary>
        public int Count { get; private set; }
        public Node<T> Root { get; private set; }
        public IComparer<T> ComparerT { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Checks if type T implements <see cref="IComparable<T>"/>
        /// If it does: saves and uses as default comparer
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when T doesn't implement <see cref="IComparable<T>"</exception>
        public BinaryTree()
        {
            if (Array.Find(typeof(T).GetInterfaces(), 
                s => s.ToString().Contains("IComparable")) != default)
            {
                ComparerT = Comparer<T>.Default;
            }
            else throw new ArgumentException("T doesn't implement IComparable<T>");
        }

        /// <summary>
        /// Creates instance of tree and saves custom comparer passed by parameter
        /// </summary>
        /// <param name="comparer"><see cref="IComparer<T>"/></param>
        public BinaryTree(IComparer<T> comparer)
        {
             ComparerT = comparer;
        }
        #endregion

        #region Methods
        #region Add
        /// <summary>
        /// Adds element to the tree according to comparer
        /// </summary>
        /// <param name="item">Object that should be added in tree</param>
        /// <exception cref="ArgumentNullException">Thrown if parameter was null</exception>
        public void Add(T item)
        {
            if (item is null)
            {
                throw new ArgumentNullException("item","item can`t be null");
            }
            if (Root is null)
            {
                Root = new Node<T>(item);
                Count++;
                TreeEventArgs<T> args = new TreeEventArgs<T>(item, $"Element {item} was added into the tree(Root)");
                ElementAdded?.Invoke(this, args);
            }
            else AddingRecursively(Root, item);
        }

        public void AddingRecursively(Node<T> node, T data)
        {
            switch (ComparerT.Compare(node.Data, data) <= 0)
            {
                case true:
                    if (node.Right == null)
                    {
                        node.Right = new Node<T>(data);
                        Count++;
                        TreeEventArgs<T> args = new TreeEventArgs<T>(data, $"Element {data} was added into the tree");
                        ElementAdded?.Invoke(this, args);
                    }
                    else AddingRecursively(node.Right, data);
                    break;
                case false:
                    if (node.Left == null)
                    {
                        node.Left = new Node<T>(data);
                        Count++;
                        TreeEventArgs<T> args = new TreeEventArgs<T>(data, $"Element {data} was added into the tree");
                        ElementAdded?.Invoke(this, args);
                    }
                    else AddingRecursively(node.Left, data);
                    break;
            }
        }
#endregion

        #region Remove
        /// <summary>
        /// Removes element from tree by its reference
        /// </summary>
        /// <param name="item">Object that should be removed from tree</param>
        /// <returns>True if element was deleted succesfully, false if element wasn't found in tree</returns>
        public bool Remove(T item)
        {
            Node<T> current = Root;
            Node<T> father = null;
            while (current != null)
            {
                if (ComparerT.Compare(current.Data, item) < 0)
                {
                    father = current;
                    current = current.Right;
                }
                else if (ComparerT.Compare(current.Data, item) > 0)
                {
                    father = current;
                    current = current.Left;
                }
                else break;
            }
            if (current == null)
            {
                return false;
            } 

            Count--;

            // checking if node is a leaf and deleting it
            if (current.Right == null && current.Left == null)
            {
                // checking if node to delete is root
                if (father == null)
                    Root = null;
                // comparing what node of children should we use for replacing
                else if (ComparerT.Compare(father.Data, current.Data) > 0)
                {
                    father.Left = null;
                }
                else
                {
                    father.Right = null;
                }
            }
            //checking if node has only left child
            else if (current.Right == null)
            {
                // checking if node to delete is root
                if (father == null)
                    Root = current.Left;
                else
                {
                    father.Left = current.Left;
                }
            }
            //checking if node has only right child
            else if (current.Left == null)
            {
                // checking if node to delete is root
                if (father == null)
                    Root = current.Right;
                else
                {
                    father.Left = current.Right;
                }
            }
            // if node has both left and right
            // we should replace node with the smallest node of right sub-tree 
            else
            {
                // temporary node to replace
                Node<T> tmp = current.Right;
                if (tmp.Left == null)
                {
                    tmp.Left = current.Left;
                }
                else
                {
                    while (tmp.Left.Left != null)
                        tmp = tmp.Left;
                    // one more temporary node, we need it for saving node to replace
                    Node<T> tmp2 = tmp.Left;
                    // deleting node from this place
                    tmp.Left = null;
                    tmp2.Left = current.Left;
                    tmp2.Right = current.Right;
                    tmp = tmp2;
                }
                if (father == null)
                {
                    Root = tmp;
                }
                else if (ComparerT.Compare(father.Data, current.Data) > 0)
                    father.Left = tmp;
                else
                {
                    father.Right = tmp;
                } 
            }
            TreeEventArgs<T> args = new TreeEventArgs<T>(item, $"Element {item} was removed from tree");
            ElementRemoved?.Invoke(this, args);
            return true;
        }
        #endregion

        #region TreeMax
        /// <summary>
        /// Returns item with the highest value
        /// </summary>
        /// <returns>The element with highest value</returns>
        /// <exception cref="InvalidOperationException">Thrown if tree is empty</exception> 
        public T TreeMax()
        {
            if (Root == null)
            {
                throw new InvalidOperationException("tree is empty");
            }
            Node<T> current = Root;
            while (current.Right != null)
                current = current.Right;
            return current.Data;
        }
        #endregion

        #region TreeMin
        /// <summary>
        /// Returns item with the lowest value
        /// </summary>
        /// <returns>The element with lowest value</returns>
        /// <exception cref="InvalidOperationException">Thrown if tree is empty</exception>
        public T TreeMin()
        {
            if (Root == null)
            {
                throw new InvalidOperationException("tree is empty");
            }
            Node<T> current = Root;
            while (current.Left != null)
                current = current.Left;
            return current.Data;
        }
        #endregion

        #region Contains
        /// <summary>
        /// Checks if tree contains element by its reference
        /// </summary>
        /// <param name="item">Object that should (or not) be found in tree</param>
        /// <returns>True if tree contains item, false if it doesn't</returns>
        public bool Contains(T data)
        {
            Node<T> current = Root;
            while (current != null)
            {
                if (ComparerT.Compare(current.Data, data) < 0)
                    current = current.Right;
                else if (ComparerT.Compare(current.Data, data) > 0)
                    current = current.Left;
                else break;
            }
            if (current == null)
                return false;
            return true;
        }
        #endregion

        #region Traverse
        /// <summary>
        /// Makes tree traversal
        /// </summary>
        /// <param name="traverseType"><see cref="TraverseType"></param>
        /// <returns>Sequense of elements of tree according to traverse type</returns>
        public IEnumerable<T> Traverse(TraverseType traverseType)
        {
            Func<Node<T>, IEnumerable<T>> traverseDelegate;
            switch (traverseType)
            {
                default:
                    traverseDelegate = TraverseInOrder;
                    break;
                case TraverseType.PreOrder:
                    traverseDelegate = TraversePreOrder;
                    break;
                case TraverseType.PostOrder:
                    traverseDelegate = TraversePostOrder;
                    break;
            }
            foreach (T t in traverseDelegate.Invoke(Root))
                yield return t;
        }

        /// <summary>
        /// Makes tree traversal InOrder
        /// </summary>
        public IEnumerable<T> TraverseInOrder(Node<T> node)
        {
            if (node != null)
            {
                foreach (T t in TraverseInOrder(node.Left))
                    yield return t;
                yield return node.Data;
                foreach (T t in TraverseInOrder(node.Right))
                    yield return t;
            }
        }

        /// <summary>
        /// Makes tree traversal InOrder
        /// </summary>
        public IEnumerable<T> TraversePreOrder(Node<T> node)
        {
            if (node != null)
            {
                yield return node.Data;
                foreach (T t in TraversePreOrder(node.Left))
                    yield return t;
                foreach (T t in TraversePreOrder(node.Right))
                    yield return t;
            }
        }

        /// <summary>
        /// Makes tree traversal InOrder
        /// </summary>
        public IEnumerable<T> TraversePostOrder(Node<T> node)
        {
            if (node != null)
            {
                foreach (T t in TraversePostOrder(node.Left))
                    yield return t;
                foreach (T t in TraversePostOrder(node.Right))
                    yield return t;
                yield return node.Data;
            }
        }
        #endregion

        #region GetEnumerator
        /// <summary>
        /// Makes in-order traverse
        /// Serves as a default <see cref="TraverseType"/> for tree
        /// </summary>
        /// <returns>Enumerator for iterations in foreach cycle</returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (T t in Traverse(TraverseType.InOrder))
                yield return t;
        }

        /// <summary>
        /// Makes in-order traverse
        /// Serves as a default <see cref="TraverseType"/> for tree
        /// </summary>
        /// <returns>Enumerator for iterations in foreach cycle</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
        #endregion
    }
}