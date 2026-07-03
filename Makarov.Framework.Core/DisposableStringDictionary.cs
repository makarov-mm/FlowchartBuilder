using System;
using System.Collections.Generic;

namespace Makarov.Framework.Core
{

    public class DisposableStringDictionary<T> : StringDictionary<T>, IDisposable
        where T: IDisposable
    {
        public void Dispose()
        {
            foreach (KeyValuePair<string, T> kvp in this)
                kvp.Value.Dispose();

            Clear();
        }
    }
}
