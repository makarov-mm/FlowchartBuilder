using System;

namespace Makarov.Framework.Core
{

    public class MakarovFrameworkException : ApplicationException
    {
        public MakarovFrameworkException()
            : base("Unknown exception in Makarov.Framework.")
        { }

        public MakarovFrameworkException(string message)
            : base(message)
        { }

        public MakarovFrameworkException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public class SingletonObjectAlreadyExistsException : MakarovFrameworkException
    {

        public SingletonObjectAlreadyExistsException(string typeName)
            : base($"Singleton object (of class '{typeName}') already exists.")
        { }

        public SingletonObjectAlreadyExistsException(Type t)
            : this(t.Name)
        { }

        public SingletonObjectAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public class InvalidValueException : MakarovFrameworkException
    {

        public InvalidValueException(string entity)
            : base($"'{entity}' contains invalid value.")
        { }

        public InvalidValueException(string entity, string val)
            : base($"'{entity}' contains invalid value: '{val}'.")
        { }

        public InvalidValueException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public sealed class NullStringValueException : InvalidValueException
    {

        public NullStringValueException(string entity)
            : base(entity, "null")
        { }

        public NullStringValueException()
            : base("String", "null")
        { }

        public NullStringValueException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
