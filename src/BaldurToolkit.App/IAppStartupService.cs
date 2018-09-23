using System;

namespace BaldurToolkit.App
{
    public interface IAppStartupService
    {
        void StartServices(IApp app);
    }
}
