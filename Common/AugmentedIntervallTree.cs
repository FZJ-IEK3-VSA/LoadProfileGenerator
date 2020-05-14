/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BrookNovak.Collections
{
    /// <summary>
    /// An interval tree that supports duplicate entries.
    /// </summary>
    /// <typeparam name="TInterval">The interval type</typeparam>
    /// <typeparam name="TPoint">The interval's start and end type</typeparam>
    /// <remarks>
    /// This interval tree is implemented as a balanced augmented AVL tree.
    /// Modifications are O(log n) typical case.
    /// Searches are O(log n) typical case.
    /// </remarks>
    [Serializable]
    public class IntervalTree<TInterval, TPoint> : ICollection<TInterval>, ICollection, ISerializable, IXmlSerializable
        where TPoint : IComparable<TPoint>
    {
        private readonly object syncRoot;
        private IntervalNode root;
        private ulong modifications;
        private IIntervalSelector<TInterval, TPoint> intervalSelector;

        /// <summary>
        /// Default ctor required for XML serialization support
        /// </summary>
        private IntervalTree()
        {
            syncRoot = new object();
        }

        public IntervalTree(IEnumerable<TInterval> intervals, IIntervalSelector<TInterval, TPoint> intervalSelector) :
            this(intervalSelector)
        {
            AddRange(intervals);
        }

        public IntervalTree(IIntervalSelector<TInterval, TPoint> intervalSelector)
            : this()
        {
            if (intervalSelector == null)
                throw new ArgumentNullException("intervalSelector");
            this.intervalSelector = intervalSelector;
        }

        /// <summary>
        /// Returns the maximum end point in the entire collection.
        /// </summary>
        public TPoint MaxEndPoint
        {
            get
            {
                if (root == null)
                    throw new InvalidOperationException("Cannot determine max end point for emtpy interval tree");
                return root.MaxEndPoint;
            }
        }

        #region Binary Serialization

        public IntervalTree(SerializationInfo info, StreamingContext context)
            : this()
        {
            // Reset the property value using the GetValue method.
            var intervals = (TInterval[])info.GetValue("intervals", typeof(TInterval[]));
            intervalSelector = (IIntervalSelector<TInterval, TPoint>)info.GetValue("selector", typeof(IIntervalSelector<TInterval, TPoint>));
            AddRange(intervals);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var intervals = new TInterval[Count];
            CopyTo(intervals, 0);
            info.AddValue("intervals", intervals, typeof(TInterval[]));
            info.AddValue("selector", intervalSelector, typeof(IIntervalSelector<TInterval, TPoint>));
        }

        #endregion

        #region IXmlSerializable

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Intervals");
            writer.WriteAttributeString("Count", Count.ToString(CultureInfo.InvariantCulture));
            var itemSerializer = new XmlSerializer(typeof(TInterval));
            foreach (var item in this)
            {
                itemSerializer.Serialize(writer, item);
            }
            writer.WriteEndElement();

            writer.WriteStartElement("Selector");
            var typeName = intervalSelector.GetType().AssemblyQualifiedName ?? intervalSelector.GetType().FullName;
            writer.WriteAttributeString("Type", typeName);
            var selectorSerializer = new XmlSerializer(intervalSelector.GetType());
            selectorSerializer.Serialize(writer, intervalSelector);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement();

            reader.MoveToAttribute("Count");
            int count = int.Parse(reader.Value);
            reader.MoveToElement();

            if (count > 0 && reader.IsEmptyElement)
                throw new FormatException("Missing tree items");
            if (count == 0 && !reader.IsEmptyElement)
                throw new FormatException("Unexpected content in tree item Xml (expected empty content)");

            reader.ReadStartElement("Intervals");

            var items = new TInterval[count];

            if (count > 0)
            {
                var itemSerializer = new XmlSerializer(typeof(TInterval));

                for (int i = 0; i < count; i++)
                {
                    items[i] = (TInterval)itemSerializer.Deserialize(reader);

                }
                reader.ReadEndElement(); // </intervals>
            }

            reader.MoveToAttribute("Type");
            string selectorTypeFullName = reader.Value;
            if (string.IsNullOrEmpty(selectorTypeFullName))
                throw new FormatException("Selector type name missing");
            reader.MoveToElement();

            reader.ReadStartElement("Selector");

            var selectorType = Type.GetType(selectorTypeFullName);
            if (selectorType == null)
                throw new XmlException(string.Format("Selector type {0} missing from loaded assemblies", selectorTypeFullName));
            var selectorSerializer = new XmlSerializer(selectorType);
            intervalSelector = (IIntervalSelector<TInterval, TPoint>)selectorSerializer.Deserialize(reader);

            reader.ReadEndElement(); // </selector>

            AddRange(items);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        #endregion

        #region IEnumerable, IEnumerable<T>

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new IntervalTreeEnumerator(this);
        }

        public IEnumerator<TInterval> GetEnumerator()
        {
            return new IntervalTreeEnumerator(this);
        }

        #endregion

        #region ICollection

        public bool IsSynchronized { get { return false; } }

        public Object SyncRoot { get { return syncRoot; } }

        public void CopyTo(
            Array array,
            int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            PerformCopy(arrayIndex, array.Length, (i, v) => array.SetValue(v, i));
        }

        #endregion

        #region ICollection<T>

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void CopyTo(
            TInterval[] array,
            int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            PerformCopy(arrayIndex, array.Length, (i, v) => array[i] = v);
        }

        /// <summary>
        /// Tests if an item is contained in the tree.
        /// </summary>
        /// <param name="item">The item to check</param>
        /// <returns>
        /// True iff the item exists in the collection. 
        /// </returns>
        /// <remarks>
        /// This method uses the collection’s objects’ Equals and CompareTo methods on item to determine whether item exists.
        /// </remarks>
        public bool Contains(TInterval item)
        {
            if (ReferenceEquals(item, null))
                throw new ArgumentNullException("item");

            return FindMatchingNodes(item).Any();
        }

        public void Clear()
        {
            SetRoot(null);
            Count = 0;
            modifications++;
        }

        public void Add(TInterval item)
        {
            if (ReferenceEquals(item, null))
                throw new ArgumentNullException("item");

            var newNode = new IntervalNode(item, Start(item), End(item));

            if (root == null)
            {
                SetRoot(newNode);
                Count = 1;
                modifications++;
                return;
            }

            IntervalNode node = root;
            while (true)
            {
                var startCmp = newNode.Start.CompareTo(node.Start);
                if (startCmp <= 0)
                {
                    if (startCmp == 0 && ReferenceEquals(node.Data, newNode.Data))
                        throw new InvalidOperationException("Cannot add the same item twice (object reference already exists in db)");

                    if (node.Left == null)
                    {
                        node.Left = newNode;
                        break;
                    }
                    node = node.Left;
                }
                else
                {
                    if (node.Right == null)
                    {
                        node.Right = newNode;
                        break;
                    }
                    node = node.Right;
                }
            }

            modifications++;
            Count++;

            // Restructure tree to be balanced
            node = newNode;
            while (node != null)
            {
                node.UpdateHeight();
                node.UpdateMaxEndPoint();
                Rebalance(node);
                node = node.Parent;
            }
        }

        /// <summary>
        /// Removes an item.
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>True if an item was removed</returns>
        /// <remarks>
        /// This method uses the collection’s objects’ Equals and CompareTo methods on item to retrieve the existing item.
        /// If there are duplicates of the item, then object reference is used to remove.
        /// If <see cref="TInterval"/> is not a reference type, then the first found equal interval will be removed.
        /// </remarks>
        public bool Remove(TInterval item)
        {
            if (ReferenceEquals(item, null))
                throw new ArgumentNullException("item");

            if (root == null)
                return false;

            var candidates = FindMatchingNodes(item).ToList();

            if (candidates.Count == 0)
                return false;

            IntervalNode toBeRemoved;
            if (candidates.Count == 1)
            {
                toBeRemoved = candidates[0];
            }
            else
            {
                toBeRemoved = candidates.SingleOrDefault(x => ReferenceEquals(x.Data, item)) ?? candidates[0];
            }

            var parent = toBeRemoved.Parent;
            var isLeftChild = toBeRemoved.IsLeftChild;

            if (toBeRemoved.Left == null && toBeRemoved.Right == null)
            {
                if (parent != null)
                {
                    if (isLeftChild)
                        parent.Left = null;
                    else
                        parent.Right = null;

                    Rebalance(parent);
                }
                else
                {
                    SetRoot(null);
                }
            }
            else if (toBeRemoved.Right == null)
            {
                if (parent != null)
                {
                    if (isLeftChild)
                        parent.Left = toBeRemoved.Left;
                    else
                        parent.Right = toBeRemoved.Left;

                    Rebalance(parent);
                }
                else
                {
                    SetRoot(toBeRemoved.Left);
                }
            }
            else if (toBeRemoved.Left == null)
            {
                if (parent != null)
                {
                    if (isLeftChild)
                        parent.Left = toBeRemoved.Right;
                    else
                        parent.Right = toBeRemoved.Right;

                    Rebalance(parent);
                }
                else
                {
                    SetRoot(toBeRemoved.Right);
                }
            }
            else
            {
                IntervalNode replacement, replacementParent, temp;

                if (toBeRemoved.Balance > 0)
                {
                    if (toBeRemoved.Left.Right == null)
                    {
                        replacement = toBeRemoved.Left;
                        replacement.Right = toBeRemoved.Right;
                        temp = replacement;
                    }
                    else
                    {
                        replacement = toBeRemoved.Left.Right;
                        while (replacement.Right != null)
                        {
                            replacement = replacement.Right;
                        }
                        replacementParent = replacement.Parent;
                        replacementParent.Right = replacement.Left;

                        temp = replacementParent;

                        replacement.Left = toBeRemoved.Left;
                        replacement.Right = toBeRemoved.Right;
                    }
                }
                else
                {
                    if (toBeRemoved.Right.Left == null)
                    {
                        replacement = toBeRemoved.Right;
                        replacement.Left = toBeRemoved.Left;
                        temp = replacement;
                    }
                    else
                    {
                        replacement = toBeRemoved.Right.Left;
                        while (replacement.Left != null)
                        {
                            replacement = replacement.Left;
                        }
                        replacementParent = replacement.Parent;
                        replacementParent.Left = replacement.Right;

                        temp = replacementParent;

                        replacement.Left = toBeRemoved.Left;
                        replacement.Right = toBeRemoved.Right;
                    }
                }

                if (parent != null)
                {
                    if (isLeftChild)
                        parent.Left = replacement;
                    else
                        parent.Right = replacement;
                }
                else
                {
                    SetRoot(replacement);
                }

                Rebalance(temp);
            }

            toBeRemoved.Parent = null;
            Count--;
            modifications++;
            return true;
        }

        #endregion

        #region Public methods

        public void AddRange(IEnumerable<TInterval> intervals)
        {
            if (intervals == null)
                throw new ArgumentNullException("intervals");
            foreach (var interval in intervals)
            {
                Add(interval);
            }
        }

        public TInterval[] this[TPoint point]
        {
            get
            {
                return FindAt(point);
            }
        }

        public TInterval[] FindAt(TPoint point)
        {
            if (ReferenceEquals(point, null))
                throw new ArgumentNullException("point");

            var found = new List<IntervalNode>();
            PerformStabbingQuery(root, point, found);
            return found.Select(node => node.Data).ToArray();
        }

        public bool ContainsPoint(TPoint point)
        {
            return FindAt(point).Any();
        }

        public bool ContainsOverlappingInterval(TInterval item)
        {
            if (ReferenceEquals(item, null))
                throw new ArgumentNullException("item");

            return PerformStabbingQuery(root, item).Count > 0;
        }

        public TInterval[] FindOverlapping(TInterval item)
        {
            if (ReferenceEquals(item, null))
                throw new ArgumentNullException("item");

            return PerformStabbingQuery(root, item).Select(node => node.Data).ToArray();
        }

        #endregion

        #region Private methods

        private void PerformCopy(int arrayIndex, int arrayLength, Action<int, TInterval> setAtIndexDelegate)
        {
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex");
            int i = arrayIndex;
            IEnumerator<TInterval> enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (i >= arrayLength)
                    throw new ArgumentOutOfRangeException("arrayIndex", "Not enough elements in array to copy content into");
                setAtIndexDelegate(i, enumerator.Current);
                i++;
            }
        }

        private IEnumerable<IntervalNode> FindMatchingNodes(TInterval interval)
        {
            return PerformStabbingQuery(root, interval).Where(node => node.Data.Equals(interval));
        }

        private void SetRoot(IntervalNode node)
        {
            root = node;
            if (root != null)
                root.Parent = null;
        }

        private TPoint Start(TInterval interval)
        {
            return intervalSelector.GetStart(interval);
        }

        private TPoint End(TInterval interval)
        {
            return intervalSelector.GetEnd(interval);
        }

        private bool DoesIntervalContain(TInterval interval, TPoint point)
        {
            return point.CompareTo(Start(interval)) >= 0
                && point.CompareTo(End(interval)) <= 0;
        }

        private bool DoIntervalsOverlap(TInterval interval, TInterval other)
        {
            return Start(interval).CompareTo(End(other)) <= 0 &&
                End(interval).CompareTo(Start(other)) >= 0;
        }

        private void PerformStabbingQuery(IntervalNode node, TPoint point, List<IntervalNode> result)
        {
            if (node == null)
                return;

            if (point.CompareTo(node.MaxEndPoint) > 0)
                return;

            if (node.Left != null)
                PerformStabbingQuery(node.Left, point, result);

            if (DoesIntervalContain(node.Data, point))
                result.Add(node);

            if (point.CompareTo(node.Start) < 0)
                return;

            if (node.Right != null)
                PerformStabbingQuery(node.Right, point, result);
        }

        private List<IntervalNode> PerformStabbingQuery(IntervalNode node, TInterval interval)
        {
            var result = new List<IntervalNode>();
            PerformStabbingQuery(node, interval, result);
            return result;
        }

        private void PerformStabbingQuery(IntervalNode node, TInterval interval, List<IntervalNode> result)
        {
            if (node == null)
                return;

            if (Start(interval).CompareTo(node.MaxEndPoint) > 0)
                return;

            if (node.Left != null)
                PerformStabbingQuery(node.Left, interval, result);

            if (DoIntervalsOverlap(node.Data, interval))
                result.Add(node);

            if (End(interval).CompareTo(node.Start) < 0)
                return;

            if (node.Right != null)
                PerformStabbingQuery(node.Right, interval, result);
        }

        private void Rebalance(IntervalNode node)
        {
            if (node.Balance > 1)
            {
                if (node.Left.Balance < 0)
                    RotateLeft(node.Left);
                RotateRight(node);
            }
            else if (node.Balance < -1)
            {
                if (node.Right.Balance > 0)
                    RotateRight(node.Right);
                RotateLeft(node);
            }
        }

        private void RotateLeft(IntervalNode node)
        {
            var parent = node.Parent;
            var isNodeLeftChild = node.IsLeftChild;

            // Make node.Right the new root of this sub tree (instead of node)
            var pivotNode = node.Right;
            node.Right = pivotNode.Left;
            pivotNode.Left = node;

            if (parent != null)
            {
                if (isNodeLeftChild)
                    parent.Left = pivotNode;
                else
                    parent.Right = pivotNode;
            }
            else
            {
                SetRoot(pivotNode);
            }
        }

        private void RotateRight(IntervalNode node)
        {
            var parent = node.Parent;
            var isNodeLeftChild = node.IsLeftChild;

            // Make node.Left the new root of this sub tree (instead of node)
            var pivotNode = node.Left;
            node.Left = pivotNode.Right;
            pivotNode.Right = node;

            if (parent != null)
            {
                if (isNodeLeftChild)
                    parent.Left = pivotNode;
                else
                    parent.Right = pivotNode;
            }
            else
            {
                SetRoot(pivotNode);
            }
        }

        #endregion

        #region Inner classes

        [Serializable]
        private class IntervalNode
        {
            private IntervalNode left;
            private IntervalNode right;
            public IntervalNode Parent { get; set; }
            public TPoint Start { get; private set; }
            private TPoint End { get; set; }
            public TInterval Data { get; private set; }
            private int Height { get; set; }
            public TPoint MaxEndPoint { get; private set; }

            public IntervalNode(TInterval data, TPoint start, TPoint end)
            {
                if (start.CompareTo(end) > 0)
                    throw new ArgumentOutOfRangeException("end", "The suplied interval has an invalid range, where start is greater than end");
                Data = data;
                Start = start;
                End = end;
                UpdateMaxEndPoint();
            }

            public IntervalNode Left
            {
                get
                {
                    return left;
                }
                set
                {
                    left = value;
                    if (left != null)
                        left.Parent = this;
                    UpdateHeight();
                    UpdateMaxEndPoint();
                }
            }

            public IntervalNode Right
            {
                get
                {
                    return right;
                }
                set
                {
                    right = value;
                    if (right != null)
                        right.Parent = this;
                    UpdateHeight();
                    UpdateMaxEndPoint();
                }
            }

            public int Balance
            {
                get
                {
                    if (Left != null && Right != null)
                        return Left.Height - Right.Height;
                    if (Left != null)
                        return Left.Height + 1;
                    if (Right != null)
                        return -(Right.Height + 1);
                    return 0;
                }
            }

            public bool IsLeftChild
            {
                get
                {
                    return Parent != null && Parent.Left == this;
                }
            }

            public void UpdateHeight()
            {
                if (Left != null && Right != null)
                    Height = Math.Max(Left.Height, Right.Height) + 1;
                else if (Left != null)
                    Height = Left.Height + 1;
                else if (Right != null)
                    Height = Right.Height + 1;
                else
                    Height = 0;
            }

            private static TPoint Max(TPoint comp1, TPoint comp2)
            {
                if (comp1.CompareTo(comp2) > 0)
                    return comp1;
                return comp2;
            }

            public void UpdateMaxEndPoint()
            {
                TPoint max = End;
                if (Left != null)
                    max = Max(max, Left.MaxEndPoint);
                if (Right != null)
                    max = Max(max, Right.MaxEndPoint);
                MaxEndPoint = max;
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}], maxEnd={2}", Start, End, MaxEndPoint);
            }
        }

        private class IntervalTreeEnumerator : IEnumerator<TInterval>
        {
            private readonly ulong modificationsAtCreation;
            private readonly IntervalTree<TInterval, TPoint> tree;
            private readonly IntervalNode startNode;
            private IntervalNode current;
            private bool hasVisitedCurrent;
            private bool hasVisitedRight;

            public IntervalTreeEnumerator(IntervalTree<TInterval, TPoint> tree)
            {
                this.tree = tree;
                modificationsAtCreation = tree.modifications;
                startNode = GetLeftMostDescendantOrSelf(tree.root);
                Reset();
            }

            public TInterval Current
            {
                get
                {
                    if (current == null)
                        throw new InvalidOperationException("Enumeration has finished.");

                    if (ReferenceEquals(current, startNode) && !hasVisitedCurrent)
                        throw new InvalidOperationException("Enumeration has not started.");

                    return current.Data;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Reset()
            {
                if (modificationsAtCreation != tree.modifications)
                    throw new InvalidOperationException("Collection was modified.");
                current = startNode;
                hasVisitedCurrent = false;
                hasVisitedRight = false;
            }

            public bool MoveNext()
            {
                if (modificationsAtCreation != tree.modifications)
                    throw new InvalidOperationException("Collection was modified.");

                if (tree.root == null)
                    return false;

                // Visit this node
                if (!hasVisitedCurrent)
                {
                    hasVisitedCurrent = true;
                    return true;
                }

                // Go right, visit the right's left most descendant (or the right node itself)
                if (!hasVisitedRight && current.Right != null)
                {
                    current = current.Right;
                    MoveToLeftMostDescendant();
                    hasVisitedCurrent = true;
                    hasVisitedRight = false;
                    return true;
                }

                // Move upward
                do
                {
                    var wasVisitingFromLeft = current.IsLeftChild;
                    current = current.Parent;
                    if (wasVisitingFromLeft)
                    {
                        hasVisitedCurrent = false;
                        hasVisitedRight = false;
                        return MoveNext();
                    }
                } while (current != null);

                return false;
            }

            private void MoveToLeftMostDescendant()
            {
                current = GetLeftMostDescendantOrSelf(current);
            }

            private IntervalNode GetLeftMostDescendantOrSelf(IntervalNode node)
            {
                if (node == null)
                    return null;
                while (node.Left != null)
                {
                    node = node.Left;
                }
                return node;
            }

            public void Dispose()
            {
            }
        }

        #endregion
    }

    /// <summary>
    /// Selects interval start and end points for an object of type <see cref="TInterval"/>.
    /// </summary>
    /// <typeparam name="TInterval">The type containing interval data</typeparam>
    /// <typeparam name="TPoint">The type of the interval start and end points</typeparam>
    /// <remarks>
    /// In order for the collection using these selectors to be XML serializable, your implementations of this interface must also be
    /// XML serializable (e.g. dont use delegates, and provide a default constructor).
    /// </remarks>
    public interface IIntervalSelector<in TInterval, out TPoint> where TPoint : IComparable<TPoint>
    {
        TPoint GetStart(TInterval item);
        TPoint GetEnd(TInterval item);
    }
}

