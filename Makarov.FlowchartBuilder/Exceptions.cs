using System;

namespace Makarov.FlowchartBuilder
{

    public class FlowchartBuilderException : ApplicationException
    {
        public FlowchartBuilderException()
            : base("Unknown exception in FlowchartBuilder.")
        { }

        public FlowchartBuilderException(string message)
            : base(message)
        { }

        public FlowchartBuilderException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public class SingletonObjectAlreadyExistsException : FlowchartBuilderException
    {

        public SingletonObjectAlreadyExistsException(string typeName)
            : base($"Singleton object (of '{typeName}' class) already exists.")
        { }

        public SingletonObjectAlreadyExistsException(Type t)
            : this(t.Name)
        { }
    }

    public class InvalidValueException : FlowchartBuilderException
    {

        public InvalidValueException(string entity)
            : base($"'{entity}' contains invalid value.")
        { }

        public InvalidValueException(string entity, string val)
            : base($"'{entity}' contains invalid value: '{val}'.")
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
    }

    public sealed class InvalidNodeTypeException : FlowchartBuilderException
    {

        public InvalidNodeTypeException(string nodeType)
            : base($"Invalid node '{nodeType}'.")
        { }
    }

    public sealed class InvalidNodeNameException : FlowchartBuilderException
    {

        public InvalidNodeNameException(string nodeName)
            : base($"Invalid node '{nodeName}'.")
        { }
    }

    public sealed class InvalidContextException : FlowchartBuilderException
    {
        public InvalidContextException()
            : base("Invalid context.")
        { }

        public InvalidContextException(string message)
            : base($"Invalid context: {message ?? string.Empty}")
        { }
    }

    public sealed class CantGetServerAppInstanceException : FlowchartBuilderException
    {
        public CantGetServerAppInstanceException()
            : base(@"Can't get server application instance.")
        { }
    }
}
