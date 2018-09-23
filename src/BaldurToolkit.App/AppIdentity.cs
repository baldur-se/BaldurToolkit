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
        /// Default application instance name.
        /// </summary>
        public const string DefaultInstanceName = "default";

        public AppIdentity(string name = DefaultName, string instanceName = DefaultInstanceName)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.InstanceName = instanceName ?? throw new ArgumentNullException(nameof(instanceName));
        }

        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the instance name of the application.
        /// </summary>
        public string InstanceName { get; set; }
    }
}
