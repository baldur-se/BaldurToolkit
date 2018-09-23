using System;
using BaldurToolkit.App;

namespace DemoApp
{
	public class DemoAppDevService : IAppStartupService
	{

		public void StartServices(IApp app)
		{
			Console.WriteLine("DemoAppDevService for {0}", app.Name);
		}
	}
}