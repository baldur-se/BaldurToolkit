using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BaldurToolkit.App.DependencyInjection
{
    public class AppModuleBundle : IAppModule
    {
        private readonly TypeValidationList<IAppModule> modules = new TypeValidationList<IAppModule>();

        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Create a temporary copy of base services
            var tmpServices = new ServiceCollection();
            foreach (var serviceDescriptor in services)
            {
                tmpServices.Add(serviceDescriptor);
            }

            // Add modules as transient services to the temporary container to be able to instantiate them
            // They will be able to use services, configured using the StageZero modules
            foreach (var moduleType in this.modules)
            {
                tmpServices.AddTransient(moduleType);
            }

            var tmpContainer = tmpServices.BuildServiceProvider();

            // Configure main app service container using modules from the temporary container
            foreach (var moduleType in this.modules)
            {
                var module = (IAppModule)tmpContainer.GetService(moduleType);
                module.ConfigureServices(services);
            }
        }

        public void Add(Type moduleType)
        {
            this.modules.Add(moduleType);
        }

        public void Add<TModuleType>()
            where TModuleType : IAppModule
        {
            this.modules.Add(typeof(TModuleType));
        }

        public void Add(IEnumerable<Type> moduleTypes)
        {
            foreach (var moduleType in moduleTypes)
            {
                this.Add(moduleType);
            }
        }
    }
}
