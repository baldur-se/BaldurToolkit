using System;
using System.Collections.Generic;

namespace BaldurToolkit.App
{
    public interface IAppHost : IDisposable
    {
        void Run(IEnumerable<IApp> apps);
    }
}
