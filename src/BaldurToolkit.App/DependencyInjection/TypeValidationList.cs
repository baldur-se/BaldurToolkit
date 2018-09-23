using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BaldurToolkit.App.DependencyInjection
{
    public class TypeValidationList<TRequiredInterfaceType> : IList<Type>
    {
        private readonly List<Type> list = new List<Type>();
        private readonly TypeInfo requiredInterfaceTypeInfo;

        public TypeValidationList()
        {
            this.requiredInterfaceTypeInfo = typeof(TRequiredInterfaceType).GetTypeInfo();
        }

        /// <inheritdoc />
        public int Count => this.list.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public Type this[int index]
        {
            get => this.list[index];
            set
            {
                this.ValidateItem(value);
                this.list[index] = value;
            }
        }

        /// <inheritdoc />
        public IEnumerator<Type> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.list).GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(Type item)
        {
            this.ValidateItem(item);
            this.list.Add(item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            this.list.Clear();
        }

        /// <inheritdoc />
        public bool Contains(Type item)
        {
            return this.list.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(Type[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(Type item)
        {
            return this.list.Remove(item);
        }

        /// <inheritdoc />
        public int IndexOf(Type item)
        {
            return this.list.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, Type item)
        {
            this.ValidateItem(item);
            this.list.Insert(index, item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        private void ValidateItem(Type item)
        {
            if (!this.requiredInterfaceTypeInfo.IsAssignableFrom(item.GetTypeInfo()))
            {
                throw new Exception($"Invalid app module: Type '{item}' not implements an {typeof(TRequiredInterfaceType)} interface.");
            }
        }
    }
}
