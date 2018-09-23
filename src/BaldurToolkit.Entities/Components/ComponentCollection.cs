using System;

namespace BaldurToolkit.Entities.Components
{
    public class ComponentCollection<TComponent> : TypeIndexedList<TComponent>, IComponentProvider<TComponent>
        where TComponent : class
    {
        public T GetComponent<T>()
            where T : TComponent
        {
            return this.TryGet<T>(out var component) ? component : default(T);
        }
    }
}
