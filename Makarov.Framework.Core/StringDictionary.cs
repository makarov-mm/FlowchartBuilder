using System;
using System.Collections.Generic;

namespace Makarov.Framework.Core
{

    public class StringDictionary<T>: Dictionary<string, T>, IDict<T>
    {
        T IDict<T>.this[string key]
        {
            get => base[key];
            set => base[key] = value;
        }

        public bool IsKeyExists(string key)
        {
            return ContainsKey(key);
        }
    }
}
