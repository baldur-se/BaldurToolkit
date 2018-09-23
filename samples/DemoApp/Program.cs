using System;
using BaldurToolkit.AppRunner;

namespace DemoApp
{
	internal static class Program
	{
		static int Main(string[] args)
		{
			var cli = new RunAppCommandLine("{WorkingDir}/DemoApp.app.xml");
			cli.Name = "DemoApp";
			cli.FullName = "BaldurToolkit Demo App";

			return cli.Execute(args);
		}
	}
}