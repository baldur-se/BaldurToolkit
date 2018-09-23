using System;

namespace BaldurToolkit.Entities.Components
{
    public interface IComponentProvider<in TComponent>
        where TComponent : class
    {
        /// <summary>
        /// Gets the specified component.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <returns>The instance of the component or null if not found.</returns>
        T GetComponent<T>()
            where T : TComponent;
    }
}
