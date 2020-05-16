using System;

namespace BaldurToolkit.DataTables
{
    public interface IDataTable
    {
        /// <summary>
        /// Gets the type of the table's elements.
        /// </summary>
        Type ElementsType { get; }

        /// <summary>
        /// Gets the type of the table under which it will be registered in the list.
        /// </summary>
        Type TableType { get; }

        /// <summary>
        /// Gets the name of the table in the data source.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Gets the list of table names this table depends on.
        /// </summary>
        string[] Dependencies { get; }
    }
}
