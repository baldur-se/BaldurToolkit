using System;
using CsvHelper;

namespace BaldurToolkit.DataTables.Providers.Csv
{
    public abstract class CsvDataTable : DataTable
    {
        protected CsvDataTable(Type elementsType, string tableName, params string[] dependencies)
            : base(elementsType, tableName, dependencies)
        {
        }

        public abstract void Build(ICsvReader reader, IDataTableContainer container);
    }
}
