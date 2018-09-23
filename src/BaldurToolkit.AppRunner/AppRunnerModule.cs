using System;
using BaldurToolkit.App;
using BaldurToolkit.App.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaldurToolkit.AppRunner
{
    public class AppRunnerModule : IAppModule
    {
        private readonly IConfigurationRoot appConriguration;
        private readonly AppEnvironment appEnvironment;

        public AppRunnerModule(
            IConfigurationRoot appConfiguration,
            AppEnvironment appEnvironment)
        {
            this.appConriguration = appConfiguration;
            this.appEnvironment = appEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton(provider => this.appConriguration)
                .AddSingleton<IConfiguration>(provider => this.appConriguration)
                .AddSingleton(provider => this.appEnvironment)
            ;
        }
    }
}
