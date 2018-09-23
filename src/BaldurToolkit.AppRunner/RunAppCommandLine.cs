using System;
using System.Collections.Generic;
using System.Linq;
using BaldurToolkit.App;
using Microsoft.Extensions.CommandLineUtils;

namespace BaldurToolkit.AppRunner
{
    public class RunAppCommandLine : CommandLineApplication
    {
        public RunAppCommandLine(string appXmlFilePath = null, bool throwOnUnexpectedArg = true)
            : base(throwOnUnexpectedArg)
        {
            this.Description = "Run an application described in the XML file";

            this.HelpOption("-?|-h|--help");

            var requiredArguments = new List<CommandArgument>();

            CommandArgument appXmlFileArgument = null;
            CommandOption appXmlFileOption = null;
            if (appXmlFilePath == null)
            {
                appXmlFileArgument = this.Argument("app_xml_file_path", "Path to the app xml file");
                requiredArguments.Add(appXmlFileArgument);
            }
            else
            {
                appXmlFileOption = this.Option("-a|--app <APP_XML_FILE_PATH>", "Specify different app xml file.", CommandOptionType.SingleValue);
            }

            var appInstanceNameOption = this.Option("-i|--id <INSTANCE_NAME>", "Specify app instance name", CommandOptionType.SingleValue);
            var envNameOption = this.Option("-e|--env <NAME>", "Set an environment name", CommandOptionType.SingleValue);
            var pathMapNameOption = this.Option("-p|--pathmap <NAME>", "Specify a pathmap name to use for the application", CommandOptionType.SingleValue);
            var pathMapPrefOption = this.Option("--pathmap-prefix <PREFIX>", "Set a pathmap prefix", CommandOptionType.SingleValue);
            var noRunnerModuleOption = this.Option("--no-runner-module", "Disable AppRunnerModule injection to the app's StageZero module list", CommandOptionType.NoValue);
            var pathmapOverrideOption = this.Option("--path <MAP>", "Override a path value. Example: --path WorkingDir=./subdir", CommandOptionType.MultipleValue);
            var assemblyDirOption = this.Option("--assembly-dir <PATH>", "Add a directory to search assemblies in.", CommandOptionType.MultipleValue);

            this.OnExecute(() =>
            {
                var missingArgument = requiredArguments.FirstOrDefault(argument => string.IsNullOrEmpty(argument.Value));
                if (missingArgument != null)
                {
                    Console.WriteLine("Missing required argument: {0}", missingArgument.Name);
                    this.ShowHint();
                    return 1;
                }

                appXmlFilePath = appXmlFilePath != null
                    ? appXmlFileOption.Value() ?? appXmlFilePath // appXmlFilePath was specified in the constructor, but can be overriden by CLI
                    : appXmlFileArgument.Value; // appXmlFilePath is required by CLI

                var pathMapOverrides = new Dictionary<string, string>();
                foreach (var entry in pathmapOverrideOption.Values)
                {
                    var parts = entry.Split('=');
                    if (parts.Length == 2)
                    {
                        pathMapOverrides[parts[0]] = parts[1];
                    }
                }

                if (assemblyDirOption.Values.Count > 0)
                {
                    var assemblyResolver = new AssemblyResolver();
                    foreach (var dir in assemblyDirOption.Values)
                    {
                        assemblyResolver.AddDirectory(dir);
                    }

                    assemblyResolver.Register();
                }

                var appInitializer = new AppInitializer(appXmlFilePath)
                {
                    AppInstanceName = appInstanceNameOption.Value() ?? AppIdentity.DefaultInstanceName,
                    PathMapName = pathMapNameOption.Value() ?? "default",
                    PathMapPrefix = pathMapPrefOption.Value(),
                    PathMapOverrides = pathMapOverrides,
                    EnvName = envNameOption.Value(),
                    DisableRunnerModule = noRunnerModuleOption.HasValue(),
                };

                Console.WriteLine("Initializing App...");
                Console.WriteLine("  App xml file: \"{0}\"", appInitializer.AppXmlFile);
                Console.WriteLine("  App instance name: \"{0}\"", appInitializer.AppInstanceName);
                Console.WriteLine("  PathMap name: \"{0}\"", appInitializer.PathMapName);
                Console.WriteLine("  PathMap prefix: \"{0}\"", appInitializer.PathMapPrefix);
                Console.WriteLine("  Environment name: \"{0}\"", appInitializer.EnvName);
                Console.WriteLine("  App runner module: {0}", appInitializer.DisableRunnerModule ? "no" : "yes");
                Console.WriteLine();

                var app = appInitializer.Initialize();

                Console.Title = $"{app.Name} ({app.InstanceName})";
                Console.WriteLine("Starting the app...");
                Console.WriteLine();

                app.Started += (sender, eventArgs) =>
                {
                    Console.WriteLine();
                    Console.WriteLine("{0} started.", sender);
                    Console.WriteLine("Press [Ctrl]+[C] to gracefully shutdown the app.");
                    Console.WriteLine();
                };
                app.Stopped += (sender, eventArgs) =>
                {
                    Console.WriteLine();
                    Console.WriteLine("{0} stopped.", sender);
                };

                var host = new AppHost();
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Console.WriteLine();
                    Console.WriteLine("Stopping app host...");
                    host.Stop();
                    eventArgs.Cancel = true;
                };
                AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    Console.WriteLine();
                    Console.WriteLine("Stopping app host...");
                    host.Stop();
                };

                host.Run(app);

                return 0;
            });
        }
    }
}
