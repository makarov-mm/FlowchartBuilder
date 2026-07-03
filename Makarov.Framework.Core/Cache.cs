using System;
using System.Collections.Generic;

namespace Makarov.Framework.Core
{

    public sealed class Cache<T> : IDisposable
    {

        public Cache(IDict<T> obj)
        {
            _obj = obj;
        }

        private IDict<T> _obj;

        public void Dispose()
        {
            if (_obj == null)
                return;

            if (_obj is IDisposable)
                ((IDisposable)_obj).Dispose();

            _dict.Clear();
            _obj = null;
        }

        private readonly Dictionary<string, T> _dict = new Dictionary<string, T>();

        public T GetValue(string name)
        {

            if (_dict.ContainsKey(name))
                return _dict[name];

            T val = _obj[name];

            _dict.Add(name, val);

            return val;
        }

        public void SetValue(string name, T val)
        {

            if (_dict.ContainsKey(name))
            {

                T v1 = _dict[name];

                if (v1 == null || !v1.Equals(val))
                {

                    _dict[name] = val;
                    _obj[name] = val;
                }

                return;
            }

            _dict.Add(name, val);
            _obj[name] = val;
        }

        public T this[string name]
        {
            get => GetValue(name);
            set => SetValue(name, value);
        }

        public bool KeyExists(string key)
        {
            return _dict.ContainsKey(key) || _obj.IsKeyExists(key);
        }
    }
}
