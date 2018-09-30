using System;

namespace BaldurToolkit.App
{
    public class AppIdentity
    {
        /// <summary>
        /// Default application name.
        /// </summary>
        public const string DefaultName = "App";

        /// <summary>
        /// Default application kind.
        /// </summary>
        public const string DefaultKind = "default";

        public AppIdentity(string name = DefaultName, string kind = DefaultKind)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Kind = kind ?? throw new ArgumentNullException(nameof(kind));
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the kind of the application.
        /// </summary>
        public string Kind { get; set; }
    }
}
