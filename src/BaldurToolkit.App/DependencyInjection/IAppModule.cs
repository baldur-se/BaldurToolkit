using System;
using Microsoft.Extensions.DependencyInjection;

namespace BaldurToolkit.App.DependencyInjection
{
    public interface IAppModule
    {
        void ConfigureServices(IServiceCollection services);
    }
}
