using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

#if NETCOREAPP
using System.Runtime.Loader;
#endif

namespace BaldurToolkit.AppRunner
{
    public class AssemblyResolver
    {
        private readonly List<string> directories = new List<string>();

        private readonly List<string> extensions = new List<string>() { ".dll", ".exe" };

        public void AddDirectory(string path)
        {
            this.directories.Add(path);
        }

        public void Register()
        {
#if NETCOREAPP
            AssemblyLoadContext.Default.Resolving += (context, assemblyName) => this.Resolve(assemblyName);
#else
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => this.Resolve(new AssemblyName(args.Name));
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (sender, args) => this.Resolve(new AssemblyName(args.Name));
#endif

        }

        protected Assembly Resolve(AssemblyName assemblyName)
        {
            foreach (var directoryPath in this.directories)
            {
                foreach (var extension in this.extensions)
                {
                    var path = Path.GetFullPath(directoryPath + Path.DirectorySeparatorChar + assemblyName.Name + extension);
                    if (File.Exists(path))
                    {
                        return this.LoadAssemblyFromPath(path);
                    }
                }
            }

            return null;
        }

        protected Assembly LoadAssemblyFromPath(string path)
        {
#if NETCOREAPP
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
#else
            return Assembly.LoadFile(path);
#endif
        }
    }
}
