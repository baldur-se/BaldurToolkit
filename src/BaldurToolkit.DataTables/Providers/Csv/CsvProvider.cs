using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;

namespace BaldurToolkit.DataTables.Providers.Csv
{
    public class CsvProvider : IDataProvider
    {
        public CsvProvider(IEnumerable<string> dataDirectories)
        {
            this.DataDirectories = dataDirectories;
            this.FileExtension = ".csv";
        }

        public event EventHandler<DataTableLoadingStartedEventArgs> DataTableLoadingStarted;

        public event EventHandler<DataTableLoadedEventArgs> DataTableLoaded;

        public IEnumerable<string> DataDirectories { get; set; }

        public string FileExtension { get; set; }

        public void Build(IEnumerable<IDataTable> tablesToBuild, IDataTableContainer containerToLoad)
        {
            var csvConfiguration = new CsvConfiguration()
            {
                CultureInfo = CultureInfo.InvariantCulture,
                HasHeaderRecord = true,
            };

            var tablesByName = tablesToBuild.ToDictionary(table => table.TableName, table => table);

            // Organize by dependencies
            var dependencies = tablesByName.Values.ToDictionary(metadata => metadata.TableName, metadata => metadata.Dependencies);
            foreach (var tableName in this.GetTableNamesByDependencies(dependencies))
            {
                var table = tablesByName[tableName];
                {
                    this.DataTableLoadingStarted?.Invoke(this, new DataTableLoadingStartedEventArgs(table.TableName));
                }

                try
                {
                    if (!(table is CsvDataTable csvTable))
                    {
                        throw new Exception("Invalid table type.");
                    }

                    var filePath = this.FindDataTableFile(table.TableName);
                    using (var csv = new CsvReader(File.OpenText(filePath), csvConfiguration))
                    {
                        csvTable.Build(csv, containerToLoad);
                    }

                    containerToLoad.Add(table.TableType, table);
                    {
                        this.DataTableLoaded?.Invoke(this, new DataTableLoadedEventArgs(table.TableName, filePath));
                    }
                }
                catch (Exception exception)
                {
                    throw new Exception($"Can not load data table \"{table.TableName}\".", exception);
                }
            }
        }

        private IEnumerable<string> GetTableNamesByDependencies(Dictionary<string, string[]> dependencies)
        {
            var sorted = new List<string>();
            var visitedNames = new HashSet<string>();

            foreach (var pair in dependencies)
            {
                this.Visit(pair.Key, visitedNames, sorted, dependencies);
            }

            return sorted;
        }

        private void Visit(string tableName, HashSet<string> visitedNames, List<string> sorted, Dictionary<string, string[]> dependencies)
        {
            if (sorted.Contains(tableName))
            {
                return; // Already added
            }

            if (visitedNames.Contains(tableName))
            {
                throw new Exception($"Cyclic dependency found for table: {tableName}");
            }

            visitedNames.Add(tableName);

            if (dependencies[tableName] != null)
            {
                foreach (var dependency in dependencies[tableName])
                {
                    if (!dependencies.ContainsKey(dependency))
                    {
                        throw new Exception($"Can not find dependency table: {dependency}");
                    }

                    this.Visit(dependency, visitedNames, sorted, dependencies);
                }
            }

            visitedNames.Remove(tableName);
            sorted.Add(tableName);
        }

        private string FindDataTableFile(string tableName)
        {
            if (this.DataDirectories == null)
            {
                throw new FileNotFoundException($"Can not find data table source file: {tableName}. No file directories configured.");
            }

            foreach (var directory in this.DataDirectories)
            {
                var filePath = Path.Combine(directory, tableName + this.FileExtension);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }

            throw new FileNotFoundException($"Can not find data table source file: {tableName}");
        }
    }
}
