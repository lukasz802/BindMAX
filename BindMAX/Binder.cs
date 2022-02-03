using BindMAX.Attributes;
using BindMAX.Enums;
using BindMAX.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Caching;

namespace BindMAX
{
    public class Binder : IBindable, INotifyPropertyChanged
    {
        #region Fields

        private static MemoryCache cache = MemoryCache.Default;

        private readonly Dictionary<string, IBindable> bindings = new Dictionary<string, IBindable>();

        private readonly Guid id = Guid.NewGuid();

        private IBindable source = null;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        public Dictionary<string, IBindable> Bindings
        {
            get
            {
                return new Dictionary<string, IBindable>(bindings);
            }
        }

        public IBindable Source { get => source; }

        #endregion

        #region Public methods

        public void Notify([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            var propertyInfo = GetPropertyInfo(this, propertyName);

            if (cache.Contains(string.Concat(id, "_", propertyName)) || !propertyInfo.IsSharedProperty)
            {
                cache.Remove(string.Concat(id, "_", propertyName));
                return;
            }

            if (propertyInfo.BindingType != BindingType.OneWayToSource)
            {
                foreach (var binding in bindings)
                {
                    var temp = GetPropertyInfo(binding.Value, propertyName);

                    if (temp.IsSharedProperty && temp.BindingType != BindingType.OneWayToSource && temp.PropertyValue != propertyInfo.PropertyValue)
                    {
                        cache.Add(string.Concat(GetId(binding.Key), "_", propertyName), temp, new CacheItemPolicy());
                        binding.Value.GetType().GetProperty(propertyName).SetValue(binding.Value, propertyInfo.PropertyValue);
                    }
                }
            }

            if (propertyInfo.BindingType != BindingType.OneWay)
            {
                Source?.GetType().GetProperty(propertyName)?.SetValue(Source, propertyInfo.PropertyValue);
            }
        }

        public void AddBinding(string key, IBindable instance)
        {
            if (instance == this)
            {
                throw new ArgumentException("Source cannot be the same as bindable instance.");
            }

            Type resultType = instance.GetType();

            while (resultType.BaseType != typeof(Binder))
            {
                resultType = instance.GetType().BaseType;
            }

            resultType.BaseType.GetField("source", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, this);
            bindings.Add(key, instance);
        }

        public void RemoveBinding(string key)
        {
            if (bindings.TryGetValue(key, out IBindable instance))
            {
                Type resultType = instance.GetType();

                while (resultType.BaseType != typeof(Binder))
                {
                    resultType = instance.GetType().BaseType;
                }

                resultType.BaseType.GetField("source", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, null);
                bindings.Remove(key);
            }
        }

        public void RemoveBinding(IBindable instance)
        {
            var result = bindings.Values.FirstOrDefault(x => x == instance);

            if (result != null)
            {
                instance.RemoveBinding(result);
            }
        }

        public T GetBinding<T>(string key) where T : IBindable
        {
            bindings.TryGetValue(key, out IBindable instance);
            return (T)instance;
        }

        #endregion

        #region Private methods

        private string GetId(string key)
        {
            if (bindings.TryGetValue(key, out IBindable instance))
            {
                Type resultType = instance.GetType();

                while (resultType.BaseType != typeof(Binder))
                {
                    resultType = instance.GetType().BaseType;
                }

                return resultType.BaseType.GetField("id", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance).ToString();
            }

            return null;
        }

        private DataTransferObjects.PropertyInfo GetPropertyInfo(object obj, string propertyName)
        {
            PropertyInfo info = obj.GetType().GetProperty(propertyName);
            var tempResult = new DataTransferObjects.PropertyInfo()
            {
                IsSharedProperty = false,
                BindingType = null,
                PropertyValue = null
            };

            if (info != null)
            {
                tempResult.IsSharedProperty = Attribute.IsDefined(info, typeof(BindingProperty));
                tempResult.BindingType = info.GetCustomAttribute<BindingProperty>()?.BindingType;
                tempResult.PropertyValue = info.GetValue(obj);
            }

            return tempResult;
        }

        #endregion
    }
}
