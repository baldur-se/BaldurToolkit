using System;

namespace BaldurToolkit.DataTables.Providers
{
    public class DataTableLoadedEventArgs : EventArgs
    {
        public DataTableLoadedEventArgs(string tableName, string path)
        {
            this.TableName = tableName;
            this.Path = path;
        }

        /// <summary>
        /// Gets the data table name.
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Gets file path or connection string.
        /// </summary>
        public string Path { get; }
    }
}