/*
 * 
 * [TestFixture]
public class ExampleTestFixture {
     
    [Fact]
    public void FindAt_Overlapping ()
    {
        // ARRANGE
        var int1 = new TestInterval (20, 60);
        var int2 = new TestInterval (10, 50);
        var int3 = new TestInterval (40, 70);
 
        var intervalColl = new[] { 
            int1, int2, int3
        };
        var tree = new IntervalTree<TestInterval, int>(intervalColl, new TestIntervalSelector());
 
        // ACT
        var res1 = tree.FindAt (0);
        var res2 = tree.FindAt (10);
        var res3 = tree.FindAt (15);
        var res4 = tree.FindAt (20);
        var res5 = tree.FindAt (30);
        var res6 = tree.FindAt (40);
        var res7 = tree.FindAt (45);
        var res8 = tree.FindAt (50);
        var res9 = tree.FindAt (55);
        var res10 = tree.FindAt (60);
        var res11 = tree.FindAt (65);
        var res12 = tree.FindAt (70);
        var res13 = tree.FindAt (75);
 
        // ASSERT
        Assert.That (res1, Is.Empty);
        Assert.That (res2, Is.EquivalentTo(new[] { int2 }));
        Assert.That (res3, Is.EquivalentTo(new[] { int2 }));
        Assert.That (res4, Is.EquivalentTo(new[] { int1, int2 }));
        Assert.That (res5, Is.EquivalentTo(new[] { int1, int2 }));
        Assert.That (res6, Is.EquivalentTo(new[] { int1, int2, int3 }));
        Assert.That (res7, Is.EquivalentTo(new[] { int1, int2, int3 }));
        Assert.That (res8, Is.EquivalentTo(new[] { int1, int2, int3 }));
        Assert.That (res9, Is.EquivalentTo(new[] { int1, int3 }));
        Assert.That (res10, Is.EquivalentTo(new[] { int1, int3 }));
        Assert.That (res11, Is.EquivalentTo(new[] { int3 }));
        Assert.That (res12, Is.EquivalentTo(new[] { int3 }));
        Assert.That (res13, Is.Empty);
    }
}
 
[Serializable]
public class TestInterval 
{
    private TestInterval() {}
 
    public TestInterval(int low, int hi) 
    {
        if(low > hi)
            throw new ArgumentOutOfRangeException("lo higher the hi");
        Low = low;
        Hi = hi;
    }
 
    public int Low { get; private set; }
    public int Hi { get; private set; }
    public string MutableData { get; set; }
 
    public override string ToString ()
    {
        return string.Format ("[Low={0}, Hi={1}, Data={2}]", Low, Hi, MutableData);
    }
}
 
[Serializable]
public class TestIntervalSelector : IIntervalSelector<TestInterval, int>
{
    public int GetStart (TestInterval item) 
    {
        return item.Low;
    }
 
    public int GetEnd (TestInterval item) 
    {
        return item.Hi;
    }
}
*/