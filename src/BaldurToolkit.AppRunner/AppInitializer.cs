using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using BaldurToolkit.App;
using BaldurToolkit.App.DependencyInjection;
using BaldurToolkit.App.FileSystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.PlatformAbstractions;

namespace BaldurToolkit.AppRunner
{
    public class AppInitializer
    {
        public AppInitializer(string appXmlFile)
        {
            this.AppXmlFile = appXmlFile;
        }

        public string AppXmlFile { get; private set; }

        public string AppKind { get; set; } = AppIdentity.DefaultKind;

        public string PathMapName { get; set; } = "default";

        public string PathMapPrefix { get; set; }

        public IReadOnlyDictionary<string, string> PathMapOverrides { get; set; }

        public string EnvName { get; set; }

        public bool DisableRunnerModule { get; set; }

        public virtual IApp Initialize()
        {
            var basePathMap = this.CreateBasePathMap();

            var xmlFilePath = basePathMap.Compile(this.AppXmlFile).FirstOrDefault();
            var appInfo = this.LoadAppInfoXml(xmlFilePath);

            var appPathMap = this.CreateAppPathMap(appInfo, basePathMap);
            var appConfiguration = this.LoadAppConfiguration(appInfo, appPathMap);

            var app = this.InitAppInstance(appInfo, appPathMap, appConfiguration);

            return app;
        }

        protected virtual PathMap CreateBasePathMap()
        {
            var basePathMap = new PathMap();
            basePathMap.Set("WorkingDir", Directory.GetCurrentDirectory());
            basePathMap.Set("AppRootDir", PlatformServices.Default?.Application.ApplicationBasePath ?? Directory.GetCurrentDirectory());
            basePathMap.Set("SystemTempDir", Path.GetTempPath());

            this.ApplyPathMapOverrides(basePathMap);

            return basePathMap;
        }

        protected virtual void ApplyPathMapOverrides(PathMap pathMap)
        {
            foreach (var kvp in this.PathMapOverrides)
            {
                pathMap.Set(kvp.Key, kvp.Value);
            }
        }

        protected virtual Xml.AppInfo LoadAppInfoXml(string path)
        {
            var serializer = new XmlSerializer(typeof(Xml.AppInfo));
            using (var stream = File.OpenRead(path))
            {
                return (Xml.AppInfo)serializer.Deserialize(stream);
            }
        }

        protected virtual PathMap CreateAppPathMap(Xml.AppInfo appInfo, PathMap basePathMap)
        {
            var appPathMaps = new Dictionary<string, PathMap>();
            foreach (var pm in appInfo.PathMaps)
            {
                var appPathMap = new PathMap(pm.Name, pm.Prefix, basePathMap);
                appPathMap.Set("AppName", appInfo.App.Name);
                appPathMap.Set("AppKind", this.AppKind);
                appPathMap.Set("AppEnvironment", this.EnvName ?? string.Empty);

                foreach (var path in pm.Paths ?? new Xml.PathMapPathElement[0])
                {
                    var items = path.Items?.Select(element => element.Path) ?? new[] { path.Path };
                    appPathMap.Set(path.Name, items);
                }

                appPathMaps[appPathMap.Name] = appPathMap;
            }

            foreach (var pm in appInfo.PathMaps)
            {
                if (!string.IsNullOrEmpty(pm.Extends))
                {
                    var appPathMap = appPathMaps[pm.Name];
                    if (!appPathMaps.TryGetValue(pm.Extends, out var parentPathMap))
                    {
                        throw new Exception($"Can not find base PathMap with name \"{pm.Extends}\"");
                    }

                    appPathMap.Merge(parentPathMap);
                }
            }

            if (!appPathMaps.TryGetValue(this.PathMapName, out var currentPathMap))
            {
                throw new Exception($"Can not find path map \"{this.PathMapName}\".");
            }

            if (this.PathMapPrefix != null)
            {
                currentPathMap.Prefix = this.PathMapPrefix;
            }

            this.ApplyPathMapOverrides(currentPathMap);

            return currentPathMap;
        }

        protected virtual IConfigurationRoot LoadAppConfiguration(Xml.AppInfo appInfo, PathMap appPathMap)
        {
            var builder = new ConfigurationBuilder();
            foreach (var file in appInfo.ConfigurationFiles)
            {
                if (file.Environment != null && !file.Environment.Equals(this.EnvName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var loaded = false;
                foreach (var configPath in appPathMap.Compile(file.Path))
                {
                    if (File.Exists(configPath))
                    {
                        try
                        {
                            FileConfigurationSource source;
                            switch (file.Type)
                            {
                                case "ini": source = new IniConfigurationSource(); break;
                                case "xml": source = new XmlConfigurationSource(); break;
                                case "json": source = new JsonConfigurationSource(); break;
                                default: throw new Exception("Unknown configuration type.");
                            }

                            source.FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(configPath));
                            source.Path = Path.GetFileName(configPath);
                            source.ReloadOnChange = file.ReloadOnChange;

                            builder.Add(source);
                        }
                        catch (Exception exception)
                        {
                            throw new Exception($"Can not read configuration file \"{configPath}\": {exception.Message}", exception);
                        }

                        loaded = true;
                        break;
                    }
                }

                if (!file.Optional && !loaded)
                {
                    throw new Exception($"Can not find required configuration file \"{file.Path}\".");
                }
            }

            return builder.Build();
        }

        protected virtual IApp InitAppInstance(Xml.AppInfo appInfo, PathMap appPathMap, IConfigurationRoot appConfiguration)
        {
            // Load types described in the application XML file
            var appType = this.LoadTypeByName(appInfo.App.Type);
            var modules = new AppModuleCollection();
            foreach (var module in appInfo.Modules)
            {
                if (module.Environment != null && !module.Environment.Equals(this.EnvName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                modules.Add(this.LoadTypeByName(module.Type));
            }

            var startupServices = new StartupServiceCollection();
            foreach (var startupService in appInfo.StartupServices)
            {
                if (startupService.Environment != null && !startupService.Environment.Equals(this.EnvName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                startupServices.Add(this.LoadTypeByName(startupService.Type));
            }

            // Initialize service container required to build and run an application
            var services = new ServiceCollection();
            services.AddTransient(appType);
            services.AddSingleton(provider => new AppIdentity(appInfo.App.Name, this.AppKind));
            services.AddSingleton(provider => new AppEnvironment(this.EnvName, appPathMap));
            services.AddSingleton(provider => appConfiguration);
            services.AddSingleton<IAppModuleCollection>(provider => modules);
            services.AddSingleton<IStartupServiceCollection>(provider => startupServices);
            services.AddTransient<AppRunnerModule>();
            if (PlatformServices.Default != null)
            {
                services.AddSingleton(provider => PlatformServices.Default);
            }

            var container = services.BuildServiceProvider();

            // Inject runner module in the app and build the app
            if (!this.DisableRunnerModule)
            {
                modules.StageZero.Add(container.GetRequiredService<AppRunnerModule>());
            }

            // Create the app
            return (IApp)container.GetService(appType);
        }

        protected virtual Type LoadTypeByName(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type == null)
            {
                throw new TypeLoadException($"Can not find type \"{typeName}\".");
            }

            return type;
        }
    }
}
