using System;

namespace BaldurToolkit.DataTables
{
    public abstract class DataTable : IDataTable
    {
        protected DataTable(Type elementsType, string tableName, params string[] dependencies)
        {
            this.ElementsType = elementsType;
            this.TableName = tableName;
            this.Dependencies = dependencies;
        }

        public Type ElementsType { get; }

        public virtual Type TableType => this.GetType();

        public string TableName { get; }

        public string[] Dependencies { get; }
    }
}
