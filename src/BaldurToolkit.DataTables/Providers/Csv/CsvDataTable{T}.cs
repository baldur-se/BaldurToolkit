using System;
using System.Collections.Generic;
using CsvHelper;

namespace BaldurToolkit.DataTables.Providers.Csv
{
    public abstract class CsvDataTable<T> : CsvDataTable
    {
        protected CsvDataTable(string tableName, params string[] dependencies)
            : base(typeof(T), tableName, dependencies)
        {
        }

        public sealed override void Build(ICsvReader reader, IDataTableContainer container)
        {
            var elements = new List<T>();
            while (reader.Read())
            {
                elements.Add(this.ConstructElement(reader, container));
            }

            this.BuildIndex(elements, container);
        }

        protected abstract T ConstructElement(ICsvReader reader, IDataTableContainer tableContainer);

        protected abstract void BuildIndex(IReadOnlyList<T> elements, IDataTableContainer tableContainer);
    }
}
