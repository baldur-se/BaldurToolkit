using System;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.PlatformAbstractions;

namespace BaldurToolkit.AppRunner
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var cli = new CommandLineApplication();
            cli.Name = "BaldurToolkit.AppRunner";
            cli.FullName = "BaldurToolkit Application Runner";

            cli.HelpOption("-?|-h|--help");
            cli.VersionOption("-V|--version", PlatformServices.Default.Application.ApplicationVersion);

            cli.Commands.Add(new RunAppCommandLine(null, true) { Name = "run", Parent = cli });

            cli.OnExecute(() =>
            {
                Console.WriteLine("Please specify which command you want to execute.");
                cli.ShowHint();
                return 1;
            });

            return cli.Execute(args);
        }
    }
}
