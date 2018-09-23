using System;

namespace BaldurToolkit.App.DependencyInjection
{
    public interface IServiceProviderAware
    {
        void SetServiceProvider(IServiceProvider serviceProvider);
    }
}
