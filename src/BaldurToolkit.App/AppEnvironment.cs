using System;
using BaldurToolkit.App.FileSystem;
using Microsoft.Extensions.PlatformAbstractions;

namespace BaldurToolkit.App
{
    public class AppEnvironment
    {
        public AppEnvironment(string environmentName, IPathMap pathMap, PlatformServices platformServices = null)
        {
            this.EnvironmentName = environmentName;
            this.PathMap = pathMap;
            this.PlatformServices = platformServices;
        }

        /// <summary>
        /// Gets the name of the current environment.
        /// </summary>
        public string EnvironmentName { get; }

        /// <summary>
        /// Gets the file system PathMap for the current environment.
        /// </summary>
        public IPathMap PathMap { get; }

        /// <summary>
        /// Gets runtime information (if any) for the current environment.
        /// </summary>
        public PlatformServices PlatformServices { get; }
    }
}
