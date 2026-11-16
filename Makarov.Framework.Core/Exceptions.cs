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
}
