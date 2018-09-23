using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BaldurToolkit.App.FileSystem
{
    public class PathMap : IPathMap
    {
        protected static readonly Regex SubstituteParamRegex = new Regex(@"\{(?<paramName>[\w\d\-\.]+)\}");

        private readonly Dictionary<string, IEnumerable<string>> map = new Dictionary<string, IEnumerable<string>>();

        private string prefix;

        public PathMap(string name = null, string prefix = null, PathMap parentPathMap = null)
        {
            this.Name = name;
            this.Prefix = prefix ?? string.Empty;

            if (parentPathMap != null)
            {
                this.Merge(parentPathMap);
            }
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Prefix
        {
            get => this.prefix;
            set => this.prefix = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, IEnumerable<string>>> Entries => this.map.ToArray();

        /// <inheritdoc />
        public void Set(string pathName, string pathTemplate)
        {
            if (pathName == null)
            {
                throw new ArgumentNullException(nameof(pathName));
            }

            if (pathTemplate == null)
            {
                throw new ArgumentNullException(nameof(pathTemplate));
            }

            this.map[pathName] = new[] { pathTemplate };
        }

        /// <inheritdoc />
        public void Set(string pathName, IEnumerable<string> pathTemplates)
        {
            if (pathName == null)
            {
                throw new ArgumentNullException(nameof(pathName));
            }

            if (pathTemplates == null)
            {
                throw new ArgumentNullException(nameof(pathTemplates));
            }

            var pathTemplatesArray = pathTemplates.ToArray();
            if (pathTemplatesArray.Length == 0)
            {
                throw new ArgumentException("At least one path template required.", nameof(pathTemplates));
            }

            this.map[pathName] = pathTemplatesArray;
        }

        /// <inheritdoc />
        public bool Contains(string pathName)
        {
            return this.map.ContainsKey(pathName);
        }

        /// <inheritdoc />
        public bool Remove(string pathName)
        {
            return this.map.Remove(pathName);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetUncompiled(string pathName)
        {
            if (this.map.TryGetValue(pathName, out var result))
            {
                return result;
            }

            return null;
        }

        /// <inheritdoc />
        public IEnumerable<string> Get(string pathName)
        {
            if (this.map.ContainsKey(pathName))
            {
                foreach (var result in this.Compile("{" + pathName + "}"))
                {
                    yield return this.Prefix + result;
                }
            }
        }

        /// <inheritdoc />
        public string GetFirst(string pathName)
        {
            return this.Get(pathName).FirstOrDefault();
        }

        /// <inheritdoc />
        public void Merge(PathMap mergingPathMap)
        {
            if (mergingPathMap == null)
            {
                throw new ArgumentNullException(nameof(mergingPathMap));
            }

            foreach (var entry in mergingPathMap.Entries)
            {
                if (entry.Value != null && !this.map.ContainsKey(entry.Key))
                {
                    this.map.Add(entry.Key, entry.Value);
                }
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> Compile(string input)
        {
            return this.Substitute(input, new List<string>());
        }

        private IEnumerable<string> Substitute(string input, IList<string> visitedParams)
        {
            return this.Substitute(SubstituteParamRegex.Match(input), input, visitedParams);
        }

        private IEnumerable<string> Substitute(Match match, string input, IList<string> visitedParams)
        {
            if (!match.Success)
            {
                return new[] { input };
            }

            var paramName = match.Groups["paramName"].Value;
            if (visitedParams.Contains(paramName, StringComparer.OrdinalIgnoreCase))
            {
                throw new RecursivePathException($"Recursive path parameter detected '{paramName}' in template '{input}'.");
            }

            match = match.NextMatch();

            if (!this.map.TryGetValue(paramName, out var pathTemplates))
            {
                // Can not find parameter in the path map.
                // Continue to the next parameter
                return this.Substitute(match, input, visitedParams);
            }

            // We have found a param that points to a valid path map entry.
            // Now we need to recursively compile all path map entry's templates.
            // Then, for each template and each recursively compiled template, we need to
            // replace the parameter token with compiled template.
            var result = new List<string>();
            foreach (var pathTemplate in pathTemplates)
            {
                // Recursive replaces for current parameter's templates.
                // We're creating a new list of visited parameters for each template
                // and adding our current parameter that we're parsed to the visited list
                // to avoid recursive infinite recursion calls.
                foreach (var compiledPath in this.Substitute(pathTemplate, new List<string>(visitedParams) { paramName }))
                {
                    // Parse the next parameter.
                    foreach (var entry in this.Substitute(match, input, visitedParams))
                    {
                        result.Add(entry.Replace("{" + paramName + "}", compiledPath));
                    }
                }
            }

            return result;
        }
    }
}
