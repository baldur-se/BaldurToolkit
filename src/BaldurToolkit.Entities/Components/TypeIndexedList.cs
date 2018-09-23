using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BaldurToolkit.Entities.Components
{
    /// <summary>
    /// A dictionary of elements by tipe with specific order or enumeration.
    /// </summary>
    /// <typeparam name="T">Base type of elements in the list.</typeparam>
    public class TypeIndexedList<T> : IList<T>
        where T : class
    {
        private readonly List<T> items = new List<T>();

        private readonly Dictionary<Type, T> itemTypeIndex = new Dictionary<Type, T>();

        public int Count => this.items.Count;

        bool ICollection<T>.IsReadOnly => ((IList<T>)this.items).IsReadOnly;

        public T this[int index]
        {
            get => this.items[index];
            set => throw new InvalidOperationException("Can not set a component. Please remove the old element first.");
        }

        public TIndex Get<TIndex>()
            where TIndex : T
        {
            if (!this.TryGet(out TIndex value))
            {
                throw new KeyNotFoundException($"Value of type '{typeof(TIndex)}' was not found in the collection.");
            }

            return value;
        }

        public T Get(Type type)
        {
            if (!this.TryGet(type, out var value))
            {
                throw new KeyNotFoundException($"Value of type '{type}' was not found in the collection.");
            }

            return value;
        }

        public bool TryGet<TIndex>(out TIndex value)
            where TIndex : T
        {
            if (this.TryGet(typeof(TIndex), out var tmp))
            {
                value = (TIndex)tmp;
                return true;
            }

            value = default(TIndex);
            return false;
        }

        public bool TryGet(Type key, out T value)
        {
            if (this.itemTypeIndex.TryGetValue(key, out var tmp))
            {
                value = tmp;
                return true;
            }

            value = default(T);
            return false;
        }

        public void Add(Type type, T item)
        {
            if (this.itemTypeIndex.ContainsKey(type))
            {
                throw new ArgumentException($"An item with type '{type}' is already exists in the collection.");
            }

            this.itemTypeIndex.Add(type, item);
            this.items.Add(item);
        }

        public void Add<TIndex>(TIndex item)
            where TIndex : T
        {
            this.Add(typeof(TIndex), item);
        }

        public void Add(T item)
        {
            this.Add<T>(item);
        }

        public void Insert(int index, Type type, T item)
        {
            if (this.itemTypeIndex.ContainsKey(type))
            {
                throw new ArgumentException($"An item with type '{type}' is already exists in the collection.");
            }

            this.itemTypeIndex.Add(type, item);
            this.items.Insert(index, item);
        }

        public void Insert<TIndex>(int index, TIndex item)
            where TIndex : T
        {
            var type = typeof(TIndex);
            this.Insert(index, type, item);
        }

        public void Insert(int index, T item)
        {
            this.Insert<T>(index, item);
        }

        public void Clear()
        {
            this.itemTypeIndex.Clear();
            this.items.Clear();
        }

        public bool Remove(Type type)
        {
            if (this.itemTypeIndex.TryGetValue(type, out var item))
            {
                this.itemTypeIndex.Remove(type);
                this.items.Remove(item);
                return true;
            }

            return false;
        }

        public bool Remove(T item)
        {
            var success = this.items.Remove(item);
            if (success)
            {
                var key = this.itemTypeIndex.First(i => i.Value == item).Key;
                this.itemTypeIndex.Remove(key);
            }

            return success;
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= this.items.Count)
            {
                throw new IndexOutOfRangeException();
            }

            var item = this.items[index];

            this.Remove(item);
        }

        public bool Contains(Type type)
        {
            return this.itemTypeIndex.ContainsKey(type);
        }

        public bool Contains(T item)
        {
            return this.items.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)this.items).GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return this.items.IndexOf(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<T>)this.items).GetEnumerator();
        }
    }
}
