using System;

namespace BaldurToolkit.Entities
{
    public interface IUpdateable
    {
        void Update(DeltaTime deltaTime);
    }
}
