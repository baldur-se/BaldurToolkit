using System;

namespace BaldurToolkit.App.DependencyInjection
{
    public class StartupServiceCollection : TypeValidationList<IAppStartupService>, IStartupServiceCollection
    {
    }
}
