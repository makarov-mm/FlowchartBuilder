using System;

namespace Makarov.FlowchartBuilder
{

    public sealed partial class Core
    {

        public abstract class CoreException : FlowchartBuilderException
        {

            protected CoreException(string message)
                : base(message)
            { }
        }

        public sealed class CoreAlreadyExistsException : SingletonObjectAlreadyExistsException
        {
            public CoreAlreadyExistsException()
                : base("Core already exists.")
            { }
        }

        public sealed class CoreAlreadyDisposedException : CoreException
        {
            public CoreAlreadyDisposedException()
                : base("Core already disposed.")
            { }
        }

        public sealed class CurrentDocumentNotExistsException : CoreException
        {
            public CurrentDocumentNotExistsException()
                : base("Current document not exists.")
            { }
        }

        public sealed class WindowTypeNotFoundException : CoreException
        {

            public WindowTypeNotFoundException(string type)
                : base($@"Window type '{type ?? string.Empty}' not found.")
            { }

            public WindowTypeNotFoundException(Type type)
                : this(type.Name)
            { }
        }

        public sealed class CantUseMainWindowAsChildException : CoreException
        {
            public CantUseMainWindowAsChildException()
                : base(@"Can't use main window as child window.")
            { }
        }
    }
}
