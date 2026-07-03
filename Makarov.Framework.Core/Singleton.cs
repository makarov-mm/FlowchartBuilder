using System;

namespace Makarov.Framework.Core
{

    public abstract class Singleton<T> where T : class
    {

        private static T _obj;

        public static T Instance
        {
            get
            {
                unchecked
                {
                    AccessCount++;
                }

                return _obj ?? (_obj = (T)Activator.CreateInstance(typeof(T), true));
            }
        }

        public static long AccessCount { get; private set; }

        public static bool IsInstanceExists => _obj != null;

        public static void FlushInstance()
        {
            _obj = null;
            AccessCount = 0;
        }
    }
}
