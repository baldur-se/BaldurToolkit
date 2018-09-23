using System;
using BaldurToolkit.App.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace BaldurToolkit.App
{
    public class GenericApp : EmptyApp, IServiceProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApp"/> class.
        /// Basic constructor for with DI container configuration support.
        /// </summary>
        /// <param name="appIdentity">App identity.</param>
        /// <param name="moduleCollection">App modules.</param>
        /// <param name="startupServiceCollection">Initial app services.</param>
        public GenericApp(
            AppIdentity appIdentity,
            IAppModuleCollection moduleCollection = null,
            IStartupServiceCollection startupServiceCollection = null)
            : base(appIdentity)
        {
            if (moduleCollection != null)
            {
                this.Modules = moduleCollection;
            }

            if (startupServiceCollection != null)
            {
                this.StartupServices = startupServiceCollection;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericApp"/> class.
        /// Basic constructor for inherited classes.
        /// </summary>
        /// <param name="name">The name of the app.</param>
        /// <param name="instanceName">The app instance name that distinguishes multiple apps with the same name.</param>
        protected GenericApp(string name, string instanceName)
            : base(name, instanceName)
        {
        }

        public IServiceProvider Container { get; protected set; }

        public IAppModuleCollection Modules { get; } = new AppModuleCollection();

        public IStartupServiceCollection StartupServices { get; } = new StartupServiceCollection();

        /// <inheritdoc />
        public virtual object GetService(Type serviceType)
        {
            var container = this.Container;
            if (container == null)
            {
                throw new InvalidOperationException("Service container is not built yet.");
            }

            return container.GetService(serviceType);
        }

        /// <summary>
        /// Creates base service collection containing main app's services.
        /// </summary>
        /// <remarks>
        /// By default, creates a new <see cref="IServiceCollection"/>,
        /// adds basic app services (such as <see cref="IApp"/>, <see cref="IServiceProvider"/>, <see cref="Microsoft.Extensions.Configuration.IConfigurationRoot"/>, etc.)
        /// and executes all registered <see cref="IAppModuleCollection.StageZero"/> modules.
        /// </remarks>
        /// <returns>Base service collection.</returns>
        protected virtual IServiceCollection CreateBaseServiceCollection()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IApp>(provider => this);
            services.AddSingleton<IServiceProvider>(provider => this);
            services.AddSingleton(provider => this.Modules);
            services.AddSingleton(provider => this.StartupServices);

            foreach (var module in this.Modules.StageZero)
            {
                module.ConfigureServices(services);
            }

            return services;
        }

        /// <summary>
        /// Populates service collection with required services.
        /// </summary>
        /// <remarks>
        /// By default, creates a temporary service provider with previously configured services in specified service collection,
        /// initializes registered module types using temporary service provider
        /// and executes all registered modules to configure all other services.
        /// </remarks>
        /// <param name="services">Service collection to be populated.</param>
        protected virtual void ConfigureServices(IServiceCollection services)
        {
            var rootBundle = new AppModuleBundle();
            rootBundle.Add(this.Modules);
            rootBundle.ConfigureServices(services);
        }

        /// <summary>
        /// Starts app's services.
        /// </summary>
        /// <remarks>
        /// By default, starts all registered <see cref="StartupServices"/> one by one.
        /// </remarks>
        protected virtual void StartServices()
        {
            foreach (var startupServiceType in this.StartupServices)
            {
                var startupService = (IAppStartupService)this.Container.GetRequiredService(startupServiceType);
                startupService.StartServices(this);
            }
        }

        /// <inheritdoc />
        protected override void OnStarting()
        {
            base.OnStarting();

            var services = this.CreateBaseServiceCollection();
            this.ConfigureServices(services);
            this.Container = services.BuildServiceProvider();

            this.StartServices();
        }
    }
}
