using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Godot;

namespace SpartansLib.Structure
{
    public interface IUpdatableNode { void UpdateNode(); }

    public class ListWrapper<T, ListT, NodeT> :
        IList<T>,
        IReadOnlyList<T>,
        ICloneable
        where ListT : class, IList<T>, new()
        where NodeT : Node
    {
        private readonly NodeT node;
        private ListT list;

        public bool ShouldUpdate { get; set; } = true;

        public ListWrapper(NodeT node, ListT wrap, bool update = true)
        {
            this.node = node ?? throw new ArgumentNullException(nameof(node));
            list = wrap ?? throw new ArgumentNullException(nameof(wrap));
            TryUpdate(update);
        }

        public void Set(ListT wrap, bool update = true)
        {
            list = wrap ?? throw new ArgumentNullException(nameof(wrap));
            TryUpdate(update);
        }

        public void TryUpdate(bool forceUpdate = false)
        {
            if (ShouldUpdate || forceUpdate)
            {
                if (node is IUpdatableNode un) un.UpdateNode();
                if (node is CanvasItem ci) ci.Update();
            }
        }

        public T this[int index]
        {
            get => list[index];
            set
            {
                list[index] = value;
                TryUpdate();
            }
        }

        public int Count => list.Count;
        public bool IsReadOnly => list.IsReadOnly;

        public void Add(T item)
        {
            list.Add(item);
            TryUpdate();
        }

        public void Clear()
        {
            list.Clear();
            TryUpdate();
        }

        public ListT CloneList()
        {
            if (list is ICloneable clone) return (ListT)clone.Clone();

            if (typeof(ListT) == typeof(List<T>) || typeof(ListT).IsSubclassOf(typeof(List<T>)))
                return (ListT)(object)new List<T>(list);
            if (typeof(ListT) == typeof(T[]))
            {
                var arr = new T[Count];
                CopyTo(arr, 0);
                return (ListT)(object)arr;
            }
            if (typeof(ListT) == typeof(Collection<T>) || typeof(ListT).IsSubclassOf(typeof(Collection<T>)))
                return (ListT)(object)new Collection<T>(list);
            if (typeof(ListT) == typeof(ReadOnlyCollection<T>))
                return (ListT)(object)new ReadOnlyCollection<T>(list);

            var l = new ListT();
            foreach (var v in list)
                l.Add(v);
            return l;
        }

        public object Clone() => CloneList();

        public bool Contains(T item) => list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

        public int IndexOf(T item) => list.IndexOf(item);
        public void Insert(int index, T item) 
        {
            list.Insert(index, item);
            TryUpdate();
        }

        public bool Remove(T item)
        {
            var result = list.Remove(item);
            TryUpdate();
            return result;
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
            TryUpdate();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
