using System;
using System.Collections.Generic;

namespace BaldurToolkit.App.FileSystem
{
    public interface IPathMap
    {
        /// <summary>
        /// Gets the name of the path map.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the preffix value that will be prepended to all compiled paths.
        /// </summary>
        string Prefix { get; set; }

        /// <summary>
        /// Gets list of all entries that was added to the current path map.
        /// </summary>
        IEnumerable<KeyValuePair<string, IEnumerable<string>>> Entries { get; }

        /// <summary>
        /// Adds or overwrites a path entry to the path map.
        /// </summary>
        /// <param name="pathName">The name of the entry.</param>
        /// <param name="pathTemplate">The value for the path template.</param>
        void Set(string pathName, string pathTemplate);

        /// <summary>
        /// Adds or overwrites a path entry with multiple values to the path map.
        /// </summary>
        /// <param name="pathName">The name of the entry.</param>
        /// <param name="pathTemplates">The values for the path template.</param>
        void Set(string pathName, IEnumerable<string> pathTemplates);

        /// <summary>
        /// Checks if the entry with given path name is present in the current path map.
        /// </summary>
        /// <param name="pathName">The name of the entry.</param>
        /// <returns>True if the path entry with the given name exists.</returns>
        bool Contains(string pathName);

        /// <summary>
        /// Removes an entry from the path map.
        /// </summary>
        /// <param name="pathName">The name of the entry.</param>
        /// <returns>True if the removal was successful.</returns>
        bool Remove(string pathName);

        /// <summary>
        /// Gets uncompiled path templates associated with the given name.
        /// </summary>
        /// <param name="pathName">The name of the entry.</param>
        /// <returns>A list of path templates or null if not found.</returns>
        IEnumerable<string> GetUncompiled(string pathName);

        /// <summary>
        /// Gets compiled paths associated with given path name.
        /// </summary>
        /// <remarks>
        /// This method recursively replaces all parameters in the path template.
        /// This method can produce huge amount of paths if template uses many parameters with multiple templates associated.
        /// </remarks>
        /// <param name="pathName">The name of the entry.</param>
        /// <returns>A list of compiled paths or an empty collection if path was not found.</returns>
        /// <exception cref="RecursivePathException">When recursive parameters detected in the path template.</exception>
        IEnumerable<string> Get(string pathName);

        /// <summary>
        /// Gets the first compiled path for given path name.
        /// </summary>
        /// <remarks>
        /// Shortcut for 'IPathMap.Get(pathName).FirstOrDefault()'.
        /// </remarks>
        /// <param name="pathName">The name of the entry.</param>
        /// <returns>First found compiled path associated with given path name or null if not found.</returns>
        string GetFirst(string pathName);

        /// <summary>
        /// Recursively replaces all known parameters in the given input string.
        /// </summary>
        /// <remarks>
        /// This method can produce huge amount of paths if input string uses many parameters with multiple templates associated.
        /// </remarks>
        /// <param name="input">The input value.</param>
        /// <returns>The resulting value.</returns>
        /// <exception cref="RecursivePathException">When recursive parameters detected in the path template.</exception>
        IEnumerable<string> Compile(string input);

        /// <summary>
        /// Merges marameters from another path map without replacing.
        /// </summary>
        /// <param name="mergingPathMap">The path map to merge in.</param>
        void Merge(PathMap mergingPathMap);
    }
}
