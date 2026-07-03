using System;

namespace Makarov.Framework.Core
{

    public static class Settings
    {
        #region Environment
        public static class Environment
        {

            private static bool? _isMono;

            public static bool IsMono
            {
                get
                {
                    if (_isMono != null && _isMono.HasValue)
                        return _isMono.Value;

                    try
                    {
                        return (_isMono = Type.GetType("Mono.Runtime") != null).Value;
                    }
                    catch
                    {
                        return (_isMono = false).Value;
                    }
                }
            }
        }
        #endregion

        #region Xml

        public static class Xml
        {
            #region Xml Namespaces

            public static class Namespaces
            {

                public const string Base = @"http://flowchartbuilder.blogspot.com";
            }
            #endregion

            #region Xml Tags

            public static class Tags
            {

                public const string FrameworkSettings = @"fb:FrameworkSettings";

                public const string Param = @"fb:Param";

                public const string Translation = @"fb:Translation";

                public const string Word = @"fb:Word";
            }
            #endregion

            #region Xml Attributes

            public static class Attributes
            {

                public static string Key = @"Key";

                public static string Language = @"Language";
            }
            #endregion
        }
        #endregion
    }
}
