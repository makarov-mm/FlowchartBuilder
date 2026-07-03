using System;

namespace Makarov.Framework.Core
{

    public abstract class AbstractSerializer
    {

        public abstract Type SerializerType { get; }

        public abstract string Serialize(object obj);

        public abstract object Deserialize(string str);
    }
}
