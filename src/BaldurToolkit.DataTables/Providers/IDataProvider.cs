using System;
using System.Collections.Generic;

namespace BaldurToolkit.DataTables.Providers
{
    public interface IDataProvider
    {
        /// <summary>
        /// Occurs when a data table loading started.
        /// </summary>
        event EventHandler<DataTableLoadingStartedEventArgs> DataTableLoadingStarted;

        /// <summary>
        /// Occurs when a data table loaded.
        /// </summary>
        event EventHandler<DataTableLoadedEventArgs> DataTableLoaded;

        /// <summary>
        /// Builds all registered data tables and adds them to the specified container.
        /// </summary>
        /// <param name="tablesToBuild">List of tables.</param>
        /// <param name="containerToLoad">The container to load tables into.</param>
        void Build(IEnumerable<IDataTable> tablesToBuild, IDataTableContainer containerToLoad);
    }
}
