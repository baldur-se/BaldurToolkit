using System;
using BaldurToolkit.App.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DemoApp
{
	public class DemoAppModule : IAppModule
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<DemoAppTestService>();
		}
	}
}