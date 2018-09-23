using System;

namespace BaldurToolkit.DataTables
{
    public interface IDataTableContainer
    {
        void Add(Type tableType, IDataTable dataTable);

        IDataTable Get(Type tableType);

        T Get<T>()
            where T : class, IDataTable;

        bool Remove(Type tableType);
    }
}
