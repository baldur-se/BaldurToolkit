using System;
using BaldurToolkit.App;
using Microsoft.Extensions.Configuration;

namespace DemoApp
{
	public class DemoAppTestService : IAppStartupService
	{
		private readonly IApp app;
		private readonly AppEnvironment env;
		private readonly IConfigurationSection config;

		public DemoAppTestService(IApp app, IConfiguration config, AppEnvironment env)
		{
			this.app = app;
			this.env = env;
			this.config = config.GetSection("test");
		}

		public void StartServices(IApp app)
		{
			Console.WriteLine("DemoAppTestService for {0}", this.app.Name);
			Console.WriteLine("Config: {0}", this.config["test4"]);
			Console.WriteLine("WorkingDir: {0}", this.env.PathMap.GetFirst("WorkingDir"));
			Console.WriteLine("AppRootDir: {0}", this.env.PathMap.GetFirst("AppRootDir"));

			for (int i = 1; i <= 3; i++)
			{
				Console.WriteLine(" (starting) {0}", i);
				System.Threading.Thread.Sleep(1000);
			}
			Console.WriteLine("Config: {0}", this.config["test4"]);

			this.app.Stopping += (sender, args) =>
			{
				for (int i = 3; i != 0; i--)
				{
					Console.WriteLine(" (stopping) {0}", i);
					System.Threading.Thread.Sleep(1000);
				}
			};
		}
	}
}