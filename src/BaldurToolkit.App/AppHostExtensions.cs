using System;

namespace BaldurToolkit.App
{
    public static class AppHostExtensions
    {
        public static void Run(this IAppHost appHost, IApp app)
        {
            appHost.Run(new[] { app });
        }
    }
}
