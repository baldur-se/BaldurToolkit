using System;
using System.Collections.Generic;

namespace BaldurToolkit.App.DependencyInjection
{
    public interface IAppModuleCollection : IList<Type>
    {
        /// <summary>
        /// Gets the list of application module instances which will be executed before main application modules.
        /// Main application modules can use services configured by StageZero application modules.
        /// </summary>
        IList<IAppModule> StageZero { get; }
    }
}
