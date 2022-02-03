using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BindMAX.Interfaces
{
    public interface IBindable
    {
        IBindable Source { get; }

        Dictionary<string, IBindable> Bindings { get; }

        void AddBinding(string key, IBindable subscriber);

        void RemoveBinding(string key);

        void RemoveBinding(IBindable subscriber);

        void Notify([CallerMemberName] string propertyName = "");

        T GetBinding<T>(string key) where T : IBindable;
    }
}
