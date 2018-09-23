using System;

namespace BaldurToolkit.DataTables.Providers
{
    public class DataTableLoadingStartedEventArgs : EventArgs
    {
        public DataTableLoadingStartedEventArgs(string tableName)
        {
            this.TableName = tableName;
        }

        /// <summary>
        /// Gets the data table name.
        /// </summary>
        public string TableName { get;  }
    }
}
