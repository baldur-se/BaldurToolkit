using System;
using System.Collections.Generic;

namespace BaldurToolkit.DataTables
{
    public class DataTableContainer : IDataTableContainer
    {
        private static readonly EmbeddedTypeAwareTypeComparer Comparer = new EmbeddedTypeAwareTypeComparer();

        private readonly Dictionary<Type, IDataTable> dataTables = new Dictionary<Type, IDataTable>(Comparer);

        public void Add(Type tableType, IDataTable dataTable)
        {
            this.dataTables.Add(tableType, dataTable);
        }

        public IDataTable Get(Type tableType)
        {
            if (!this.dataTables.TryGetValue(tableType, out var dataTable))
            {
                return null;
            }

            return dataTable;
        }

        public T Get<T>()
            where T : class, IDataTable
        {
            return this.Get(typeof(T)) as T;
        }

        public bool Remove(Type tableType)
        {
            return this.dataTables.Remove(tableType);
        }

        private sealed class EmbeddedTypeAwareTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x == y;
            }

            public int GetHashCode(Type obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }
}
