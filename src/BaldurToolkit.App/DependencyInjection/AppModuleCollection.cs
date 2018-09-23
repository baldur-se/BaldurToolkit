using System;
using System.Collections.Generic;

namespace BaldurToolkit.App.DependencyInjection
{
    public class AppModuleCollection : TypeValidationList<IAppModule>, IAppModuleCollection
    {
        /// <inheritdoc />
        public IList<IAppModule> StageZero { get; } = new List<IAppModule>();
    }
}
